namespace BlazorShop.Domain.Entities
{
    public class SupportTicket
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.New;

        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedOn { get; set; }
    }

    public enum TicketStatus
    {
        New = 0,
        InProgress = 1,
        Resolved = 2,
        Closed = 3
    }
}
