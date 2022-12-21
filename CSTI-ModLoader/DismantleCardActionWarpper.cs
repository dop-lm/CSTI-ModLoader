using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ModLoader
{
    public class DismantleCardActionWarpper : CardActionWarpper
    {
        public DismantleCardActionWarpper(string SrcPath) : base(SrcPath) { }
        //public new void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("DismantleCardActionWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //}

        //public new void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("DismantleCardActionWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //}

        public void WarpperCustomSelf(DismantleCardAction obj)
        {
            base.WarpperCustomSelf(obj);
        }

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("DismantleCardActionWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //}

        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("DismantleCardActionWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    {
        //        var target = field.GetValue(obj) as List<DismantleCardAction>;
        //        foreach (var file in data)
        //        {
        //            DismantleCardAction instance = new DismantleCardAction();
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
        //        var target = field.GetValue(obj) as DismantleCardAction[];
        //        Array.Resize<DismantleCardAction>(ref target, data.Count);
        //        for (int i = 0; i < data.Count; i++)
        //        {
        //            DismantleCardAction instance = new DismantleCardAction();
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
    }
}
