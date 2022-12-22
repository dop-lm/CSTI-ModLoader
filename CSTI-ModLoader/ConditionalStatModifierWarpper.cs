using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ConditionalStatModifierWarpper : WarpperBase
    {
        public ConditionalStatModifierWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref ConditionalStatModifier obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "Stat", StatWarpType, StatWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredCardsOnBoard", RequiredCardsOnBoardWarpType, RequiredCardsOnBoardWarpData, SrcPath);

            obj = (ConditionalStatModifier)box;
        }

        // Object Name
        public String ObjectName;

        // Stat: GameStat
        public WarpperFunction.WarpType StatWarpType;
        public String StatWarpData;

        // RequiredCardsOnBoard: CardData[]
        public WarpperFunction.WarpType RequiredCardsOnBoardWarpType;
        public List<String> RequiredCardsOnBoardWarpData;
    }
}
