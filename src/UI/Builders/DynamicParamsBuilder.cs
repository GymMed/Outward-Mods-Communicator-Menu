using System;
using System.Collections.Generic;
using OutwardModsCommunicatorMenu.Managers;
using OutwardModsCommunicatorMenu.UI.Components;
using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class DynamicParamsBuilder
    {
        private readonly List<(string name, string type, string value, GameObject button)> _dynamicParams = new();
        private GameObject _dynamicParamsContainer;
        private GameObject _currentRow;
        
        private GameObject _paramEditorGroup;
        private InputFieldRef _paramNameInput;
        private AutocompleteInput _paramTypeInput;
        private InputFieldRef _paramValueInput;
        private int _editingParamIndex = -1;

        private const int MAX_ROW_WIDTH = 700;
        private const int BUTTON_MIN_WIDTH = 120;
        private const int BUTTON_SPACING = 3;
        private const int ROW_PADDING = 6;
        
        private int _currentRowWidth = 0;

        public IReadOnlyList<(string name, string type, string value)> DynamicParams 
            => _dynamicParams.ConvertAll(p => (p.name, p.type, p.value));

        public event Action OnDynamicParamsChanged;

        public void Update()
        {
            if (_paramEditorGroup != null && _paramEditorGroup.activeSelf)
            {
                _paramTypeInput?.Update();
            }
        }

        public void Build(GameObject parent)
        {
            CreateDynamicParamsSection(parent);
            CreateParamEditor(parent);
        }

        private void CreateDynamicParamsSection(GameObject parent)
        {
            var label = UIFactory.CreateLabel(
                parent,
                "DynamicParamsLabel",
                "Dynamic Parameters:",
                TextAnchor.MiddleLeft,
                Color.white,
                true,
                14
            );
            UIFactory.SetLayoutElement(label.gameObject, minWidth: 200, minHeight: 25);

            _dynamicParamsContainer = UIFactory.CreateVerticalGroup(
                parent,
                "DynamicParamsContainer",
                false, false, true, true,
                spacing: 3,
                padding: new Vector4(3, 3, 3, 3)
            );
            UIFactory.SetLayoutElement(_dynamicParamsContainer, flexibleWidth: 9999, minHeight: 30);

            _currentRow = CreateNewRow();

            var addParamBtn = UIFactory.CreateButton(
                parent, 
                "AddParamBtn", 
                "+ Add",
                new Color(0.2f, 0.3f, 0.2f)
            );
            UIFactory.SetLayoutElement(addParamBtn.Component.gameObject, minWidth: 80, minHeight: 30);
            addParamBtn.OnClick += () => AddDynamicParameter();
        }

        private GameObject CreateNewRow()
        {
            var row = UIFactory.CreateHorizontalGroup(
                _dynamicParamsContainer,
                $"Row_{_dynamicParamsContainer.transform.childCount}",
                false, false, true, true,
                spacing: BUTTON_SPACING,
                padding: new Vector4(ROW_PADDING, 2, ROW_PADDING, 2)
            );
            UIFactory.SetLayoutElement(row, flexibleWidth: 9999, minHeight: 30);
            _currentRowWidth = ROW_PADDING * 2;
            return row;
        }

        private void CreateParamEditor(GameObject parent)
        {
            _paramEditorGroup = UIFactory.CreateVerticalGroup(
                parent,
                "ParamEditor",
                false, false, true, true,
                spacing: 3,
                padding: new Vector4(5, 5, 5, 5),
                bgColor: new Color(0.15f, 0.15f, 0.2f, 0.95f)
            );
            UIFactory.SetLayoutElement(_paramEditorGroup, minWidth: 300, minHeight: 120);
            _paramEditorGroup.SetActive(false);

            var nameLabel = UIFactory.CreateLabel(_paramEditorGroup, "EditNameLabel", "Name:", TextAnchor.MiddleLeft, Color.white);
            UIFactory.SetLayoutElement(nameLabel.gameObject, minHeight: 25);
            
            _paramNameInput = UIFactory.CreateInputField(_paramEditorGroup, "EditNameInput", "");
            UIFactory.SetLayoutElement(_paramNameInput.Component.gameObject, flexibleWidth: 9999, minHeight: 25);

            var typeLabel = UIFactory.CreateLabel(_paramEditorGroup, "EditTypeLabel", "Type:", TextAnchor.MiddleLeft, Color.white);
            UIFactory.SetLayoutElement(typeLabel.gameObject, minHeight: 25);
            
            _paramTypeInput = new AutocompleteInput(
                _paramEditorGroup,
                "EditTypeInput",
                "type",
                new Vector2(150, 25),
                filter => EventDataManager.Instance.GetSupportedTypes(filter)
            );

            var valueLabel = UIFactory.CreateLabel(_paramEditorGroup, "EditValueLabel", "Value:", TextAnchor.MiddleLeft, Color.white);
            UIFactory.SetLayoutElement(valueLabel.gameObject, minHeight: 25);
            
            _paramValueInput = UIFactory.CreateInputField(_paramEditorGroup, "EditValueInput", "");
            UIFactory.SetLayoutElement(_paramValueInput.Component.gameObject, flexibleWidth: 9999, minHeight: 25);

            var buttonRow = UIFactory.CreateHorizontalGroup(_paramEditorGroup, "ButtonRow", false, false, true, true, spacing: 5);
            UIFactory.SetLayoutElement(buttonRow, minHeight: 30);

            var saveBtn = UIFactory.CreateButton(buttonRow, "SaveBtn", "Save", new Color(0.2f, 0.4f, 0.2f));
            UIFactory.SetLayoutElement(saveBtn.Component.gameObject, minWidth: 60, minHeight: 25);
            saveBtn.OnClick += () => SaveParamEdit();

            var deleteBtn = UIFactory.CreateButton(buttonRow, "DeleteBtn", "Delete", new Color(0.4f, 0.2f, 0.2f));
            UIFactory.SetLayoutElement(deleteBtn.Component.gameObject, minWidth: 60, minHeight: 25);
            deleteBtn.OnClick += () => DeleteCurrentParam();

            var cancelBtn = UIFactory.CreateButton(buttonRow, "CancelBtn", "Cancel", new Color(0.3f, 0.2f, 0.2f));
            UIFactory.SetLayoutElement(cancelBtn.Component.gameObject, minWidth: 60, minHeight: 25);
            cancelBtn.OnClick += () => HideParamEditor();
        }

        public void AddDynamicParameter(string name = null, string type = null)
        {
            if (_dynamicParamsContainer == null) return;

            int index = _dynamicParams.Count;
            string paramName = name ?? $"param{index}";
            string paramType = type ?? "string";
            string paramValue = "";

            int buttonWidth = Mathf.Max(BUTTON_MIN_WIDTH, paramName.Length * 8 + 60);
            
            if (_currentRowWidth + buttonWidth > MAX_ROW_WIDTH && _dynamicParams.Count > 0)
            {
                _currentRow = CreateNewRow();
            }

            var paramBtn = UIFactory.CreateButton(
                _currentRow,
                $"Param_{index}",
                $"[{index}] {paramName}: {paramType}",
                new Color(0.2f, 0.25f, 0.3f)
            );
            UIFactory.SetLayoutElement(paramBtn.Component.gameObject, minWidth: buttonWidth, minHeight: 25);
            
            _currentRowWidth += buttonWidth + BUTTON_SPACING;
            
            int capturedIndex = index;
            paramBtn.OnClick += () => EditDynamicParameter(capturedIndex);

            _dynamicParams.Add((paramName, paramType, paramValue, paramBtn.Component.gameObject));
            UpdateParamButtonLabels();
            OnDynamicParamsChanged?.Invoke();

            UIManager.Instance.Log?.LogMessage($"[DynamicParams] Added parameter {index}: {paramName} ({paramType})");
        }

        private void UpdateParamButtonLabels()
        {
            for (int i = 0; i < _dynamicParams.Count; i++)
            {
                var (name, type, value, button) = _dynamicParams[i];
                var text = button.GetComponentInChildren<UnityEngine.UI.Text>();
                if (text != null)
                {
                    text.text = $"[{i}] {name}: {type}";
                }
            }
        }

        private void EditDynamicParameter(int index)
        {
            if (index < 0 || index >= _dynamicParams.Count) return;

            _editingParamIndex = index;
            var (name, type, value, button) = _dynamicParams[index];

            _paramNameInput.Text = name;
            _paramTypeInput.SetText(type);
            _paramValueInput.Text = value;

            _paramEditorGroup.SetActive(true);
        }

        private void SaveParamEdit()
        {
            if (_editingParamIndex >= 0 && _editingParamIndex < _dynamicParams.Count)
            {
                _dynamicParams[_editingParamIndex] = (
                    _paramNameInput.Text,
                    _paramTypeInput.Text,
                    _paramValueInput.Text,
                    _dynamicParams[_editingParamIndex].button
                );
                UpdateParamButtonLabels();
                OnDynamicParamsChanged?.Invoke();
            }
            HideParamEditor();
        }

        private void DeleteCurrentParam()
        {
            if (_editingParamIndex >= 0 && _editingParamIndex < _dynamicParams.Count)
            {
                var (name, type, value, button) = _dynamicParams[_editingParamIndex];
                if (button != null)
                    UnityEngine.Object.Destroy(button);
                _dynamicParams.RemoveAt(_editingParamIndex);
                RebuildRows();
                OnDynamicParamsChanged?.Invoke();
            }
            HideParamEditor();
        }

        private void RebuildRows()
        {
            var newParamsList = new List<(string name, string type, string value, GameObject button)>();
            
            foreach (Transform child in _dynamicParamsContainer.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            
            _currentRow = CreateNewRow();
            _currentRowWidth = ROW_PADDING * 2;
            
            for (int i = 0; i < _dynamicParams.Count; i++)
            {
                var (name, type, value, _) = _dynamicParams[i];
                int buttonWidth = Mathf.Max(BUTTON_MIN_WIDTH, name.Length * 8 + 60);
                
                if (_currentRowWidth + buttonWidth > MAX_ROW_WIDTH && _dynamicParams.Count > 0)
                {
                    _currentRow = CreateNewRow();
                }

                var paramBtn = UIFactory.CreateButton(
                    _currentRow,
                    $"Param_{i}",
                    $"[{i}] {name}: {type}",
                    new Color(0.2f, 0.25f, 0.3f)
                );
                UIFactory.SetLayoutElement(paramBtn.Component.gameObject, minWidth: buttonWidth, minHeight: 25);
                
                int capturedIndex = i;
                paramBtn.OnClick += () => EditDynamicParameter(capturedIndex);
                
                _currentRowWidth += buttonWidth + BUTTON_SPACING;
                
                newParamsList.Add((name, type, value, paramBtn.Component.gameObject));
            }
            
            _dynamicParams.Clear();
            _dynamicParams.AddRange(newParamsList);
            
            UpdateParamButtonLabels();
        }

        private void HideParamEditor()
        {
            _paramEditorGroup.SetActive(false);
            _editingParamIndex = -1;
        }

        public void Clear()
        {
            foreach (var (_, _, _, button) in _dynamicParams)
            {
                UnityEngine.Object.Destroy(button);
            }
            _dynamicParams.Clear();
            
            foreach (Transform child in _dynamicParamsContainer.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            _currentRow = CreateNewRow();
            
            OnDynamicParamsChanged?.Invoke();
        }
    }
}
