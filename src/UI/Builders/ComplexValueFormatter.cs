using OutwardModsCommunicatorMenu.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UniverseLib.UI;

namespace OutwardModsCommunicatorMenu.UI.Builders
{
    public class ComplexValueFormatter : IValueFormatter
    {
        private const int MaxDepth = 3;

        public bool CanFormat(object value)
        {
            return value != null && !IsSimpleType(value);
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
            
            ExpandableValueBuilder.Create(parent, name, typeName, value, displayType);
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

        public static string FormatAsString(object value, int depth)
        {
            return value switch
            {
                Array arr => FormatArray(arr),
                IDictionary dict => FormatDictionary(dict),
                ICollection col => FormatCollection(col),
                IEnumerable enumerable => FormatEnumerable(enumerable),
                _ => null
            };
        }

        private static string FormatCollection(ICollection collection)
        {
            var sb = new StringBuilder();
            sb.Append('[');

            int count = 0;
            foreach (var item in collection)
            {
                if (count > 0) sb.Append(", ");
                if (count >= 10) { sb.Append("..."); break; }

                sb.Append(FormatCollectionItem(item));
                count++;
            }

            sb.Append(']');
            return sb.ToString();
        }

        private static string FormatArray(Array array)
        {
            var sb = new StringBuilder();
            sb.Append('[');

            for (int i = 0; i < array.Length && i < 10; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(FormatCollectionItem(array.GetValue(i)));
            }

            if (array.Length > 10) sb.Append("...");
            sb.Append(']');
            return sb.ToString();
        }

        private static string FormatDictionary(IDictionary dict)
        {
            var sb = new StringBuilder();
            sb.Append('{');

            int count = 0;
            foreach (DictionaryEntry entry in dict)
            {
                if (count > 0) sb.Append(", ");
                if (count >= 5) { sb.Append("..."); break; }

                sb.Append($"{FormatCollectionItem(entry.Key)}: {FormatCollectionItem(entry.Value)}");
                count++;
            }

            sb.Append('}');
            return sb.ToString();
        }

        private static string FormatEnumerable(IEnumerable enumerable)
        {
            var sb = new StringBuilder();
            sb.Append('[');

            int count = 0;
            foreach (var item in enumerable)
            {
                if (count > 0) sb.Append(", ");
                if (count >= 10) { sb.Append("..."); break; }

                sb.Append(FormatCollectionItem(item));
                count++;
            }

            sb.Append(']');
            return sb.ToString();
        }

        private static string FormatCollectionItem(object item)
        {
            return item switch
            {
                null => "null",
                string s => $"\"{s}\"",
                bool b => b ? "true" : "false",
                int i => i.ToString(),
                float f => f.ToString("F2"),
                double d => d.ToString("F2"),
                Vector2 v2 => $"({v2.x:F2},{v2.y:F2})",
                Vector3 v3 => $"({v3.x:F2},{v3.y:F2},{v3.z:F2})",
                Vector4 v4 => $"({v4.x:F2},{v4.y:F2},{v4.z:F2},{v4.w:F2})",
                Quaternion q => $"({q.x:F2},{q.y:F2},{q.z:F2},{q.w:F2})",
                Color c => $"({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})",
                _ when item?.GetType().IsPrimitive == true => item.ToString(),
                _ when item is ICollection || item is Array || item is IDictionary || item is IEnumerable => $"<{item.GetType().Name}>",
                _ => item?.ToString() ?? "null"
            };
        }

        public static string GetValuePreview(object value)
        {
            return value switch
            {
                null => "(null)",
                Array arr => $"Length={arr.Length}",
                IDictionary dict => $"Count={dict.Count}",
                ICollection col => $"Count={col.Count}",
                IEnumerable enumerable => $">= {GetEnumerableCount(enumerable)}",
                _ => string.Empty
            };
        }

        private static int GetEnumerableCount(IEnumerable enumerable)
        {
            int count = 0;
            foreach (var _ in enumerable)
            {
                count++;
                if (count > 5) break;
            }
            return count;
        }

        public static List<MemberInfo> GetMembers(object value, int depth)
        {
            var members = new List<MemberInfo>();
            if (value == null) return members;

            var type = value.GetType();

            try
            {
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.GetIndexParameters().Length > 0) continue;
                    try
                    {
                        var propValue = prop.GetValue(value);
                        members.Add(new MemberInfo { Name = prop.Name, Value = propValue, MemberType = "Property" });
                    }
                    catch
                    {
                        members.Add(new MemberInfo { Name = prop.Name, Value = null, MemberType = "Property" });
                    }
                }
            }
            catch { }

            try
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    try
                    {
                        var fieldValue = field.GetValue(value);
                        members.Add(new MemberInfo { Name = field.Name, Value = fieldValue, MemberType = "Field" });
                    }
                    catch
                    {
                        members.Add(new MemberInfo { Name = field.Name, Value = null, MemberType = "Field" });
                    }
                }
            }
            catch { }

            return members;
        }

        public class MemberInfo
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public string MemberType { get; set; }
        }
    }
}
