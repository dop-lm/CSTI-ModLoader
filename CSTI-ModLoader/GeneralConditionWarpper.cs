using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class GeneralConditionWarpper : WarpperBase
    {
        public GeneralConditionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref GeneralCondition obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "RequiredEnvironment", RequiredEnvironmentWarpType, RequiredEnvironmentWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredEnvironmentTags", RequiredEnvironmentTagsWarpType, RequiredEnvironmentTagsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredContainer", RequiredContainerWarpType, RequiredContainerWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredContainerTags", RequiredContainerTagsWarpType, RequiredContainerTagsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredCardsOnBoard", RequiredCardsOnBoardWarpType, RequiredCardsOnBoardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredTagsOnBoard", RequiredTagsOnBoardWarpType, RequiredTagsOnBoardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredCardsNOTOnBoard", RequiredCardsNOTOnBoardWarpType, RequiredCardsNOTOnBoardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredCardsNOTOnBoard", RequiredCardsNOTOnBoardWarpType, RequiredCardsNOTOnBoardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredLiquidContainers", RequiredLiquidContainersWarpType, RequiredLiquidContainersWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredCardsInInventory", RequiredCardsInInventoryWarpType, RequiredCardsInInventoryWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredTagsInInventory", RequiredTagsInInventoryWarpType, RequiredTagsInInventoryWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "RequiredStatValues", RequiredStatValuesWarpType, RequiredStatValuesWarpData, SrcPath);

            obj = (GeneralCondition)box;
        }

        // RequiredEnvironment: CardData
        public WarpperFunction.WarpType RequiredEnvironmentWarpType;
        public String RequiredEnvironmentWarpData;

        // RequiredEnvironmentTags: CardTag[]
        public WarpperFunction.WarpType RequiredEnvironmentTagsWarpType;
        public List<String> RequiredEnvironmentTagsWarpData;

        // RequiredContainer: CardData[]
        public WarpperFunction.WarpType RequiredContainerWarpType;
        public List<String> RequiredContainerWarpData;

        // RequiredContainerTags: CardTag[]
        public WarpperFunction.WarpType RequiredContainerTagsWarpType;
        public List<String> RequiredContainerTagsWarpData;

        // RequiredCardsOnBoard: CardData[]
        public WarpperFunction.WarpType RequiredCardsOnBoardWarpType;
        public List<String> RequiredCardsOnBoardWarpData;

        // RequiredTagsOnBoard: CardTag[]
        public WarpperFunction.WarpType RequiredTagsOnBoardWarpType;
        public List<String> RequiredTagsOnBoardWarpData;

        // RequiredCardsNOTOnBoard: CardData[]
        public WarpperFunction.WarpType RequiredCardsNOTOnBoardWarpType;
        public List<String> RequiredCardsNOTOnBoardWarpData;

        // RequiredContainerTags: CardTag[]
        public WarpperFunction.WarpType RequiredTagsNOTOnBoardWarpType;
        public List<String> RequiredTagsNOTOnBoardWarpData;

        // RequiredLiquidContainers: CardGeneralCondition[]
        public WarpperFunction.WarpType RequiredLiquidContainersWarpType;
        public List<String> RequiredLiquidContainersWarpData;

        // RequiredCardsInInventory: CardData[]
        public WarpperFunction.WarpType RequiredCardsInInventoryWarpType;
        public List<String> RequiredCardsInInventoryWarpData;

        // RequiredTagsInInventory: CardTag[]
        public WarpperFunction.WarpType RequiredTagsInInventoryWarpType;
        public List<String> RequiredTagsInInventoryWarpData;

        // RequiredStatValues: StatValueTrigger[]
        public WarpperFunction.WarpType RequiredStatValuesWarpType;
        public List<String> RequiredStatValuesWarpData;
    }
}
