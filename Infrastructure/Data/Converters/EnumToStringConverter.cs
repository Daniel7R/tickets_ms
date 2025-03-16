using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;
using System.Runtime.Serialization;

namespace TicketsMS.Infrastructure.Data.Converters
{
    public class EnumToStringConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : struct, Enum
    {
        public EnumToStringConverter() : base(
            e => ConvertToString(e),
            s => ConvertToEnum(s))
        {
        }

        private static string ConvertToString(TEnum value)
        {
            return value.GetType()
                .GetField(value.ToString())?
                .GetCustomAttribute<EnumMemberAttribute>()?
                .Value ?? value.ToString();
        }

        private static TEnum ConvertToEnum(string value)
        {
            var type = typeof(TEnum);
            foreach (var field in type.GetFields())
            {
                var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
                if (attribute?.Value == value)
                    return (TEnum)field.GetValue(null)!;
            }
            return Enum.Parse<TEnum>(value);
        }
    }
}
