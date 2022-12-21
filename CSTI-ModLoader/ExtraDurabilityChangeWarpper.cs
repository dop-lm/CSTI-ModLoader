using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ExtraDurabilityChangeWarpper : WarpperBase
    {
        public ExtraDurabilityChangeWarpper(string SrcPath) : base(SrcPath) { }
        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("ExtraDurabilityChangeWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("ExtraDurabilityChangeWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //}

        public void WarpperCustomSelf(ExtraDurabilityChange obj)
        {
            WarpperFunction.ClassWarpper(obj, "AffectedCards", AffectedCardsWarpType, AffectedCardsWarpData, SrcPath);
            WarpperFunction.ClassWarpper(obj, "AffectedTags", AffectedTagsWarpType, AffectedTagsWarpData, SrcPath);
            WarpperFunction.ClassWarpper(obj, "NOTAffectedThings", NOTAffectedThingsWarpType, NOTAffectedThingsWarpData, SrcPath);
            WarpperFunction.ClassWarpper(obj, "SendToEnvironment", SendToEnvironmentWarpType, SendToEnvironmentWarpData, SrcPath);
        }

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("ExtraDurabilityChangeWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    var instance = field.GetValue(obj) as ExtraDurabilityChange;
        //    using (StreamReader sr = new StreamReader(SrcPath + "\\" + data))
        //    {
        //        string json_data = sr.ReadToEnd();
        //        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, instance);
        //        UnityEngine.JsonUtility.FromJsonOverwrite(json_data, this);
        //    }
        //    WarpperCustom(instance);
        //}

        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("ExtraDurabilityChangeWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    {
        //        var target = field.GetValue(obj) as List<ExtraDurabilityChange>;
        //        foreach (var file in data)
        //        {
        //            ExtraDurabilityChange instance = new ExtraDurabilityChange();
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
        //        var target = field.GetValue(obj) as ExtraDurabilityChange[];
        //        Array.Resize<ExtraDurabilityChange>(ref target, data.Count);
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            ExtraDurabilityChange instance = new ExtraDurabilityChange();
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

        // AffectedCards: List<CardData>
        public WarpperFunction.WarpType AffectedCardsWarpType;
        public List<string> AffectedCardsWarpData;

        // AffectedTags: CardTag[]
        public WarpperFunction.WarpType AffectedTagsWarpType;
        public List<string> AffectedTagsWarpData;

        // NOTAffectedThings: CardOrTagRef[]
        public WarpperFunction.WarpType NOTAffectedThingsWarpType;
        public List<string> NOTAffectedThingsWarpData;

        // SendToEnvironment: EnvironmentCardDataRef[]
        public WarpperFunction.WarpType SendToEnvironmentWarpType;
        public List<string> SendToEnvironmentWarpData;
    }
}
