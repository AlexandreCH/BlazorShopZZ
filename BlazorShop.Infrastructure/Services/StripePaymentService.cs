namespace BlazorShop.Infrastructure.Services
{
    using BlazorShop.Application.DTOs.Payment;
    using BlazorShop.Application.Services.Contracts.Payment;
    using BlazorShop.Domain.Contracts.Payment;
    using BlazorShop.Domain.Entities.Payment;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Stripe;
    using Stripe.Checkout;
    using StripeConfig = BlazorShop.Application.DTOs.Payment.StripeConfiguration;

    public class StripePaymentService : IPaymentService
    {
        private readonly ILogger<StripePaymentService> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly AppConfiguration _appConfig;
        private readonly StripeConfig _stripeConfig;

        public StripePaymentService(
            ILogger<StripePaymentService> logger,
            IOrderRepository orderRepository,
            IOptions<AppConfiguration> appConfig,
            IOptions<StripeConfig> stripeConfig)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _appConfig = appConfig.Value;
            _stripeConfig = stripeConfig.Value;
        }

        public async Task<PaymentResult> CreateCheckoutSessionAsync(Order order)
        {
            try
            {
                _logger.LogInformation(
                    "Creating Stripe checkout session for order {OrderId}, Amount {Amount}",
                    order.Id,
                    order.TotalAmount);

                var lineItems = order.Lines.Select(line => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Order Item #{line.ProductId.ToString()[..8]}",
                            Description = $"Quantity: {line.Quantity}"
                        },
                        UnitAmount = (long)(line.UnitPrice * 100), // Convert to cents
                    },
                    Quantity = line.Quantity,
                }).ToList();

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    ClientReferenceId = order.Id.ToString(),
                    SuccessUrl = $"{_appConfig.FrontendUrl}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{_appConfig.FrontendUrl}/payment-cancel?order_id={order.Id}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_id", order.Id.ToString() },
                        { "user_id", order.UserId },
                        { "order_reference", order.Reference }
                    }
                };

                var requestOptions = new RequestOptions
                {
                    IdempotencyKey = $"order-{order.Id}-{DateTime.UtcNow.Ticks}"
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options, requestOptions);

                _logger.LogInformation(
                    "Stripe session created: {SessionId} for order {OrderId}",
                    session.Id, order.Id);

                // Update order with payment reference
                await _orderRepository.UpdatePaymentReferenceAsync(order.Id, session.Id);
                await _orderRepository.UpdateStatusAsync(order.Id, "PaymentInitiated");

                return PaymentResult.SuccessResult(
                    provider: "Stripe",
                    redirectUrl: session.Url,
                    paymentId: session.Id,
                    orderId: order.Id);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex,
                    "Stripe error creating checkout session for order {OrderId}: {ErrorMessage}",
                    order.Id,
                    ex.Message);

                await _orderRepository.UpdateStatusAsync(order.Id, "PaymentFailed");

                return PaymentResult.FailureResult(
                    provider: "Stripe",
                    errorMessage: $"Payment gateway error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error creating checkout session for order {OrderId}",
                    order.Id);

                await _orderRepository.UpdateStatusAsync(order.Id, "PaymentFailed");

                return PaymentResult.FailureResult(
                    provider: "Stripe",
                    errorMessage: "An unexpected error occurred");
            }
        }

        public async Task<bool> HandleWebhookAsync(string json, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(_stripeConfig.WebhookSecret))
                {
                    _logger.LogWarning("Stripe webhook secret not configured. Webhook verification skipped.");
                    return false;
                }

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signature,
                    _stripeConfig.WebhookSecret,
                    throwOnApiVersionMismatch: false
                );

                _logger.LogInformation(
                    "Received Stripe webhook: {EventType} - {EventId}",
                    stripeEvent.Type,
                    stripeEvent.Id);

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        var session = stripeEvent.Data.Object as Session;
                        await HandlePaymentSuccessAsync(session!);
                        break;

                    case "checkout.session.expired":
                        var expiredSession = stripeEvent.Data.Object as Session;
                        await HandlePaymentExpiredAsync(expiredSession!);
                        break;

                    case "charge.refunded":
                        var charge = stripeEvent.Data.Object as Charge;
                        await HandleRefundAsync(charge!);
                        break;

                    default:
                        _logger.LogInformation(
                            "Unhandled Stripe event type: {EventType}",
                            stripeEvent.Type);
                        break;
                }

                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook verification failed: {ErrorMessage}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return false;
            }
        }

        private async Task HandlePaymentSuccessAsync(Session session)
        {
            if (session.ClientReferenceId == null)
            {
                _logger.LogWarning("Stripe session {SessionId} has no client reference ID", session.Id);
                return;
            }

            if (!Guid.TryParse(session.ClientReferenceId, out var orderId))
            {
                _logger.LogWarning(
                    "Invalid order ID format in Stripe session {SessionId}: {ClientReferenceId}",
                    session.Id,
                    session.ClientReferenceId);
                return;
            }

            _logger.LogInformation(
                "Payment completed for order {OrderId}, Session {SessionId}, PaymentIntent {PaymentIntentId}",
                orderId,
                session.Id,
                session.PaymentIntentId);

            // Update order status
            await _orderRepository.UpdateStatusAsync(orderId, "Paid");

            // Update payment reference with payment intent ID if available
            if (!string.IsNullOrEmpty(session.PaymentIntentId))
            {
                await _orderRepository.UpdatePaymentReferenceAsync(orderId, session.PaymentIntentId);
            }

            // TODO: Send order confirmation email
            // TODO: Trigger fulfillment workflow
        }

        private async Task HandlePaymentExpiredAsync(Session session)
        {
            if (session.ClientReferenceId == null) return;

            if (!Guid.TryParse(session.ClientReferenceId, out var orderId)) return;

            _logger.LogWarning(
                "Payment expired for order {OrderId}, Session {SessionId}",
                orderId,
                session.Id);

            await _orderRepository.UpdateStatusAsync(orderId, "PaymentExpired");

            // TODO: Send payment expired email
        }

        private async Task HandleRefundAsync(Charge charge)
        {
            _logger.LogInformation(
                "Refund processed for charge {ChargeId}, Amount {Amount}",
                charge.Id,
                charge.AmountRefunded);

            // TODO: Find order by payment reference and update status
            // TODO: Send refund confirmation email
            await Task.CompletedTask;
        }
    }
}
