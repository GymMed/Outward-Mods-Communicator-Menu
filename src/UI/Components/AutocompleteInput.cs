using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace OutwardModsCommunicatorMenu.UI.Components
{
    public class AutocompleteInput
    {
        private readonly InputFieldRef _inputField;
        private readonly GameObject _suggestionsScrollView;
        private readonly GameObject _suggestionsContainer;
        private readonly List<ButtonRef> _suggestionButtons = new();
        private readonly List<string> _currentSuggestions = new();
        private readonly Func<string, List<string>> _getSuggestions;
        private readonly Func<string> _externalFilterProvider;
        private readonly float _width;
        
        private int _selectedIndex = -1;
        private bool _isInitialized;
        private float _debounceTimer = -1f;
        private string _pendingFilter = string.Empty;
        private const float DEBOUNCE_DELAY = 0.3f;
        
        public event Action<string> OnSelect;
        public event Action<string> OnTextChanged;

        public string Text => _inputField.Text;
        public InputFieldRef InputField => _inputField;

        public AutocompleteInput(
            GameObject parent,
            string name,
            string placeholder,
            Vector2 size,
            Func<string, List<string>> getSuggestions,
            Func<string> externalFilterProvider = null)
        {
            _getSuggestions = getSuggestions;
            _externalFilterProvider = externalFilterProvider;
            _width = size.x;

            _inputField = UniverseLib.UI.UIFactory.CreateInputField(parent, name, placeholder);
            UIFactory.SetLayoutElement(_inputField.Component.gameObject, minWidth: (int)size.x, flexibleWidth: 9999, minHeight: (int)size.y);

            var suggestionsContainerResult = ScrollViewHelper.CreateScrollView(
                parent,
                $"{name}_Suggestions",
                new Vector2(size.x, 80),
                new Color(0.15f, 0.15f, 0.2f, 0.98f)
            );
            _suggestionsScrollView = suggestionsContainerResult.root;
            _suggestionsContainer = suggestionsContainerResult.content;
            _suggestionsScrollView.SetActive(false);

            _inputField.Component.onValueChanged.AddListener(OnValueChanged);
            
            _isInitialized = true;
            UpdateSuggestions(string.Empty);
        }

        public void Update()
        {
            if (_debounceTimer > 0f)
            {
                _debounceTimer -= Time.unscaledDeltaTime;
                if (_debounceTimer <= 0f)
                {
                    _debounceTimer = -1f;
                    UpdateSuggestions(_pendingFilter);
                }
            }
        }

        private void OnValueChanged(string value)
        {
            _selectedIndex = -1;
            _pendingFilter = value;
            
            // When input is cleared to empty, show all suggestions immediately without debounce
            if (string.IsNullOrEmpty(value))
            {
                _debounceTimer = -1f;
                UpdateSuggestions(value);
            }
            else
            {
                _debounceTimer = DEBOUNCE_DELAY;
            }
            
            OnTextChanged?.Invoke(value);
        }

        private void UpdateSuggestions(string filter)
        {
            if (!_isInitialized) return;

            _currentSuggestions.Clear();
            foreach (var button in _suggestionButtons)
            {
                UnityEngine.Object.Destroy(button.GameObject);
            }
            _suggestionButtons.Clear();

            string effectiveFilter = filter;
            
            // Get suggestions - external filter provider (mod GUID) is accessed within the lambda closure
            // We pass the user's typed text as the filter parameter
            List<string> suggestions = _getSuggestions?.Invoke(filter) ?? new List<string>();
            
            // Filter by input text if provided
            if (!string.IsNullOrEmpty(filter))
            {
                suggestions = suggestions.Where(s => s.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            _currentSuggestions.AddRange(suggestions);

            if (_currentSuggestions.Count == 0)
            {
                _suggestionsScrollView.SetActive(false);
                return;
            }

            _suggestionsScrollView.SetActive(true);

            for (int i = 0; i < _currentSuggestions.Count; i++)
            {
                var suggestion = _currentSuggestions[i];
                CreateSuggestionButton(suggestion, i);
            }

            UpdateSelectionVisual();
        }

        private void CreateSuggestionButton(string text, int index)
        {
            var button = UniverseLib.UI.UIFactory.CreateButton(
                _suggestionsContainer,
                $"Suggestion_{index}",
                text,
                new Color(0.2f, 0.2f, 0.2f)
            );
            UIFactory.SetLayoutElement(button.Component.gameObject, minWidth: (int)_width, flexibleWidth: 9999, minHeight: 25);

            int capturedIndex = index;
            button.OnClick += () => SelectItem(capturedIndex);

            _suggestionButtons.Add(button);
        }

        private void UpdateSelectionVisual()
        {
            for (int i = 0; i < _suggestionButtons.Count; i++)
            {
                var button = _suggestionButtons[i];
                
                if (i == _selectedIndex)
                {
                    button.Component.colors = new ColorBlock
                    {
                        normalColor = new Color(0.3f, 0.5f, 0.7f, 1f),
                        highlightedColor = new Color(0.4f, 0.6f, 0.8f, 1f),
                        pressedColor = new Color(0.2f, 0.4f, 0.6f, 1f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.1f
                    };
                }
                else
                {
                    button.Component.colors = new ColorBlock
                    {
                        normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f),
                        highlightedColor = new Color(0.3f, 0.4f, 0.5f, 1f),
                        pressedColor = new Color(0.2f, 0.3f, 0.4f, 1f),
                        colorMultiplier = 1f,
                        fadeDuration = 0.1f
                    };
                }
            }
        }

        public void HandleKeyDown(KeyCode key)
        {
            if (!_suggestionsScrollView.activeSelf || _currentSuggestions.Count == 0)
                return;

            if (key == KeyCode.DownArrow)
            {
                _selectedIndex = Mathf.Min(_selectedIndex + 1, _currentSuggestions.Count - 1);
                UpdateSelectionVisual();
            }
            else if (key == KeyCode.UpArrow)
            {
                _selectedIndex = Mathf.Max(_selectedIndex - 1, 0);
                UpdateSelectionVisual();
            }
            else if (key == KeyCode.Return || key == KeyCode.KeypadEnter)
            {
                if (_selectedIndex >= 0 && _selectedIndex < _currentSuggestions.Count)
                {
                    SelectItem(_selectedIndex);
                }
            }
            else if (key == KeyCode.Escape)
            {
                _suggestionsScrollView.SetActive(false);
                _selectedIndex = -1;
            }
        }

        private void SelectItem(int index)
        {
            if (index >= 0 && index < _currentSuggestions.Count)
            {
                var selected = _currentSuggestions[index];
                _inputField.Text = selected;
                _suggestionsScrollView.SetActive(false);
                _selectedIndex = -1;
                _debounceTimer = -1f;
                OnSelect?.Invoke(selected);
            }
        }

        public void SetText(string text)
        {
            _inputField.Text = text;
        }

        public void Clear()
        {
            _inputField.Text = string.Empty;
            _suggestionsScrollView.SetActive(false);
            _selectedIndex = -1;
            _debounceTimer = -1f;
        }

        public void HideSuggestions()
        {
            _suggestionsScrollView.SetActive(false);
            _selectedIndex = -1;
        }
        
        public void RefreshSuggestions()
        {
            _pendingFilter = _inputField.Text;
            _debounceTimer = -1f;
            UpdateSuggestions(_pendingFilter);
        }
    }
}
