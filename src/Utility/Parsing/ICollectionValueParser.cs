using System;

namespace OutwardModsCommunicatorMenu.Utility.Parsing
{
    public interface ICollectionValueParser
    {
        (object Value, string Error) TryParse(Type collectionType, string valueString);
    }
}