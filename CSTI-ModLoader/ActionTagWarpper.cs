using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ActionTagWarpper : WarpperBase
    {
        public ActionTagWarpper(string SrcPath) : base(SrcPath) { }
        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("ActionTagWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("ActionTagWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //}

        public void WarpperCustomSelf(ActionTag instance)
        {
        }

        public override void WarpperReference(System.Object obj, string data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.ActionTagDict);
        }

        public override void WarpperReference(System.Object obj, List<string> data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.ActionTagDict);

            //var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            //var field = obj.GetType().GetField(field_name, bindingFlags);
            //if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
            //{
            //    var target = field.GetValue(obj) as List<ActionTag>;
            //    foreach (var name in data)
            //        if (ModLoader.ActionTagDict.TryGetValue(name, out var ele))
            //            target.Add(ele);
            //}
            //else if (field.FieldType.IsArray)
            //{
            //    var target = field.GetValue(obj) as ActionTag[];
            //    Array.Resize<ActionTag>(ref target, data.Count);
            //    for (int i = 0; i < data.Count; i++)
            //        if (ModLoader.ActionTagDict.TryGetValue(data[i], out var ele))
            //            target[i] = ele;
            //    field.SetValue(obj, target);
            //}
        }


    }
}
