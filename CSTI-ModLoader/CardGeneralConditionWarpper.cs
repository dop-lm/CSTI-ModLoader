using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardGeneralConditionWarpper : WarpperBase
    {
        public CardGeneralConditionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref CardGeneralCondition obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "Card", CardWarpType, CardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "Tag", TagWarpType, TagWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "Liquid", LiquidWarpType, LiquidWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "LiquidTag", LiquidTagWarpType, LiquidTagWarpData, SrcPath);

            obj = (CardGeneralCondition)box;
        }

        // Card: CardData
        public WarpperFunction.WarpType CardWarpType;
        public String CardWarpData;

        // Tag: CardTag
        public WarpperFunction.WarpType TagWarpType;
        public String TagWarpData;

        // Liquid: CardData
        public WarpperFunction.WarpType LiquidWarpType;
        public String LiquidWarpData;

        // LiquidTag: CardTag
        public WarpperFunction.WarpType LiquidTagWarpType;
        public String LiquidTagWarpData;
    }
}
