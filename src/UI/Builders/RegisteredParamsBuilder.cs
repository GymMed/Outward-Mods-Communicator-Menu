using System;
using System.Collections.Generic;
using System.Linq;
using OutwardModsCommunicatorMenu.Managers;
using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Widgets;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class RegisteredParamsBuilder
    {
        private GameObject _registeredParamsContent;
        
        public event Action<string, string> OnRegisteredParamClicked;

        public void Build(GameObject parent)
        {
            var label = UIFactory.CreateLabel(
                parent, 
                "RegisteredParamsLabel", 
                "Registered Parameters (click to add):", 
                TextAnchor.MiddleLeft, 
                Color.white, 
                true, 
                14
            );
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 250, minHeight: 25);

            GameObject scrollContent;
            AutoSliderScrollbar autoScrollbar;

            var scrollViewObj = UIFactory.CreateScrollView(
                parent, 
                "ParamsScrollView", 
                out scrollContent, 
                out autoScrollbar,
                new Color(0.2f, 0.2f, 0.2f)
            );
            _registeredParamsContent = scrollContent;
            UIFactory.SetLayoutElement(scrollViewObj, flexibleHeight: 9999, minHeight: 120);
        }

        public void Refresh(string modGuid, string eventName, string currentEventName)
        {
            if (_registeredParamsContent == null) return;

            foreach (Transform child in _registeredParamsContent.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }

            List<(string name, string typeName, string description)> parameters;

            // If event name is empty, show all events for the mod
            if (string.IsNullOrEmpty(eventName) && !string.IsNullOrEmpty(modGuid))
            {
                parameters = GetAllEventsForMod(modGuid);
            }
            else if (!string.IsNullOrEmpty(modGuid) && !string.IsNullOrEmpty(eventName))
            {
                parameters = EventDataManager.Instance.GetEventParameters(modGuid, eventName);
            }
            else
            {
                parameters = new List<(string, string, string)>();
            }

            if (parameters.Count > 0)
            {
                foreach (var (name, typeName, description) in parameters)
                {
                    CreateParamButton(name, typeName);
                }
            }
            else
            {
                var noParamsLabel = UIFactory.CreateLabel(
                    _registeredParamsContent,
                    "NoParams",
                    string.IsNullOrEmpty(modGuid) ? "Select a Mod GUID first" : "No registered parameters",
                    TextAnchor.MiddleLeft,
                    Color.gray,
                    false,
                    12
                );
                UIFactory.SetLayoutElement(noParamsLabel.gameObject, minHeight: 20);
            }
        }

        private List<(string name, string typeName, string description)> GetAllEventsForMod(string modGuid)
        {
            var result = new List<(string, string, string)>();
            
            var events = EventDataManager.Instance.GetRegisteredEvents();
            if (events.TryGetValue(modGuid, out var modEvents))
            {
                foreach (var kvp in modEvents)
                {
                    var eventName = kvp.Key;
                    var eventDef = kvp.Value;
                    if (eventDef?.Schema?.Fields != null)
                    {
                        foreach (var field in eventDef.Schema.Fields)
                        {
                            result.Add((field.Key, Utility.TypeNameFormatter.Format(field.Value), eventDef.Schema.GetDescription(field.Key)));
                        }
                    }
                }
            }
            
            return result;
        }

        private void CreateParamButton(string name, string typeName)
        {
            var paramBtn = UIFactory.CreateButton(
                _registeredParamsContent,
                $"RegisteredParam_{name}_{typeName}",
                $"{name} ({typeName})",
                new Color(0.15f, 0.15f, 0.2f)
            );
            UIFactory.SetLayoutElement(paramBtn.Component.gameObject, flexibleWidth: 9999, minHeight: 25);
            
            string capturedName = name;
            string capturedType = typeName;
            paramBtn.OnClick += () => OnRegisteredParamClicked?.Invoke(capturedName, capturedType);
        }
    }
}
