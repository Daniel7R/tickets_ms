using TicketsMS.Application.DTOs.Request;
using TicketsMS.Application.DTOs.Response;
using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;

namespace TicketsMS.Application.Interfaces
{
    public interface ITicketService
    {
        public Task<TicketResponseDTO> CreateTicketAsync(Tickets request);
        Task<IEnumerable<TicketResponseDTO>> GetTicketsByStatus(TicketStatus status);
        Task<IEnumerable<TicketParticipantResponseDTO>> GetTicketsByStatusAndIdTournament(TicketStatus status, int idTournament);
        Task<IEnumerable<TicketsDetailsDTO>> GetTicketsByUser(int idUser, int idRequester);
        Task<bool> UseTicket(UseTicketRequest request);
    }
}
