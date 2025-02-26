namespace TicketsMS.Application.Queues
{
    public static class Queues
    {
        //to consume
        public const string GENERATE_PARTICIPANTS_TICKETS_ASYNC = "generate.tournament.participant.tickets.async";
        //to produce
        public const string GET_TOURNAMENT_BY_ID = "tournament.by_id";
        public const string GET_MATCH_BY_ID = "match.by_id";
    }
}
