namespace BlazorShop.Application.DTOs.Payment
{
    /// <summary>
    /// Application configuration for URLs
    /// </summary>
    public class AppConfiguration
    {
        public string FrontendUrl { get; set; } = "https://localhost:7258";
        public string ApiUrl { get; set; } = "https://localhost:7094";
    }

    /// <summary>
    /// Stripe payment gateway configuration
    /// </summary>
    public class StripeConfiguration
    {
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// PayPal payment gateway configuration
    /// </summary>
    public class PayPalConfiguration
    {
        public string ClientId { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = "https://api-m.sandbox.paypal.com";
        public string WebhookId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payment system configuration
    /// </summary>
    public class PaymentSystemConfiguration
    {
        public bool UseMockPayments { get; set; } = false;
        public int MockSuccessRate { get; set; } = 90; // 90% success rate for mock
        public int MockDelayMs { get; set; } = 1000;
    }
}
