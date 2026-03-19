using System;
using UnityEngine;
using UniverseLib.UI;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class ActionButtonsBuilder
    {
        public event Action OnPublishClicked;
        public event Action OnClearClicked;

        private UnityEngine.UI.Text _validationLabel;

        public void Build(GameObject parent)
        {
            var buttonRow = UIFactory.CreateHorizontalGroup(
                parent,
                "ActionButtonsRow",
                false, false, true, true,
                spacing: 10,
                padding: new Vector4(3, 3, 3, 3)
            );
            UIFactory.SetLayoutElement(buttonRow, minHeight: 35);

            var publishBtn = UIFactory.CreateButton(
                buttonRow, 
                "PublishBtn", 
                "PUBLISH EVENT",
                new Color(0.2f, 0.3f, 0.2f)
            );
            UIFactory.SetLayoutElement(publishBtn.Component.gameObject, minWidth: 130, minHeight: 30);
            publishBtn.OnClick += () => OnPublishClicked?.Invoke();

            var clearBtn = UIFactory.CreateButton(
                buttonRow, 
                "ClearBtn", 
                "Clear",
                new Color(0.3f, 0.2f, 0.2f)
            );
            UIFactory.SetLayoutElement(clearBtn.Component.gameObject, minWidth: 100, minHeight: 30);
            clearBtn.OnClick += () => OnClearClicked?.Invoke();

            _validationLabel = UIFactory.CreateLabel(
                parent,
                "ValidationLabel",
                "",
                TextAnchor.MiddleLeft,
                Color.yellow,
                false,
                12
            );
            UIFactory.SetLayoutElement(_validationLabel.gameObject, flexibleWidth: 9999, minHeight: 20);
        }

        public void SetValidationMessage(string message, bool isError)
        {
            if (_validationLabel != null)
            {
                _validationLabel.text = message;
                _validationLabel.color = isError ? Color.red : Color.green;
            }
        }

        public void ClearValidationMessage()
        {
            if (_validationLabel != null)
            {
                _validationLabel.text = "";
            }
        }
    }
}
