using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class StatValueTriggerWarpper : WarpperBase
    {
        public StatValueTriggerWarpper(string SrcPath) : base(SrcPath) { }

        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("StatValueTriggerWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("StatValueTriggerWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //}
        public void WarpperCustomSelf(ref StatValueTrigger obj)
        {
            if (StatWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(StatWarpData, out var ele) && ele is GameStat)
                {
                    obj.Stat = ele as GameStat;
                }
            }
        }

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("StatValueTriggerWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //}
        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("StatValueTriggerWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.ObjectCustomWarpper(obj, data, field_name, this);
        //    //var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    //var field = obj.GetType().GetField(field_name, bindingFlags);
        //    //if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    //{
        //    //    var target = field.GetValue(obj) as List<StatValueTrigger>;
        //    //    foreach (var file in data)
        //    //    {
        //    //        StatValueTrigger instance = new StatValueTrigger();
        //    //        using (StreamReader sr = new StreamReader(SrcPath + "\\" + file))
        //    //        {
        //    //            string json_data = sr.ReadToEnd();
        //    //            instance = UnityEngine.JsonUtility.FromJson<StatValueTrigger>(json_data);
        //    //            UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //    //        }
        //    //        WarpperCustom(ref instance);
        //    //        target.Add(instance);
        //    //    }
        //    //    field.SetValue(obj, target);
        //    //}
        //    //else if (field.FieldType.IsArray)
        //    //{
        //    //    var target = field.GetValue(obj) as StatValueTrigger[];
        //    //    Array.Resize<StatValueTrigger>(ref target, data.Count);
        //    //    for (int i = 0; i < data.Count; i++)
        //    //    {
        //    //        StatValueTrigger instance = new StatValueTrigger();
        //    //        using (StreamReader sr = new StreamReader(SrcPath + "\\" + data[i]))
        //    //        {
        //    //            string json_data = sr.ReadToEnd();
        //    //            instance = UnityEngine.JsonUtility.FromJson<StatValueTrigger>(json_data);
        //    //            UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //    //        }
        //    //        WarpperCustom(ref instance);
        //    //        target[i] = instance;
        //    //    }
        //    //    field.SetValue(obj, target);
        //    //}
        //}

        // Object Name
        public String ObjectName;

        // Stat: GameStat
        public WarpperFunction.WarpType StatWarpType;
        public String StatWarpData;
    }
}
