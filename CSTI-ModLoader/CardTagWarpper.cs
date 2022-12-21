using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ModLoader
{
    public class CardTagWarpper : WarpperBase
    {
        public CardTagWarpper(string SrcPath) : base(SrcPath) { }
        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardTagWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //    if (ModLoader.AllGUIDDict.TryGetValue(data, out var ele))
        //    {
        //        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //        var field = obj.GetType().GetField(field_name, bindingFlags);
        //        if (field != ele.GetType().GetField(field_name, bindingFlags))
        //        {
        //            UnityEngine.Debug.LogError("CardTagWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name + "Field not Same");
        //            return;
        //        }
        //        field.SetValue(obj, field.GetValue(ele));
        //    }
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardTagWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //    if (data.Count > 0 && ModLoader.AllGUIDDict.TryGetValue(data[0], out var ele))
        //    {
        //        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //        var field = obj.GetType().GetField(field_name, bindingFlags);
        //        if (field != ele.GetType().GetField(field_name, bindingFlags))
        //        {
        //            UnityEngine.Debug.LogError("CardTagWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name + "Field not Same");
        //            return;
        //        }
        //        field.SetValue(obj, field.GetValue(ele));
        //    }
        //}

        public void WarpperCustomSelf(CardTag instance)
        {
        }

        public override void WarpperReference(System.Object obj, string data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.CardTagDict);
            //if (ModLoader.CardTagDict.TryGetValue(data, out var ele))
            //{
            //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            //    var field = obj.GetType().GetField(field_name, bindingFlags);
            //    field.SetValue(obj, ele);
            //}
        }

        public override void WarpperReference(System.Object obj, List<string> data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.CardTagDict);
            //var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            //var field = obj.GetType().GetField(field_name, bindingFlags);
            //if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
            //{
            //    var target = field.GetValue(obj) as List<CardTag>;
            //    foreach (var name in data)
            //        if (ModLoader.CardTagDict.TryGetValue(name, out var ele))
            //            target.Add(ele);
            //}
            //else if (field.FieldType.IsArray)
            //{
            //    var target = field.GetValue(obj) as CardTag[];
            //    Array.Resize<CardTag>(ref target, data.Count);
            //    for (int i = 0; i < data.Count; i++)
            //        if (ModLoader.CardTagDict.TryGetValue(data[i], out var ele))
            //            target[i] = ele;
            //    field.SetValue(obj, target);
            //}
        }

    
    }
}
