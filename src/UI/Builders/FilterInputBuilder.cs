using System;
using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class FilterInputBuilder
    {
        private readonly InputFieldRef _inputField;
        private float _debounceTimer = -1f;
        private string _pendingFilter = string.Empty;
        private const float DEBOUNCE_DELAY = 0.3f;

        public string FilterText => _inputField.Text;
        
        public event Action<string> OnFilterChanged;

        public FilterInputBuilder(
            GameObject parent,
            string name,
            string labelText,
            int inputWidth,
            int inputHeight,
            string placeholder = "Filter...")
        {
            var inputRow = UIFactory.CreateHorizontalGroup(
                parent,
                $"{name}_FilterRow",
                false, false, true, true,
                spacing: 5,
                padding: new Vector4(3, 3, 3, 3)
            );
            UIFactory.SetLayoutElement(inputRow, minHeight: inputHeight + 5);

            var label = UIFactory.CreateLabel(
                inputRow,
                $"{name}_Label",
                labelText,
                TextAnchor.MiddleLeft,
                Color.white
            );
            UIFactory.SetLayoutElement(label.gameObject, minWidth: GetLabelWidth(labelText), minHeight: inputHeight);

            _inputField = UIFactory.CreateInputField(inputRow, $"{name}_Input", placeholder);
            UIFactory.SetLayoutElement(_inputField.Component.gameObject, minWidth: inputWidth, minHeight: inputHeight);

            _inputField.Component.onValueChanged.AddListener(OnValueChanged);
        }

        private int GetLabelWidth(string text)
        {
            return text.Length * 8 + 10;
        }

        private void OnValueChanged(string value)
        {
            _pendingFilter = value;
            _debounceTimer = DEBOUNCE_DELAY;
        }

        public void Update()
        {
            if (_debounceTimer > 0f)
            {
                _debounceTimer -= Time.unscaledDeltaTime;
                if (_debounceTimer <= 0f)
                {
                    _debounceTimer = 0f;
                    OnFilterChanged?.Invoke(_pendingFilter);
                }
            }
        }

        public void Clear()
        {
            _inputField.Text = string.Empty;
            _debounceTimer = -1f;
            _pendingFilter = string.Empty;
        }

        public void ForceFilterRefresh()
        {
            _debounceTimer = 0f;
        }
    }
}
