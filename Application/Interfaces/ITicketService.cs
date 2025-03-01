using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.DTOs.Response;
using TicketsMS.Domain.Entities;

namespace TicketsMS.Application.Interfaces
{
    public interface ITicketService
    {
        public Task<TicketResponseDTO> CreateTicketAsync(Tickets request);

    }
}
