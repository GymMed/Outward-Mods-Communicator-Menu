using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OutwardModsCommunicatorMenu.Utility.Parsing
{
    public class CollectionValueParser : ICollectionValueParser
    {
        private readonly IPrimitiveValueParser _primitiveParser;
        private readonly IEnumValueParser _enumParser;

        public CollectionValueParser(IPrimitiveValueParser primitiveParser, IEnumValueParser enumParser)
        {
            _primitiveParser = primitiveParser ?? throw new ArgumentNullException(nameof(primitiveParser));
            _enumParser = enumParser ?? throw new ArgumentNullException(nameof(enumParser));
        }

        public (object Value, string Error) TryParse(Type collectionType, string valueString)
        {
            if (collectionType == null || string.IsNullOrWhiteSpace(valueString))
                return (null, "Type or value is null/empty");

            if (!IsCollectionType(collectionType))
                return (null, $"Type '{collectionType.Name}' is not a collection type");

            var elementType = GetElementType(collectionType);
            if (elementType == null)
                return (null, $"Cannot determine element type for '{collectionType.Name}'");

            var values = valueString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 0)
                return (null, "No values provided");

            if (collectionType.IsArray)
            {
                return TryParseArray(elementType, values);
            }

            return TryParseCollection(collectionType, elementType, values);
        }

        private (object Value, string Error) TryParseArray(Type elementType, string[] values)
        {
            var parsedValues = new List<object>();
            
            for (int i = 0; i < values.Length; i++)
            {
                var (parsed, error) = ParseValue(elementType, values[i]);
                if (error != null)
                {
                    return (null, $"Invalid value at position {i + 1}: {error}");
                }
                parsedValues.Add(parsed);
            }

            try
            {
                var array = Array.CreateInstance(elementType, parsedValues.Count);
                for (int i = 0; i < parsedValues.Count; i++)
                {
                    array.SetValue(parsedValues[i], i);
                }
                return (array, null);
            }
            catch (Exception ex)
            {
                return (null, $"Failed to create array: {ex.Message}");
            }
        }

        private (object Value, string Error) TryParseCollection(Type collectionType, Type elementType, string[] values)
        {
            var parsedValues = new List<object>();
            
            for (int i = 0; i < values.Length; i++)
            {
                var (parsed, error) = ParseValue(elementType, values[i]);
                if (error != null)
                {
                    return (null, $"Invalid value at position {i + 1}: {error}");
                }
                parsedValues.Add(parsed);
            }

            try
            {
                object collection;

                bool isSet = IsSetType(collectionType);
                bool isList = IsListType(collectionType);
                bool isCollection = typeof(ICollection).IsAssignableFrom(collectionType);
                bool isEnumerable = typeof(IEnumerable).IsAssignableFrom(collectionType);

                if (isSet || (isEnumerable && !isList && collectionType.GetConstructor(Type.EmptyTypes) != null))
                {
                    var genericType = typeof(HashSet<>).MakeGenericType(elementType);
                    collection = Activator.CreateInstance(genericType);
                    
                    var addMethod = genericType.GetMethod("Add");
                    foreach (var value in parsedValues)
                    {
                        addMethod.Invoke(collection, new[] { value });
                    }
                }
                else if (isList || (isCollection && collectionType.GetConstructor(Type.EmptyTypes) != null))
                {
                    var genericType = typeof(List<>).MakeGenericType(elementType);
                    collection = Activator.CreateInstance(genericType);
                    
                    var addMethod = genericType.GetMethod("Add");
                    foreach (var value in parsedValues)
                    {
                        addMethod.Invoke(collection, new[] { value });
                    }
                }
                else if (collectionType.IsAssignableFrom(typeof(List<>).MakeGenericType(elementType)))
                {
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    collection = Activator.CreateInstance(listType);
                    
                    var addMethod = listType.GetMethod("Add");
                    foreach (var value in parsedValues)
                    {
                        addMethod.Invoke(collection, new[] { value });
                    }
                }
                else
                {
                    return (null, $"Cannot instantiate collection type '{collectionType.Name}'");
                }

                return (collection, null);
            }
            catch (Exception ex)
            {
                return (null, $"Failed to create collection: {ex.Message}");
            }
        }

        private (object Value, string Error) ParseValue(Type targetType, string valueString)
        {
            if (string.IsNullOrWhiteSpace(valueString))
                return (null, "Empty value");

            valueString = valueString.Trim();

            if (targetType.IsEnum)
            {
                return _enumParser.TryParse(valueString, targetType);
            }

            return _primitiveParser.TryParse(valueString, targetType);
        }

        public static bool IsCollectionType(Type type)
        {
            if (type == null)
                return false;

            if (type.IsArray)
                return true;

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    var genericDef = type.GetGenericTypeDefinition();
                    if (genericDef == typeof(HashSet<>) ||
                        genericDef == typeof(List<>) ||
                        genericDef == typeof(IList<>) ||
                        genericDef == typeof(ICollection<>) ||
                        genericDef == typeof(IEnumerable<>))
                    {
                        return true;
                    }
                }

                if (typeof(ICollection).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static Type GetElementType(Type collectionType)
        {
            if (collectionType == null)
                return null;

            if (collectionType.IsArray)
            {
                return collectionType.GetElementType();
            }

            if (collectionType.IsGenericType)
            {
                var args = collectionType.GetGenericArguments();
                if (args.Length > 0)
                {
                    return args[0];
                }
            }

            foreach (var iface in collectionType.GetInterfaces())
            {
                if (iface.IsGenericType)
                {
                    var genericDef = iface.GetGenericTypeDefinition();
                    if (genericDef == typeof(IEnumerable<>) ||
                        genericDef == typeof(ICollection<>) ||
                        genericDef == typeof(IList<>))
                    {
                        var args = iface.GetGenericArguments();
                        if (args.Length > 0)
                        {
                            return args[0];
                        }
                    }
                }
            }

            return null;
        }

        private static bool IsSetType(Type type)
        {
            if (type == null)
                return false;

            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                return genericDef == typeof(HashSet<>) ||
                       genericDef == typeof(ISet<>);
            }

            return false;
        }

        private static bool IsListType(Type type)
        {
            if (type == null)
                return false;

            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();
                return genericDef == typeof(List<>) ||
                       genericDef == typeof(IList<>) ||
                       genericDef == typeof(IList<>);
            }

            return typeof(IList).IsAssignableFrom(type);
        }
    }
}
