using System;
using System.Linq;
using UnityEngine;
using UniverseLib.UI;
using OutwardModsCommunicatorMenu;
using OutwardModsCommunicatorMenu.Utility;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public static class ValueFormatterFactory
    {
        private static readonly IValueFormatter[] Formatters = new IValueFormatter[]
        {
            new SimpleValueFormatter(),
            new ComplexValueFormatter()
        };

        public static IValueFormatter GetFormatter(object value)
        {
            return Formatters.FirstOrDefault(f => f.CanFormat(value)) ?? new SimpleValueFormatter();
        }

        public static void CreateDisplay(GameObject parent, string name, object value, Type displayType)
        {
            try
            {
                var formatter = GetFormatter(value);
                formatter.CreateDisplay(parent, name, value, displayType);
            }
            catch (Exception ex)
            {
                OMCM.LogMessage($"[ValueFormatterFactory] Error formatting value '{name}': {ex.Message}");
                CreateFallbackDisplay(parent, name, value, displayType);
            }
        }

        private static void CreateFallbackDisplay(GameObject parent, string name, object value, Type displayType)
        {
            try
            {
                string typeName = displayType != null ? TypeNameFormatter.Format(displayType) : (value?.GetType().Name ?? "unknown");
                string valueStr = value?.ToString() ?? "null";

                if (valueStr.Length > 100)
                    valueStr = valueStr.Substring(0, 100) + "...";

                var label = UIFactory.CreateLabel(
                    parent,
                    $"{name}_Fallback",
                    $"[ERROR] {typeName}: {valueStr}",
                    TextAnchor.MiddleLeft,
                    Color.red,
                    false,
                    11
                );
                UIFactory.SetLayoutElement(label.gameObject, flexibleWidth: 9999, minHeight: 18);
            }
            catch
            {
                // Absolute fallback - just create something
                var label = UIFactory.CreateLabel(
                    parent,
                    $"{name}_Error",
                    "[ERROR] Failed to display value",
                    TextAnchor.MiddleLeft,
                    Color.red,
                    false,
                    11
                );
                UIFactory.SetLayoutElement(label.gameObject, flexibleWidth: 9999, minHeight: 18);
            }
        }
    }
}
