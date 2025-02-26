using TicketsMS.Domain.Enums;

namespace TicketsMS.Application.DTOs.Response
{
    public class TicketResponseDTO
    {
        public int Id { get; set; }
        public string IdEvent { get; set; }
        public string Code { get; set; }
        //if VIEWER, the IdEvent would be a match id, otherwise it would be a tournament id
        public TicketType Type { get; set; }
        public TicketStatus Status { get; set; }
    }
}
