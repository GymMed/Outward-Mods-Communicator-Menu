using System;
using UniverseLib.Utility;
using OutwardModsCommunicatorMenu;

namespace OutwardModsCommunicatorMenu.Utility.Parsing
{
    public class PrimitiveValueParser : IPrimitiveValueParser
    {
        public (object Value, string Error) TryParse(string valueString, Type targetType)
        {
            if (string.IsNullOrEmpty(valueString))
                return (null, "Empty value");

            if (ParseUtility.TryParse(valueString, targetType, out var result, out Exception _))
            {
                return (result, null);
            }

            return (null, $"Cannot convert '{valueString}' to {targetType.Name}");
        }
    }
}