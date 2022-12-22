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
            object box = obj;

            WarpperFunction.ClassWarpper(box, "TransformInto", TransformIntoWarpType, TransformIntoWarpData, SrcPath);

            obj = (CardStateChange)box;
        }

        // Object Name
        public String ObjectName;

        // TransformInto: CardData
        public WarpperFunction.WarpType TransformIntoWarpType;
        public string TransformIntoWarpData;
    }
}
