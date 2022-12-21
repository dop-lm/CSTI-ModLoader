using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class AudioClipWarpper : WarpperBase
    {
        public AudioClipWarpper(string SrcPath) : base(SrcPath) { }
        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("AudioClipWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //    //if (ModLoader.AllGUIDDict.TryGetValue(data, out var card))
        //    //{
        //    //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    //    if (field != card.GetType().GetField(field_name, bindingFlags))
        //    //    {
        //    //        UnityEngine.Debug.LogError("AudioClipWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name + "Field not Same");
        //    //        return;
        //    //    }
        //    //    field.SetValue(obj, field.GetValue(card));
        //    //}
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("AudioClipWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //}

        public override void WarpperReference(System.Object obj, string data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.AudioClipDict);
            //if (ModLoader.AudioClipDict.TryGetValue(data, out var ele))
            //{
            //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            //    obj.GetType().GetField(field_name, bindingFlags).SetValue(obj, ele);
            //}
        }

        public override void WarpperReference(System.Object obj, List<string> data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.AudioClipDict);
            //var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            //var field = obj.GetType().GetField(field_name, bindingFlags);
            //if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
            //{
            //    var target = field.GetValue(obj) as List<UnityEngine.AudioClip>;
            //    foreach (var name in data)
            //        if (ModLoader.AudioClipDict.TryGetValue(name, out var ele))
            //            target.Add(ele);
            //}
            //else if (field.FieldType.IsArray)
            //{
            //    var target = field.GetValue(obj) as UnityEngine.AudioClip[];
            //    Array.Resize<UnityEngine.AudioClip>(ref target, data.Count);
            //    for (int i = 0; i < data.Count; i++)
            //        if (ModLoader.AudioClipDict.TryGetValue(data[i], out var ele))
            //            target[i] = ele;
            //    field.SetValue(obj, target);
            //}
        }
    }
}
