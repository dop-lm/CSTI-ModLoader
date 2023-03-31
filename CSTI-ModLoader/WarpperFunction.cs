using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using LitJson;
using UnityEngine;
using Object = System.Object;

namespace ModLoader
{
    public class WarpperFunction
    {
        public enum WarpType
        {
            NONE,
            COPY,
            CUSTOM,
            REFERENCE,
            ADD,
            MODIFY,
            ADD_REFERENCE
        }

        public static void JsonCommonRefWarpper(Object obj, string data, string field_name, Type field_type,
            WarpType warp_type = WarpType.REFERENCE)
        {
            if (field_type.IsSubclassOf(typeof(UniqueIDScriptable)))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
            }
            else if (field_type.IsSubclassOf(typeof(ScriptableObject)))
            {
                if (ModLoader.AllScriptableObjectWithoutGuidTypeDict.TryGetValue(field_type, out var type_dict))
                    ObjectReferenceWarpper(obj, data, field_name, type_dict);
                else
                    ModLoader.LogErrorWithModInfo("CommonWarpper No Such Dict " + field_type.Name);
            }
            else if (field_type == typeof(Sprite))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
            }
            else if (field_type == typeof(AudioClip))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.AudioClipDict);
            }
            else if (field_type == typeof(WeatherSpecialEffect) ||
                     field_type.IsSubclassOf(typeof(WeatherSpecialEffect)))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.WeatherSpecialEffectDict);
            }
            else if (field_type == typeof(ScriptableObject))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllScriptableObjectDict);
            }
            else
            {
                ModLoader.LogErrorWithModInfo("JsonCommonRefWarpper Unexpect Object Type " + field_type.Name);
            }
        }

        public static void JsonCommonRefWarpper(Object obj, List<string> data, string field_name,
            Type field_type, WarpType warp_type = WarpType.REFERENCE)
        {
            if (field_type.IsSubclassOf(typeof(UniqueIDScriptable)))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
            }
            else if (field_type.IsSubclassOf(typeof(ScriptableObject)))
            {
                if (ModLoader.AllScriptableObjectWithoutGuidTypeDict.TryGetValue(field_type, out var type_dict))
                {
                    if (warp_type == WarpType.ADD_REFERENCE)
                        ObjectAddReferenceWarpper(obj, data, field_name, type_dict);
                    else
                        ObjectReferenceWarpper(obj, data, field_name, type_dict);
                }
                else
                    ModLoader.LogErrorWithModInfo("CommonWarpper No Such Dict " + field_type.Name);
            }
            else if (field_type == typeof(Sprite))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
            }
            else if (field_type == typeof(AudioClip))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, ModLoader.AudioClipDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, ModLoader.AudioClipDict);
            }
            else if (field_type == typeof(WeatherSpecialEffect) ||
                     field_type.IsSubclassOf(typeof(WeatherSpecialEffect)))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.WeatherSpecialEffectDict);
            }
            else if (field_type == typeof(ScriptableObject))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, ModLoader.AllScriptableObjectDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllScriptableObjectDict);
            }
            else
            {
                ModLoader.LogErrorWithModInfo("JsonCommonRefWarpper Unexpect List Object Type " + field_type.Name);
            }
        }

        public static void JsonCommonWarpper(Object obj, JsonData json)
        {
            if (!json.IsObject)
                return;

            var obj_type = obj.GetType();
            foreach (var key in json.Keys)
            {
                try
                {
                    if (key.EndsWith("WarpType"))
                    {
                        if (!json[key].IsInt || !json.ContainsKey(key.Substring(0, key.Length - 8) + "WarpData"))
                            continue;
                        if ((int) json[key] == (int) WarpType.REFERENCE ||
                            (int) json[key] == (int) WarpType.ADD_REFERENCE)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var (field, _, _) = FieldFromCache(obj_type, field_name, getter_use: false,
                                setter_use: false);
                            var field_type = field.FieldType;

                            if (json[field_name + "WarpData"].IsString)
                            {
                                JsonCommonRefWarpper(obj, json[field_name + "WarpData"].ToString(), field_name,
                                    field_type, (WarpType) (int) json[key]);
                            }
                            else if (json[field_name + "WarpData"].IsArray)
                            {
                                Type sub_field_type = null;
                                if (field.FieldType.IsGenericType &&
                                    (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                                {
                                    sub_field_type = field.FieldType.GetGenericArguments().Single();
                                }
                                else if (field.FieldType.IsArray)
                                {
                                    sub_field_type = field.FieldType.GetElementType();
                                }
                                else
                                {
                                    ModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Must be list or array " +
                                                                  field_type.Name);
                                }

                                List<string> list_data = new List<string>();
                                for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                {
                                    if (json[field_name + "WarpData"][i].IsString)
                                        list_data.Add(json[field_name + "WarpData"][i].ToString());
                                    else
                                        ModLoader.LogErrorWithModInfo(
                                            "CommonWarpper REFERENCE Wrong SubWarpData Format " + field_type.Name);
                                }

                                if (list_data.Count != json[field_name + "WarpData"].Count)
                                    ModLoader.LogErrorWithModInfo(
                                        "CommonWarpper REFERENCE Size Error" + field_type.Name);

                                JsonCommonRefWarpper(obj, list_data, field_name, sub_field_type,
                                    (WarpType) (int) json[key]);
                            }
                            else
                            {
                                ModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Wrong WarpData Format " +
                                                              field_type.Name);
                            }
                        }
                        else if ((int) json[key] == (int) WarpType.ADD)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var (field, getter, setter) = FieldFromCache(obj_type, field_name);
                            var field_type = field.FieldType;

                            if (json[field_name + "WarpData"].IsArray)
                            {
                                Type sub_field_type;
                                if (field.FieldType.IsGenericType &&
                                    field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    sub_field_type = field.FieldType.GetGenericArguments().Single();
                                    var instance = getter(obj) as IList;
                                    for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                    {
                                        if (json[field_name + "WarpData"][i].IsObject)
                                        {
                                            var new_obj = Activator.CreateInstance(sub_field_type);
                                            JsonUtility.FromJsonOverwrite(json[field_name + "WarpData"][i].ToJson(),
                                                new_obj);
                                            JsonCommonWarpper(new_obj, json[field_name + "WarpData"][i]);
                                            var temp_obj = new[] {new_obj};
                                            instance.Add(new_obj);
                                        }
                                        else
                                            ModLoader.LogErrorWithModInfo(
                                                "CommonWarpper ADD Wrong SubWarpData Format " + field_type.Name);
                                    }
                                }
                                else if (field.FieldType.IsArray)
                                {
                                    sub_field_type = field.FieldType.GetElementType();
                                    var instance = getter(obj) as Array;
                                    int start_idx = instance.Length;
                                    ArrayResize(ref instance, json[field_name + "WarpData"].Count + instance.Length);
                                    for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                    {
                                        if (json[field_name + "WarpData"][i].IsObject)
                                        {
                                            var new_obj = Activator.CreateInstance(sub_field_type);
                                            JsonWriter jw = new JsonWriter();
                                            json[field_name + "WarpData"][i].ToJson(jw);
                                            JsonUtility.FromJsonOverwrite(jw.TextWriter.ToString(), new_obj);
                                            JsonCommonWarpper(new_obj, json[field_name + "WarpData"][i]);
                                            instance.SetValue(new_obj, i + start_idx);
                                        }
                                        else
                                            ModLoader.LogErrorWithModInfo(
                                                "CommonWarpper ADD Wrong SubWarpData Format " + field_type.Name);
                                    }

                                    if (obj_type.IsValueType)
                                    {
                                        field.SetValue(obj, instance);
                                    }
                                    else
                                    {
                                        setter(obj, instance);
                                    }
                                }
                                else
                                {
                                    ModLoader.LogErrorWithModInfo("CommonWarpper ADD Must be list or array " +
                                                                  field_type.Name);
                                }
                            }
                            else
                            {
                                ModLoader.LogErrorWithModInfo("CommonWarpper ADD Wrong WarpData Format " +
                                                              field_type.Name);
                            }
                        }
                        else if ((int) json[key] == (int) WarpType.MODIFY)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var (field, getter, setter) = FieldFromCache(obj_type, field_name);
                            var field_type = field.FieldType;

                            if (json[field_name + "WarpData"].IsObject)
                            {
                                var target_obj = getter(obj);
                                JsonCommonWarpper(target_obj, json[field_name + "WarpData"]);
                                if (obj_type.IsValueType)
                                {
                                    field.SetValue(obj, target_obj);
                                }
                                else
                                {
                                    setter(obj, target_obj);
                                }
                            }
                            else if (json[field_name + "WarpData"].IsArray)
                            {
                                if (field.FieldType.IsGenericType &&
                                    (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                                {
                                    var instance = getter(obj) as IList;
                                    for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                    {
                                        if (json[field_name + "WarpData"][i].IsObject)
                                        {
                                            var target_obj = instance[i];
                                            JsonCommonWarpper(target_obj, json[field_name + "WarpData"][i]);
                                            instance[i] = target_obj;
                                        }
                                        else
                                            ModLoader.LogErrorWithModInfo(
                                                "CommonWarpper MODIFY Wrong SubWarpData Format " + field_type.Name);
                                    }
                                }
                                else if (field.FieldType.IsArray)
                                {
                                    var instance = getter(obj) as Array;
                                    for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                    {
                                        if (json[field_name + "WarpData"][i].IsObject)
                                        {
                                            var target_obj = instance.GetValue(i);
                                            JsonCommonWarpper(target_obj, json[field_name + "WarpData"][i]);
                                            instance.SetValue(target_obj, i);
                                        }
                                        else
                                            ModLoader.LogErrorWithModInfo(
                                                "CommonWarpper MODIFY Wrong SubWarpData Format " + field_type.Name);
                                    }

                                    if (obj_type.IsValueType)
                                    {
                                        field.SetValue(obj, instance);
                                    }
                                    else
                                    {
                                        setter(obj, instance);
                                    }
                                }
                                else
                                {
                                    ModLoader.LogErrorWithModInfo("CommonWarpper MODIFY Must be list or array " +
                                                                  field_type.Name);
                                }
                            }
                            else
                            {
                                ModLoader.LogErrorWithModInfo("CommonWarpper MODIFY Wrong WarpData Format " +
                                                              field_type.Name);
                            }
                        }
                        else
                        {
                            ModLoader.LogErrorWithModInfo("CommonWarpper Unexpect WarpType");
                        }
                    }
                    else if (key.EndsWith("WarpData"))
                        continue;
                    else
                    {
                        if ((json[key].IsObject))
                        {
                            var field_name = key;
                            var (field, getter, setter) = FieldFromCache(obj_type, field_name);
                            if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                continue;
                            var sub_obj = getter(obj);
                            JsonCommonWarpper(sub_obj, json[key]);
                            if (obj_type.IsValueType)
                            {
                                field.SetValue(obj, sub_obj);
                            }
                            else
                            {
                                setter(obj, sub_obj);
                            }
                        }
                        else if (json[key].IsArray)
                        {
                            var field_name = key;
                            var (field, getter, setter) = FieldFromCache(obj_type, field_name);

                            for (int i = 0; i < json[key].Count; i++)
                            {
                                if (json[key][i].IsObject)
                                {
                                    if (field.FieldType.IsGenericType &&
                                        (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                                    {
                                        // var ele_type = field.FieldType.GetGenericArguments().Single();
                                        if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                            break;
                                        var list = getter(obj) as IList;
                                        var ele = list[i];
                                        if (ele == null)
                                            continue;
                                        JsonCommonWarpper(ele, json[key][i]);
                                        list[i] = ele;
                                        if (obj_type.IsValueType)
                                        {
                                            field.SetValue(obj, list);
                                        }
                                        else
                                        {
                                            setter(obj, list);
                                        }
                                    }
                                    else if (field.FieldType.IsArray)
                                    {
                                        // var ele_type = field.FieldType.GetElementType();
                                        if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                            break;
                                        var array = getter(obj) as Array;
                                        var ele = array.GetValue(i);
                                        if (ele == null)
                                            continue;
                                        JsonCommonWarpper(ele, json[key][i]);
                                        array.SetValue(ele, i);
                                        if (obj_type.IsValueType)
                                        {
                                            field.SetValue(obj, array);
                                        }
                                        else
                                        {
                                            setter(obj, array);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModLoader.LogErrorWithModInfo(string.Format("CommonWarpper {0} {1}", obj_type.Name, ex.Message));
                }
            }
        }

        public static void ObjectReferenceWarpper<TValueType>(Object obj, string data, string field_name,
            Dictionary<string, TValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    ModLoader.LogErrorWithModInfo("ObjectReferenceWarpper Object IsNotClass");
            //    return;
            //}
            if (dict.TryGetValue(data, out var ele))
            {
                try
                {
                    var (field, _, setter) = FieldFromCache(obj.GetType(), field_name, getter_use: false);
                    if (obj.GetType().IsValueType)
                    {
                        field.SetValue(obj, ele);
                    }
                    else
                    {
                        setter(obj, ele);
                    }
                }
                catch (Exception ex)
                {
                    ModLoader.LogErrorWithModInfo(string.Format("ObjectReferenceWarpper {0}.{1} {2}",
                        obj.GetType().Name, field_name, ex.Message));
                }
            }
        }

        public static void ObjectReferenceWarpper<TValueType>(Object obj, List<string> data, string field_name,
            Dictionary<string, TValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    ModLoader.LogErrorWithModInfo("ObjectReferenceWarpper Object IsNotClass");
            //    return;
            //}
            try
            {
                var (field, getter, setter) = FieldFromCache(obj.GetType(), field_name);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = getter(obj) as IList;
                    foreach (var name in data)
                        if (dict.TryGetValue(name, out var ele))
                            instance.Add(ele);
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = getter(obj) as Array;
                    ArrayResize(ref instance, data.Count);
                    for (int i = 0; i < data.Count; i++)
                        if (dict.TryGetValue(data[i], out var ele))
                            instance.SetValue(ele, i);
                    if (obj.GetType().IsValueType)
                    {
                        field.SetValue(obj, instance);
                    }
                    else
                    {
                        setter(obj, instance);
                    }
                }
            }
            catch (Exception ex)
            {
                ModLoader.LogErrorWithModInfo(string.Format("ObjectReferenceWarpper {0}.{1} {2}", obj.GetType().Name,
                    field_name, ex.Message));
            }
        }

        public static void ObjectAddReferenceWarpper<TValueType>(Object obj, string data, string field_name,
            Dictionary<string, TValueType> dict)
        {
            ModLoader.LogErrorWithModInfo(string.Format("ObjectAddReferenceWarpper {0}.{1} {2}", obj.GetType().Name,
                field_name, "AddReferenceWarpper Only Vaild in List or Array Filed"));
        }

        public static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldInfoCache =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        public static readonly Dictionary<Type, Dictionary<string, Func<object, object>>>
            FieldGetterDynamicMethodCache =
                new Dictionary<Type, Dictionary<string, Func<object, object>>>();

        public static readonly Dictionary<Type, Dictionary<string, Action<object, object>>>
            FieldSetterDynamicMethodCache =
                new Dictionary<Type, Dictionary<string, Action<object, object>>>();

        public static (FieldInfo field, Func<object, object> getter, Action<object, object> setter) FieldFromCache(
            Type type, string field_name, bool getter_use = true, bool setter_use = true)
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

            if (fieldInfo!=null&&getter_use)
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

            if (fieldInfo!=null&&setter_use)
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

            var field = dynamicMethod.CreateDelegate(typeof(Func<object, object>)) as Func<object, object>;
            return field;
        }

        public static Action<object, object> GenSetter(FieldInfo fieldInfo, Type type)
        {
            if (type.IsValueType)
            {
                return null;
            }

            var objType = typeof(object);
            var fieldInfoFieldType = fieldInfo.FieldType;
            var arg1 = Expression.Parameter(objType, "arg1");
            var arg2 = Expression.Parameter(objType, "arg2");
            return Expression.Lambda<Action<object, object>>(Expression
                        .Assign(
                            Expression.Field(
                                !type.IsValueType ? Expression.TypeAs(arg1, type) : Expression.Convert(arg1, type),
                                fieldInfo),
                            !fieldInfoFieldType.IsValueType
                                ? Expression.TypeAs(arg2, fieldInfoFieldType)
                                : Expression.Convert(arg2, fieldInfoFieldType)),
                    arg1,
                    arg2)
                .Compile();
        }

        public static void ObjectAddReferenceWarpper<TValueType>(Object obj, List<string> data,
            string field_name, Dictionary<string, TValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    ModLoader.LogErrorWithModInfo("ObjectAddReferenceWarpper Object IsNotClass");
            //    return;
            //}
            try
            {
                var (field, getter, setter) = FieldFromCache(obj.GetType(), field_name);
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var instance = getter(obj) as IList;
                    foreach (var name in data)
                        if (dict.TryGetValue(name, out var ele))
                            instance.Add(ele);
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = getter(obj) as Array;
                    int start_idx = instance.Length;
                    ArrayResize(ref instance, data.Count + instance.Length);
                    for (int i = 0; i < data.Count; i++)
                        if (dict.TryGetValue(data[i], out var ele))
                            instance.SetValue(ele, i + start_idx);
                    if (obj.GetType().IsValueType)
                    {
                        field.SetValue(obj, instance);
                    }
                    else
                    {
                        setter(obj, instance);
                    }
                }
            }
            catch (Exception ex)
            {
                ModLoader.LogErrorWithModInfo(string.Format("ObjectAddReferenceWarpper {0}.{1} {2}", obj.GetType().Name,
                    field_name, ex.Message));
            }
        }

        public static void ArrayResize(ref Array array, int newSize)
        {
            Type elementType = array.GetType().GetElementType();
            Array newArray = Array.CreateInstance(elementType, newSize);
            Array.Copy(array, newArray, Math.Min(array.Length, newArray.Length));
            array = newArray;
        }
    }
}