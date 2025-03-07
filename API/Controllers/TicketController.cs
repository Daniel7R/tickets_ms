using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using TicketsMS.Application.Interfaces;
using TicketsMS.Domain.Enums;

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

        /// <summary>
        /// Get tickets by id tournament
        /// </summary>
        /// <param name="ticketType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet]
        [Route("tickets")]
        [Description("This endpoint returns tickets according ticket types")]
        public Task<IActionResult> GetAvailableTickets([FromQuery] int idTournament)
        {
            //get availableTicket
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet]
        [Route("tickets/{userId}")]
        public Task<IActionResult> GetTicketByUser(int userId)
        {

            //validate if the user is equals to the provided in token
            throw new NotImplementedException();
        }
    }
}
