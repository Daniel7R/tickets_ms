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
        public const string VALIDATE_USER_HAS_TICKETS_TOURNAMENT = "ticket.user.tournament";
        //to produce
        public const string GET_TOURNAMENT_BY_ID = "tournament.by_id";
        public const string ASSIGN_ROLE_VIEWER = "viewer.role";
        public const string GET_MATCH_BY_ID = "match.by_id";
        // produce for email service 
        public const string SEND_EMAIL_NOTIFICATION_SALE = "ticket.sale.notification";

        //tournaments
        public const string GET_BULK_TOURNAMENTS = "tournament.bulk.names";
        public const string MATCH_BELONGS_TOURNAMENT = "match.belongs.tournament";

        public const string CHANGE_TICKETS_PARTICIPANT_USED = "tickets.participant.used";
        public const string CHANGE_TICKETS_VIEWERS_USED = "tickets.viewers.used";
    }
}
