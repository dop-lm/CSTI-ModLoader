using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardActionWarpper : WarpperBase
    {
        public CardActionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CardAction obj)
        {
            WarpperFunction.ClassWarpper(obj, "ActionLog", ActionLogWarpType, ActionLogWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ActionSounds", ActionSoundsWarpType, ActionSoundsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ActionTags", ActionTagsWarpType, ActionTagsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RequiredStatValues", RequiredStatValuesWarpType, RequiredStatValuesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RequiredCardsOnBoard", RequiredCardsOnBoardWarpType, RequiredCardsOnBoardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RequiredTagsOnBoard", RequiredTagsOnBoardWarpType, RequiredTagsOnBoardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RequiredReceivingContainer", RequiredReceivingContainerWarpType, RequiredReceivingContainerWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RequiredReceivingContainerTag", RequiredReceivingContainerTagWarpType, RequiredReceivingContainerTagWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RequiredReceivingLiquidContent", RequiredReceivingLiquidContentWarpType, RequiredReceivingLiquidContentWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ResetWhenDone", ResetWhenDoneWarpType, ResetWhenDoneWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "StatInterruptions", StatInterruptionsWarpType, StatInterruptionsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ProducedCards", ProducedCardsWarpType, ProducedCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "StatModifications", StatModificationsWarpType, StatModificationsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ExtraDurabilityModifications", ExtraDurabilityModificationsWarpType, ExtraDurabilityModificationsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "BlueprintsFullUnlock", BlueprintsFullUnlockWarpType, BlueprintsFullUnlockWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ReceivingCardChanges", ReceivingCardChangesWarpType, ReceivingCardChangesWarpData, SrcPath);

            //WarpperFunction.ClassWarpper(obj, "CustomWindowPrefab", CustomWindowPrefabWarpType, CustomWindowPrefabWarpData, SrcPath);
        }

        // ActionLog: EndgameLog
        public WarpperFunction.WarpType ActionLogWarpType;
        public String ActionLogWarpData;

        // ActionSounds: UnityEngine.AudioClip[]
        public WarpperFunction.WarpType ActionSoundsWarpType;
        public List<String> ActionSoundsWarpData;

        // ActionTags: ActionTag[]
        public WarpperFunction.WarpType ActionTagsWarpType;
        public List<String> ActionTagsWarpData;

        // RequiredStatValues: StatValueTrigger[]
        public WarpperFunction.WarpType RequiredStatValuesWarpType;
        public List<String> RequiredStatValuesWarpData;

        // RequiredCardsOnBoard: CardOnBoardCondition[]
        public WarpperFunction.WarpType RequiredCardsOnBoardWarpType;
        public List<String> RequiredCardsOnBoardWarpData;

        // RequiredTagsOnBoard: TagOnBoardCondition[]
        public WarpperFunction.WarpType RequiredTagsOnBoardWarpType;
        public List<String> RequiredTagsOnBoardWarpData;

        // RequiredReceivingContainer: CardData[]
        public WarpperFunction.WarpType RequiredReceivingContainerWarpType;
        public List<String> RequiredReceivingContainerWarpData;

        // RequiredReceivingContainerTag: CardTag[]
        public WarpperFunction.WarpType RequiredReceivingContainerTagWarpType;
        public List<String> RequiredReceivingContainerTagWarpData;

        // RequiredReceivingLiquidContent: LiquidContentCondition
        public WarpperFunction.WarpType RequiredReceivingLiquidContentWarpType;
        public String RequiredReceivingLiquidContentWarpData;

        // ResetWhenDone: GameStat
        public WarpperFunction.WarpType ResetWhenDoneWarpType;
        public String ResetWhenDoneWarpData;

        // StatInterruptions: StatInterruptionCondition[]
        public WarpperFunction.WarpType StatInterruptionsWarpType;
        public List<String> StatInterruptionsWarpData;

        // ProducedCards: CardsDropCollection[]
        public WarpperFunction.WarpType ProducedCardsWarpType;
        public List<String> ProducedCardsWarpData;


        // StatModifications: StatModifier[]
        public WarpperFunction.WarpType StatModificationsWarpType;
        public List<String> StatModificationsWarpData;

        // ExtraDurabilityModifications: ExtraDurabilityChange[]
        public WarpperFunction.WarpType ExtraDurabilityModificationsWarpType;
        public List<String> ExtraDurabilityModificationsWarpData;

        // BlueprintsFullUnlock: CardData[]
        public WarpperFunction.WarpType BlueprintsFullUnlockWarpType;
        public List<String> BlueprintsFullUnlockWarpData;

        // ReceivingCardChanges: CardStateChange
        public WarpperFunction.WarpType ReceivingCardChangesWarpType;
        public String ReceivingCardChangesWarpData;

        // CustomWindowPrefab: UnityEngine.GameObject
        public WarpperFunction.WarpType CustomWindowPrefabWarpType;
        public String CustomWindowPrefabWarpData;
    }
}
