using System;
using OutwardModsCommunicatorMenu.Utility.Parsing;

namespace OutwardModsCommunicatorMenu.Utility.Parsing
{
    public class EnumValueParser : IEnumValueParser
    {
        public (object Value, string Error) TryParse(string valueString, Type enumType)
        {
            if (string.IsNullOrWhiteSpace(valueString) || enumType == null || !enumType.IsEnum)
                return (null, "Invalid enum type or empty value");

            try
            {
                var result = Enum.Parse(enumType, valueString, true);
                return (result, null);
            }
            catch
            {
                var enumNames = string.Join(", ", Enum.GetNames(enumType));
                return (null, $"Expected one of: {enumNames}");
            }
        }
    }
}