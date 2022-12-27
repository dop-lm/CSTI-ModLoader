using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public abstract class WarpperBase
    {
        public WarpperBase(string SrcPath)
        {
            this.SrcPath = SrcPath;
        }

        public virtual void WarpperCopy(System.Object obj, string data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperCopy Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        }

        public virtual void WarpperCopy(System.Object obj, List<string> data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperCopy List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        }

        public virtual void WarpperCustom(System.Object obj, string data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperCustom Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectCustomWarpper(obj, data, field_name, this);
        }
        public virtual void WarpperCustom(System.Object obj, List<string> data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperCustom List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectCustomWarpper(obj, data, field_name, this);
        }

        public virtual void WarpperReference(System.Object obj, string data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
        }

        public virtual void WarpperReference(System.Object obj, List<string> data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
        }

        public virtual void WarpperAdd(System.Object obj, string data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperAdd Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectAddWarpper(obj, data, field_name, this);
        }
        public virtual void WarpperAdd(System.Object obj, List<string> data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} WarpperAdd List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectAddWarpper(obj, data, field_name, this);
        }

        public virtual void WarpperModify(System.Object obj, string data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} ObjectWarpperModify Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectWarpperModify(obj, data, field_name, this);
        }
        public virtual void WarpperModify(System.Object obj, List<string> data, string field_name)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} ObjectWarpperModify List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
#endif
            WarpperFunction.ObjectWarpperModify(obj, data, field_name, this);
        }

        // Source Path
        public string SrcPath;

        // Object Name
        public String ObjectName;
    }
}
