using Microsoft.AspNetCore.Mvc;
using TicketsMS.Application.Interfaces;

namespace TicketsMS.API.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        //private readonly IEventBusProducer _eventBusProducer;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        [Route("get-available-tickets")]
        public Task<IActionResult> GetAvailableTickets()
        {
            throw new NotImplementedException();
        }
    }
}
