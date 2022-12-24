﻿using System;
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
        public override void WarpperCustom(System.Object obj, string data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperCustom Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
        }
        public override void WarpperCustom(System.Object obj, List<string> data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperCustom List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
        }
        public override void WarpperReference(System.Object obj, string data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
        }

        public override void WarpperReference(System.Object obj, List<string> data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.SpriteDict);
        }

    }
}
