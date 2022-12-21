using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardDataRefWarpper : WarpperBase
    {
        public CardDataRefWarpper(string SrcPath) : base(SrcPath) { }

        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDataRefWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDataRefWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //}
        public void WarpperCustomSelf(CardDataRef obj)
        {
            if (CardWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(CardWarpData, out var ele) && ele is CardData)
                    obj.Card = ele as CardData;
            }
        }
        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDataRefWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.ObjectCustomWarpper(obj, data, field_name, this);
        //}
        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDataRefWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.ObjectCustomWarpper(obj, data, field_name, this);
        //    //var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    //var field = obj.GetType().GetField(field_name, bindingFlags);
        //    //if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    //{
        //    //    var target = field.GetValue(obj) as List<CardDataRef>;
        //    //    foreach (var file in data)
        //    //    {
        //    //        CardDataRef instance = new CardDataRef();
        //    //        using (StreamReader sr = new StreamReader(SrcPath + "\\" + file))
        //    //        {
        //    //            string json_data = sr.ReadToEnd();
        //    //            instance = UnityEngine.JsonUtility.FromJson<CardDataRef>(json_data);
        //    //            UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //    //        }
        //    //        WarpperCustom(instance);
        //    //        target.Add(instance);
        //    //    }
        //    //    field.SetValue(obj, target);
        //    //}
        //    //else if (field.FieldType.IsArray)
        //    //{
        //    //    var target = field.GetValue(obj) as CardDataRef[];
        //    //    Array.Resize<CardDataRef>(ref target, data.Count);
        //    //    for (int i = 0; i < data.Count; i++)
        //    //    {
        //    //        CardDataRef instance = new CardDataRef();
        //    //        using (StreamReader sr = new StreamReader(SrcPath + "\\" + data[i]))
        //    //        {
        //    //            string json_data = sr.ReadToEnd();
        //    //            instance = UnityEngine.JsonUtility.FromJson<CardDataRef>(json_data);
        //    //            UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //    //        }
        //    //        WarpperCustom(instance);
        //    //        target[i] = instance;
        //    //    }
        //    //    field.SetValue(obj, target);
        //    //}
        //}

        // Object Name
        public String ObjectName;

        // Card: CardData
        public WarpperFunction.WarpType CardWarpType;
        public String CardWarpData;
    }
}
