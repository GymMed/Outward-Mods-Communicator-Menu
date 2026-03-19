using OutwardModsCommunicatorMenu.Utility;
using System;
using System.Linq;
using UnityEngine;
using UniverseLib.UI;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class SimpleValueFormatter : IValueFormatter
    {
        public bool CanFormat(object value)
        {
            return value == null || IsSimpleType(value);
        }

        public void CreateDisplay(GameObject parent, string name, object value, Type displayType)
        {
            string typeName = "unknown";
            try
            {
                typeName = displayType != null ? TypeNameFormatter.Format(displayType) : (value?.GetType().Name ?? "null");
            }
            catch
            {
                typeName = value?.GetType().Name ?? "null";
            }
            
            string valueStr = FormatValue(value);
            if (valueStr.Length > 100)
                valueStr = valueStr.Substring(0, 100) + "...";

            var label = UIFactory.CreateLabel(
                parent,
                $"{name}_Simple",
                $"{typeName}: {valueStr}",
                TextAnchor.MiddleLeft,
                new Color(0.7f, 0.8f, 0.9f, 1f),
                false,
                11
            );
            UIFactory.SetLayoutElement(label.gameObject, flexibleWidth: 9999, minHeight: 18);
        }

        private static bool IsSimpleType(object value)
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
                DateTimeOffset => true,
                TimeSpan => true,
                Guid => true,
                _ => value?.GetType().IsPrimitive ?? false
            };
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                null => "null",
                string s => $"\"{s}\"",
                bool b => b ? "true" : "false",
                char c => $"'{c}'",
                int i => i.ToString(),
                long l => l.ToString(),
                float f => f.ToString("F4"),
                double d => d.ToString("F4"),
                decimal dec => dec.ToString(),
                DateTime dt => dt.ToString("o"),
                DateTimeOffset dto => dto.ToString("o"),
                TimeSpan ts => ts.ToString(),
                Guid g => g.ToString(),
                _ => value?.ToString() ?? "null"
            };
        }
    }
}
