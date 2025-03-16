using TicketsMS.Domain.Enums;

namespace TicketsMS.Application.DTOs.Request
{
    public class UseTicketRequest
    {
        /// <summary>
        /// Ticket code 
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Use who will be using ticket
        /// </summary>
        public int IdUser { get; set; }

        /// <summary>
        /// Match where ticket is being used
        /// </summary>
        public int IdMatch { get; set; }
        /// <summary>
        /// Ticket type(PARTICIPANT, VIEWER)
        /// </summary>
        public TicketType Type { get; set; }
    }
}
