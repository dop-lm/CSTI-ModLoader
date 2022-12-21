using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.GraphicsBuffer;

namespace ModLoader
{

    public class LiquidContentConditionWarpper : WarpperBase
    {
        public LiquidContentConditionWarpper(string SrcPath) : base(SrcPath) { }

        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("LiquidContentConditionWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("LiquidContentConditionWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //}
        public void WarpperCustomSelf(ref LiquidContentCondition obj)
        {
            if (RequiredLiquidWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(RequiredLiquidWarpData, out var ele) && ele is CardData)
                {
                    obj.RequiredLiquid = ele as CardData;
                }
            }
            if (RequiredGroupWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.CardTabGroupDict.TryGetValue(RequiredGroupWarpData, out var ele))
                {
                    obj.RequiredGroup = ele;
                }
            }
        }

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("LiquidContentConditionWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    LiquidContentCondition instance = new LiquidContentCondition();
        //    using (StreamReader sr = new StreamReader(SrcPath + "\\" + data))
        //    {
        //        string json_data = sr.ReadToEnd();
        //        instance = UnityEngine.JsonUtility.FromJson<LiquidContentCondition>(json_data);
        //        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //    }
        //    WarpperCustom(ref instance);
        //    field.SetValue(obj, instance);
        //}
        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("LiquidContentConditionWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    {
        //        var target = field.GetValue(obj) as List<LiquidContentCondition>;
        //        foreach (var file in data)
        //        {
        //            LiquidContentCondition instance = new LiquidContentCondition();
        //            using (StreamReader sr = new StreamReader(SrcPath + "\\" + file))
        //            {
        //                string json_data = sr.ReadToEnd();
        //                instance = UnityEngine.JsonUtility.FromJson<LiquidContentCondition>(json_data);
        //                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //            }
        //            WarpperCustom(ref instance);
        //            target.Add(instance);
        //        }
        //        field.SetValue(obj, target);
        //    }
        //    else if (field.FieldType.IsArray)
        //    {
        //        var target = field.GetValue(obj) as LiquidContentCondition[];
        //        Array.Resize<LiquidContentCondition>(ref target, data.Count);
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            LiquidContentCondition instance = new LiquidContentCondition();
        //            using (StreamReader sr = new StreamReader(SrcPath + "\\" + data[i]))
        //            {
        //                string json_data = sr.ReadToEnd();
        //                instance = UnityEngine.JsonUtility.FromJson<LiquidContentCondition>(json_data);
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

        // RequiredLiquid: CardData
        public WarpperFunction.WarpType RequiredLiquidWarpType;
        public String RequiredLiquidWarpData;

        // RequiredGroup: CardTabGroup
        public WarpperFunction.WarpType RequiredGroupWarpType;
        public String RequiredGroupWarpData;
    }
}
