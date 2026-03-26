using System;
using System.Collections.Generic;
using System.Text;
using OutwardModsCommunicator.EventBus;
using OutwardModsCommunicatorMenu.Utility;
using OutwardModsCommunicatorMenu.Utility.Parsing;

namespace OutwardModsCommunicatorMenu.Utility
{
    public class EventPayloadBuilder
    {
        private readonly List<PayloadParameter> _parameters = new();
        private readonly IValueParser _valueParser;

        public EventPayloadBuilder() : this(new ValueParser())
        {
        }

        public EventPayloadBuilder(IValueParser valueParser)
        {
            _valueParser = valueParser ?? throw new ArgumentNullException(nameof(valueParser));
        }

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
                var (value, error) = _valueParser.TryParse(param.Value, param.Type);
                
                if (error == null)
                {
                    payload[param.Name] = value;
                }
                else
                {
                    errors.AppendLine($"Failed to parse '{param.Value}' as {displayTypeName}: {error}");
                }
            }

            return (payload, errors.ToString());
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
