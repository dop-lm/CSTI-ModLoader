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

        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardActionWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardActionWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //}

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
            WarpperFunction.ClassWarpper(obj, "StatInterruptions", StatInterruptionsWarpType, StatInterruptionsWarpData, SrcPath);
            WarpperFunction.ClassWarpper(obj, "ProducedCards", ProducedCardsWarpType, ProducedCardsWarpData, SrcPath);

        }

        //public void WarpperCustom(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardActionWarpper WarpperCustom Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.ObjectCustomWarpper(obj, data, field_name, this);
        //    //using (StreamReader sr = new StreamReader(SrcPath + "\\" + data))
        //    //    UnityEngine.JsonUtility.FromJsonOverwrite(sr.ReadToEnd(), this);
        //    //var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    //var field = obj.GetType().GetField(field_name, bindingFlags);
        //    //var sub_obj = field.GetValue(obj) as CardAction;
        //    //WarpperCustom(sub_obj);
        //}
        //public void WarpperCustom(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardActionWarpper WarpperCustom List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.ObjectCustomWarpper(obj, data, field_name, this);
        //}

        // Object Name
        public String ObjectName;

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
