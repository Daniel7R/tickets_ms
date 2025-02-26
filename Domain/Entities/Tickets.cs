using TicketsMS.Domain.Enums;

namespace TicketsMS.Domain.Entities
{
    public class Tickets
    {
        public int Id { get; set; }
        public int? IdTournament { get; set; }
        public int? IdMatch { get; set; }
        public string Code {  get; set; }
        public TicketType Type { get; set; }
        public decimal Price {  get; set; }
        public TicketStatus Status { get; set; }

        public TicketSales TicketSales { get; set; }
    }
}
