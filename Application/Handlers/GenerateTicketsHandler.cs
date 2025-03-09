using Microsoft.EntityFrameworkCore;
using TicketsMS.Application.Interfaces;
using TicketsMS.Application.Messages.Request;
using TicketsMS.Application.Services;
using TicketsMS.Application.Queues;
using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;
using TicketsMS.Infrastructure.Data;
using TicketsMS.Infrastructure.Repository;

namespace TicketsMS.Application.Handlers
{
    public class GenerateTicketsHandler
    {
        private readonly IRepository<Tickets> _ticketsRepo;
        IEventBusConsumer _eventBusConsumer;
        private readonly IRepository<TicketSales> _saleTicketRepo;
        private readonly ILogger<GenerateTicketsHandler> _logger;
        private readonly TicketDbContext _dbContext;
        private readonly IGenerateTicket _generateTicketService;
        public GenerateTicketsHandler(TicketDbContext dbContext, IRepository<Tickets> repository, IRepository<TicketSales> ticketSales,IGenerateTicket ticketService, ILogger<GenerateTicketsHandler> logger, IEventBusConsumer eventBusConsumer)
        {
            _logger = logger;
            _dbContext = dbContext;
            _generateTicketService = ticketService;
            _ticketsRepo = repository;
            _saleTicketRepo = ticketSales;
            _eventBusConsumer = eventBusConsumer;
        }

        public async Task GenerateTicketSaleViewer(GenerateTicketSaleViewer request)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var ticket = await _generateTicketService.GenerateTicket(TicketType.VIEWER, request.IdMatch, false, TicketPrices.VIEWER);
                //update ticket status
                ticket.Status = TicketStatus.ACTIVE;
                await _ticketsRepo.AddAsync(ticket);
                //create sale ticket
                var ticketSale = new TicketSales
                {
                    IdTicket = ticket.Id,
                    IdUser = request.IdUser,
                    IdTransaction = request.IdTransaction
                };

                await _saleTicketRepo.AddAsync(ticketSale);

                var email = new EmailNotificationRequest
                {
                    IdUser = request.IdUser,
                    Subject = "🎫 Ticket Information - Important Update!",
                    Body = $@"
                        <h2 style='color: #2C3E50;'>🎟️ Ticket Details</h2>
                        <p>Dear User,</p>
                        <p>We are pleased to inform you about your ticket update.</p>
                        <p><strong>Your Ticket Code:</strong> <span style='color: #3498DB; font-size: 18px;'>{ticket.Code}</span></p>
                        <p>Use this code to track your request or for further assistance.</p>
                        <p>Best regards,</p>
                        <p><strong>Your Support Team</strong></p>"
                };
                await _eventBusConsumer.PublishEventAsync<EmailNotificationRequest>(email, Queues.Queues.SEND_EMAIL_NOTIFICATION_SALE);
                // assignar role en el torneo
                var viewerRoleReques = new AssignViewerRole
                {
                    IdUser = request.IdUser,
                    IdMatch= request.IdMatch,
                };
                await _eventBusConsumer.PublishEventAsync<AssignViewerRole>(viewerRoleReques, Queues.Queues.ASSIGN_ROLE_VIEWER);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {

                _logger.LogError($"Error assigning ticket sale: {ex.Message}");
                await transaction.RollbackAsync();
            }
        }

        public async Task GenerateTicketSale(GenerateTicketSale request)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var ticketById = await _ticketsRepo.GetByIdAsync((int)request.IdTicket);
                ticketById.Status = TicketStatus.ACTIVE;

                var ticketSale = new TicketSales
                {
                    IdTicket = (int)request.IdTicket,
                    IdUser = request.IdUser,
                    IdTransaction = request.IdTransaction
                };

                //create sale ticket
                await _saleTicketRepo.AddAsync(ticketSale);
                //update ticket status
                await _ticketsRepo.UpdateAsync(ticketById);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                var email = new EmailNotificationRequest
                {
                    IdUser = request.IdUser,
                    Subject = "🎟️ Your Ticket Details Inside!",
                    Body = $"👋 Hello,<br><br>" +
                           $"Here is your unique ticket code: <b>{ticketById.Code}</b> 🎫<br><br>" +
                           $"If you have any questions or need assistance, we're here to help! 😊<br><br>" +
                           $"📩 Feel free to contact our support team anytime.<br><br>" +
                           $"Best regards,<br>" +
                           $"💼 <b>Support Team</b>"
                };
                await _eventBusConsumer.PublishEventAsync<EmailNotificationRequest>(email, Queues.Queues.SEND_EMAIL_NOTIFICATION_SALE);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error assigning ticket sale: {ex.Message}");
                await transaction.RollbackAsync();
            }
        }
        public async Task GenerateTicketsParticipantsAsync(GenerateParticipantsTicketRequest request)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var quantity = request.QuantityTickets;
                var priceTicket = request.IsFree ? 0 : TicketPrices.PARTICIPANT;
                var tasks = Enumerable.Range(0, quantity)
                    .Select(async _ =>
                    {
                        var ticketResponse = await _generateTicketService.GenerateTicket(TicketType.PARTICIPANT, request.IdTournament, request.IsFree, priceTicket);
                        _dbContext.Tickets.Add(ticketResponse);
                    }).ToList();
                await Task.WhenAll(tasks);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generation tickets: {ex.Message}");
                await transaction.RollbackAsync();
            }

        }
    }
}
