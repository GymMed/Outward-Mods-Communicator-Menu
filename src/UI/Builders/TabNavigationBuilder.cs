using System;
using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public enum EventMenuTab
    {
        Publish,
        Subscribers,
        Publishers
    }

    public class TabNavigationBuilder
    {
        private ButtonRef _publishTab;
        private ButtonRef _subscribersTab;
        private ButtonRef _publishersTab;
        
        private EventMenuTab _currentTab = EventMenuTab.Publish;

        public event Action<EventMenuTab> OnTabChanged;

        public EventMenuTab CurrentTab => _currentTab;

        public void Build(GameObject parent)
        {
            var tabContainer = UIFactory.CreateHorizontalGroup(
                parent,
                "TabContainer",
                false, false, true, true,
                spacing: 5,
                padding: new Vector4(3, 3, 3, 3)
            );
            UIFactory.SetLayoutElement(tabContainer, flexibleWidth: 9999, minHeight: 35);

            _publishTab = CreateTabButton(tabContainer, "PublishTab", "Publish", new Color(0.2f, 0.4f, 0.3f), () => SwitchToTab(EventMenuTab.Publish));
            _subscribersTab = CreateTabButton(tabContainer, "SubscribersTab", "Subscribers", new Color(0.2f, 0.25f, 0.35f), () => SwitchToTab(EventMenuTab.Subscribers));
            _publishersTab = CreateTabButton(tabContainer, "PublishersTab", "Publishers", new Color(0.25f, 0.2f, 0.35f), () => SwitchToTab(EventMenuTab.Publishers));

            UpdateTabColors();
        }

        private ButtonRef CreateTabButton(GameObject parent, string name, string text, Color bgColor, Action onClick)
        {
            var button = UIFactory.CreateButton(parent, name, text, bgColor);
            UIFactory.SetLayoutElement(button.Component.gameObject, minWidth: 120, minHeight: 30);
            button.OnClick += onClick;
            return button;
        }

        private void SwitchToTab(EventMenuTab tab)
        {
            if (_currentTab == tab) return;
            
            _currentTab = tab;
            UpdateTabColors();
            OnTabChanged?.Invoke(tab);
        }

        private void UpdateTabColors()
        {
            SetActiveTabColor(_publishTab, _currentTab == EventMenuTab.Publish);
            SetActiveTabColor(_subscribersTab, _currentTab == EventMenuTab.Subscribers);
            SetActiveTabColor(_publishersTab, _currentTab == EventMenuTab.Publishers);
        }

        private void SetActiveTabColor(ButtonRef button, bool isActive)
        {
            button.Component.colors = new UnityEngine.UI.ColorBlock
            {
                normalColor = isActive ? new Color(0.3f, 0.5f, 0.4f, 1f) : new Color(0.2f, 0.25f, 0.3f, 0.9f),
                highlightedColor = isActive ? new Color(0.4f, 0.6f, 0.5f, 1f) : new Color(0.3f, 0.35f, 0.4f, 1f),
                pressedColor = isActive ? new Color(0.2f, 0.4f, 0.3f, 1f) : new Color(0.2f, 0.25f, 0.3f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
        }

        public void SetActiveTab(EventMenuTab tab)
        {
            _currentTab = tab;
            UpdateTabColors();
        }
    }
}
