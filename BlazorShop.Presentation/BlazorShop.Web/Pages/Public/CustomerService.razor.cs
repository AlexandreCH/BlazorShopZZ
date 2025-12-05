namespace BlazorShop.Web.Pages.Public
{
    using BlazorShop.Web.Shared.Models.SupportTicket;
    using BlazorShop.Web.Shared.Services.Contracts;
    using Microsoft.AspNetCore.Components;

    public partial class CustomerService
    {
        [Inject]
        private ISupportTicketService SupportTicketService { get; set; } = default!;

        private SubmitTicketRequest Model { get; set; } = new();
        private bool IsSubmitting { get; set; }

        private async Task SubmitTicket()
        {
            if (IsSubmitting)
                return;

            IsSubmitting = true;

            try
            {
                var result = await SupportTicketService.SubmitTicketAsync(Model);

                if (result.Success)
                {
                    ToastService.ShowSuccessToast(result.Message);
                    Model = new SubmitTicketRequest();
                }
                else
                {
                    ToastService.ShowErrorToast(result.Message);
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowErrorToast($"An error occurred: {ex.Message}");
            }
            finally
            {
                IsSubmitting = false;
            }
        }
    }
}