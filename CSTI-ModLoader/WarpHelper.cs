using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ModLoader
{
    public static class WarpHelper
    {
        public static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldInfoCache =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        public static readonly Dictionary<Type, Dictionary<string, Func<object, object>>>
            FieldGetterDynamicMethodCache =
                new Dictionary<Type, Dictionary<string, Func<object, object>>>();

        public static readonly Dictionary<Type, Dictionary<string, Action<object, object>>>
            FieldSetterDynamicMethodCache =
                new Dictionary<Type, Dictionary<string, Action<object, object>>>();

        public static readonly Dictionary<Type, Func<object>> ClassConstructorCache =
            new Dictionary<Type, Func<object>>();

        public static Func<object> ConstructorFromCache(this Type type)
        {
            if (ClassConstructorCache.TryGetValue(type, out var cache))
            {
                return cache;
            }

            if (type.IsValueType)
            {
                ClassConstructorCache[type] = CreateConstructor(type, null);
                return ClassConstructorCache[type];
            }

            ClassConstructorCache[type] = CreateConstructor(type,
                AccessTools.FirstConstructor(type, info => info.GetParameters().Length == 0));
            return ClassConstructorCache[type];
        }

        public static Func<object> CreateConstructor(Type type, ConstructorInfo constructorInfo)
        {
            var dynamicMethod = new DynamicMethod("createInstance", typeof(object), Type.EmptyTypes);
            var ilGenerator = dynamicMethod.GetILGenerator();
            if (!type.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
                ilGenerator.Emit(OpCodes.Ret);
            }
            else
            {
                var declareLocal = ilGenerator.DeclareLocal(type);
                ilGenerator.Emit(OpCodes.Ldloc_S, declareLocal);
                ilGenerator.Emit(OpCodes.Box, type);
                ilGenerator.Emit(OpCodes.Ret);
            }

            return dynamicMethod.CreateDelegate(typeof(Func<object>)) as Func<object>;
        }

        public static (FieldInfo field, Func<object, object> getter, Action<object, object> setter) FieldFromCache(
            this Type type, string field_name, bool getter_use = true, bool setter_use = true)
        {
            FieldInfo fieldInfo;
            Func<object, object> getter = null;
            Action<object, object> setter = null;
            if (FieldInfoCache.TryGetValue(type, out var fieldInfos))
            {
                if (fieldInfos.TryGetValue(field_name, out var info))
                {
                    fieldInfo = info;
                }
                else
                {
                    fieldInfo = AccessTools.Field(type, field_name);
                    fieldInfos[field_name] = fieldInfo;
                }
            }
            else
            {
                fieldInfo = AccessTools.Field(type, field_name);
                FieldInfoCache[type] = new Dictionary<string, FieldInfo> {{field_name, fieldInfo}};
            }

            if (fieldInfo != null && getter_use)
            {
                if (FieldGetterDynamicMethodCache.TryGetValue(type, out var delegates))
                {
                    if (delegates.TryGetValue(field_name, out var getFunc))
                    {
                        getter = getFunc;
                    }
                    else
                    {
                        getter = GenGetter(fieldInfo, type);
                        delegates[field_name] = getter;
                    }
                }
                else
                {
                    getter = GenGetter(fieldInfo, type);
                    FieldGetterDynamicMethodCache[type] = new Dictionary<string, Func<object, object>>
                        {{field_name, getter}};
                }
            }

            if (fieldInfo != null && setter_use)
            {
                if (FieldSetterDynamicMethodCache.TryGetValue(type, out var delegates))
                {
                    if (delegates.TryGetValue(field_name, out var setFunc))
                    {
                        setter = setFunc;
                    }
                    else
                    {
                        setter = GenSetter(fieldInfo, type);
                        delegates[field_name] = setter;
                    }
                }
                else
                {
                    setter = GenSetter(fieldInfo, type);
                    FieldSetterDynamicMethodCache[type] = new Dictionary<string, Action<object, object>>
                        {{field_name, setter}};
                }
            }

            return (fieldInfo, getter, setter);
        }

        public static Func<object, object> GenGetter(FieldInfo fieldInfo, Type type)
        {
            var fieldInfoFieldType = fieldInfo.FieldType;
            var objType = typeof(object);
            var dynamicMethod = new DynamicMethod("simple_getter", objType, new[] {objType}, true);
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(type.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, type);
            ilGenerator.Emit(OpCodes.Ldfld, fieldInfo);
            if (fieldInfoFieldType.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Box, fieldInfoFieldType);
            }

            ilGenerator.Emit(OpCodes.Ret);
            var getter = dynamicMethod.CreateDelegate(typeof(Func<object, object>)) as Func<object, object>;
            return getter;
        }

        public static Action<object, object> GenSetter(FieldInfo fieldInfo, Type type)
        {
            var fieldInfoFieldType = fieldInfo.FieldType;
            var objType = typeof(object);
            var dynamicMethod = new DynamicMethod("simple_setter", typeof(void), new[] {objType, objType}, true);
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(type.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, type);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(fieldInfoFieldType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass,
                fieldInfoFieldType);
            ilGenerator.Emit(OpCodes.Stfld, fieldInfo);
            ilGenerator.Emit(OpCodes.Ret);
            var setter = dynamicMethod.CreateDelegate(typeof(Action<object, object>)) as Action<object, object>;
            return setter;
        }

        public static void Deconstruct<TKey, TVal>(this KeyValuePair<TKey, TVal> keyValuePair, out TKey key,
            out TVal val)
        {
            key = keyValuePair.Key;
            val = keyValuePair.Value;
        }
    }
}