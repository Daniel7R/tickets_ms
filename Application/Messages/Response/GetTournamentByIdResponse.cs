using TicketsMS.Application.Messages.Enums;

namespace TicketsMS.Application.Messages.Response
{
    public class GetTournamentByIdResponse
    {
        public int Id { get; set; }
        public bool IsPaid { get; set; }
        public TournamentStatus Status { get; set; }
    }
}
