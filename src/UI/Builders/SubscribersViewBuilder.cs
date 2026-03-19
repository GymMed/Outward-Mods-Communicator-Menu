using System;
using System.Collections.Generic;
using System.Linq;
using OutwardModsCommunicator.EventBus;
using OutwardModsCommunicatorMenu.Managers;
using OutwardModsCommunicatorMenu.UI.Components;
using OutwardModsCommunicatorMenu.UI.Builders;
using UnityEngine;
using UniverseLib.UI;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class SubscribersViewBuilder
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
                "SubscribersView",
                false, false, true, true,
                spacing: 5,
                padding: new Vector4(5, 5, 5, 5)
            );
            UIFactory.SetLayoutElement(_rootObject, flexibleWidth: 9999, flexibleHeight: 9999, minHeight: 400);

            var titleLabel = UIFactory.CreateLabel(
                _rootObject,
                "Title",
                "Event Subscribers",
                TextAnchor.MiddleLeft,
                Color.white,
                true,
                16
            );
            UIFactory.SetLayoutElement(titleLabel.gameObject, minWidth: 300, minHeight: 25);

            CreateFilterRow(_rootObject);

            var scrollResult = ScrollViewHelper.CreateScrollView(
                _rootObject,
                "SubscribersScrollView",
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

            var subscribers = EventBus.GetModSubscribers();
            
            var filteredMods = subscribers
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
                    var handlers = eventPair.Value;

                    if (handlers.Count == 0) continue;

                    CreateSubscriberEntry(modGuid, eventName, handlers);
                }
            }

            if (!subscribers.Any())
            {
                var emptyLabel = UIFactory.CreateLabel(
                    _contentContainer,
                    "EmptyLabel",
                    "No subscribers found.",
                    TextAnchor.MiddleLeft,
                    Color.yellow,
                    false,
                    14
                );
                UIFactory.SetLayoutElement(emptyLabel.gameObject, minWidth: 200, minHeight: 30);
            }
        }

        private void CreateSubscriberEntry(string modGuid, string eventName, List<Action<EventPayload>> handlers)
        {
            var expandable = new ExpandableItemBuilder(
                _contentContainer,
                $"Subscriber_{modGuid}_{eventName}",
                $"Mod: {modGuid} | Event: {eventName} | Subscribers: {handlers.Count}",
                new Color(0.15f, 0.18f, 0.25f, 0.95f),
                " [+]",
                " [-]"
            );

            foreach (var handler in handlers)
            {
                var method = handler.Method;
                var declaringType = method.DeclaringType;
                string typeName = declaringType?.FullName ?? "UnknownType";
                string assemblyName = declaringType?.Assembly.GetName().Name ?? "UnknownAssembly";
                string targetType = handler.Target?.GetType().Name ?? "static";
                string methodName = method.Name;

                CreateDetailLabel(expandable.DetailContent, "Assembly:", assemblyName, new Color(0.6f, 0.7f, 0.9f, 1f));
                CreateDetailLabel(expandable.DetailContent, "Type:", typeName, new Color(0.7f, 0.8f, 0.9f, 1f));
                CreateDetailLabel(expandable.DetailContent, "Method:", methodName, new Color(0.8f, 0.85f, 0.9f, 1f));
                CreateDetailLabel(expandable.DetailContent, "Target:", targetType, new Color(0.7f, 0.75f, 0.8f, 1f));

                var separator = UIFactory.CreateLabel(
                    expandable.DetailContent,
                    "Separator",
                    "---",
                    TextAnchor.MiddleLeft,
                    new Color(0.4f, 0.4f, 0.45f, 1f),
                    false,
                    11
                );
                UIFactory.SetLayoutElement(separator.gameObject, flexibleWidth: 9999, minHeight: 15);
            }
        }

        private void CreateDetailLabel(GameObject parent, string label, string value, Color color)
        {
            var detailLabel = UIFactory.CreateLabel(
                parent,
                $"Detail_{label}",
                $"  {label} {value}",
                TextAnchor.MiddleLeft,
                color,
                false,
                12
            );
            UIFactory.SetLayoutElement(detailLabel.gameObject, flexibleWidth: 9999, minHeight: 18);
        }

        public void Update()
        {
            _modFilterInput?.Update();
            _eventFilterInput?.Update();
        }

        public void Refresh()
        {
            RefreshView();
        }

        public void SetVisible(bool visible)
        {
            if (_rootObject != null)
            {
                _rootObject.SetActive(visible);
            }
        }
    }
}
