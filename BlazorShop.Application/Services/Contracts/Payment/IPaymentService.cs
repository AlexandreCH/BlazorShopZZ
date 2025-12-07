namespace BlazorShop.Application.Services.Contracts.Payment
{
    using BlazorShop.Application.DTOs.Payment;
    using BlazorShop.Domain.Entities.Payment;

    public interface IPaymentService
    {
        Task<PaymentResult> CreateCheckoutSessionAsync(Order order);
        
        Task<bool> HandleWebhookAsync(string json, string signature);
    }
}
