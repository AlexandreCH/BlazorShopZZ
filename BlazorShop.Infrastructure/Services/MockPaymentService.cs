namespace BlazorShop.Infrastructure.Services
{
    using BlazorShop.Application.DTOs.Payment;
    using BlazorShop.Application.Services.Contracts.Payment;
    using BlazorShop.Domain.Contracts.Payment;
    using BlazorShop.Domain.Entities.Payment;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Mock payment service for development and testing
    /// Simulates payment processing without real payment gateways
    /// </summary>
    public class MockPaymentService : IPaymentService
    {
        private readonly ILogger<MockPaymentService> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly AppConfiguration _appConfig;
        private readonly PaymentSystemConfiguration _paymentConfig;
        private static readonly Random _random = new();

        public MockPaymentService(
            ILogger<MockPaymentService> logger,
            IOrderRepository orderRepository,
            IOptions<AppConfiguration> appConfig,
            IOptions<PaymentSystemConfiguration> paymentConfig)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _appConfig = appConfig.Value;
            _paymentConfig = paymentConfig.Value;
        }

        public async Task<PaymentResult> CreateCheckoutSessionAsync(Order order)
        {
            _logger.LogInformation(
                "MOCK: Payment initiated for order {OrderId}, Amount {Amount}", 
                order.Id, 
                order.TotalAmount);

            // Simulate payment processing delay
            await Task.Delay(_paymentConfig.MockDelayMs);

            // Determine success based on configured success rate
            var randomValue = _random.Next(100);
            var success = randomValue < _paymentConfig.MockSuccessRate;

            if (success)
            {
                var mockPaymentId = $"mock_pi_{Guid.NewGuid():N}";
                
                _logger.LogInformation(
                    "MOCK: Payment successful for order {OrderId}, PaymentId {PaymentId} (random: {RandomValue} < {SuccessRate})", 
                    order.Id, 
                    mockPaymentId,
                    randomValue,
                    _paymentConfig.MockSuccessRate);

                // Update order immediately (no webhook in mock)
                await _orderRepository.UpdateStatusAsync(order.Id, "Paid");
                await _orderRepository.UpdatePaymentReferenceAsync(order.Id, mockPaymentId);

                return PaymentResult.SuccessResult(
                    provider: "Mock",
                    redirectUrl: $"{_appConfig.FrontendUrl}/payment-success?mock=true&order_id={order.Id}",
                    paymentId: mockPaymentId,
                    orderId: order.Id);
            }
            else
            {
                _logger.LogWarning(
                    "MOCK: Payment failed for order {OrderId} (random: {RandomValue} >= {SuccessRate})", 
                    order.Id,
                    randomValue,
                    _paymentConfig.MockSuccessRate);

                await _orderRepository.UpdateStatusAsync(order.Id, "PaymentFailed");

                return PaymentResult.FailureResult(
                    provider: "Mock",
                    errorMessage: "Mock payment declined (simulated failure for testing)");
            }
        }

        public Task<bool> HandleWebhookAsync(string json, string signature)
        {
            _logger.LogInformation("MOCK: Webhook received (no-op in mock mode)");
            return Task.FromResult(true);
        }
    }
}
