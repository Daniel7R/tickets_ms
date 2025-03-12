using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.DTOs.Response;
using TicketsMS.Application.Interfaces;
using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;
using TicketsMS.Domain.Exceptions;
using TicketsMS.Infrastructure.Filters;

namespace TicketsMS.API.Controllers
{
    [Route("api/v1/tickets")]
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
        [Route("")]
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
        /// This method is in charge to use a ticket for a event
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("use", Name ="UseTicket")]
        [ApiKeyFilter]
        public async Task<IActionResult> UseTicket([FromBody] UseTicketRequest request)
        {
            var response = new ResponseDTO<bool?>();
            try
            {
                var isValid = await _ticketService.UseTicket(request);
                response.Result = isValid;
                response.Message = "Ticket is valid";
                return Ok(response);
            } catch(BusinessRuleException br)
            {
                response.Message = br.Message;
                return BadRequest(response);
            }
        }

        /// <summary>
        ///     Get tickets participant by userid
        /// </summary>
        /// <param name="userId"></param>
        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> GetTicketByUser(int userId)
        {
            var response = new ResponseDTO<IEnumerable<TicketsDetailsDTO>>();

            var ticketes = await _ticketService.GetTicketsByUser(userId);

            response.Result = ticketes;
            //validate if the user is equals to the provided in token
            return Ok(response);
        }
    }
}
