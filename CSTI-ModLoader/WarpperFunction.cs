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
            MODIFY
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
                var warpper = Activator.CreateInstance(warpper_type, src_dir + "\\" + field_name);
                var temp_obj = new object[] { obj, data, field_name };
                warpper_type.GetMethod(method_name, bindingFlags, null, new Type[] { obj.GetType(), typeof(string), typeof(string) }, null).Invoke(warpper, temp_obj);
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ClassWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
                var warpper = Activator.CreateInstance(warpper_type, src_dir + "\\" + field_name);
                var temp_obj = new object[] { obj, data, field_name };
                warpper_type.GetMethod(method_name, bindingFlags, null, new Type[] { obj.GetType(), typeof(List<string>), typeof(string) }, null).Invoke(warpper, temp_obj);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(string.Format("Error: ClassWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
                        UnityEngine.Debug.LogError("UniqueIDScriptableCopyWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name + "Field not Same");
                        return;
                    }
                    field.SetValue(obj, field.GetValue(ele));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(string.Format("Error: UniqueIDScriptableCopyWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
                        UnityEngine.Debug.LogError("UniqueIDScriptableCopyWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name + "Field not Same");
                        return;
                    }
                    field.SetValue(obj, field.GetValue(ele));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(string.Format("Error: UniqueIDScriptableCopyWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
                    using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data))
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
                    using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data))
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
                UnityEngine.Debug.LogError(string.Format("Error: ObjectCustomWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                UnityEngine.Debug.LogError(string.Format("Error: ObjectCustomWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
                    UnityEngine.Debug.LogError(string.Format("Error: ObjectReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
                    var instance = field.GetValue(obj);
                    foreach (var name in data)
                        if (dict.TryGetValue(name, out var ele))
                        {
                            var temp_obj = new object[] { ele };
                            field.FieldType.GetMethod("Add", bindingFlags).Invoke(instance, temp_obj);
                        }
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
                UnityEngine.Debug.LogError(string.Format("Error: ObjectReferenceWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
            }
        }

        public static void ObjectAddWarpper(System.Object obj, string data, string field_name, WarpperBase warpper)
        {
            UnityEngine.Debug.LogError(string.Format("Error: ObjectAddWarpper {0}.{1} {2}", obj.GetType().Name, field_name, "AddWarpper Only Vaild in List or Array Filed"));
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                UnityEngine.Debug.LogError(string.Format("Error: ObjectAddWarpper {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
                    using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data))
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
                    using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data))
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
                UnityEngine.Debug.LogError(string.Format("Error: ObjectWarpperModify {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                            using (StreamReader sr = new StreamReader(warpper.SrcPath + "\\" + data[i]))
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
                UnityEngine.Debug.LogError(string.Format("Error: ObjectWarpperModify {0}.{1} {2}", obj.GetType().Name, field_name, ex.Message));
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
