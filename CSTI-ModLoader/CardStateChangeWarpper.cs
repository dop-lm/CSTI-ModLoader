using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardStateChangeWarpper : WarpperBase
    {
        public CardStateChangeWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref CardStateChange obj)
        {
            if (TransformIntoWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(TransformIntoWarpData, out var ele) && ele is CardData)
                {
                    obj.TransformInto = ele as CardData;
                }
            }
        }

        // Object Name
        public String ObjectName;

        // TransformInto: CardData
        public WarpperFunction.WarpType TransformIntoWarpType;
        public string TransformIntoWarpData;
    }
}
