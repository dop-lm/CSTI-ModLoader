using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class BlueprintElementWarpper : WarpperBase
    {
        public BlueprintElementWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref BlueprintElement obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "RequiredCard", RequiredCardWarpType, RequiredCardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredTabGroup", RequiredTabGroupWarpType, RequiredTabGroupWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredLiquidContent", RequiredLiquidContentWarpType, RequiredLiquidContentWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "EffectOnIngredient", EffectOnIngredientWarpType, EffectOnIngredientWarpData, SrcPath);

            obj = (BlueprintElement)box;
        }

        // RequiredCard: CardData
        public WarpperFunction.WarpType RequiredCardWarpType;
        public String RequiredCardWarpData;

        // RequiredTabGroup: CardTabGroup
        public WarpperFunction.WarpType RequiredTabGroupWarpType;
        public String RequiredTabGroupWarpData;

        // RequiredLiquidContent: LiquidContentCondition
        public WarpperFunction.WarpType RequiredLiquidContentWarpType;
        public String RequiredLiquidContentWarpData;

        // EffectOnIngredient: CardStateChange
        public WarpperFunction.WarpType EffectOnIngredientWarpType;
        public String EffectOnIngredientWarpData;
    }
}
