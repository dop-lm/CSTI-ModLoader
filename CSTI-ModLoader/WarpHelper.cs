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

        
        public static (FieldInfo field, Func<object, object> getter, Action<object, object> setter) FieldFromCache(
            this Type type, string field_name, bool getter_use = true, bool setter_use = true)
        {
            FieldInfo fieldInfo;
            Func<object, object> getter = null;
            Action<object, object> setter = null;
            if (FieldInfoCache.ContainsKey(type))
            {
                var fieldInfos = FieldInfoCache[type];
                if (fieldInfos.ContainsKey(field_name))
                {
                    fieldInfo = fieldInfos[field_name];
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
                if (FieldGetterDynamicMethodCache.ContainsKey(type))
                {
                    var delegates = FieldGetterDynamicMethodCache[type];
                    if (delegates.ContainsKey(field_name))
                    {
                        getter = delegates[field_name];
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
                if (FieldSetterDynamicMethodCache.ContainsKey(type))
                {
                    var delegates = FieldSetterDynamicMethodCache[type];
                    if (delegates.ContainsKey(field_name))
                    {
                        setter = delegates[field_name];
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
            return dynamicMethod.CreateDelegate(typeof(Func<object, object>)) as Func<object, object>;
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
            return dynamicMethod.CreateDelegate(typeof(Action<object, object>)) as Action<object, object>;
        }

    }
}