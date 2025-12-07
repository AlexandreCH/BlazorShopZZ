namespace BlazorShop.Application.DTOs.Payment
{
    /// <summary>
    /// Result of a payment operation
    /// </summary>
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? PaymentId { get; set; }
        public string? RedirectUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public string Provider { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public string? Message { get; set; }
        public object? Metadata { get; set; }

        public static PaymentResult SuccessResult(
            string provider, 
            string? redirectUrl = null, 
            string? paymentId = null,
            Guid? orderId = null)
        {
            return new PaymentResult
            {
                Success = true,
                Provider = provider,
                RedirectUrl = redirectUrl,
                PaymentId = paymentId,
                OrderId = orderId
            };
        }

        public static PaymentResult FailureResult(
            string provider, 
            string errorMessage)
        {
            return new PaymentResult
            {
                Success = false,
                Provider = provider,
                ErrorMessage = errorMessage
            };
        }
    }
}
