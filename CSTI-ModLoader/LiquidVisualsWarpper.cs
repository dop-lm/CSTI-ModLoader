using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class LiquidVisualsWarpper : WarpperBase
    {
        public LiquidVisualsWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(LiquidVisuals obj)
        {
            WarpperFunction.ClassWarpper(obj, "LiquidCards", LiquidCardsWarpType, LiquidCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "LiquidImage", LiquidImageWarpType, LiquidImageWarpData, SrcPath);
        }

        // LiquidCards: List<CardData>
        public WarpperFunction.WarpType LiquidCardsWarpType;
        public List<string> LiquidCardsWarpData;

        // LiquidImage: Sprite
        public WarpperFunction.WarpType LiquidImageWarpType;
        public string LiquidImageWarpData;
    }
}
