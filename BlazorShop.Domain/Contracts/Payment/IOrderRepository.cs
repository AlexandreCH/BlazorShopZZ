namespace BlazorShop.Domain.Contracts.Payment
{
    using BlazorShop.Domain.Entities.Payment;

    public interface IOrderRepository
    {
        Task<Guid> CreateAsync(Order order);

        Task<Order?> GetByReferenceAsync(string reference);

        Task<Order?> GetByIdAsync(Guid orderId);

        Task<Order?> GetByPaymentReferenceAsync(string paymentReference);

        Task<int> UpdateStatusAsync(Guid orderId, string status);

        Task<int> UpdatePaymentReferenceAsync(Guid orderId, string paymentReference);

        Task<int> UpdatePaymentMethodAsync(Guid orderId, string paymentMethod);

        Task<List<Order>> GetByUserIdAsync(string userId);

        Task<List<Order>> GetAllAsync();
    }
}
