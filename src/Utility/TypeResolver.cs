using System;
using UnityEngine;
using OutwardModsCommunicatorMenu;

namespace OutwardModsCommunicatorMenu.Utility
{
    public static class TypeResolver
    {
        public static Type Resolve(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

#if DEBUG
            OMCM.LogMessage($"[DEBUG] TypeResolver.Resolve: ENTRY with typeName=\"{typeName}\"");
#endif

            if (typeName.EndsWith("?"))
            {
                return ResolveNullableType(typeName);
            }

            return ResolveNonNullableType(typeName);
        }

        private static Type ResolveNullableType(string typeName)
        {
            string baseTypeName = typeName.Substring(0, typeName.Length - 1);
            
#if DEBUG
            OMCM.LogMessage($"[DEBUG] TypeResolver.ResolveNullableType: baseTypeName=\"{baseTypeName}\"");
#endif

            Type baseType = ResolveNonNullableType(baseTypeName);

            if (baseType != null && baseType.IsValueType && baseType != typeof(void))
            {
                var nullableType = typeof(Nullable<>).MakeGenericType(baseType);
#if DEBUG
                OMCM.LogMessage($"[DEBUG] TypeResolver.ResolveNullableType: created nullable type: {nullableType.FullName}");
#endif
                return nullableType;
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] TypeResolver.ResolveNullableType: returning baseType: {(baseType != null ? baseType.FullName : "null")}");
#endif
            return baseType;
        }

        private static Type ResolveNonNullableType(string typeName)
        {
            if (TryGetSimpleType(typeName, out var simpleType))
            {
#if DEBUG
                OMCM.LogMessage($"[DEBUG] TypeResolver.ResolveNonNullableType: simpleType hit for \"{typeName}\" -> {simpleType.FullName}");
#endif
                return simpleType;
            }

#if DEBUG
            OMCM.LogMessage($"[DEBUG] TypeResolver.ResolveNonNullableType: delegating to GenericTypeParser.Parse for \"{typeName}\"");
#endif
            var result = GenericTypeParser.Parse(typeName);
#if DEBUG
            OMCM.LogMessage($"[DEBUG] TypeResolver.ResolveNonNullableType: GenericTypeParser.Parse returned: {(result != null ? result.FullName : "null")}");
#endif
            return result;
        }

        private static bool TryGetSimpleType(string typeName, out Type result)
        {
            result = typeName.ToLower() switch
            {
                "string" => typeof(string),
                "int" => typeof(int),
                "bool" => typeof(bool),
                "float" => typeof(float),
                "double" => typeof(double),
                "vector2" => typeof(Vector2),
                "vector3" => typeof(Vector3),
                "vector4" => typeof(Vector4),
                "quaternion" => typeof(Quaternion),
                "color" => typeof(Color),
                _ => null
            };
            return result != null;
        }

        public static string GetExampleInput(Type type)
        {
            return UniverseLib.Utility.ParseUtility.GetExampleInput(type);
        }

        public static bool CanParse(Type type)
        {
            return UniverseLib.Utility.ParseUtility.CanParse(type);
        }
    }
}
