using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardDropWarpper : WarpperBase
    {
        public CardDropWarpper(string SrcPath) : base(SrcPath) { }

        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDropWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDropWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //}

        public void WarpperCustomSelf(ref CardDrop obj)
        {
            if (DroppedCardWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(DroppedCardWarpData, out var ele) && ele is CardData)
                {
                    obj.DroppedCard = ele as CardData;
                }
            }
        }

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDropWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.ObjectCustomWarpper(obj, data, field_name, this);
        //}
        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDropWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.ObjectCustomWarpper(obj, data, field_name, this);
        //    //var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    //var field = obj.GetType().GetField(field_name, bindingFlags);
        //    //if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    //{
        //    //    var target = field.GetValue(obj) as List<CardDrop>;
        //    //    foreach (var file in data)
        //    //    {
        //    //        CardDrop instance = new CardDrop();
        //    //        using (StreamReader sr = new StreamReader(SrcPath + "\\" + file))
        //    //        {
        //    //            string json_data = sr.ReadToEnd();
        //    //            instance = UnityEngine.JsonUtility.FromJson<CardDrop>(json_data);
        //    //            UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //    //        }
        //    //        WarpperCustom(ref instance);
        //    //        target.Add(instance);
        //    //    }
        //    //    field.SetValue(obj, target);
        //    //}
        //    //else if (field.FieldType.IsArray)
        //    //{
        //    //    var target = field.GetValue(obj) as CardDrop[];
        //    //    Array.Resize<CardDrop>(ref target, data.Count);
        //    //    for (int i = 0; i < data.Count; i++)
        //    //    {
        //    //        CardDrop instance = new CardDrop();
        //    //        using (StreamReader sr = new StreamReader(SrcPath + "\\" + data[i]))
        //    //        {
        //    //            string json_data = sr.ReadToEnd();
        //    //            instance = UnityEngine.JsonUtility.FromJson<CardDrop>(json_data);
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

        // DroppedCard: CardData
        public WarpperFunction.WarpType DroppedCardWarpType;
        public string DroppedCardWarpData;
    }

}
