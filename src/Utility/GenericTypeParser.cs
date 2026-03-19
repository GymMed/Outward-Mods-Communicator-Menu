using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OutwardModsCommunicatorMenu;

namespace OutwardModsCommunicatorMenu.Utility
{
    public static class GenericTypeParser
    {
        private static readonly Dictionary<string, Type> SimpleTypeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "string", typeof(string) },
            { "int", typeof(int) },
            { "bool", typeof(bool) },
            { "float", typeof(float) },
            { "double", typeof(double) },
            { "decimal", typeof(decimal) },
            { "long", typeof(long) },
            { "short", typeof(short) },
            { "byte", typeof(byte) },
            { "char", typeof(char) },
            { "object", typeof(object) },
            { "void", typeof(void) },
            { "hashset", typeof(System.Collections.Generic.HashSet<>) },
            { "list", typeof(System.Collections.Generic.List<>) },
            { "dictionary", typeof(System.Collections.Generic.Dictionary<,>) },
            { "queue", typeof(System.Collections.Generic.Queue<>) },
            { "stack", typeof(System.Collections.Generic.Stack<>) },
            { "set", typeof(System.Collections.Generic.HashSet<>) },
        };

        public static Type Parse(string typeString)
        {
            if (string.IsNullOrWhiteSpace(typeString))
                return null;

            typeString = typeString.Trim();

#if DEBUG
            OMCM.LogMessage($"[DEBUG] GenericTypeParser.Parse: ENTRY typeString=\"{typeString}\"");
#endif

            int arrayBracketIndex = typeString.IndexOf('[');
            int genericBracketIndex = typeString.IndexOf('<');

            if (arrayBracketIndex >= 0 && (genericBracketIndex < 0 || arrayBracketIndex < genericBracketIndex))
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] GenericTypeParser.Parse: detected array type, calling ParseArrayType");
#endif
                return ParseArrayType(typeString);
            }

            if (genericBracketIndex >= 0)
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] GenericTypeParser.Parse: detected generic type, calling ParseGenericType");
#endif
                return ParseGenericType(typeString);
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] GenericTypeParser.Parse: simple type, calling ParseSimpleType");
#endif
            return ParseSimpleType(typeString);
        }

        private static Type ParseArrayType(string typeString)
        {
            int bracketIndex = typeString.IndexOf('[');
            if (bracketIndex < 0)
                return null;

            string elementTypeString = typeString.Substring(0, bracketIndex);
            string suffix = typeString.Substring(bracketIndex);

            Type elementType = Parse(elementTypeString);
            if (elementType == null)
                return null;

            if (suffix == "[]")
            {
                return elementType.MakeArrayType();
            }

            int rank = 1;
            for (int i = 1; i < suffix.Length; i++)
            {
                if (suffix[i] == ',')
                    rank++;
            }

            return elementType.MakeArrayType(rank);
        }

        private static Type ParseGenericType(string typeString)
        {
            int openBracket = typeString.IndexOf('<');
            int closeBracket = typeString.LastIndexOf('>');

            if (openBracket < 0 || closeBracket < 0 || closeBracket <= openBracket)
                return null;

            string baseTypeString = typeString.Substring(0, openBracket);
            string argsString = typeString.Substring(openBracket + 1, closeBracket - openBracket - 1);

#if DEBUG
            OMCM.LogMessage($"[DEBUG] ParseGenericType: typeString=\"{typeString}\"");
            OMCM.LogMessage($"[DEBUG] ParseGenericType: baseTypeString=\"{baseTypeString}\", argsString=\"{argsString}\"");
#endif

            var genericArgs = ParseGenericArguments(argsString);
            if (genericArgs == null || genericArgs.Count == 0)
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] ParseGenericType: ParseGenericArguments returned null or empty");
#endif
                return null;
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] ParseGenericType: parsed {genericArgs.Count} generic args: [{string.Join(", ", genericArgs.Select(t => t.FullName))}]");
#endif

            Type baseType = ParseSimpleType(baseTypeString);
            if (baseType == null)
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] ParseGenericType: ParseSimpleType(\"{baseTypeString}\") returned null");
#endif
                return null;
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] ParseGenericType: baseType resolved to {baseType.FullName}");
#endif

            try
            {
                var result = baseType.MakeGenericType(genericArgs.ToArray());
#if DEBUG
                OMCM.LogMessage($"[DEBUG] ParseGenericType: MakeGenericType succeeded: {result.FullName}");
#endif
                return result;
            }
            catch (Exception ex)
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] ParseGenericType: MakeGenericType FAILED: {ex.Message}");
#endif
                return null;
            }
        }

        private static List<Type> ParseGenericArguments(string argsString)
        {
            var args = new List<Type>();
            int depth = 0;
            int start = 0;

            for (int i = 0; i <= argsString.Length; i++)
            {
                char c = i < argsString.Length ? argsString[i] : ',';
                
                if (c == '<' || c == '[')
                    depth++;
                else if (c == '>' || c == ']')
                    depth--;
                else if (c == ',' && depth == 0)
                {
                    string argString = argsString.Substring(start, i - start).Trim();
                    if (!string.IsNullOrEmpty(argString))
                    {
                        Type argType = Parse(argString);
                        if (argType == null)
                            return null;
                        args.Add(argType);
                    }
                    start = i + 1;
                }
            }

            if (start < argsString.Length)
            {
                string argString = argsString.Substring(start).Trim();
                if (!string.IsNullOrEmpty(argString))
                {
                    Type argType = Parse(argString);
                    if (argType == null)
                        return null;
                    args.Add(argType);
                }
            }

            return args;
        }

        private static Type ParseSimpleType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            typeName = typeName.Trim();

#if DEBUG
            OMCM.LogMessage($"[DEBUG] ParseSimpleType: checking \"{typeName}\"");
#endif

            if (SimpleTypeMap.TryGetValue(typeName, out var simpleType))
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] ParseSimpleType: SimpleTypeMap hit for \"{typeName}\" -> {simpleType.FullName}");
#endif
                return simpleType;
            }

            if (typeName.EndsWith("?"))
            {
                string underlyingTypeName = typeName.Substring(0, typeName.Length - 1);
                Type underlyingType = ParseSimpleType(underlyingTypeName);
                if (underlyingType != null)
                    return typeof(Nullable<>).MakeGenericType(underlyingType);
                return null;
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] ParseSimpleType: \"{typeName}\" not in SimpleTypeMap, delegating to FindTypeInAssemblies");
#endif

            return FindTypeInAssemblies(typeName);
        }

        private static Type FindTypeInAssemblies(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

#if DEBUG
            OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: typeName=\"{typeName}\"");
            var assemblyNames = string.Join(", ", assemblies.Select(a => a.GetName().Name).OrderBy(n => n));
            OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: loaded assemblies ({assemblies.Length}): {assemblyNames}");
            
            bool hasAssemblyCSharp = assemblies.Any(a => a.GetName().Name == "Assembly-CSharp");
            OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: Assembly-CSharp loaded: {hasAssemblyCSharp}");
#endif

            if (typeName.Contains('.') || typeName.Contains('+'))
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: \"{typeName}\" contains '.' or '+', attempting nested resolution");
#endif
                var nestedType = TryResolveNestedClass(typeName);
                if (nestedType != null)
                {
#if DEBUG
                    OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: nested resolution found: {nestedType.FullName}");
#endif
                    return nestedType;
                }
#if DEBUG
                OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: nested resolution returned null");
#endif
            }
#if DEBUG
            else
            {
                OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: \"{typeName}\" has no '.' or '+', skipping nested resolution");
            }
#endif

#if DEBUG
            OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: attempting direct type lookup for \"{typeName}\"");
#endif

            foreach (var assembly in assemblies)
            {
                try
                {
                    var type = assembly.GetType(typeName, false, true);
                    if (type != null)
                    {
#if DEBUG
                        OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: direct lookup found \"{typeName}\" in {assembly.GetName().Name} as {type.FullName}");
#endif
                        return type;
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: direct lookup exception in {assembly.GetName().Name}: {ex.Message}");
#endif
                }
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: direct lookup failed, attempting short name fallback for \"{typeName}\"");
#endif

            foreach (var assembly in assemblies)
            {
                try
                {
                    string shortName = typeName;
                    int dotIndex = typeName.LastIndexOf('.');
                    if (dotIndex >= 0)
                    {
                        shortName = typeName.Substring(dotIndex + 1);
                    }
                    
                    var type = assembly.GetType(shortName, false, true);
                    if (type != null)
                    {
#if DEBUG
                        OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: short name fallback found \"{shortName}\" in {assembly.GetName().Name} as {type.FullName}");
#endif
                        return type;
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: short name fallback exception in {assembly.GetName().Name}: {ex.Message}");
#endif
                }
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: short name fallback failed, attempting full type scan for \"{typeName}\"");
#endif

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (string.Equals(type.Name, typeName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(type.FullName, typeName, StringComparison.OrdinalIgnoreCase))
                        {
#if DEBUG
                            OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: full scan found \"{typeName}\" in {assembly.GetName().Name} as {type.FullName}");
#endif
                            return type;
                        }
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: full scan exception in {assembly.GetName().Name}: {ex.Message}");
#endif
                }
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] FindTypeInAssemblies: FAILED to resolve \"{typeName}\"");
#endif

            return null;
        }

        private static Type TryResolveNestedClass(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

#if DEBUG
            OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: typeName=\"{typeName}\"");
            var assemblyNames = string.Join(", ", assemblies.Select(a => a.GetName().Name).OrderBy(n => n));
            OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: searching in {assemblies.Length} assemblies");
#endif

            var formats = new List<string>();
            formats.Add(typeName);

            if (typeName.Contains('.'))
            {
                var parts = typeName.Split('.');
                
                for (int plusCount = 1; plusCount < parts.Length; plusCount++)
                {
                    var converted = string.Join(".", parts, 0, parts.Length - plusCount) + 
                                   "+" + 
                                   string.Join("+", parts, parts.Length - plusCount, plusCount);
                    if (!formats.Contains(converted))
                        formats.Add(converted);
                }
                
                formats.Add(typeName.Replace(".", "+"));
            }

            if (typeName.Contains('+'))
            {
                var withDots = typeName.Replace("+", ".");
                if (!formats.Contains(withDots))
                    formats.Add(withDots);
                
                var parts = typeName.Split('+');
                for (int dotCount = 1; dotCount < parts.Length; dotCount++)
                {
                    var converted = string.Join("+", parts, 0, parts.Length - dotCount) +
                                   "." +
                                   string.Join(".", parts, parts.Length - dotCount, dotCount);
                    if (!formats.Contains(converted))
                        formats.Add(converted);
                }
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: generated formats: [{string.Join(", ", formats)}]");
#endif

            foreach (var assembly in assemblies)
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: searching {assembly.GetName().Name}...");
#endif
                try
                {
                    foreach (var format in formats)
                    {
                        try
                        {
                            var type = assembly.GetType(format, false, true);
                            if (type != null)
                            {
#if DEBUG
                                OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: SUCCESS! Found \"{format}\" in {assembly.GetName().Name} as {type.FullName}");
#endif
                                return type;
                            }
                        }
                        catch (Exception ex)
                        {
#if DEBUG
                            OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: GetType(\"{format}\") exception in {assembly.GetName().Name}: {ex.Message}");
#endif
                        }
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: outer loop exception in {assembly.GetName().Name}: {ex.Message}");
#endif
                }
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: format-based search failed, attempting full type scan...");
#endif

            foreach (var assembly in assemblies)
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: full scanning {assembly.GetName().Name}...");
#endif
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        string typeFullName = type.FullName;
                        if (typeFullName == null)
                            continue;

                        string lastComponent = typeFullName.Contains('+') 
                            ? typeFullName.Substring(typeFullName.LastIndexOf('+') + 1)
                            : (typeFullName.Contains('.') ? typeFullName.Substring(typeFullName.LastIndexOf('.') + 1) : typeFullName);

                        string inputLastComponent = typeName.Contains('+') 
                            ? typeName.Substring(typeName.LastIndexOf('+') + 1)
                            : (typeName.Contains('.') ? typeName.Substring(typeName.LastIndexOf('.') + 1) : typeName);

                        if (string.Equals(lastComponent, inputLastComponent, StringComparison.OrdinalIgnoreCase))
                        {
#if DEBUG
                            OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: full scan MATCH! \"{inputLastComponent}\" -> {type.FullName} in {assembly.GetName().Name}");
#endif
                            return type;
                        }
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: full scan exception in {assembly.GetName().Name}: {ex.Message}");
#endif
                }
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] TryResolveNestedClass: FAILED to resolve \"{typeName}\"");
#endif

            return null;
        }

        public static bool IsGenericType(string typeString)
        {
            return !string.IsNullOrWhiteSpace(typeString) && 
                   typeString.Contains('<') && 
                   typeString.Contains('>');
        }

        public static bool IsArrayType(string typeString)
        {
            return !string.IsNullOrWhiteSpace(typeString) && 
                   typeString.Contains('[') && 
                   typeString.Contains(']');
        }
    }
}
