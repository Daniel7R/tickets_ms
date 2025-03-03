namespace TicketsMS.Application.Queues
{
    public static class Queues
    {
        //to consume
        public const string GENERATE_PARTICIPANTS_TICKETS_ASYNC = "tournament.participant.tickets";
        //consumed in PaymentsMS
        public const string GET_TICKET_INFO = "ticket.info";
        public const string SELL_TICKET_PARTICIPANT = "ticket.participant.sale";
        public const string SELL_TICKET_VIEWER = "ticket.viewer.sale";
        //to produce
        public const string GET_TOURNAMENT_BY_ID = "tournament.by_id";
        public const string GET_MATCH_BY_ID = "match.by_id";
        // produce for email service 
        public const string SEND_EMAIL_NOTIFICATION = "ticket.sale.notification";
    }
}
