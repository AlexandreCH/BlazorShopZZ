namespace BlazorShop.API.Controllers
{
    using BlazorShop.Application.DTOs;
    using BlazorShop.Application.DTOs.SupportTicket;
    using BlazorShop.Application.Services.Contracts;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketController : ControllerBase
    {
        private readonly ISupportTicketService _supportTicketService;

        public SupportTicketController(ISupportTicketService supportTicketService)
        {
            _supportTicketService = supportTicketService;
        }

        /// <summary>
        /// Submit a new support ticket
        /// </summary>
        /// <param name="dto">Support ticket details</param>
        /// <returns>Result of the operation</returns>
        [HttpPost("submit")]
        public async Task<ActionResult<ServiceResponse>> SubmitTicket([FromBody] CreateSupportTicket dto)
        {
            if (dto == null)
            {
                return BadRequest(new ServiceResponse(false, "Invalid ticket data."));
            }

            var result = await _supportTicketService.CreateTicketAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all support tickets (Admin only)
        /// </summary>
        /// <returns>List of all support tickets</returns>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GetSupportTicket>>> GetAllTickets()
        {
            var tickets = await _supportTicketService.GetAllTicketsAsync();
            return Ok(tickets);
        }

        /// <summary>
        /// Get a specific support ticket by ID (Admin only)
        /// </summary>
        /// <param name="id">Ticket ID</param>
        /// <returns>Support ticket details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GetSupportTicket>> GetTicketById(Guid id)
        {
            var ticket = await _supportTicketService.GetTicketByIdAsync(id);
            return ticket == null ? NotFound() : Ok(ticket);
        }
    }
}
