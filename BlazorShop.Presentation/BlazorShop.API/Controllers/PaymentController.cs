namespace BlazorShop.API.Controllers
{
    using BlazorShop.Application.DTOs.Payment;
    using BlazorShop.Application.Services.Contracts.Payment;

    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IPayPalPaymentService _payPalPaymentService;
        private readonly IPaymentService _paymentService;

        public PaymentController(
            IPaymentMethodService paymentMethodService, 
            IPayPalPaymentService payPalPaymentService,
            IPaymentService paymentService)
        {
            _paymentMethodService = paymentMethodService;
            _payPalPaymentService = payPalPaymentService;
            _paymentService = paymentService;
        }

        /// <summary>
        /// Get all payment methods
        /// </summary>
        /// <returns>The payment methods </returns>
        [HttpGet("methods")]
        public async Task<ActionResult<IEnumerable<GetPaymentMethod>>> GetPaymentMethods()
        {
            var paymentMethods = await _paymentMethodService.GetPaymentMethodsAsync();
            return !paymentMethods.Any() ? this.NotFound() : this.Ok(paymentMethods);
        }

        /// <summary>
        /// PayPal capture redirect destination (stub implementation)
        /// </summary>
        [HttpGet("paypal/capture")]
        public async Task<IActionResult> CapturePayPal([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest("Missing token");
            var ok = await _payPalPaymentService.CaptureAsync(token);
            if (!ok) return BadRequest("Capture failed");

            return Redirect("https://localhost:7258/payment-success");
        }

        /// <summary>
        /// Stripe webhook endpoint for payment confirmations
        /// This endpoint receives notifications from Stripe about payment events
        /// </summary>
        /// <returns>200 OK if webhook processed successfully, 400 Bad Request if verification fails</returns>
        [HttpPost("stripe/webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            if (string.IsNullOrEmpty(signature))
            {
                return BadRequest("Missing Stripe signature");
            }

            var success = await _paymentService.HandleWebhookAsync(json, signature);

            return success ? Ok() : BadRequest("Webhook verification failed");
        }
    }
}
