using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LitJson;
using ModLoader.LoaderUtil;
using Unity.Collections.LowLevel.Unsafe;
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
                var (_, _, setter) = obj.GetType().FieldFromCache(field_name,getter_use:false);
                obj.PostSetEnQueue(setter,data);
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
            if (obj is UniqueIDScriptable idScriptable && json.ContainsKey(ExtraData))
            {
                ModLoader.UniqueIdObjectExtraData[idScriptable.UniqueID] = json[ExtraData];
            }
            else if (obj is ScriptableObject scriptableObject && json.ContainsKey(ExtraData))
            {
                ModLoader.ScriptableObjectExtraData[scriptableObject.GetInstanceID()] = json[ExtraData];
            }
            else if (!obj_type.IsValueType && json.ContainsKey(ExtraData))
            {
                ModLoader.ClassObjectExtraData[obj] = json[ExtraData];
            }

            foreach (var key in json.Keys)
            {
                try
                {
                    var keyData = json[key];
                    if (key.EndsWith("WarpType"))
                    {
                        if (!keyData.IsInt || !json.ContainsKey(key.Substring(0, key.Length - 8) + "WarpData"))
                            continue;
                        if ((int) keyData == (int) WarpType.REFERENCE ||
                            (int) keyData == (int) WarpType.ADD_REFERENCE)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var (field, _, _) = obj_type.FieldFromCache(field_name, getter_use: false,
                                setter_use: false);
                            var field_type = field.FieldType;

                            var fieldWarpData = json[field_name + "WarpData"];
                            if (fieldWarpData.IsString)
                            {
                                JsonCommonRefWarpper(obj, fieldWarpData.ToString(), field_name,
                                    field_type, (WarpType) (int) keyData);
                            }
                            else if (fieldWarpData.IsArray)
                            {
                                Type sub_field_type = null;
                                if (field.FieldType.IsGenericType &&
                                    field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
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
                                for (int i = 0; i < fieldWarpData.Count; i++)
                                {
                                    if (fieldWarpData[i].IsString)
                                        list_data.Add(fieldWarpData[i].ToString());
                                    else
                                        ModLoader.LogErrorWithModInfo(
                                            "CommonWarpper REFERENCE Wrong SubWarpData Format " + field_type.Name);
                                }

                                if (list_data.Count != fieldWarpData.Count)
                                    ModLoader.LogErrorWithModInfo(
                                        "CommonWarpper REFERENCE Size Error" + field_type.Name);

                                JsonCommonRefWarpper(obj, list_data, field_name, sub_field_type,
                                    (WarpType) (int) keyData);
                            }
                            else
                            {
                                ModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Wrong WarpData Format " +
                                                              field_type.Name);
                            }
                        }
                        else if ((int) keyData == (int) WarpType.ADD)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var (field, getter, setter) = obj_type.FieldFromCache(field_name);
                            var field_type = field.FieldType;

                            var fieldWarpData = json[field_name + "WarpData"];
                            if (fieldWarpData.IsArray)
                            {
                                Type sub_field_type;
                                if (field.FieldType.IsGenericType &&
                                    field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    sub_field_type = field.FieldType.GetGenericArguments().Single();
                                    var instance = getter(obj) as IList;
                                    for (int i = 0; i < fieldWarpData.Count; i++)
                                    {
                                        if (fieldWarpData[i].IsObject)
                                        {
                                            var new_obj = sub_field_type.IsSubclassOf(typeof(ScriptableObject))
                                                ? ScriptableObject.CreateInstance(sub_field_type)
                                                : sub_field_type.ConstructorFromCache()();
                                            JsonUtility.FromJsonOverwrite(fieldWarpData[i].ToJson(),
                                                new_obj);
                                            JsonCommonWarpper(new_obj, fieldWarpData[i]);
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
                                    ArrayResize(ref instance, fieldWarpData.Count + instance.Length);
                                    for (int i = 0; i < fieldWarpData.Count; i++)
                                    {
                                        if (fieldWarpData[i].IsObject)
                                        {
                                            var new_obj = sub_field_type.IsSubclassOf(typeof(ScriptableObject))
                                                ? ScriptableObject.CreateInstance(sub_field_type)
                                                : sub_field_type.ConstructorFromCache()();
                                            JsonUtility.FromJsonOverwrite(fieldWarpData[i].ToJson(),
                                                new_obj);
                                            JsonCommonWarpper(new_obj, fieldWarpData[i]);
                                            instance.SetValue(new_obj, i + start_idx);
                                        }
                                        else
                                            ModLoader.LogErrorWithModInfo(
                                                "CommonWarpper ADD Wrong SubWarpData Format " + field_type.Name);
                                    }

                                    setter(obj, instance);
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
                        else if ((int) keyData == (int) WarpType.MODIFY)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var (field, getter, setter) = obj_type.FieldFromCache(field_name);
                            var field_type = field.FieldType;

                            var fieldWarpData = json[field_name + "WarpData"];
                            if (fieldWarpData.IsObject)
                            {
                                var target_obj = getter(obj);
                                JsonCommonWarpper(target_obj, fieldWarpData);
                                setter(obj, target_obj);
                            }
                            else if (fieldWarpData.IsArray)
                            {
                                if (field.FieldType.IsGenericType &&
                                    field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    var instance = getter(obj) as IList;
                                    for (int i = 0; i < fieldWarpData.Count; i++)
                                    {
                                        if (fieldWarpData[i].IsObject)
                                        {
                                            var target_obj = instance[i];
                                            JsonCommonWarpper(target_obj, fieldWarpData[i]);
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
                                    for (int i = 0; i < fieldWarpData.Count; i++)
                                    {
                                        if (fieldWarpData[i].IsObject)
                                        {
                                            object target_obj = null;
                                            try
                                            {
                                                target_obj = instance.GetValue(i);
                                            }
                                            catch (Exception e)
                                            {
                                                var id = "NullId";
                                                if (obj is UniqueIDScriptable uniqueIDScriptable)
                                                {
                                                    id = uniqueIDScriptable.UniqueID;
                                                }
                                                else if (obj is ScriptableObject scriptableObject)
                                                {
                                                    id = scriptableObject.name;
                                                }
                                                else if (obj is CardAction cardAction)
                                                {
                                                    id = cardAction.ActionName.ParentObjectID;
                                                }

                                                Debug.LogWarning($"On access {id}::{obj_type}.{field_name} : {e}");
                                            }

                                            JsonCommonWarpper(target_obj, fieldWarpData[i]);
                                            instance.SetValue(target_obj, i);
                                        }
                                        else
                                            ModLoader.LogErrorWithModInfo(
                                                "CommonWarpper MODIFY Wrong SubWarpData Format " + field_type.Name);
                                    }

                                    setter(obj, instance);
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
                        if (keyData.IsObject)
                        {
                            var field_name = key;
                            var (field, getter, setter) = obj_type.FieldFromCache(field_name);
                            if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                continue;
                            var sub_obj = getter(obj);
                            JsonCommonWarpper(sub_obj, keyData);
                            setter(obj, sub_obj);
                        }
                        else if (keyData.IsArray)
                        {
                            var field_name = key;
                            var (field, getter, setter) = obj_type.FieldFromCache(field_name);

                            for (int i = 0; i < keyData.Count; i++)
                            {
                                if (keyData[i].IsObject)
                                {
                                    if (field.FieldType.IsGenericType &&
                                        field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                                    {
                                        // var ele_type = field.FieldType.GetGenericArguments().Single();
                                        if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                            break;
                                        var list = getter(obj) as IList;
                                        var ele = list[i];
                                        if (ele == null)
                                            continue;
                                        JsonCommonWarpper(ele, keyData[i]);
                                        list[i] = ele;
                                        setter(obj, list);
                                    }
                                    else if (field.FieldType.IsArray)
                                    {
                                        // var ele_type = field.FieldType.GetElementType();
                                        if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                            break;
                                        var array = getter(obj) as Array;
                                        object ele = null;
                                        try
                                        {
                                            ele = array.GetValue(i);
                                        }
                                        catch (Exception e)
                                        {
                                            var id = "NullId";
                                            if (obj is UniqueIDScriptable uniqueIDScriptable)
                                            {
                                                id = uniqueIDScriptable.UniqueID;
                                            }
                                            else if (obj is ScriptableObject scriptableObject)
                                            {
                                                id = scriptableObject.name;
                                            }

                                            Debug.LogWarning($"On access {id}::{obj_type}.{field_name} : {e}");
                                        }

                                        if (ele == null)
                                            continue;
                                        JsonCommonWarpper(ele, keyData[i]);
                                        array.SetValue(ele, i);
                                        setter(obj, array);
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

        private static readonly string ExtraData = "额外数据ExtraData";

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
                    var (_, _, setter) = obj.GetType().FieldFromCache(field_name, getter_use: false);
                    setter(obj, ele);
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
                var (field, getter, setter) = obj.GetType().FieldFromCache(field_name);
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var instance = getter(obj) as IList;
                    foreach (var name in data)
                        if (dict.TryGetValue(name, out var ele))
                            instance?.Add(ele);
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = getter(obj) as Array;
                    ArrayResize(ref instance, data.Count);
                    for (int i = 0; i < data.Count; i++)
                        if (dict.TryGetValue(data[i], out var ele))
                            instance.SetValue(ele, i);
                    setter(obj, instance);
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
                var (field, getter, setter) = obj.GetType().FieldFromCache(field_name);
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
                    setter(obj, instance);
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