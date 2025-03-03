using TicketsMS.Domain.Entities;
using TicketsMS.Domain.Enums;

namespace TicketsMS.Application.Messages.Response
{
    public class GetTicketInfoResponse
    {

        public int IdTicket { get; set; }
        public TicketType Type { get; set; }
        public TicketStatus Status { get; set; }
    }
}
