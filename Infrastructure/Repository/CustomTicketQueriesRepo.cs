using Microsoft.EntityFrameworkCore;
using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;
using TicketsMS.Infrastructure.Data;

namespace TicketsMS.Infrastructure.Repository
{
    public class CustomTicketQueriesRepo : ICustomTicketQueriesRepo
    {
        private readonly TicketDbContext _context;
        public CustomTicketQueriesRepo(TicketDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Tickets>> GetTicketsByStatus(TicketStatus status)
        {
            return await _context.Tickets.Where(ticket => ticket.Status == status).ToListAsync();
        }

        public async  Task<IEnumerable<Tickets>> GetTicketsByStatusAndIdTournament(TicketStatus status, int idTournament)
        {
            return await _context.Tickets.Where(ticket => ticket.Status == status && ticket.IdTournament == idTournament).ToListAsync();
        }
    }
}
