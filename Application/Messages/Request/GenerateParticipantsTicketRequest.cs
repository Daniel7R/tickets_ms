namespace TicketsMS.Application.Messages.Request
{
    public class GenerateParticipantsTicketRequest
    {
        public int IdTournament {  get; set; }
        public bool IsFree { get; set; }
        public int QuantityTickets { get; set; }
    }
}
