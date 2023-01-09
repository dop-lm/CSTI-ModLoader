using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using System.Runtime.Remoting;
using System.Collections;
using LitJson;

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

        public static void JsonCommonRefWarpper(System.Object obj, string data, string field_name, Type field_type, WarpType warp_type = WarpType.REFERENCE)
        {
            if (field_type.IsSubclassOf(typeof(UniqueIDScriptable)))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
            }
            else if (field_type.IsSubclassOf(typeof(ScriptableObject)))
            {
                if (ModLoader.AllScriptableObjectWithoutGUIDDict.TryGetValue(field_type.Name, out var type_dict))
                    ObjectReferenceWarpper(obj, data, field_name, type_dict);
                else
                    ModLoader.LogErrorWithModInfo("CommonWarpper No Such Dict " + field_type.Name);
            }
            else if (field_type == typeof(UnityEngine.Sprite))
            {

                ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
            }
            else if (field_type == typeof(UnityEngine.AudioClip))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.AudioClipDict);
            }
            else if (field_type == typeof(WeatherSet))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.WeatherSetDict);
            }
            else if (field_type == typeof(ScriptableObject))
            {
                ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllCardOrTagDict);
            }
            else
            {
                ModLoader.LogErrorWithModInfo("JsonCommonRefWarpper Unexpect Object Type " + field_type.Name);
            }
        }

        public static void JsonCommonRefWarpper(System.Object obj, List<string> data, string field_name, Type field_type, WarpType warp_type = WarpType.REFERENCE)
        {
            if (field_type.IsSubclassOf(typeof(UniqueIDScriptable)))
            {
                if(warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
            }
            else if (field_type.IsSubclassOf(typeof(ScriptableObject)))
            {
                if (ModLoader.AllScriptableObjectWithoutGUIDDict.TryGetValue(field_type.Name, out var type_dict))
                {
                    if (warp_type == WarpType.ADD_REFERENCE)
                        ObjectAddReferenceWarpper(obj, data, field_name, type_dict);
                    else
                        ObjectReferenceWarpper(obj, data, field_name, type_dict);
                }
                else
                    ModLoader.LogErrorWithModInfo("CommonWarpper No Such Dict " + field_type.Name);
            }
            else if (field_type == typeof(UnityEngine.Sprite))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
            }
            else if (field_type == typeof(UnityEngine.AudioClip))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, ModLoader.AudioClipDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, ModLoader.AudioClipDict);
            }
            else if (field_type == typeof(WeatherSet))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, ModLoader.WeatherSetDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, ModLoader.WeatherSetDict);
            }
            else if (field_type == typeof(ScriptableObject))
            {
                if (warp_type == WarpType.ADD_REFERENCE)
                    ObjectAddReferenceWarpper(obj, data, field_name, ModLoader.AllCardOrTagDict);
                else
                    ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllCardOrTagDict);
            }
            else
            {
                ModLoader.LogErrorWithModInfo("JsonCommonRefWarpper Unexpect List Object Type " + field_type.Name);
            }
        }

        public static void JsonCommonWarpper(System.Object obj, LitJson.JsonData json)
        {
            if(!json.IsObject)
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
                                    ModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Must be list or array " + field_type.Name);
                                }

                                List<string> list_data = new List<string>();
                                for (int i = 0; i < json[field_name + "WarpData"].Count; i++)
                                {
                                    if (json[field_name + "WarpData"][i].IsString)
                                        list_data.Add(json[field_name + "WarpData"][i].ToString());
                                    else
                                        ModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Wrong SubWarpData Format " + field_type.Name);
                                }

                                if (list_data.Count != json[field_name + "WarpData"].Count)
                                    ModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Size Error" + field_type.Name);

                                JsonCommonRefWarpper(obj, list_data, field_name, sub_field_type, (WarpType)(int)json[key]);
                            }
                            else
                            {
                                ModLoader.LogErrorWithModInfo("CommonWarpper REFERENCE Wrong WarpData Format " + field_type.Name);
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
                                            ModLoader.LogErrorWithModInfo("CommonWarpper ADD SubWarpData Format " + field_type.Name);
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
                                            ModLoader.LogErrorWithModInfo("CommonWarpper ADD SubWarpData Format " + field_type.Name);
                                    }
                                    field.SetValue(obj, instance);
                                }
                                else
                                {
                                    ModLoader.LogErrorWithModInfo("CommonWarpper ADD Must be list or array " + field_type.Name);
                                }
                            }
                            else
                            {
                                ModLoader.LogErrorWithModInfo("CommonWarpper ADD Wrong WarpData Format " + field_type.Name);
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
                                            ModLoader.LogErrorWithModInfo("CommonWarpper MODIFY SubWarpData Format " + field_type.Name);
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
                                            ModLoader.LogErrorWithModInfo("CommonWarpper MODIFY SubWarpData Format " + field_type.Name);
                                    }
                                    field.SetValue(obj, instance);
                                }
                                else
                                {
                                    ModLoader.LogErrorWithModInfo("CommonWarpper MODIFY Must be list or array " + field_type.Name);
                                }
                            }
                            else
                            {
                                ModLoader.LogErrorWithModInfo("CommonWarpper MODIFY Wrong WarpData Format " + field_type.Name);
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
                                        JsonCommonWarpper(ele, json[key][i]);
                                        array.SetValue(ele, i);
                                        field.SetValue(obj, array);
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    ModLoader.LogErrorWithModInfo(string.Format("CommonWarpper {0} {1} {2}", obj_type.Name, ex.Message));
                }
            }
        }

        public static void ClassWarpper(System.Object obj, string field_name, WarpType warp_type, string data, string src_dir)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ClassWarpper Object IsNotClass");
            //    return;
            //}
            string method_name;
            if (warp_type == WarpType.NONE)
                return;
            else if (warp_type == WarpType.COPY)
                method_name = "WarpperCopy";
            else if (warp_type == WarpType.CUSTOM)
                method_name = "WarpperCustom";
            else if (warp_type == WarpType.REFERENCE)
                method_name = "WarpperReference";
            else if (warp_type == WarpType.ADD)
                method_name = "WarpperAdd";
            else if (warp_type == WarpType.MODIFY)
                method_name = "WarpperModify";
            else
            {
                UnityEngine.Debug.LogWarning("ClassWarpper Unkown Warp Type");
                return;
            }

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                //if (warp_type == WarpType.REFERENCE && !field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                //{
                //    UnityEngine.Debug.LogWarning("ClassWarpper Reference Warp Field Must be Subclass of ScriptableObject");
                //    return;
                //}

                Type warpper_type = Type.GetType("ModLoader." + field.FieldType.Name + "Warpper");
                var warpper = Activator.CreateInstance(warpper_type, ModLoader.CombinePaths(src_dir, field_name));
                var temp_obj = new object[] { obj, data, field_name };
                warpper_type.GetMethod(method_name, bindingFlags, null, new Type[] { obj.GetType(), typeof(string), typeof(string) }, null).Invoke(warpper, temp_obj);
            }
            catch(Exception ex)
            {
                ModLoader.LogErrorWithModInfo(string.Format("ClassWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ClassWarpper(System.Object obj, string field_name, WarpType warp_type, List<string> data, string src_dir)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ClassWarpper Object IsNotClass");
            //    return;
            //}
            string method_name;
            if (warp_type == WarpType.NONE)
                return;
            else if (warp_type == WarpType.COPY)
                method_name = "WarpperCopy";
            else if (warp_type == WarpType.CUSTOM)
                method_name = "WarpperCustom";
            else if (warp_type == WarpType.REFERENCE)
                method_name = "WarpperReference";
            else if (warp_type == WarpType.ADD)
                method_name = "WarpperAdd";
            else if (warp_type == WarpType.MODIFY)
                method_name = "WarpperModify";
            else
            {
                UnityEngine.Debug.LogWarning("ClassWarpper Unkown Warp Type");
                return;
            }

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                //if (warp_type == WarpType.REFERENCE && !field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                //{
                //    UnityEngine.Debug.LogWarning("ClassWarpper Reference Warp Field Must be Subclass of ScriptableObject");
                //    return;
                //}

                Type ele_type;
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    ele_type = field.FieldType.GetGenericArguments().Single();
                }
                else if (field.FieldType.IsArray)
                {
                    ele_type = field.FieldType.GetElementType();
                }
                else
                {
                    UnityEngine.Debug.LogWarning("ClassWarpper Object Field Must be Array or List");
                    return;
                }

                Type warpper_type = Type.GetType("ModLoader." + ele_type.Name + "Warpper");
                var warpper = Activator.CreateInstance(warpper_type, ModLoader.CombinePaths(src_dir, field_name));
                var temp_obj = new object[] { obj, data, field_name };
                warpper_type.GetMethod(method_name, bindingFlags, null, new Type[] { obj.GetType(), typeof(List<string>), typeof(string) }, null).Invoke(warpper, temp_obj);
            }
            catch (Exception ex)
            {
                ModLoader.LogErrorWithModInfo(string.Format("ClassWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void UniqueIDScriptableCopyWarpper(System.Object obj, string data, string field_name)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("UniqueIDScriptableCopyWarpper Object IsNotClass");
            //    return;
            //}
            if (ModLoader.AllGUIDDict.TryGetValue(data, out var ele))
            {

                try
                {
                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    var field = obj.GetType().GetField(field_name, bindingFlags);
                    if (field != ele.GetType().GetField(field_name, bindingFlags))
                    {
                        ModLoader.LogErrorWithModInfo("UniqueIDScriptableCopyWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name + "Field not Same");
                        return;
                    }
                    field.SetValue(obj, field.GetValue(ele));
                }
                catch (Exception ex)
                {
                    ModLoader.LogErrorWithModInfo(string.Format("UniqueIDScriptableCopyWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
                }
            }
        }

        public static void UniqueIDScriptableCopyWarpper(System.Object obj, List<string> data, string field_name)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("UniqueIDScriptableCopyWarpper Object IsNotClass");
            //    return;
            //}
            if (data.Count > 0 && ModLoader.AllGUIDDict.TryGetValue(data[0], out var ele))
            {
                try
                {
                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    var field = obj.GetType().GetField(field_name, bindingFlags);
                    if (field != ele.GetType().GetField(field_name, bindingFlags))
                    {
                        ModLoader.LogErrorWithModInfo("UniqueIDScriptableCopyWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name + "Field not Same");
                        return;
                    }
                    field.SetValue(obj, field.GetValue(ele));
                }
                catch (Exception ex)
                {
                    ModLoader.LogErrorWithModInfo(string.Format("UniqueIDScriptableCopyWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
                }
        }
        }

        public static void ObjectCustomWarpper(System.Object obj, string data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectCustomWarpper Object IsNotClass");
            //    return;
            //}
            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
         
                var instance = field.GetValue(obj);
                if (!instance.GetType().IsClass)
                {
                    using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data)))
                    {
                        string json_data = sr.ReadToEnd();
                        instance = UnityEngine.JsonUtility.FromJson(json_data, field.FieldType);
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, warpper);
                    }
                    var temp_obj = new object[] { instance };
                    warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(warpper, temp_obj);
                    field.SetValue(obj, temp_obj[0]);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data)))
                    {
                        string json_data = sr.ReadToEnd();
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, instance);
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, warpper);
                    }
                    warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { field.FieldType }, null).Invoke(warpper, new object[] { instance });
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                ModLoader.LogErrorWithModInfo(string.Format("ObjectCustomWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectCustomWarpper(System.Object obj, List<string> data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectCustomWarpper Object IsNotClass");
            //    return;
            //}

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = field.GetValue(obj);
                    field.FieldType.GetMethod("Clear").Invoke(instance, null);
                    for (int i = 0; i < data.Count; i++)
                    {
                        var ele_type = field.FieldType.GetGenericArguments().Single();
                        var ele = Activator.CreateInstance(ele_type);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                ele = UnityEngine.JsonUtility.FromJson(json_data, ele_type);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            field.FieldType.GetMethod("Add").Invoke(instance, temp_obj);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            field.FieldType.GetMethod("Add").Invoke(instance, new object[] { ele });
                        }
                    }
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = field.GetValue(obj) as Array;
                    ArrayResize(ref instance, data.Count);
                    for (int i = 0; i < data.Count; i++)
                    {
                        var ele_type = field.FieldType.GetElementType();
                        var ele = Activator.CreateInstance(ele_type);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                ele = UnityEngine.JsonUtility.FromJson(json_data, ele_type);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            instance.SetValue(temp_obj[0], i);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            instance.SetValue(ele, i);
                        }
                    }
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                ModLoader.LogErrorWithModInfo(string.Format("ObjectCustomWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectReferenceWarpper<ValueType>(System.Object obj, string data, string field_name, Dictionary<string, ValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectReferenceWarpper Object IsNotClass");
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
                    ModLoader.LogErrorWithModInfo(string.Format("ObjectReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
                }
            }
        }

        public static void ObjectReferenceWarpper<ValueType>(System.Object obj, List<string> data, string field_name, Dictionary<string, ValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectReferenceWarpper Object IsNotClass");
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
                ModLoader.LogErrorWithModInfo(string.Format("ObjectReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectAddReferenceWarpper<ValueType>(System.Object obj, string data, string field_name, Dictionary<string, ValueType> dict)
        {
            ModLoader.LogErrorWithModInfo(string.Format("ObjectAddReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, "AddReferenceWarpper Only Vaild in List or Array Filed"));
        }

        public static void ObjectAddReferenceWarpper<ValueType>(System.Object obj, List<string> data, string field_name, Dictionary<string, ValueType> dict)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectAddReferenceWarpper Object IsNotClass");
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
                ModLoader.LogErrorWithModInfo(string.Format("ObjectAddReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectAddWarpper(System.Object obj, string data, string field_name, WarpperBase warpper)
        {
            ModLoader.LogErrorWithModInfo(string.Format("ObjectAddWarpper {0}.{1} {2}", obj.GetType().Name, field_name, "AddWarpper Only Vaild in List or Array Filed"));
        }

        public static void ObjectAddWarpper(System.Object obj, List<string> data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectAddWarpper Object IsNotClass");
            //    return;
            //}

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = field.GetValue(obj) as IList;
                    for (int i = 0; i < data.Count; i++)
                    {
                        var ele_type = field.FieldType.GetGenericArguments().Single();
                        var ele = Activator.CreateInstance(ele_type);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                ele = UnityEngine.JsonUtility.FromJson(json_data, ele_type);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            instance.Add(temp_obj[0]);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            instance.Add(ele);
                        }
                    }
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = field.GetValue(obj) as Array;
                    int start_idx = instance.Length;
                    ArrayResize(ref instance, data.Count + instance.Length);
                    for (int i = 0; i < data.Count; i++)
                    {
                        var ele_type = field.FieldType.GetElementType();
                        var ele = Activator.CreateInstance(ele_type);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                ele = UnityEngine.JsonUtility.FromJson(json_data, ele_type);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            instance.SetValue(temp_obj[0], i + start_idx);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            instance.SetValue(ele, i + start_idx);
                        }
                    }
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                ModLoader.LogErrorWithModInfo(string.Format("ObjectAddWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectWarpperModify(System.Object obj, string data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectWarpperModify Object IsNotClass");
            //    return;
            //}
            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);

                var instance = field.GetValue(obj);
                if (!instance.GetType().IsClass)
                {
                    using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data)))
                    {
                        string json_data = sr.ReadToEnd();
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, instance);
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, warpper);
                    }
                    var temp_obj = new object[] { instance };
                    warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(warpper, temp_obj);
                    field.SetValue(obj, temp_obj[0]);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data)))
                    {
                        string json_data = sr.ReadToEnd();
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, instance);
                        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, warpper);
                    }
                    warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { field.FieldType }, null).Invoke(warpper, new object[] { instance });
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                ModLoader.LogErrorWithModInfo(string.Format("ObjectWarpperModify {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectWarpperModify(System.Object obj, List<string> data, string field_name, WarpperBase warpper)
        {
            //if (!obj.GetType().IsClass)
            //{
            //    UnityEngine.Debug.LogWarning("ObjectWarpperModify Object IsNotClass");
            //    return;
            //}

            try
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var field = obj.GetType().GetField(field_name, bindingFlags);
                if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var instance = field.GetValue(obj) as IList;
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i] == "")
                            continue;
                        var ele_type = field.FieldType.GetGenericArguments().Single();
                        var ele = instance[i];
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                        }
                        instance[i] = ele;
                    }
                }
                else if (field.FieldType.IsArray)
                {
                    var instance = field.GetValue(obj) as Array;
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i] == "")
                            continue;
                        var ele_type = field.FieldType.GetElementType();
                        var ele = instance.GetValue(i);
                        var new_warpper = Activator.CreateInstance(warpper.GetType(), warpper.SrcPath);
                        if (!ele.GetType().IsClass)
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            var temp_obj = new object[] { ele };
                            new_warpper.GetType().GetMethod("WarpperCustomSelf").Invoke(new_warpper, temp_obj);
                            instance.SetValue(temp_obj[0], i);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(ModLoader.CombinePaths(warpper.SrcPath, data[i])))
                            {
                                string json_data = sr.ReadToEnd();
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, ele);
                                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, new_warpper);
                            }
                            new_warpper.GetType().GetMethod("WarpperCustomSelf", bindingFlags, null, new Type[] { ele_type }, null).Invoke(new_warpper, new object[] { ele });
                            instance.SetValue(ele, i);
                        }
                    }
                    field.SetValue(obj, instance);
                }
            }
            catch (Exception ex)
            {
                ModLoader.LogErrorWithModInfo(string.Format("ObjectWarpperModify {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
