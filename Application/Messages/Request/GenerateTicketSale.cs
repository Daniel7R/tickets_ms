namespace TicketsMS.Application.Messages.Request
{
    /// <summary>
    ///    dto to handle Ticket Sale Queue
    /// </summary>
    public class GenerateTicketSale
    {
        /// <summary>
        ///   Id of the transaction
        /// </summary>
        public int IdTransaction { get; set; }
        /// <summary>
        ///  Id of the user
        /// </summary>
        public int IdUser { get; set; }
        /// <summary>
        ///  Id of the ticket
        /// </summary>
        public int? IdTicket { get; set; }
    }
}
