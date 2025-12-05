namespace BlazorShop.Application.Services.Contracts
{
    using BlazorShop.Application.DTOs;
    using BlazorShop.Application.DTOs.SupportTicket;

    public interface ISupportTicketService
    {
        Task<ServiceResponse> CreateTicketAsync(CreateSupportTicket dto);

        Task<IEnumerable<GetSupportTicket>> GetAllTicketsAsync();

        Task<GetSupportTicket?> GetTicketByIdAsync(Guid id);
    }
}
