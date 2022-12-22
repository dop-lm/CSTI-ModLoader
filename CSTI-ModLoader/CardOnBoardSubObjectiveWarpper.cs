using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardOnBoardSubObjectiveWarpper : WarpperBase
    {
        public CardOnBoardSubObjectiveWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CardOnBoardSubObjective obj)
        {
            WarpperFunction.ClassWarpper(obj, "Card", CardWarpType, CardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "OnlyInEnvironment", OnlyInEnvironmentWarpType, OnlyInEnvironmentWarpData, SrcPath);
        }

        // Card: CardData
        public WarpperFunction.WarpType CardWarpType;
        public string CardWarpData;

        // OnlyInEnvironment: CardData
        public WarpperFunction.WarpType OnlyInEnvironmentWarpType;
        public string OnlyInEnvironmentWarpData;
    }
}
