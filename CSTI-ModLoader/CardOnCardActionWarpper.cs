using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardOnCardActionWarpper : CardActionWarpper
    {
        public CardOnCardActionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CardOnCardAction obj)
        {
            base.WarpperCustomSelf(obj);

            WarpperFunction.ClassWarpper(obj, "RequiredGivenContainer", RequiredGivenContainerWarpType, RequiredGivenContainerWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RequiredGivenContainerTag", RequiredGivenContainerTagWarpType, RequiredGivenContainerTagWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RequiredGivenLiquidContent", RequiredGivenLiquidContentWarpType, RequiredGivenLiquidContentWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "GivenCardChanges", GivenCardChangesWarpType, GivenCardChangesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CreatedLiquidInGivenCard", CreatedLiquidInGivenCardWarpType, CreatedLiquidInGivenCardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CompatibleCards", CompatibleCardsWarpType, CompatibleCardsWarpData, SrcPath);
        }

        // RequiredGivenContainer: CardData[]
        public WarpperFunction.WarpType RequiredGivenContainerWarpType;
        public List<String> RequiredGivenContainerWarpData;

        // RequiredGivenContainerTag: CardTag[]
        public WarpperFunction.WarpType RequiredGivenContainerTagWarpType;
        public List<String> RequiredGivenContainerTagWarpData;

        // RequiredGivenLiquidContent: LiquidContentCondition
        public WarpperFunction.WarpType RequiredGivenLiquidContentWarpType;
        public String RequiredGivenLiquidContentWarpData;

        // GivenCardChanges: CardStateChange
        public WarpperFunction.WarpType GivenCardChangesWarpType;
        public String GivenCardChangesWarpData;

        // CreatedLiquidInGivenCard: LiquidDrop
        public WarpperFunction.WarpType CreatedLiquidInGivenCardWarpType;
        public String CreatedLiquidInGivenCardWarpData;

        // CompatibleCards: CardInteractionTrigger
        public WarpperFunction.WarpType CompatibleCardsWarpType;
        public String CompatibleCardsWarpData;
    }
}
