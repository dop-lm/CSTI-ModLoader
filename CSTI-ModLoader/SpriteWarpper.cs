using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModLoader
{
    public class SpriteWarpper : WarpperBase
    {
        public SpriteWarpper(string SrcPath) : base(SrcPath) { }
        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("SpriteWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //    if (ModLoader.AllGUIDDict.TryGetValue(data, out var card))
        //    {
        //        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //        var field = obj.GetType().GetField(field_name, bindingFlags);
        //        if (field != card.GetType().GetField(field_name, bindingFlags))
        //        {
        //            UnityEngine.Debug.LogError("SpriteWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name + "Field not Same");
        //            return;
        //        }
        //        field.SetValue(obj, field.GetValue(card));
        //    }
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("SpriteWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //}

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("SpriteWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //    if (ModLoader.SpriteDict.TryGetValue(data, out var ele))
        //    {
        //        var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //        obj.GetType().GetField(field_name, bindingFlags).SetValue(obj, ele);
        //    }
        //}

        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("SpriteWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //}

        public override void WarpperReference(System.Object obj, string data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
        }

        public override void WarpperReference(System.Object obj, List<string> data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
        }

    }
}
