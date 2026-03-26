using System;

namespace OutwardModsCommunicatorMenu.Utility.Parsing
{
    public interface IPrimitiveValueParser
    {
        (object Value, string Error) TryParse(string valueString, Type targetType);
    }
}