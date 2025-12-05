namespace BlazorShop.Web.Shared.Services.Contracts
{
    using BlazorShop.Web.Shared.Models;
    using BlazorShop.Web.Shared.Models.SupportTicket;

    public interface ISupportTicketService
    {
        Task<ServiceResponse> SubmitTicketAsync(SubmitTicketRequest request);
    }
}
