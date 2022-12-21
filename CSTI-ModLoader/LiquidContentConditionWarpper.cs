using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.GraphicsBuffer;

namespace ModLoader
{

    public class LiquidContentConditionWarpper : WarpperBase
    {
        public LiquidContentConditionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref LiquidContentCondition obj)
        {
            if (RequiredLiquidWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(RequiredLiquidWarpData, out var ele) && ele is CardData)
                {
                    obj.RequiredLiquid = ele as CardData;
                }
            }
            if (RequiredGroupWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.CardTabGroupDict.TryGetValue(RequiredGroupWarpData, out var ele))
                {
                    obj.RequiredGroup = ele;
                }
            }
        }

        // Object Name
        public String ObjectName;

        // RequiredLiquid: CardData
        public WarpperFunction.WarpType RequiredLiquidWarpType;
        public String RequiredLiquidWarpData;

        // RequiredGroup: CardTabGroup
        public WarpperFunction.WarpType RequiredGroupWarpType;
        public String RequiredGroupWarpData;
    }
}
