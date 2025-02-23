using System.ComponentModel.DataAnnotations;

namespace TicketsMS.Domain.Entities
{
    public class TicketSales
    {
        public int Id { get; set; }
        [Required]
        public int IdTicket { get; set; }
        public Tickets Ticket { get; set; }
        [Required]
        public int IdTransaction { get; set; }
        [Required]
        public int IdUser { get; set; }
    }
}
