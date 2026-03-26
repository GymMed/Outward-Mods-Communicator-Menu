using System;

namespace OutwardModsCommunicatorMenu.Utility.Parsing
{
    public interface IValueParser
    {
        (object Value, string Error) TryParse(string valueString, Type targetType);
    }

    public class ValueParser : IValueParser
    {
        private readonly IPrimitiveValueParser _primitiveParser;
        private readonly INullableValueParser _nullableParser;
        private readonly IEnumValueParser _enumParser;
        private readonly ICollectionValueParser _collectionParser;

        public ValueParser()
        {
            _primitiveParser = new PrimitiveValueParser();
            _enumParser = new EnumValueParser();
            _nullableParser = new NullableValueParser(_primitiveParser, _enumParser);
            _collectionParser = new CollectionValueParser(_primitiveParser, _enumParser);
        }

        public (object Value, string Error) TryParse(string valueString, Type targetType)
        {
            if (targetType == null)
                return (null, "Target type is null");

            if (string.IsNullOrEmpty(valueString))
                return (null, "Empty value");

            if (IsNullableType(targetType))
            {
                return _nullableParser.TryParse(valueString, targetType);
            }

            if (targetType.IsEnum)
            {
                return _enumParser.TryParse(valueString, targetType);
            }

            if (IsCollectionType(targetType))
            {
                return _collectionParser.TryParse(targetType, valueString);
            }

            return _primitiveParser.TryParse(valueString, targetType);
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static bool IsCollectionType(Type type)
        {
            if (type == null) return false;

            if (type.IsArray) return true;

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    var genericDef = type.GetGenericTypeDefinition();
                    if (genericDef == typeof(System.Collections.Generic.HashSet<>) ||
                        genericDef == typeof(System.Collections.Generic.List<>) ||
                        genericDef == typeof(System.Collections.Generic.IList<>) ||
                        genericDef == typeof(System.Collections.Generic.ICollection<>) ||
                        genericDef == typeof(System.Collections.Generic.IEnumerable<>))
                    {
                        return true;
                    }
                }

                if (typeof(System.Collections.ICollection).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}