using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace ModLoader.LoaderUtil
{
    public static class CheckCanBeLoad
    {
        public static readonly Dictionary<Type, bool> CheckResultCache = new Dictionary<Type, bool>();
        public static readonly Type ScriptObjType = typeof(ScriptableObject);
        public static readonly Type SerializeFieldType = typeof(SerializeField);
        public static readonly Type NonSerializedAttributeType = typeof(NonSerializedAttribute);
        public static readonly Type IDictionaryType = typeof(IDictionary);
        public static readonly Type IListType = typeof(IList);
        public static readonly string FullManagedPath = Path.GetFullPath(Paths.ManagedPath);
        public static readonly List<Assembly> GameAssemblies;

        static CheckCanBeLoad()
        {
            GameAssemblies = AccessTools.AllAssemblies().Where(assembly =>
                    Path.GetFullPath(Path.GetDirectoryName(assembly.Location) ?? Paths.ConfigPath) == FullManagedPath)
                .ToList();
        }

        private static HashSet<Type> CheckingTypes;

        public static bool CheckCanUnityLoad(this Type type)
        {
            CheckingTypes = new HashSet<Type>();
            var innerCheckCanUnityLoad = InnerCheckCanUnityLoad(type);
            CheckingTypes = null;
            return innerCheckCanUnityLoad;
        }

        private static bool InnerCheckCanUnityLoad(Type type)
        {
            if (CheckResultCache.TryGetValue(type, out var load))
            {
                return load;
            }

            if (type.IsSubclassOf(IDictionaryType))
            {
                return false;
            }

            if (type.IsArray)
            {
                return InnerCheckCanUnityLoad(type.GetElementType());
            }

            if (type.IsSubclassOf(IListType))
            {
                return InnerCheckCanUnityLoad(type.GenericTypeArguments[0]);
            }

            if (CheckingTypes.Contains(type))
            {
                return true;
            }

            if (type.IsSubclassOf(ScriptObjType))
            {
                CheckResultCache[type] = true;
                return true;
            }

            if (GameAssemblies.Any(assembly => assembly.DefinedTypes.Contains(type)))
            {
                CheckingTypes.Add(type);
                var result = AccessTools.GetDeclaredFields(type).Where(IsSerializeField).Select(info => info.FieldType)
                    .All(InnerCheckCanUnityLoad);
                CheckResultCache[type] = result;
                return result;
            }

            CheckResultCache[type] = false;
            return false;
        }

        private static bool IsSerializeField(FieldInfo fieldInfo)
        {
            return (fieldInfo.IsPublic &&
                    fieldInfo.CustomAttributes.All(data => data.AttributeType != NonSerializedAttributeType)) ||
                   fieldInfo.CustomAttributes.Any(data => data.AttributeType == SerializeFieldType);
        }
    }
}