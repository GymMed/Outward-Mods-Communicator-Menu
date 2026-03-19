using System.Collections.Generic;
using System.Linq;
using OutwardModsCommunicator.EventBus;

namespace OutwardModsCommunicatorMenu.Managers
{
    public class EventFilterStrategy
    {
        private Dictionary<string, List<string>> _cachedModEvents;
        private bool _cacheValid;

        public List<string> GetAllEvents()
        {
            var allModEvents = GetAllModEvents();
            return allModEvents.Values
                .SelectMany(events => events)
                .Distinct()
                .OrderBy(e => e)
                .ToList();
        }

        public List<string> GetEventsByModGuid(string modGuidFilter)
        {
            if (string.IsNullOrEmpty(modGuidFilter))
            {
                return GetAllEvents();
            }

            var allModEvents = GetAllModEvents();
            var result = new List<string>();

            foreach (var modPair in allModEvents)
            {
                if (modPair.Key.Contains(modGuidFilter, System.StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var eventName in modPair.Value)
                    {
                        if (!result.Contains(eventName))
                        {
                            result.Add(eventName);
                        }
                    }
                }
            }

            return result.OrderBy(e => e).ToList();
        }

        public List<string> GetEventsByModAndName(string modGuidFilter, string eventNameFilter)
        {
            var allModEvents = GetAllModEvents();
            var result = new List<string>();

            foreach (var modPair in allModEvents)
            {
                bool modMatches = string.IsNullOrEmpty(modGuidFilter) || 
                    modPair.Key.Contains(modGuidFilter, System.StringComparison.OrdinalIgnoreCase);

                if (!modMatches)
                    continue;

                foreach (var eventName in modPair.Value)
                {
                    bool eventMatches = string.IsNullOrEmpty(eventNameFilter) ||
                        eventName.Contains(eventNameFilter, System.StringComparison.OrdinalIgnoreCase);

                    if (eventMatches)
                    {
                        result.Add(eventName);
                    }
                }
            }

            return result.OrderBy(e => e).ToList();
        }

        public List<string> GetAllModGuids()
        {
            var allModEvents = GetAllModEvents();
            return allModEvents.Keys.OrderBy(m => m).ToList();
        }

        public List<string> GetModGuidsByFilter(string modGuidFilter)
        {
            var allModGuids = GetAllModGuids();

            if (string.IsNullOrEmpty(modGuidFilter))
            {
                return allModGuids;
            }

            return allModGuids
                .Where(m => m.Contains(modGuidFilter, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private Dictionary<string, List<string>> GetAllModEvents()
        {
            if (_cacheValid && _cachedModEvents != null)
            {
                return _cachedModEvents;
            }

            _cachedModEvents = new Dictionary<string, List<string>>();

            var registeredEvents = EventBus.GetRegisteredEvents();
            foreach (var mod in registeredEvents)
            {
                if (!_cachedModEvents.ContainsKey(mod.Key))
                {
                    _cachedModEvents[mod.Key] = new List<string>();
                }
                _cachedModEvents[mod.Key].AddRange(mod.Value.Keys);
            }

            var publishedPayloads = EventBus.GetModPublishedPayloads();
            foreach (var mod in publishedPayloads)
            {
                if (!_cachedModEvents.ContainsKey(mod.Key))
                {
                    _cachedModEvents[mod.Key] = new List<string>();
                }
                foreach (var evt in mod.Value.Keys)
                {
                    if (!_cachedModEvents[mod.Key].Contains(evt))
                    {
                        _cachedModEvents[mod.Key].Add(evt);
                    }
                }
            }

            _cacheValid = true;
            return _cachedModEvents;
        }

        public void InvalidateCache()
        {
            _cacheValid = false;
        }
    }
}
