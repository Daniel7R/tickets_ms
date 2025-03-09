using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.DTOs.Response;
using TicketsMS.Application.Interfaces;
using TicketsMS.Domain.Enums;

namespace TicketsMS.API.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
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
        [HttpGet]
        [Route("tickets")]
        [Description("This endpoint returns tickets according ticket types")]
        [ProducesResponseType(200, Type = typeof(ResponseDTO<List<TicketParticipantResponseDTO?>>))]
        public async Task<IActionResult> GetAvailableTickets([FromQuery, BindRequired] int idTournament)
        {
            //get availableTicket by tournament
            var response = new ResponseDTO<IEnumerable<TicketParticipantResponseDTO>>();
            //generated is that is available
            var tickets = await _ticketService.GetTicketsByStatusAndIdTournament(TicketStatus.GENERATED, idTournament);

            response.Result = tickets;

            return Ok(response);
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
