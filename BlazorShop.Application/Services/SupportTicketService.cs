namespace BlazorShop.Application.Services
{
    using AutoMapper;
    using BlazorShop.Application.DTOs;
    using BlazorShop.Application.DTOs.SupportTicket;
    using BlazorShop.Application.Services.Contracts;
    using BlazorShop.Application.Services.Contracts.Logging;
    using BlazorShop.Domain.Contracts;

    public class SupportTicketService : ISupportTicketService
    {
        private readonly IGenericRepository<Domain.Entities.SupportTicket> _repository;
        private readonly IEmailService _emailService;
        private readonly IAppLogger<SupportTicketService> _logger;
        private readonly IMapper _mapper;

        public SupportTicketService(
            IGenericRepository<Domain.Entities.SupportTicket> repository,
            IEmailService emailService,
            IAppLogger<SupportTicketService> logger,
            IMapper mapper)
        {
            _repository = repository;
            _emailService = emailService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ServiceResponse> CreateTicketAsync(CreateSupportTicket dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName) || 
                string.IsNullOrWhiteSpace(dto.CustomerEmail) || 
                string.IsNullOrWhiteSpace(dto.Message))
            {
                return new ServiceResponse(false, "All fields are required.");
            }

            try
            {
                var ticket = new Domain.Entities.SupportTicket
                {
                    CustomerName = dto.CustomerName.Trim(),
                    CustomerEmail = dto.CustomerEmail.Trim(),
                    Message = dto.Message.Trim(),
                    Status = Domain.Entities.TicketStatus.New,
                    SubmittedOn = DateTime.UtcNow
                };

                var result = await _repository.AddAsync(ticket);

                if (result <= 0)
                {
                    return new ServiceResponse(false, "Failed to submit ticket.");
                }

                _logger.LogInformation($"Support ticket created: {ticket.Id} from {ticket.CustomerEmail}");

                // Send confirmation email to customer
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            ticket.CustomerEmail,
                            "Support Ticket Received - BlazorShop",
                            GenerateCustomerConfirmationEmail(ticket));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send customer confirmation email to {ticket.CustomerEmail}");
                    }
                });

                // Send notification to support team
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            "support@az-solve.com",
                            $"New Support Ticket #{ticket.Id}",
                            GenerateSupportTeamNotificationEmail(ticket));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send support team notification email");
                    }
                });

                return new ServiceResponse(true, "Your ticket has been submitted successfully. We'll get back to you soon!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket");
                return new ServiceResponse(false, "An error occurred while submitting your ticket.");
            }
        }

        public async Task<IEnumerable<GetSupportTicket>> GetAllTicketsAsync()
        {
            var tickets = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<GetSupportTicket>>(tickets.OrderByDescending(t => t.SubmittedOn));
        }

        public async Task<GetSupportTicket?> GetTicketByIdAsync(Guid id)
        {
            var ticket = await _repository.GetByIdAsync(id);
            return ticket == null ? null : _mapper.Map<GetSupportTicket>(ticket);
        }

        private string GenerateCustomerConfirmationEmail(Domain.Entities.SupportTicket ticket)
        {
            return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #2c3e50;'>Thank You for Contacting BlazorShop Support</h2>
        <p>Dear {ticket.CustomerName},</p>
        <p>We have received your support request and will get back to you as soon as possible.</p>
        
        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #10b981; margin: 20px 0;'>
            <p style='margin: 0;'><strong>Ticket ID:</strong> #{ticket.Id}</p>
            <p style='margin: 10px 0 0 0;'><strong>Submitted:</strong> {ticket.SubmittedOn:MMM dd, yyyy HH:mm} UTC</p>
        </div>
        
        <p><strong>Your Message:</strong></p>
        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 10px 0;'>
            <p style='margin: 0; white-space: pre-wrap;'>{ticket.Message}</p>
        </div>
        
        <p>Our support team typically responds within 24-48 hours during business days.</p>
        
        <p style='margin-top: 30px;'>Best regards,<br/>
        <strong>BlazorShop Customer Service Team</strong></p>
        
        <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;'/>
        <p style='font-size: 12px; color: #6b7280;'>
            This is an automated message. Please do not reply to this email. 
            If you need further assistance, please submit a new ticket or contact us at support@az-solve.com.
        </p>
    </div>
</body>
</html>";
        }

        private string GenerateSupportTeamNotificationEmail(Domain.Entities.SupportTicket ticket)
        {
            return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #dc2626;'>🎫 New Support Ticket Received</h2>
        
        <div style='background-color: #fef2f2; padding: 15px; border-left: 4px solid #dc2626; margin: 20px 0;'>
            <p style='margin: 0;'><strong>Ticket ID:</strong> #{ticket.Id}</p>
            <p style='margin: 10px 0 0 0;'><strong>Status:</strong> {ticket.Status}</p>
            <p style='margin: 10px 0 0 0;'><strong>Submitted:</strong> {ticket.SubmittedOn:MMM dd, yyyy HH:mm} UTC</p>
        </div>
        
        <p><strong>Customer Information:</strong></p>
        <ul>
            <li><strong>Name:</strong> {ticket.CustomerName}</li>
            <li><strong>Email:</strong> <a href='mailto:{ticket.CustomerEmail}'>{ticket.CustomerEmail}</a></li>
        </ul>
        
        <p><strong>Message:</strong></p>
        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 10px 0;'>
            <p style='margin: 0; white-space: pre-wrap;'>{ticket.Message}</p>
        </div>
        
        <p style='margin-top: 30px;'>
            <a href='https://localhost:7258/admin/tickets/{ticket.Id}' 
               style='display: inline-block; padding: 10px 20px; background-color: #10b981; color: white; 
                      text-decoration: none; border-radius: 5px;'>View Ticket in Dashboard</a>
        </p>
        
        <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;'/>
        <p style='font-size: 12px; color: #6b7280;'>
            BlazorShop Support System - Automated Notification
        </p>
    </div>
</body>
</html>";
        }
    }
}
