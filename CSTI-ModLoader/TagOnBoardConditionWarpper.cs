using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class TagOnBoardConditionWarpper : WarpperBase
    {
        public TagOnBoardConditionWarpper(string SrcPath) : base(SrcPath) { }

        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("TagOnBoardConditionWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("TagOnBoardConditionWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //}
        public void WarpperCustomSelf(ref TagOnBoardCondition obj)
        {
            if (TriggerTagWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.CardTagDict.TryGetValue(TriggerTagWarpData, out var ele) && ele is CardTag)
                {
                    obj.TriggerTag = ele;
                }
            }
        }

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("TagOnBoardConditionWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //}
        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("TagOnBoardConditionWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    {
        //        var target = field.GetValue(obj) as List<TagOnBoardCondition>;
        //        foreach (var file in data)
        //        {
        //            TagOnBoardCondition instance = new TagOnBoardCondition();
        //            using (StreamReader sr = new StreamReader(SrcPath + "\\" + file))
        //            {
        //                string json_data = sr.ReadToEnd();
        //                instance = UnityEngine.JsonUtility.FromJson<TagOnBoardCondition>(json_data);
        //                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //            }
        //            WarpperCustom(ref instance);
        //            target.Add(instance);
        //        }
        //        field.SetValue(obj, target);
        //    }
        //    else if (field.FieldType.IsArray)
        //    {
        //        var target = field.GetValue(obj) as TagOnBoardCondition[];
        //        Array.Resize<TagOnBoardCondition>(ref target, data.Count);
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            TagOnBoardCondition instance = new TagOnBoardCondition();
        //            using (StreamReader sr = new StreamReader(SrcPath + "\\" + data[i]))
        //            {
        //                string json_data = sr.ReadToEnd();
        //                instance = UnityEngine.JsonUtility.FromJson<TagOnBoardCondition>(json_data);
        //                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //            }
        //            WarpperCustom(ref instance);
        //            target[i] = instance;
        //        }
        //        field.SetValue(obj, target);
        //    }
        //}

        // Object Name
        public String ObjectName;

        // TriggerTag: CardTag
        public WarpperFunction.WarpType TriggerTagWarpType;
        public String TriggerTagWarpData;
    }
}
