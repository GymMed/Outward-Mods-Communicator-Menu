using System;
using System.Collections.Generic;
using System.Linq;
using OutwardModsCommunicator.EventBus;
using OutwardModsCommunicatorMenu;
using OutwardModsCommunicatorMenu.Managers;
using OutwardModsCommunicatorMenu.UI.Components;
using OutwardModsCommunicatorMenu.UI.Builders;
using OutwardModsCommunicatorMenu.Utility;
using UnityEngine;
using UniverseLib.UI;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class PublishersViewBuilder
    {
        private GameObject _rootObject;
        private GameObject _contentContainer;

        private FilterInputBuilder _modFilterInput;
        private FilterInputBuilder _eventFilterInput;

        private string _currentModFilter = string.Empty;
        private string _currentEventFilter = string.Empty;

        public void Build(GameObject parent)
        {
            _rootObject = UIFactory.CreateVerticalGroup(
                parent,
                "PublishersView",
                false, false, true, true,
                spacing: 5,
                padding: new Vector4(5, 5, 5, 5)
            );
            UIFactory.SetLayoutElement(_rootObject, flexibleWidth: 9999, flexibleHeight: 9999, minHeight: 400);

            var titleLabel = UIFactory.CreateLabel(
                _rootObject,
                "Title",
                "Published Events & Payloads",
                TextAnchor.MiddleLeft,
                Color.white,
                true,
                16
            );
            UIFactory.SetLayoutElement(titleLabel.gameObject, minWidth: 300, minHeight: 25);

            var infoLabel = UIFactory.CreateLabel(
                _rootObject,
                "Info",
                "Click entries to view call count and payload details.",
                TextAnchor.MiddleLeft,
                Color.yellow,
                false,
                12
            );
            UIFactory.SetLayoutElement(infoLabel.gameObject, flexibleWidth: 9999, minHeight: 20);

            CreateFilterRow(_rootObject);

            var scrollResult = ScrollViewHelper.CreateScrollView(
                _rootObject,
                "PublishersScrollView",
                400,
                300,
                new Color(0.15f, 0.15f, 0.2f, 0.95f)
            );
            _contentContainer = scrollResult.Item2;

            RefreshView();
        }

        private void CreateFilterRow(GameObject parent)
        {
            var filterContainer = UIFactory.CreateHorizontalGroup(
                parent,
                "FilterContainer",
                false, false, true, true,
                spacing: 10,
                padding: new Vector4(3, 3, 3, 3)
            );
            UIFactory.SetLayoutElement(filterContainer, minHeight: 35);

            var leftSide = UIFactory.CreateVerticalGroup(
                filterContainer,
                "FilterLeftSide",
                false, false, true, true,
                spacing: 2,
                padding: new Vector4(3, 3, 3, 3)
            );
            UIFactory.SetLayoutElement(leftSide, flexibleWidth: 5000, minHeight: 30);

            var rightSide = UIFactory.CreateVerticalGroup(
                filterContainer,
                "FilterRightSide",
                false, false, true, true,
                spacing: 2,
                padding: new Vector4(3, 3, 3, 3)
            );
            UIFactory.SetLayoutElement(rightSide, flexibleWidth: 5000, minHeight: 30);

            _modFilterInput = new FilterInputBuilder(
                leftSide,
                "ModGuid",
                "Mod:",
                280,
                25,
                "Filter by mod GUID..."
            );
            _modFilterInput.OnFilterChanged += OnModFilterChanged;

            _eventFilterInput = new FilterInputBuilder(
                rightSide,
                "EventName",
                "Event:",
                280,
                25,
                "Filter by event..."
            );
            _eventFilterInput.OnFilterChanged += OnEventFilterChanged;
        }

        private void OnModFilterChanged(string filter)
        {
            _currentModFilter = filter;
            RefreshView();
        }

        private void OnEventFilterChanged(string filter)
        {
            _currentEventFilter = filter;
            RefreshView();
        }

        private void RefreshView()
        {
            foreach (Transform child in _contentContainer.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }

            var publishers = EventBus.GetModPublishedPayloads();
            
            var filteredMods = publishers
                .Where(m => string.IsNullOrEmpty(_currentModFilter) || 
                           m.Key.Contains(_currentModFilter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Key);

            foreach (var modPair in filteredMods)
            {
                string modGuid = modPair.Key;
                
                var filteredEvents = modPair.Value
                    .Where(e => string.IsNullOrEmpty(_currentEventFilter) || 
                               e.Key.Contains(_currentEventFilter, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(e => e.Key);

                foreach (var eventPair in filteredEvents)
                {
                    string eventName = eventPair.Key;
                    var payload = eventPair.Value;

                    CreatePublisherEntry(modGuid, eventName, payload);
                }
            }

            if (!publishers.Any())
            {
                var emptyLabel = UIFactory.CreateLabel(
                    _contentContainer,
                    "EmptyLabel",
                    "No published events found.",
                    TextAnchor.MiddleLeft,
                    Color.yellow,
                    false,
                    14
                );
                UIFactory.SetLayoutElement(emptyLabel.gameObject, minWidth: 200, minHeight: 30);
            }
        }

        private void CreatePublisherEntry(string modGuid, string eventName, EventPayload payload)
        {
            int callCount = EventDataManager.Instance.GetCallCount(modGuid, eventName);
            string callCountStr;
            
            if (callCount == -2)
            {
                callCountStr = "Disabled";
            }
            else if (callCount == -1)
            {
                callCountStr = "Unknown";
            }
            else
            {
                callCountStr = callCount.ToString();
            }
            
            int paramCount = payload?.Count ?? 0;
            
            var expandable = new ExpandableItemBuilder(
                _contentContainer,
                $"Publisher_{modGuid}_{eventName}",
                $"Mod: {modGuid} | Event: {eventName} | Calls: {callCountStr} | Params: {paramCount}",
                new Color(0.18f, 0.15f, 0.22f, 0.95f),
                " [+]",
                " [-]"
            );

            CreatePayloadDetails(expandable.DetailContent, modGuid, eventName, payload);
        }

        private void CreatePayloadDetails(GameObject parent, string modGuid, string eventName, EventPayload payload)
        {
            var infoHeader = UIFactory.CreateLabel(
                parent,
                "InfoHeader",
                $"  --- Payload Details ---",
                TextAnchor.MiddleLeft,
                new Color(0.5f, 0.6f, 0.8f, 1f),
                true,
                12
            );
            UIFactory.SetLayoutElement(infoHeader.gameObject, flexibleWidth: 9999, minHeight: 20);

            if (payload == null || payload.Count == 0)
            {
                var emptyLabel = UIFactory.CreateLabel(
                    parent,
                    "EmptyPayload",
                    "  (no payload data)",
                    TextAnchor.MiddleLeft,
                    new Color(0.6f, 0.6f, 0.6f, 1f),
                    false,
                    12
                );
                UIFactory.SetLayoutElement(emptyLabel.gameObject, flexibleWidth: 9999, minHeight: 20);
                return;
            }

            foreach (var kvp in payload)
            {
                string key = kvp.Key;
                object value = kvp.Value;
                
                // Try to get type from registered event schema first, fallback to runtime type
                Type displayType = GetSchemaType(modGuid, eventName, key) ?? value?.GetType();
                string typeName = displayType != null ? TypeNameFormatter.Format(displayType) : "null";
                
#if DEBUG
                OMCM.LogMessage($"[DEBUG] PublishersView CreatePayloadDetails: key=\"{key}\", value.GetType()={value?.GetType()?.FullName ?? "null"}, displayType={displayType?.FullName ?? "null"}");
#endif

                var keyLabel = UIFactory.CreateLabel(
                    parent,
                    $"ParamKey_{key}",
                    $"  {key}:",
                    TextAnchor.MiddleLeft,
                    new Color(0.7f, 0.8f, 0.9f, 1f),
                    true,
                    12
                );
                UIFactory.SetLayoutElement(keyLabel.gameObject, flexibleWidth: 9999, minHeight: 18);

                ValueFormatterFactory.CreateDisplay(
                    parent,
                    $"ParamValue_{key}",
                    value,
                    displayType
                );
            }
        }

        private bool CanCastToString(object value)
        {
            if (value == null) return false;
            return value is string || value.GetType().IsPrimitive || value is decimal || value is DateTime;
        }

        private string FormatDetailedValue(object value)
        {
            if (value == null) return "null";

            if (value is Vector2 v2)
                return $"Vector2({v2.x:F4}, {v2.y:F4})";
            if (value is Vector3 v3)
                return $"Vector3({v3.x:F4}, {v3.y:F4}, {v3.z:F4})";
            if (value is Vector4 v4)
                return $"Vector4({v4.x:F4}, {v4.y:F4}, {v4.z:F4}, {v4.w:F4})";
            if (value is Quaternion q)
                return $"Quaternion({q.x:F4}, {q.y:F4}, {q.z:F4}, {q.w:F4})";
            if (value is Color c)
                return $"Color({c.r:F4}, {c.g:F4}, {c.b:F4}, {c.a:F4})";

            string str = value.ToString() ?? "null";
            if (str.Length > 200)
                str = str.Substring(0, 200) + "...";
            return str;
        }

        public void Update()
        {
            _modFilterInput?.Update();
            _eventFilterInput?.Update();
        }

        public void SetVisible(bool visible)
        {
            if (_rootObject != null)
            {
                _rootObject.SetActive(visible);
            }
        }

        public void Refresh()
        {
            RefreshView();
        }

        private static Type GetSchemaType(string modGuid, string eventName, string paramName)
        {
            try
            {
                var events = EventBus.GetRegisteredEvents();
                if (events.TryGetValue(modGuid, out var modEvents) && modEvents.TryGetValue(eventName, out var eventDef))
                {
                    if (eventDef?.Schema?.Fields != null && eventDef.Schema.Fields.TryGetValue(paramName, out var paramType))
                    {
                        return paramType;
                    }
                }
            }
            catch
            {
                // Ignore errors, will fallback to runtime type
            }
            return null;
        }
    }
}
