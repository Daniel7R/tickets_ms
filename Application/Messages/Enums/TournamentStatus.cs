using System.Runtime.Serialization;

namespace TicketsMS.Application.Messages.Enums
{
    public enum TournamentStatus
    {
        [EnumMember(Value = "PENDING")]
        PENDING,
        [EnumMember(Value = "ONGOING")]
        ONGOING,
        [EnumMember(Value = "FINISHED")]
        FINISHED
    }
}
