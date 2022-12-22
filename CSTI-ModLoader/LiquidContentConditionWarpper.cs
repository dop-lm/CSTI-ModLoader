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
            object box = obj;

            WarpperFunction.ClassWarpper(box, "RequiredLiquid", RequiredLiquidWarpType, RequiredLiquidWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredGroup", RequiredGroupWarpType, RequiredGroupWarpData, SrcPath);

            obj = (LiquidContentCondition)box;
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
