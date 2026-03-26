using System;

namespace OutwardModsCommunicatorMenu.Utility.Parsing
{
    public interface INullableValueParser
    {
        (object Value, string Error) TryParse(string valueString, Type nullableType);
    }
}