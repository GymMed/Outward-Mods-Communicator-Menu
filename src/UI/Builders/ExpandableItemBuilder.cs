using System;
using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class ExpandableItemBuilder
    {
        private readonly ButtonRef _toggleButton;
        private readonly GameObject _detailContent;
        private bool _isExpanded;
        private string _collapsedText;
        private string _expandedText;

        public bool IsExpanded => _isExpanded;
        public GameObject DetailContent => _detailContent;

        public ExpandableItemBuilder(
            GameObject parent,
            string name,
            string title,
            Color? buttonColor = null,
            string collapsedSuffix = " [+]",
            string expandedSuffix = " [-]")
        {
            _collapsedText = title + collapsedSuffix;
            _expandedText = title + expandedSuffix;

            _toggleButton = UIFactory.CreateButton(
                parent,
                $"{name}_ToggleBtn",
                _collapsedText,
                buttonColor ?? new Color(0.2f, 0.25f, 0.3f)
            );
            UIFactory.SetLayoutElement(_toggleButton.Component.gameObject, flexibleWidth: 9999, minHeight: 28);

            _detailContent = UIFactory.CreateVerticalGroup(
                parent,
                $"{name}_DetailContent",
                false, false, true, true,
                spacing: 2,
                padding: new Vector4(8, 5, 5, 5),
                bgColor: new Color(0.08f, 0.1f, 0.12f, 0.95f)
            );
            UIFactory.SetLayoutElement(_detailContent, flexibleWidth: 9999);
            _detailContent.SetActive(false);

            _toggleButton.OnClick += Toggle;
        }

        private void Toggle()
        {
            SetExpanded(!_isExpanded);
        }

        public void SetExpanded(bool expanded)
        {
            _isExpanded = expanded;
            _detailContent.SetActive(_isExpanded);
            
            var textComp = _toggleButton.Component.GetComponentInChildren<UnityEngine.UI.Text>();
            if (textComp != null)
            {
                textComp.text = _isExpanded ? _expandedText : _collapsedText;
            }

            UpdateButtonColor();
        }

        private void UpdateButtonColor()
        {
            _toggleButton.Component.colors = new UnityEngine.UI.ColorBlock
            {
                normalColor = _isExpanded ? new Color(0.25f, 0.35f, 0.45f, 1f) : new Color(0.2f, 0.25f, 0.3f, 0.9f),
                highlightedColor = _isExpanded ? new Color(0.35f, 0.45f, 0.55f, 1f) : new Color(0.3f, 0.35f, 0.4f, 1f),
                pressedColor = _isExpanded ? new Color(0.2f, 0.3f, 0.4f, 1f) : new Color(0.2f, 0.25f, 0.3f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
        }

        public void Collapse()
        {
            SetExpanded(false);
        }

        public void Expand()
        {
            SetExpanded(true);
        }

        public void SetTitle(string title, string collapsedSuffix = " [+]", string expandedSuffix = " [-]")
        {
            _collapsedText = title + collapsedSuffix;
            _expandedText = title + expandedSuffix;
            UpdateButtonColor();
        }
    }
}
