namespace TicketsMS.Application.Messages.Request
{
    public class AssignViewerRole
    {
        public int IdUser { get; set; }
        public int IdMatch { get; set; }
    }
}
