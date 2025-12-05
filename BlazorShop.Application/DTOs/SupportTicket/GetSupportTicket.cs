namespace BlazorShop.Application.DTOs.SupportTicket
{
    public class GetSupportTicket
    {
        public Guid Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime SubmittedOn { get; set; }

        public DateTime? ResolvedOn { get; set; }
    }
}
