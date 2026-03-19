using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OutwardModsCommunicatorMenu;

namespace OutwardModsCommunicatorMenu.Utility
{
    public static class TypeNameFormatter
    {
        public static string Format(Type type)
        {
            if (type == null)
                return "null";

#if DEBUG
            OMCM.LogMessage($"[DEBUG] TypeNameFormatter.Format: ENTRY type.FullName={type.FullName}, IsGenericType={type.IsGenericType}");
#endif

            if (type.IsArray)
            {
                return $"{Format(type.GetElementType())}[]";
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return $"{Format(underlyingType)}?";
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = type.GetGenericArguments();
                
                var baseName = GetGenericTypeBaseName(genericTypeDefinition.Name);
                
#if DEBUG
                OMCM.LogMessage($"[DEBUG] TypeNameFormatter.Format: IsGenericType=true, baseName={baseName}");
                foreach (var arg in genericArguments)
                {
                    OMCM.LogMessage($"[DEBUG] TypeNameFormatter.Format: genericArg={arg.FullName}");
                }
#endif

                var formattedArgs = string.Join(", ", genericArguments.Select(Format));
                
                return $"{baseName}<{formattedArgs}>";
            }

            if (type == typeof(string))
                return "string";
            if (type == typeof(int))
                return "int";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";

            return GetDisplayName(type);
        }

        private static string GetGenericTypeBaseName(string genericTypeDefinitionName)
        {
            int backtickIndex = genericTypeDefinitionName.IndexOf('`');
            if (backtickIndex > 0)
            {
                return genericTypeDefinitionName.Substring(0, backtickIndex);
            }
            return genericTypeDefinitionName;
        }

        private static string GetDisplayName(Type type)
        {
            var nestedPath = GetNestedClassPath(type);
            bool hasCollision = HasNameCollision(nestedPath);
            string fullQualifiedName = GetFullQualifiedName(type);

            if (hasCollision)
            {
                return fullQualifiedName;
            }

            return string.Join(".", nestedPath);
        }

        private static List<string> GetNestedClassPath(Type type)
        {
            var parts = new List<string>();
            var current = type;

            while (current != null)
            {
                string name = current.Name;
                int backtickIndex = name.IndexOf('`');
                if (backtickIndex > 0)
                {
                    name = name.Substring(0, backtickIndex);
                }

                parts.Insert(0, name);
                current = current.DeclaringType;
            }

            return parts;
        }

        private static string GetFullQualifiedName(Type type)
        {
            string ns = type.Namespace;
            var nestedPath = GetNestedClassPath(type);

            if (string.IsNullOrEmpty(ns))
            {
                return string.Join(".", nestedPath);
            }

            return $"{ns}.{string.Join(".", nestedPath)}";
        }

        private static bool HasNameCollision(List<string> nestedPath)
        {
            if (nestedPath.Count == 0)
                return false;

            var pathString = string.Join(".", nestedPath);
            var matchingTypes = new List<string>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var loadedType in assembly.GetTypes())
                    {
                        if (loadedType.FullName == null)
                            continue;

                        var loadedPath = GetNestedClassPath(loadedType);
                        if (loadedPath.Count != nestedPath.Count)
                            continue;

                        bool matches = true;
                        for (int i = 0; i < loadedPath.Count; i++)
                        {
                            if (!string.Equals(loadedPath[i], nestedPath[i], StringComparison.OrdinalIgnoreCase))
                            {
                                matches = false;
                                break;
                            }
                        }

                        if (matches)
                        {
                            matchingTypes.Add(loadedType.Namespace ?? "");
                        }
                    }
                }
                catch
                {
                }
            }

            var uniqueNamespaces = matchingTypes.Distinct().ToList();
            return uniqueNamespaces.Count > 1;
        }
    }
}
