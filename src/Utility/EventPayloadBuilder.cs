using System;
using System.Collections.Generic;
using System.Text;
using OutwardModsCommunicator.EventBus;
using UniverseLib.Utility;
using OutwardModsCommunicatorMenu.Utility;

namespace OutwardModsCommunicatorMenu.Utility
{
    public class EventPayloadBuilder
    {
        private readonly List<PayloadParameter> _parameters = new();

        public void AddParameter(string name, Type type, string value)
        {
            _parameters.Add(new PayloadParameter(name, type, value));
        }

        public (EventPayload payload, string errors) Build()
        {
            var payload = new EventPayload();
            var errors = new StringBuilder();

            foreach (var param in _parameters)
            {
                if (string.IsNullOrEmpty(param.Name) || param.Type == null)
                    continue;

                string displayTypeName = TypeNameFormatter.Format(param.Type);
                var parseResult = TryParseParameterValue(param.Type, param.Value);
                
                if (parseResult.Success)
                {
                    payload[param.Name] = parseResult.Value;
                }
                else
                {
                    errors.AppendLine($"Failed to parse '{param.Value}' as {displayTypeName}: {parseResult.ErrorMessage}");
                }
            }

            return (payload, errors.ToString());
        }

        private static ParseResult TryParseParameterValue(Type type, string value)
        {
            if (string.IsNullOrEmpty(value))
                return ParseResult.CreateFailure("Empty value");

            if (CollectionValueParser.IsCollectionType(type))
            {
                var (collectionValue, collectionError) = CollectionValueParser.TryParse(type, value);
                if (collectionError != null)
                    return ParseResult.CreateFailure(collectionError);
                return ParseResult.CreateSuccess(collectionValue);
            }

            Type enumType = GetEnumType(type);
            if (enumType != null)
            {
                if (TryParseEnumValue(enumType, value, out object enumValue))
                {
                    return ParseResult.CreateSuccess(enumValue);
                }
                var enumNames = string.Join(", ", Enum.GetNames(enumType));
                return ParseResult.CreateFailure($"Expected one of: {enumNames}");
            }

            if (ParseUtility.TryParse(value, type, out object parsedValue, out Exception ex))
            {
                return ParseResult.CreateSuccess(parsedValue);
            }

            return ParseResult.CreateFailure("Unable to parse value");
        }

        private static Type GetEnumType(Type type)
        {
            if (type.IsEnum)
                return type;
            
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null && underlyingType.IsEnum)
                return underlyingType;
            
            return null;
        }

        private static bool TryParseEnumValue(Type enumType, string valueString, out object result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(valueString) || enumType == null || !enumType.IsEnum)
                return false;

            try
            {
                result = Enum.Parse(enumType, valueString, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private readonly struct ParseResult
        {
            public bool Success { get; }
            public object Value { get; }
            public string ErrorMessage { get; }

            private ParseResult(bool success, object value, string errorMessage)
            {
                Success = success;
                Value = value;
                ErrorMessage = errorMessage;
            }

            public static ParseResult CreateSuccess(object value) => new ParseResult(true, value, null);
            public static ParseResult CreateFailure(string errorMessage) => new ParseResult(false, null, errorMessage);
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        private class PayloadParameter
        {
            public string Name { get; }
            public Type Type { get; }
            public string Value { get; }

            public PayloadParameter(string name, Type type, string value)
            {
                Name = name;
                Type = type;
                Value = value;
            }
        }
    }
}
