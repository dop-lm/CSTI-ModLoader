using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardOnBoardConditionWarpper : WarpperBase
    {
        public CardOnBoardConditionWarpper(string SrcPath) : base(SrcPath) { }

        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardOnBoardConditionWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardOnBoardConditionWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //}
        public void WarpperCustomSelf(ref CardOnBoardCondition obj)
        {
            if (TriggerCardWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(TriggerCardWarpData, out var ele) && ele is CardData)
                {
                    obj.TriggerCard = ele as CardData;
                }
            }
        }

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardOnBoardConditionWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //}
        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardOnBoardConditionWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    {
        //        var target = field.GetValue(obj) as List<CardOnBoardCondition>;
        //        foreach (var file in data)
        //        {
        //            CardOnBoardCondition instance = new CardOnBoardCondition();
        //            using (StreamReader sr = new StreamReader(SrcPath + "\\" + file))
        //            {
        //                string json_data = sr.ReadToEnd();
        //                instance = UnityEngine.JsonUtility.FromJson<CardOnBoardCondition>(json_data);
        //                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //            }
        //            WarpperCustom(ref instance);
        //            target.Add(instance);
        //        }
        //        field.SetValue(obj, target);
        //    }
        //    else if (field.FieldType.IsArray)
        //    {
        //        var target = field.GetValue(obj) as CardOnBoardCondition[];
        //        Array.Resize<CardOnBoardCondition>(ref target, data.Count);
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            CardOnBoardCondition instance = new CardOnBoardCondition();
        //            using (StreamReader sr = new StreamReader(SrcPath + "\\" + data[i]))
        //            {
        //                string json_data = sr.ReadToEnd();
        //                instance = UnityEngine.JsonUtility.FromJson<CardOnBoardCondition>(json_data);
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

        // TriggerCard: CardData
        public WarpperFunction.WarpType TriggerCardWarpType;
        public String TriggerCardWarpData;
    }
}
