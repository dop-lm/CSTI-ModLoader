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
            if (LiquidCardWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(LiquidCardWarpData, out var ele) && ele is CardData)
                {
                    obj.LiquidCard = ele as CardData;
                }
            }
        }

        // Object Name
        public String ObjectName;

        // LiquidCard: CardData
        public WarpperFunction.WarpType LiquidCardWarpType;
        public string LiquidCardWarpData;
    }
}
