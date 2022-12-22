using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    internal class CardTabGroupWarpper : WarpperBase
    {
        public CardTabGroupWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CardTabGroup obj)
        {
        }
        public override void WarpperReference(System.Object obj, string data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference Single {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.CardTabGroupDict);
        }
        public override void WarpperReference(System.Object obj, List<string> data, string field_name)
        {
            UnityEngine.Debug.Log(string.Format("{0} WarpperReference List {1}.{2}", this.GetType().Name, obj.GetType().Name, field_name));
            WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.CardTabGroupDict);
        }

        // Object Name
        public String ObjectName;
    }
}
