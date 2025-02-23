using System.Runtime.Serialization;

namespace TicketsMS.Domain.Enums
{
    public enum TicketType
    {
        [EnumMember(Value = "VIEWER")]
        VIEWER,
        [EnumMember(Value = "PARTICIPANT")]
        PARTICIPANT
    }
}
