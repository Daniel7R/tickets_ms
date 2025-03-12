using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;

namespace TicketsMS.Infrastructure.Repository
{
    public interface ICustomTicketQueriesRepo
    {
        Task<IEnumerable<Tickets>> GetTicketsByStatus(TicketStatus status);
        Task<IEnumerable<Tickets>> GetTicketsByStatusAndIdTournament(TicketStatus status, int idTournament);
        Task<IEnumerable<Tickets>> GetTicketsByUser(int idUser);
        Task<Tickets> GetTicketByCode(string code);
        Task UpdateStatus(int idTicket, TicketStatus newStatus);
    }
}
