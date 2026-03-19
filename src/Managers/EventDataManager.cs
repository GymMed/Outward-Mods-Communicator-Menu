#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using OutwardModsCommunicator.EventBus;

namespace OutwardModsCommunicatorMenu.Managers
{
    public class EventDataManager
    {
        private static EventDataManager? _instance;
        public static EventDataManager Instance => _instance ??= new EventDataManager();

        private static readonly List<string> SupportedTypes = new()
        {
            "string", "int", "bool", "float", "double",
            "Vector2", "Vector3", "Vector4", "Quaternion", "Color"
        };

        private readonly EventFilterStrategy _filterStrategy = new();

        private EventDataManager() { }

        public IReadOnlyDictionary<string, Dictionary<string, EventDefinition>> GetRegisteredEvents()
        {
            return EventBus.GetRegisteredEvents();
        }

        public IReadOnlyDictionary<string, Dictionary<string, List<System.Action<EventPayload?>>>> GetSubscribers()
        {
            return EventBus.GetModSubscribers();
        }

        public IReadOnlyDictionary<string, Dictionary<string, EventPayload>> GetPublishedPayloads()
        {
            return EventBus.GetModPublishedPayloads();
        }

        public List<string> GetMatchingModGuids(string? filter)
        {
            return _filterStrategy.GetModGuidsByFilter(filter ?? string.Empty);
        }

        public List<string> GetMatchingEvents(string? modGuid, string? filter)
        {
            return _filterStrategy.GetEventsByModAndName(modGuid ?? string.Empty, filter ?? string.Empty);
        }

        public List<string> GetSupportedTypes(string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return SupportedTypes.Take(5).ToList();
            }

            return SupportedTypes
                .Where(t => t.Contains(filter, System.StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .ToList();
        }

        public EventDefinition? GetEventDefinition(string modGuid, string eventName)
        {
            var events = EventBus.GetRegisteredEvents();
            if (events.TryGetValue(modGuid, out var modEvents) && modEvents.TryGetValue(eventName, out var eventDef))
            {
                return eventDef;
            }
            return null;
        }

        public List<(string name, string typeName, string? description)> GetEventParameters(string modGuid, string eventName)
        {
            var result = new List<(string, string, string?)>();
            var eventDef = GetEventDefinition(modGuid, eventName);
            
            if (eventDef != null && eventDef.Schema != null && eventDef.Schema.Fields != null)
            {
                foreach (var field in eventDef.Schema.Fields)
                {
                    var name = field.Key;
                    var type = field.Value;
                    var description = eventDef.Schema.GetDescription(name);
                    result.Add((name, Utility.TypeNameFormatter.Format(type), description));
                }
            }
            
            return result;
        }

        public int GetCallCount(string modGuid, string eventName)
        {
            try
            {
                var profilerType = typeof(EventProfiler);
                var enabledProperty = profilerType.GetProperty("Enabled", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                bool profilerEnabled = enabledProperty != null && (bool)(enabledProperty.GetValue(null) ?? false);
                
                if (!profilerEnabled) return -2;

                var profilesField = profilerType.GetField("_profiles", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (profilesField == null) return -1;

                var profiles = profilesField.GetValue(null) as System.Collections.Generic.IDictionary<string, object>;
                if (profiles == null) return -1;

                string key = $"{modGuid}.{eventName}";
                if (profiles.TryGetValue(key, out var profileData))
                {
                    var callCountProperty = profileData?.GetType().GetProperty("CallCount");
                    if (callCountProperty != null)
                    {
                        return (int)(callCountProperty.GetValue(profileData) ?? 0);
                    }
                }
                
                return 0;
            }
            catch
            {
            }
            return -1;
        }
    }
}
