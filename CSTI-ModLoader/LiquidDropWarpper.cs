using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class LiquidDropWarpper : WarpperBase
    {
        public LiquidDropWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref LiquidDrop obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "LiquidCard", LiquidCardWarpType, LiquidCardWarpData, SrcPath);

            obj = (LiquidDrop)box;
        }

        // LiquidCard: CardData
        public WarpperFunction.WarpType LiquidCardWarpType;
        public string LiquidCardWarpData;
    }
}
