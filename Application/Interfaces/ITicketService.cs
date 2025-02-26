using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.DTOs.Response;
using TicketsMS.Domain.Entities;

namespace TicketsMS.Application.Interfaces
{
    public interface ITicketService
    {
        public Task<TicketResponseDTO> CreateTicketAsync(Tickets request);
        public Task<Tickets> GenerateTicketParticipant(int idTournament, bool isFree, decimal price );
        public Tickets GenerateTicketViewer(int idMatch, bool isFree, decimal price);
        public string GenerateTicketCode();
    }
}
