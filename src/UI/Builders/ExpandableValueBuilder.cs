using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using OutwardModsCommunicatorMenu;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class ExpandableValueBuilder
    {
        private const int MaxReflectionDepth = 3;
        private const int PageSize = 30;
        
        private readonly GameObject _rootObject;
        private readonly GameObject _summaryLine;
        private readonly GameObject _detailContent;
        private readonly ButtonRef _toggleButton;
        private readonly bool _isExpanded;
        private readonly string _name;
        private readonly object _storedValue;
        private readonly Type _storedType;
        
        public GameObject DetailContent => _detailContent;

        private ExpandableValueBuilder(GameObject parent, string name, string typeName, object value, Type displayType, bool isExpanded)
        {
            _name = name;
            _isExpanded = isExpanded;
            _storedValue = value;
            _storedType = displayType;
            
            var container = UIFactory.CreateVerticalGroup(
                parent,
                $"{name}_Container",
                false, false, true, true,
                spacing: 2,
                padding: new Vector4(2, 2, 2, 2)
            );
            UIFactory.SetLayoutElement(container, flexibleWidth: 9999, minHeight: 20);
            
            _rootObject = container;
            
            _summaryLine = UIFactory.CreateHorizontalGroup(
                container,
                $"{name}_Summary",
                false, false, true, true,
                spacing: 3
            );
            UIFactory.SetLayoutElement(_summaryLine, minHeight: 20);
            
            string expandIcon = isExpanded ? "[-]" : "[+]";
            _toggleButton = UIFactory.CreateButton(
                _summaryLine,
                $"{name}_Toggle",
                expandIcon,
                new Color(0.25f, 0.28f, 0.35f)
            );
            _toggleButton.OnClick += OnToggleClicked;
            UIFactory.SetLayoutElement(_toggleButton.Component.gameObject, minWidth: 30, minHeight: 20);
            
            var typeLabel = UIFactory.CreateLabel(
                _summaryLine,
                $"{name}_Type",
                $"{typeName}",
                TextAnchor.MiddleLeft,
                new Color(0.6f, 0.7f, 0.9f, 1f),
                false,
                11
            );
            UIFactory.SetLayoutElement(typeLabel.gameObject, minWidth: 80, minHeight: 18);
            
            if (value != null)
            {
                string preview = GetValuePreview(value);
                if (!string.IsNullOrEmpty(preview))
                {
                    var valuePreview = UIFactory.CreateLabel(
                        _summaryLine,
                        $"{name}_Preview",
                        preview,
                        TextAnchor.MiddleLeft,
                        new Color(0.7f, 0.7f, 0.75f, 1f),
                        false,
                        10
                    );
                    UIFactory.SetLayoutElement(valuePreview.gameObject, flexibleWidth: 9999, minHeight: 18);
                }
            }
            
            _detailContent = UIFactory.CreateVerticalGroup(
                container,
                $"{name}_Detail",
                false, false, true, true,
                spacing: 2,
                padding: new Vector4(10, 2, 2, 2)
            );
            UIFactory.SetLayoutElement(_detailContent, flexibleWidth: 9999);
            _detailContent.SetActive(isExpanded);
            
            if (isExpanded)
            {
                BuildDetailContent(_detailContent, value, displayType, 0);
            }
        }

        public static ExpandableValueBuilder Create(
            GameObject parent,
            string name,
            string typeName,
            object value,
            Type displayType)
        {
            return new ExpandableValueBuilder(parent, name, typeName, value, displayType, false);
        }

        private void OnToggleClicked()
        {
            _detailContent.SetActive(!_detailContent.activeSelf);
            
            var textComp = _toggleButton.Component.GetComponentInChildren<UnityEngine.UI.Text>();
            if (textComp != null)
            {
                textComp.text = _detailContent.activeSelf ? "[-]" : "[+]";
            }
            
            if (_detailContent.activeSelf && _detailContent.transform.childCount == 0)
            {
                BuildDetailContent(_detailContent, _storedValue, _storedType, 0);
            }
        }

        private void BuildDetailContent(GameObject content, object value, Type displayType, int depth)
        {
            if (value == null)
            {
                var nullLabel = UIFactory.CreateLabel(
                    content,
                    $"{content.name}_Null",
                    "  (null)",
                    TextAnchor.MiddleLeft,
                    new Color(0.5f, 0.5f, 0.5f, 1f),
                    false,
                    10
                );
                UIFactory.SetLayoutElement(nullLabel.gameObject, minHeight: 16);
                return;
            }

            if (value is IEnumerable enumerable && !(value is string))
            {
                BuildCollectionContent(content, value, displayType, depth);
                return;
            }

            string asString = ComplexValueFormatter.FormatAsString(value, depth);
            if (!string.IsNullOrEmpty(asString))
            {
                var asStringLabel = UIFactory.CreateLabel(
                    content,
                    $"{content.name}_AsString",
                    $"  {asString}",
                    TextAnchor.MiddleLeft,
                    new Color(0.6f, 0.85f, 0.6f, 1f),
                    false,
                    10
                );
                UIFactory.SetLayoutElement(asStringLabel.gameObject, flexibleWidth: 9999, minHeight: 16);
                
                CreateSeparator(content, $"{content.name}_Sep1");
            }
            
            BuildReflectionContent(content, value, depth);
        }

        private void BuildCollectionContent(GameObject content, object value, Type displayType, int depth)
        {
            var collection = value as IEnumerable;
            if (collection == null) return;

            var items = new List<object>();
            foreach (var item in collection)
            {
                items.Add(item);
            }

            int totalCount = items.Count;
            int startIndex = 0;
            int displayCount = Math.Min(PageSize, totalCount);
            bool isPaginated = totalCount > PageSize;

            CreateCollectionItems(content, items, startIndex, displayCount, totalCount, depth, $"{content.name}_page0");

            if (isPaginated)
            {
                CreatePaginationControls(content, items, displayCount, totalCount, depth, $"{content.name}_pagination");
            }
        }

        private void CreateCollectionItems(GameObject parent, List<object> items, int startIndex, int displayCount, int totalCount, int depth, string namePrefix)
        {
            for (int i = startIndex; i < startIndex + displayCount && i < items.Count; i++)
            {
                var item = items[i];
                string itemName = $"{namePrefix}_{i}";
                string typeName = item?.GetType().Name ?? "null";
                string preview = GetItemPreview(item);

                var itemContainer = UIFactory.CreateVerticalGroup(
                    parent,
                    $"{itemName}_container",
                    false, false, true, true,
                    spacing: 2,
                    padding: new Vector4(2, 2, 2, 2)
                );
                UIFactory.SetLayoutElement(itemContainer, flexibleWidth: 9999, minHeight: 18);

                var itemHeader = UIFactory.CreateHorizontalGroup(
                    itemContainer,
                    $"{itemName}_header",
                    false, false, true, true,
                    spacing: 3
                );
                UIFactory.SetLayoutElement(itemHeader, minHeight: 18);

                var indexLabel = UIFactory.CreateLabel(
                    itemHeader,
                    $"{itemName}_index",
                    $"#{i}:",
                    TextAnchor.MiddleLeft,
                    new Color(0.6f, 0.7f, 0.8f, 1f),
                    false,
                    10
                );
                UIFactory.SetLayoutElement(indexLabel.gameObject, minWidth: 35, minHeight: 18);

                string itemTypeName = item?.GetType().Name ?? "null";
                var typeLabel = UIFactory.CreateLabel(
                    itemHeader,
                    $"{itemName}_type",
                    itemTypeName,
                    TextAnchor.MiddleLeft,
                    new Color(0.6f, 0.7f, 0.9f, 1f),
                    false,
                    10
                );
                UIFactory.SetLayoutElement(typeLabel.gameObject, minWidth: 80, minHeight: 18);

                if (!string.IsNullOrEmpty(preview))
                {
                    var previewLabel = UIFactory.CreateLabel(
                        itemHeader,
                        $"{itemName}_preview",
                        $" = {preview}",
                        TextAnchor.MiddleLeft,
                        new Color(0.7f, 0.7f, 0.75f, 1f),
                        false,
                        10
                    );
                    UIFactory.SetLayoutElement(previewLabel.gameObject, flexibleWidth: 9999, minHeight: 18);
                }

                var detailContent = UIFactory.CreateVerticalGroup(
                    itemContainer,
                    $"{itemName}_detail",
                    false, false, true, true,
                    spacing: 2,
                    padding: new Vector4(10, 2, 2, 2)
                );
                UIFactory.SetLayoutElement(detailContent, flexibleWidth: 9999);
                detailContent.SetActive(false);

                bool isSimple = item == null || IsSimpleType(item);
                
                if (!isSimple && depth < MaxReflectionDepth)
                {
                    var expandBtn = UIFactory.CreateButton(
                        itemHeader,
                        $"{itemName}_expand",
                        "[+]",
                        new Color(0.2f, 0.25f, 0.3f)
                    );
                    expandBtn.OnClick += () =>
                    {
                        bool isNowActive = !detailContent.activeSelf;
                        detailContent.SetActive(isNowActive);
                        expandBtn.ButtonText.text = isNowActive ? "[-]" : "[+]";
                        
                        if (isNowActive && detailContent.transform.childCount == 0)
                        {
                            BuildReflectionContent(detailContent, item, depth + 1);
                        }
                    };
                    UIFactory.SetLayoutElement(expandBtn.Component.gameObject, minWidth: 30, minHeight: 16);
                }
            }
        }

        private void CreatePaginationControls(GameObject parent, List<object> items, int currentEnd, int totalCount, int depth, string namePrefix)
        {
            var loadMoreBtn = UIFactory.CreateButton(
                parent,
                $"{namePrefix}_loadMore",
                $"Load More ({totalCount - currentEnd} remaining)",
                new Color(0.2f, 0.3f, 0.4f)
            );
            UIFactory.SetLayoutElement(loadMoreBtn.Component.gameObject, flexibleWidth: 9999, minHeight: 25);

            int capturedStart = currentEnd;
            loadMoreBtn.OnClick += () =>
            {
                UnityEngine.Object.Destroy(loadMoreBtn.Component.gameObject);
                
                int newEnd = Math.Min(capturedStart + PageSize, totalCount);
                CreateCollectionItems(parent, items, capturedStart, newEnd - capturedStart, totalCount, depth, $"{namePrefix}_page{capturedStart / PageSize}");
                
                if (newEnd < totalCount)
                {
                    CreatePaginationControls(parent, items, newEnd, totalCount, depth, $"{namePrefix}_page{newEnd / PageSize}");
                }
            };
        }

        private bool IsSimpleType(object value)
        {
            return value switch
            {
                null => true,
                string => true,
                bool => true,
                char => true,
                int => true,
                long => true,
                float => true,
                double => true,
                decimal => true,
                DateTime => true,
                _ => value?.GetType().IsPrimitive ?? false
            };
        }

        private string GetItemPreview(object item)
        {
            if (item == null) return "null";
            if (item is string s) return $"\"{s}\"";
            if (item is bool b) return b ? "true" : "false";
            if (item is int i) return i.ToString();
            if (item is float f) return f.ToString("F2");
            if (item is double d) return d.ToString("F2");
            return string.Empty;
        }

        private void BuildReflectionContent(GameObject content, object value, int depth)
        {
            if (depth >= MaxReflectionDepth)
            {
                var maxDepthLabel = UIFactory.CreateLabel(
                    content,
                    $"{content.name}_MaxDepth",
                    "  (max depth reached)",
                    TextAnchor.MiddleLeft,
                    new Color(0.5f, 0.5f, 0.5f, 1f),
                    false,
                    10
                );
                UIFactory.SetLayoutElement(maxDepthLabel.gameObject, minHeight: 16);
                return;
            }

            List<ComplexValueFormatter.MemberInfo> members = null;
            try
            {
                members = ComplexValueFormatter.GetMembers(value, depth);
            }
            catch (Exception ex)
            {
                OMCM.LogMessage($"[ExpandableValueBuilder] GetMembers failed: {ex.Message}");
            }
            
            if (members != null && members.Count > 0)
            {
                var headerLabel = UIFactory.CreateLabel(
                    content,
                    $"{content.name}_ReflectionHeader",
                    $"  --- Members ({members.Count}) ---",
                    TextAnchor.MiddleLeft,
                    new Color(0.5f, 0.6f, 0.8f, 1f),
                    true,
                    10
                );
                UIFactory.SetLayoutElement(headerLabel.gameObject, flexibleWidth: 9999, minHeight: 18);
                
                foreach (var member in members)
                {
                    try
                    {
                        CreateMemberLabel(content, member.Name, member.Value, member.MemberType, depth + 1);
                    }
                    catch (Exception ex)
                    {
                        OMCM.LogMessage($"[ExpandableValueBuilder] CreateMemberLabel failed for {member.Name}: {ex.Message}");
                    }
                }
            }
        }

        private void CreateMemberLabel(GameObject parent, string name, object memberValue, string memberType, int depth)
        {
            string prefix = memberType == "Property" ? "P:" : "F:";
            
            var memberContainer = UIFactory.CreateHorizontalGroup(
                parent,
                $"Member_{name}",
                false, false, true, true,
                spacing: 5
            );
            UIFactory.SetLayoutElement(memberContainer, minHeight: 16);
            
            var nameLabel = UIFactory.CreateLabel(
                memberContainer,
                $"MemberName_{name}",
                $"  {prefix} {name}:",
                TextAnchor.MiddleLeft,
                new Color(0.6f, 0.7f, 0.85f, 1f),
                false,
                10
            );
            UIFactory.SetLayoutElement(nameLabel.gameObject, minWidth: 80, minHeight: 16);
            
            string valueStr;
            Color valueColor;
            
            if (memberValue == null)
            {
                valueStr = "null";
                valueColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else if (memberValue is ICollection col)
            {
                valueStr = $"<{col.GetType().Name} Count={col.Count}>";
                valueColor = new Color(0.7f, 0.8f, 0.9f, 1f);
            }
            else if (memberValue is Array arr)
            {
                valueStr = $"<{arr.GetType().Name} Length={arr.Length}>";
                valueColor = new Color(0.7f, 0.8f, 0.9f, 1f);
            }
            else
            {
                valueStr = memberValue.ToString();
                if (valueStr.Length > 60) valueStr = valueStr.Substring(0, 60) + "...";
                valueColor = new Color(0.8f, 0.8f, 0.85f, 1f);
            }
            
            var valueLabel = UIFactory.CreateLabel(
                memberContainer,
                $"MemberValue_{name}",
                valueStr,
                TextAnchor.MiddleLeft,
                valueColor,
                false,
                10
            );
            UIFactory.SetLayoutElement(valueLabel.gameObject, flexibleWidth: 9999, minHeight: 16);
        }

        private static string GetValuePreview(object value)
        {
            return ComplexValueFormatter.GetValuePreview(value);
        }

        private void CreateSeparator(GameObject parent, string name)
        {
            var separator = UIFactory.CreateLabel(
                parent,
                name,
                "",
                TextAnchor.MiddleLeft,
                new Color(0.3f, 0.3f, 0.35f, 1f),
                false,
                6
            );
            UIFactory.SetLayoutElement(separator.gameObject, flexibleWidth: 9999, minHeight: 4);
        }
    }
}
