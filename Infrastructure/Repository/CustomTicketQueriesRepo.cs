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

        public async Task<Tickets> GetTicketByCode(string code)
        {
            return await _context.Tickets.Where(t => t.Code == code).Include(t => t.TicketSales).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Tickets>> GetTicketsByStatus(TicketStatus status)
        {
            return await _context.Tickets.Where(ticket => ticket.Status == status).ToListAsync();
        }

        public async Task<IEnumerable<Tickets>> GetTicketsByStatusAndIdTournament(TicketStatus status, int idTournament)
        {
            return await _context.Tickets.Where(ticket => ticket.Status == status && ticket.IdTournament == idTournament).ToListAsync();
        }


        public async Task<IEnumerable<Tickets>> GetTicketsByUser(int idUser)
        {
            return await _context.TicketSales.Where(ts => ts.IdUser==idUser).Select(ts => ts.Ticket).ToListAsync();
        }

        public async Task UpdateStatus(int idTicket, TicketStatus newStatus)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await _context.Tickets.Where(t => t.Id == idTicket)
                       .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.Status, newStatus));
                await transaction.CommitAsync();   
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
