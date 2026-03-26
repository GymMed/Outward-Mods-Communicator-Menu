using System;

namespace OutwardModsCommunicatorMenu.Utility.Parsing
{
    public interface IEnumValueParser
    {
        (object Value, string Error) TryParse(string valueString, Type enumType);
    }
}