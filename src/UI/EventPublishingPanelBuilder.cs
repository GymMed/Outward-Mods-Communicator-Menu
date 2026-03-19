using System;
using System.Collections.Generic;
using System.Linq;
using OutwardModsCommunicator.EventBus;
using OutwardModsCommunicatorMenu.Managers;
using OutwardModsCommunicatorMenu.UI.Builders;
using OutwardModsCommunicatorMenu.UI.Components;
using OutwardModsCommunicatorMenu.Utility;
using UnityEngine;
using UniverseLib.UI;

namespace OutwardModsCommunicatorMenu.UI
{
    public static class EventPublishingPanelBuilder
    {
        private static GameObject _contentParent;
        
        private static TabNavigationBuilder _tabNavigationBuilder;
        private static GameObject _publishViewContainer;
        private static SubscribersViewBuilder _subscribersViewBuilder;
        private static PublishersViewBuilder _publishersViewBuilder;
        
        private static InputSectionBuilder _inputSectionBuilder;
        private static RegisteredParamsBuilder _registeredParamsBuilder;
        private static DynamicParamsBuilder _dynamicParamsBuilder;
        private static ActionButtonsBuilder _actionButtonsBuilder;

        public static void Build(GameObject content)
        {
            _contentParent = content;

            _tabNavigationBuilder = new TabNavigationBuilder();
            _tabNavigationBuilder.Build(content);
            _tabNavigationBuilder.OnTabChanged += OnTabChanged;

            _publishViewContainer = UIFactory.CreateVerticalGroup(
                content,
                "PublishViewContainer",
                false, false, true, true,
                spacing: 5,
                padding: new Vector4(5, 5, 5, 5)
            );
            UIFactory.SetLayoutElement(_publishViewContainer, flexibleWidth: 9999, flexibleHeight: 9999, minHeight: 400);

            BuildPublishView(_publishViewContainer);

            _subscribersViewBuilder = new SubscribersViewBuilder();
            _subscribersViewBuilder.Build(content);

            _publishersViewBuilder = new PublishersViewBuilder();
            _publishersViewBuilder.Build(content);

            ShowTab(EventMenuTab.Publish);
        }

        private static void BuildPublishView(GameObject parent)
        {
            CreateTitleLabel(parent);
            
            _inputSectionBuilder = new InputSectionBuilder();
            _inputSectionBuilder.Build(parent);
            _inputSectionBuilder.OnEventNameChanged += OnInputChanged;
            _inputSectionBuilder.OnModGuidChanged += OnInputChanged;

            _registeredParamsBuilder = new RegisteredParamsBuilder();
            _registeredParamsBuilder.Build(parent);
            _registeredParamsBuilder.OnRegisteredParamClicked += OnRegisteredParamClicked;

            _actionButtonsBuilder = new ActionButtonsBuilder();
            _actionButtonsBuilder.Build(parent);
            _actionButtonsBuilder.OnPublishClicked += PublishEvent;
            _actionButtonsBuilder.OnClearClicked += ClearAll;

            _dynamicParamsBuilder = new DynamicParamsBuilder();
            _dynamicParamsBuilder.Build(parent);

            CreateInfoLabel(parent);
            
            RefreshRegisteredParams();
        }

        private static void OnTabChanged(EventMenuTab tab)
        {
            ShowTab(tab);
        }

        private static void ShowTab(EventMenuTab tab)
        {
            bool isPublish = tab == EventMenuTab.Publish;
            bool isSubscribers = tab == EventMenuTab.Subscribers;
            bool isPublishers = tab == EventMenuTab.Publishers;

            if (_publishViewContainer != null)
                _publishViewContainer.SetActive(isPublish);

            if (_subscribersViewBuilder != null)
            {
                _subscribersViewBuilder.SetVisible(isSubscribers);
                if (isSubscribers)
                    _subscribersViewBuilder.Refresh();
            }

            if (_publishersViewBuilder != null)
            {
                _publishersViewBuilder.SetVisible(isPublishers);
                if (isPublishers)
                    _publishersViewBuilder.Refresh();
            }

            UIManager.Instance.Log?.LogMessage($"[EventPublishingPanel] Switched to tab: {tab}");
        }

        private static void OnRegisteredParamClicked(string name, string type)
        {
            _dynamicParamsBuilder.AddDynamicParameter(name, type);
        }

        private static void OnInputChanged()
        {
            _actionButtonsBuilder?.ClearValidationMessage();
            RefreshRegisteredParams();
        }

        private static void RefreshRegisteredParams()
        {
            var modGuid = _inputSectionBuilder?.ModGuidInput?.Text ?? string.Empty;
            var eventName = _inputSectionBuilder?.EventNameInput?.Text ?? string.Empty;
            
            _registeredParamsBuilder?.Refresh(modGuid, eventName, eventName);
        }

        private static void CreateTitleLabel(GameObject content)
        {
            var titleLabel = UIFactory.CreateLabel(
                content, 
                "Title", 
                "Publish Event to Mods", 
                TextAnchor.MiddleLeft, 
                Color.white, 
                true, 
                16
            );
            UIFactory.SetLayoutElement(titleLabel.gameObject, minWidth: 300, minHeight: 25);
        }

        private static void CreateInfoLabel(GameObject content)
        {
            var infoLabel = UIFactory.CreateLabel(
                content, 
                "InfoLabel", 
                "Type mod GUID and event name. Click registered params to add. Dynamic params: click to edit.", 
                TextAnchor.MiddleLeft, 
                Color.yellow
            );
            UIFactory.SetLayoutElement(infoLabel.gameObject, minWidth: 420, minHeight: 20);
        }

        private static void ClearAll()
        {
            _dynamicParamsBuilder?.Clear();
            _inputSectionBuilder?.ModGuidInput?.Clear();
            _inputSectionBuilder?.EventNameInput?.Clear();
            RefreshRegisteredParams();
            _actionButtonsBuilder?.ClearValidationMessage();
            
            UIManager.Instance.Log?.LogMessage("[EventPublishingPanel] Cleared all");
        }

        private static void PublishEvent()
        {
            var modGuid = _inputSectionBuilder?.ModGuidInput?.Text?.Trim() ?? string.Empty;
            var eventName = _inputSectionBuilder?.EventNameInput?.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(modGuid) || string.IsNullOrEmpty(eventName))
            {
                _actionButtonsBuilder?.SetValidationMessage("Error: Mod GUID and Event Name are required!", true);
                UIManager.Instance.Log?.LogMessage("[EventPublishingPanel] Error: Mod GUID and Event Name are required!");
                return;
            }

            var builder = new EventPayloadBuilder();
            string warnings = string.Empty;

            var registeredEvents = EventBus.GetRegisteredEvents();
            bool eventExists = false;
            
            if (registeredEvents.TryGetValue(modGuid, out var modEvents) && modEvents.ContainsKey(eventName))
            {
                eventExists = true;
            }
            else
            {
                var published = EventBus.GetModPublishedPayloads();
                if (published.TryGetValue(modGuid, out var publishedEvents) && publishedEvents.ContainsKey(eventName))
                {
                    eventExists = true;
                }
            }
            
            if (!eventExists)
            {
                warnings += $"Warning: Event '{eventName}' for mod '{modGuid}' is not registered.\n";
            }

            var dynamicParams = _dynamicParamsBuilder?.DynamicParams;
            if (dynamicParams != null)
            {
                foreach (var (name, typeName, value) in dynamicParams)
                {
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(typeName))
                        continue;

                    var paramType = TypeResolver.Resolve(typeName);
                    if (paramType == null)
                    {
                        warnings += $"Warning: Unknown type '{typeName}' for parameter '{name}'.\n";
                        continue;
                    }

                    if (CollectionValueParser.IsCollectionType(paramType) && !string.IsNullOrWhiteSpace(value))
                    {
                        var (_, parseError) = CollectionValueParser.TryParse(paramType, value);
                        if (parseError != null)
                        {
                            var elementType = CollectionValueParser.GetElementType(paramType);
                            if (elementType != null && elementType.IsEnum)
                            {
                                warnings += $"Warning: Failed to parse enum collection '{name}' ({typeName}): {parseError}. For enum values, use only member names separated by spaces (e.g., 'Abrassar Enmerkar').\n";
                            }
                            else
                            {
                                warnings += $"Warning: Failed to parse collection '{name}' ({typeName}): {parseError}\n";
                            }
                            continue;
                        }
                    }

                    builder.AddParameter(name, paramType, value);
                }
            }

            var (payload, parseErrors) = builder.Build();
            
            if (!string.IsNullOrEmpty(parseErrors))
            {
                warnings += parseErrors;
            }

            if (!string.IsNullOrEmpty(warnings))
            {
                _actionButtonsBuilder?.SetValidationMessage(warnings.TrimEnd('\n'), true);
                UIManager.Instance.Log?.LogMessage($"[EventPublishingPanel] Warning publishing event:\n{warnings}");
            }
            else
            {
                _actionButtonsBuilder?.SetValidationMessage("Event published successfully!", false);
            }

            EventBus.Publish(modGuid, eventName, payload);
            UIManager.Instance.Log?.LogMessage($"[EventPublishingPanel] Published event '{eventName}' to mod '{modGuid}' with {dynamicParams?.Count ?? 0} dynamic params");
        }

        public static void Update()
        {
            _inputSectionBuilder?.Update();
            _dynamicParamsBuilder?.Update();
            _subscribersViewBuilder?.Update();
            _publishersViewBuilder?.Update();
        }
    }
}
