using System;
using System.Collections.Generic;
using OutwardModsCommunicatorMenu.Managers;
using OutwardModsCommunicatorMenu.UI.Components;
using UnityEngine;
using UniverseLib.UI;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class InputSectionBuilder
    {
        private readonly List<AutocompleteInput> _autocompleteInputs = new();
        
        public AutocompleteInput ModGuidInput { get; private set; }
        public AutocompleteInput EventNameInput { get; private set; }
        
        public event Action OnModGuidChanged;
        public event Action OnEventNameChanged;

        public void Build(GameObject parent)
        {
            var horizontalSplit = UIFactory.CreateHorizontalGroup(
                parent,
                "InputSplit",
                false, false, true, true,
                spacing: 10,
                padding: new Vector4(5, 5, 5, 5)
            );
            UIFactory.SetLayoutElement(horizontalSplit, flexibleWidth: 9999, minHeight: 60);

            var leftSide = UIFactory.CreateVerticalGroup(
                horizontalSplit,
                "LeftSide",
                false, false, true, true,
                spacing: 3,
                padding: new Vector4(3, 3, 3, 3)
            );
            UIFactory.SetLayoutElement(leftSide, flexibleWidth: 5000, minHeight: 55);

            var rightSide = UIFactory.CreateVerticalGroup(
                horizontalSplit,
                "RightSide",
                false, false, true, true,
                spacing: 3,
                padding: new Vector4(3, 3, 3, 3)
            );
            UIFactory.SetLayoutElement(rightSide, flexibleWidth: 5000, minHeight: 55);

            var modGuidLabel = UIFactory.CreateLabel(
                leftSide, 
                "ModGuidLabel", 
                "Mod GUID:", 
                TextAnchor.MiddleLeft, 
                Color.white
            );
            UIFactory.SetLayoutElement(modGuidLabel.gameObject, minWidth: 80, minHeight: 25);

            ModGuidInput = new AutocompleteInput(
                leftSide,
                "ModGuidInput",
                "Enter mod GUID...",
                new Vector2(300, 25),
                filter => EventDataManager.Instance.GetMatchingModGuids(filter)
            );
            RegisterInput(ModGuidInput);
            ModGuidInput.OnSelect += _ => OnModGuidChanged?.Invoke();
            ModGuidInput.OnTextChanged += _ => OnModGuidChanged?.Invoke();
            ModGuidInput.OnTextChanged += _ => EventNameInput?.RefreshSuggestions();

            var eventNameLabel = UIFactory.CreateLabel(
                rightSide, 
                "EventNameLabel", 
                "Event Name:", 
                TextAnchor.MiddleLeft, 
                Color.white
            );
            UIFactory.SetLayoutElement(eventNameLabel.gameObject, minWidth: 100, minHeight: 25);

            EventNameInput = new AutocompleteInput(
                rightSide,
                "EventNameInput",
                "Enter event name...",
                new Vector2(300, 25),
                filter => EventDataManager.Instance.GetMatchingEvents(ModGuidInput.Text, filter),
                () => ModGuidInput.Text
            );
            RegisterInput(EventNameInput);
            EventNameInput.OnSelect += _ => OnEventNameChanged?.Invoke();
            EventNameInput.OnTextChanged += _ => OnEventNameChanged?.Invoke();
        }

        private void RegisterInput(AutocompleteInput input)
        {
            if (!_autocompleteInputs.Contains(input))
                _autocompleteInputs.Add(input);
        }

        public void Update()
        {
            foreach (var input in _autocompleteInputs)
            {
                input.Update();
            }
        }
    }
}
