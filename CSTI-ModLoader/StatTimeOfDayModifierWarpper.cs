using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class StatTimeOfDayModifierWarpper : WarpperBase
    {
        public StatTimeOfDayModifierWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref StatTimeOfDayModifier obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "RequiredCardsOrTagsOnBoard", RequiredCardsOrTagsOnBoardWarpType, RequiredCardsOrTagsOnBoardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredStatValues", RequiredStatValuesWarpType, RequiredStatValuesWarpData, SrcPath);

            obj = (StatTimeOfDayModifier)box;
        }

        // RequiredCardsOrTagsOnBoard: CardOrTagRef[]
        public WarpperFunction.WarpType RequiredCardsOrTagsOnBoardWarpType;
        public List<string> RequiredCardsOrTagsOnBoardWarpData;

        // RequiredStatValues: StatValueTrigger[]
        public WarpperFunction.WarpType RequiredStatValuesWarpType;
        public List<string> RequiredStatValuesWarpData;
    }
}
