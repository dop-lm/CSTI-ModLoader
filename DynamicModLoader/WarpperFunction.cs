using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections;
using LitJson;

namespace DynamicModLoader
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

        public static void JsonCommonRefWarpper(System.Object obj, string data, string field_name, Type field_type, WarpType warp_type = WarpType.REFERENCE)
        {
            if (field_type.IsSubclassOf(typeof(UniqueIDScriptable)))
            {
                ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.AllGUIDDict);
            }
            else if (field_type.IsSubclassOf(typeof(ScriptableObject)))
            {
                if (DynamicModLoader.AllScriptableObjectWithoutGuidTypeDict.TryGetValue(field_type, out var type_dict))
                    ObjectReferenceWarpper(obj, data, field_name, type_dict);
                else
                    DynamicModLoader.LogErrorWithModInfo("CommonWarpper No Such Dict " + field_type.Name);
            }
            else if (field_type == typeof(UnityEngine.Sprite))
            {

                ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.SpriteDict);
            }
            else if (field_type == typeof(UnityEngine.AudioClip))
            {
                ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.AudioClipDict);
            }
            else if (field_type == typeof(WeatherSpecialEffect) || field_type.IsSubclassOf(typeof(WeatherSpecialEffect)))
            {
                ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.WeatherSpecialEffectDict);
            }
            else if (field_type == typeof(ScriptableObject))
            {
                ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.AllScriptableObjectDict);
            }
            else
            {
                DynamicModLoader.LogErrorWithModInfo("JsonCommonRefWarpper Unexpect Object Type " + field_type.Name);
            }
        }

        public static void JsonCommonRefWarpper(System.Object obj, List<string> data, string field_name, Type field_type, WarpType warp_type = WarpType.REFERENCE)
        {
            if (field_type.IsSubclassOf(typeof(UniqueIDScriptable)))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, DynamicModLoader.AllGUIDDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.AllGUIDDict);
            }
            else if (field_type.IsSubclassOf(typeof(ScriptableObject)))
            {
                if (DynamicModLoader.AllScriptableObjectWithoutGuidTypeDict.TryGetValue(field_type, out var type_dict))
                {
                    if (warp_type == WarpType.ADD_REFERENCE)
                        ObjectAddReferenceWarpper(obj, data, field_name, type_dict);
                    else
                        ObjectReferenceWarpper(obj, data, field_name, type_dict);
                }
                else
                    DynamicModLoader.LogErrorWithModInfo("CommonWarpper No Such Dict " + field_type.Name);
            }
            else if (field_type == typeof(UnityEngine.Sprite))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, DynamicModLoader.SpriteDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.SpriteDict);
            }
            else if (field_type == typeof(UnityEngine.AudioClip))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, DynamicModLoader.AudioClipDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.AudioClipDict);
            }
            else if (field_type == typeof(WeatherSpecialEffect) || field_type.IsSubclassOf(typeof(WeatherSpecialEffect)))
            {
                ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.WeatherSpecialEffectDict);
            }
            else if (field_type == typeof(ScriptableObject))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, DynamicModLoader.AllScriptableObjectDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, DynamicModLoader.AllScriptableObjectDict);
            }
            else
            {
                DynamicModLoader.LogErrorWithModInfo("JsonCommonRefWarpper Unexpect List Object Type " + field_type.Name);
            }
        }

        public static void JsonCommonWarpper(System.Object obj, LitJson.JsonData json)
        {
            if (!json.IsObject)
                return;

            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var obj_type = obj.GetType();
            foreach (var key in json.Keys)
            {
                try
                {
                    if (key.EndsWith("WarpType"))
                    {
                        if (!json[key].IsInt || !json.ContainsKey(key.Substring(0, key.Length - 8) + "WarpData"))
                            continue;
                        if ((int)json[key] == (int)WarpType.REFERENCE || (int)json[key] == (int)WarpType.ADD_REFERENCE)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var field = obj_type.GetField(field_name, bindingFlags);
                            var field_type = field.FieldType;

                            if (json[field_name + "WarpData"].IsString)
                            {
                                JsonCommonRefWarpper(obj, json[field_name + "WarpData"].ToString(), field_name, field_type, (WarpType)(int)json[key]);
                            }
                            else if (json[field_name + "WarpData"].IsArray)
                            {
                                Type sub_field_type = null;
                                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                                {
                                    sub_field_type = field.FieldType.GetGenericArguments().Single();
                                }
                                else if (field.FieldType.IsArray)
                                {
                                    sub_field_type = field.FieldType.GetElementType();
                                }
                                else
                                {
                                    DynamicModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Must be list or array " + field_type.Name);
                                }

                                List<string> list_data = new List<string>();
                                for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                {
                                    if (json[field_name + "WarpData"][i].IsString)
                                        list_data.Add(json[field_name + "WarpData"][i].ToString());
                                    else
                                        DynamicModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Wrong SubWarpData Format " + field_type.Name);
                                }

                                if (list_data.Count != json[field_name + "WarpData"].Count)
                                    DynamicModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Size Error" + field_type.Name);

                                JsonCommonRefWarpper(obj, list_data, field_name, sub_field_type, (WarpType)(int)json[key]);
                            }
                            else
                            {
                                DynamicModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Wrong WarpData Format " + field_type.Name);
                            }
                        }
                        else if ((int)json[key] == (int)WarpType.ADD)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var field = obj_type.GetField(field_name, bindingFlags);
                            var field_type = field.FieldType;

                            if (json[field_name + "WarpData"].IsArray)
                            {
                                Type sub_field_type = null;
                                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                                {
                                    sub_field_type = field.FieldType.GetGenericArguments().Single();
                                    var instance = field.GetValue(obj) as IList;
                                    for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                    {
                                        if (json[field_name + "WarpData"][i].IsObject)
                                        {
                                            var new_obj = Activator.CreateInstance(sub_field_type);
                                            JsonWriter jw = new JsonWriter();
                                            json[field_name + "WarpData"][i].ToJson(jw);
                                            JsonUtility.FromJsonOverwrite(jw.TextWriter.ToString(), new_obj);
                                            JsonCommonWarpper(new_obj, json[field_name + "WarpData"][i]);
                                            var temp_obj = new object[] { new_obj };
                                            instance.Add(new_obj);
                                        }
                                        else
                                            DynamicModLoader.LogErrorWithModInfo("CommonWarpper ADD Wrong SubWarpData Format " + field_type.Name);
                                    }
                                }
                                else if (field.FieldType.IsArray)
                                {

                                    sub_field_type = field.FieldType.GetElementType();
                                    var instance = field.GetValue(obj) as Array;
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
                                            DynamicModLoader.LogErrorWithModInfo("CommonWarpper ADD Wrong SubWarpData Format " + field_type.Name);
                                    }
                                    field.SetValue(obj, instance);
                                }
                                else
                                {
                                    DynamicModLoader.LogErrorWithModInfo("CommonWarpper ADD Must be list or array " + field_type.Name);
                                }
                            }
                            else
                            {
                                DynamicModLoader.LogErrorWithModInfo("CommonWarpper ADD Wrong WarpData Format " + field_type.Name);
                            }
                        }
                        else if ((int)json[key] == (int)WarpType.MODIFY)
                        {
                            var field_name = key.Substring(0, key.Length - 8);
                            var field = obj_type.GetField(field_name, bindingFlags);
                            var field_type = field.FieldType;

                            if (json[field_name + "WarpData"].IsObject)
                            {
                                var target_obj = field.GetValue(obj);
                                JsonCommonWarpper(target_obj, json[field_name + "WarpData"]);
                                field.SetValue(obj, target_obj);
                            }
                            else if (json[field_name + "WarpData"].IsArray)
                            {
                                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                                {
                                    var instance = field.GetValue(obj) as IList;
                                    for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                    {
                                        if (json[field_name + "WarpData"][i].IsObject)
                                        {
                                            var target_obj = instance[i];
                                            JsonCommonWarpper(target_obj, json[field_name + "WarpData"][i]);
                                            instance[i] = target_obj;
                                        }
                                        else
                                            DynamicModLoader.LogErrorWithModInfo("CommonWarpper MODIFY Wrong SubWarpData Format " + field_type.Name);
                                    }
                                }
                                else if (field.FieldType.IsArray)
                                {
                                    var instance = field.GetValue(obj) as Array;
                                    for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                    {
                                        if (json[field_name + "WarpData"][i].IsObject)
                                        {
                                            var target_obj = instance.GetValue(i);
                                            JsonCommonWarpper(target_obj, json[field_name + "WarpData"][i]);
                                            instance.SetValue(target_obj, i);
                                        }
                                        else
                                            DynamicModLoader.LogErrorWithModInfo("CommonWarpper MODIFY Wrong SubWarpData Format " + field_type.Name);
                                    }
                                    field.SetValue(obj, instance);
                                }
                                else
                                {
                                    DynamicModLoader.LogErrorWithModInfo("CommonWarpper MODIFY Must be list or array " + field_type.Name);
                                }
                            }
                            else
                            {
                                DynamicModLoader.LogErrorWithModInfo("CommonWarpper MODIFY Wrong WarpData Format " + field_type.Name);
                            }
                        }
                        else
                        {
                            DynamicModLoader.LogErrorWithModInfo("CommonWarpper Unexpect WarpType");
                        }
                    }
                    else if (key.EndsWith("WarpData"))
                        continue;
                    else
                    {
                        if ((json[key].IsObject))
                        {
                            var field_name = key;
                            var field = obj_type.GetField(field_name, bindingFlags);
                            if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                continue;
                            var sub_obj = field.GetValue(obj);
                            JsonCommonWarpper(sub_obj, json[key]);
                            field.SetValue(obj, sub_obj);
                        }
                        else if (json[key].IsArray)
                        {
                            var field_name = key;
                            var field = obj_type.GetField(field_name, bindingFlags);

                            for (int i = 0; i < json[key].Count; i++)
                            {
                                if (json[key][i].IsObject)
                                {
                                    if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                                    {
                                        var ele_type = field.FieldType.GetGenericArguments().Single();
                                        if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                            break;
                                        var list = field.GetValue(obj) as IList;
                                        var ele = list[i];
                                        if (ele == null)
                                            continue;
                                        JsonCommonWarpper(ele, json[key][i]);
                                        list[i] = ele;
                                        field.SetValue(obj, list);
                                    }
                                    else if (field.FieldType.IsArray)
                                    {
                                        var ele_type = field.FieldType.GetElementType();
                                        if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
                                            break;
                                        var array = field.GetValue(obj) as Array;
                                        var ele = array.GetValue(i);
                                        if (ele == null)
                                            continue;
                                        JsonCommonWarpper(ele, json[key][i]);
                                        array.SetValue(ele, i);
                                        field.SetValue(obj, array);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DynamicModLoader.LogErrorWithModInfo(string.Format("CommonWarpper {0} {1}", obj_type.Name, ex.Message));
                }
            }
        }

        public static void ObjectReferenceWarpper<ValueType>(System.Object obj, string data, string field_name, Dictionary<string, ValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    DynamicModLoader.LogErrorWithModInfo("ObjectReferenceWarpper Object IsNotClass");
            //    return;
            //}
            if (dict.TryGetValue(data, out var ele))
            {
                try
                {
                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    var field = obj.GetType().GetField(field_name, bindingFlags);
                    field.SetValue(obj, ele);
                }
                catch (Exception ex)
                {
                    DynamicModLoader.LogErrorWithModInfo(string.Format("ObjectReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
                }
            }
        }

        public static void ObjectReferenceWarpper<ValueType>(System.Object obj, List<string> data, string field_name, Dictionary<string, ValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    DynamicModLoader.LogErrorWithModInfo("ObjectReferenceWarpper Object IsNotClass");
            //    return;
            //}
            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = field.GetValue(obj) as IList;
                    foreach (var name in data)
                        if (dict.TryGetValue(name, out var ele))
                            instance.Add(ele);
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = field.GetValue(obj) as Array;
                    ArrayResize(ref instance, data.Count);
                    for (int i = 0; i < data.Count; i++)
                        if (dict.TryGetValue(data[i], out var ele))
                            instance.SetValue(ele, i);
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                DynamicModLoader.LogErrorWithModInfo(string.Format("ObjectReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectAddReferenceWarpper<ValueType>(System.Object obj, string data, string field_name, Dictionary<string, ValueType> dict)
        {
            DynamicModLoader.LogErrorWithModInfo(string.Format("ObjectAddReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, "AddReferenceWarpper Only Vaild in List or Array Filed"));
        }

        public static void ObjectAddReferenceWarpper<ValueType>(System.Object obj, List<string> data, string field_name, Dictionary<string, ValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    DynamicModLoader.LogErrorWithModInfo("ObjectAddReferenceWarpper Object IsNotClass");
            //    return;
            //}
            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = field.GetValue(obj) as IList;
                    foreach (var name in data)
                        if (dict.TryGetValue(name, out var ele))
                            instance.Add(ele);
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = field.GetValue(obj) as Array;
                    int start_idx = instance.Length;
                    ArrayResize(ref instance, data.Count + instance.Length);
                    for (int i = 0; i < data.Count; i++)
                        if (dict.TryGetValue(data[i], out var ele))
                            instance.SetValue(ele, i + start_idx);
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                DynamicModLoader.LogErrorWithModInfo(string.Format("ObjectAddReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
