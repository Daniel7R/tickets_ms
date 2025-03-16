using TicketsMS.Domain.Enums;

namespace TicketsMS.Application.DTOs.Response
{
    public class TicketParticipantResponseDTO
    {

        public int Id { get; set; }
        public int IdTournament { get; set; }
        public TicketType Type { get; set; } = TicketType.PARTICIPANT;
    }
}
