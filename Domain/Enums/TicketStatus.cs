using System.Runtime.Serialization;

namespace TicketsMS.Domain.Enums
{
    public enum TicketStatus
    {
        [EnumMember(Value ="ACTIVE")]
        ACTIVE,
        [EnumMember(Value = "USED")]
        USED,
        [EnumMember(Value = "CANCELED")]
        CANCELED,
    }
}
