using TicketsMS.Domain.Enums;

namespace TicketsMS.Application.DTOs.Request
{
    public class TicketRequestDTO
    {
        public int? IdTournament { get; set; }
        public int? IdMatch { get; set; }
        public TicketType Type {  get; set; }
        public bool? IsFree { get; set; }
        public decimal Price {  get; set; }
    }
}
