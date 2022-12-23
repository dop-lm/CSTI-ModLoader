﻿using System;
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

        public void WarpperCustomSelf(ActionTag instance)
        {
        }

        public override void WarpperReference(System.Object obj, string data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.ActionTagDict);
        }

        public override void WarpperReference(System.Object obj, List<string> data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.ActionTagDict);
        }


    }
}
