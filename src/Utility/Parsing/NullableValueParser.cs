using System;

namespace OutwardModsCommunicatorMenu.Utility.Parsing
{
    public class NullableValueParser : INullableValueParser
    {
        private readonly IPrimitiveValueParser _primitiveParser;
        private readonly IEnumValueParser _enumParser;

        public NullableValueParser(IPrimitiveValueParser primitiveParser, IEnumValueParser enumParser)
        {
            _primitiveParser = primitiveParser ?? throw new ArgumentNullException(nameof(primitiveParser));
            _enumParser = enumParser ?? throw new ArgumentNullException(nameof(enumParser));
        }

        public (object Value, string Error) TryParse(string valueString, Type nullableType)
        {
            if (nullableType == null || !nullableType.IsGenericType || nullableType.GetGenericTypeDefinition() != typeof(Nullable<>))
                return (null, "Type is not nullable");

            Type underlyingType = Nullable.GetUnderlyingType(nullableType);
            if (underlyingType == null)
                return (null, "Cannot get underlying type");

            if (string.IsNullOrEmpty(valueString))
            {
                return (null, null);
            }

            if (underlyingType.IsEnum)
            {
                return _enumParser.TryParse(valueString, underlyingType);
            }

            return _primitiveParser.TryParse(valueString, underlyingType);
        }
    }
}