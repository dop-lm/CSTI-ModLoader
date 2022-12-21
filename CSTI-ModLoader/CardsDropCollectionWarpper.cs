using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardsDropCollectionWarpper : WarpperBase
    {
        public CardsDropCollectionWarpper(string SrcPath) : base(SrcPath) { }
        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardsDropCollectionWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardsDropCollectionWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //}

        public void WarpperCustomSelf(CardsDropCollection obj)
        {
            WarpperFunction.ClassWarpper(obj, "DroppedCards", DroppedCardsWarpType, DroppedCardsWarpData, SrcPath);
        }

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardsDropCollectionWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //}

        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardsDropCollectionWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    {
        //        var target = field.GetValue(obj) as List<CardsDropCollection>;
        //        foreach (var file in data)
        //        {
        //            CardsDropCollection instance = new CardsDropCollection();
        //            using (StreamReader sr = new StreamReader(SrcPath + "\\" + file))
        //            {
        //                string json_data = sr.ReadToEnd();
        //                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, instance);
        //                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //            }
        //            WarpperCustom(instance);
        //            target.Add(instance);
        //        }
        //    }
        //    else if (field.FieldType.IsArray)
        //    {
        //        var target = field.GetValue(obj) as CardsDropCollection[];
        //        Array.Resize<CardsDropCollection>(ref target, data.Count);
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            CardsDropCollection instance = new CardsDropCollection();
        //            using (StreamReader sr = new StreamReader(SrcPath + "\\" + data[i]))
        //            {
        //                string json_data = sr.ReadToEnd();
        //                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, instance);
        //                UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //            }
        //            WarpperCustom(instance);
        //            target[i] = instance;
        //        }
        //        field.SetValue(obj, target);
        //    }
        //}

        // Object Name
        public String ObjectName;

        // DroppedCards: CardDrop[]
        public WarpperFunction.WarpType DroppedCardsWarpType;
        public List<string> DroppedCardsWarpData;
    }
}
