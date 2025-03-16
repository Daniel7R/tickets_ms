using TicketsMS.Domain.Enums;

namespace TicketsMS.Application.DTOs.Response
{
    public class TicketsDetailsDTO
    {
        public int Id { get; set; }
        public string Code {  get; set; }
        public int IdTournament {  get; set; }
        public TicketType Type {  get; set; }
        public string Name { get; set; }
    }
}
