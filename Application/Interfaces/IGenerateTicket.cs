using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;

namespace TicketsMS.Application.Interfaces
{
    public interface IGenerateTicket
    {
        public Task<Tickets> GenerateTicket(TicketType type, int idTournament, bool isFree, decimal price);
        //public Task<Tickets> GenerateTicketParticipant(int idTournament, bool isFree, decimal price);
        //public Tickets GenerateTicketViewer(int idMatch, bool isFree, decimal price);
        public string GenerateTicketCode();
    }
}
