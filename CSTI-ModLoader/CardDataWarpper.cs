using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModLoader
{
    [Serializable]
    public class CardDataWarpper : WarpperBase
    {
        public CardDataWarpper(string SrcPath) : base(SrcPath) { }
        //public void WarpperCopy(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDataWarpper WarpperCopy Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //    //if (ModLoader.AllGUIDDict.TryGetValue(data, out var ele))
        //    //{
        //    //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    //    if (field != ele.GetType().GetField(field_name, bindingFlags))
        //    //    {
        //    //        UnityEngine.Debug.LogError("CardDataWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name + "Field not Same");
        //    //        return;
        //    //    }
        //    //    field.SetValue(obj, field.GetValue(ele));
        //    //}
        //}

        //public void WarpperCopy(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDataWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.UniqueIDScriptableCopyWarpper(obj, data, field_name);
        //    //if (data.Count > 0 && ModLoader.AllGUIDDict.TryGetValue(data[0], out var ele))
        //    //{
        //    //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    //    if (field != ele.GetType().GetField(field_name, bindingFlags))
        //    //    {
        //    //        UnityEngine.Debug.LogError("CardDataWarpper WarpperCopy List " + obj.GetType().Name + "." + field_name + "Field not Same");
        //    //        return;
        //    //    }
        //    //    field.SetValue(obj, field.GetValue(ele));
        //    //}
        //}

        public void WarpperCustomSelf(CardData instance)
        {
            WarpperFunction.ClassWarpper(instance, "CardImage", CardImageWarpType, CardImageWarpData, SrcPath);

            WarpperFunction.ClassWarpper(instance, "CardBackground", CardBackgroundWarpType, CardBackgroundWarpData, SrcPath);

            WarpperFunction.ClassWarpper(instance, "CardTags", CardTagsWarpType, CardTagsWarpData, SrcPath);

            //WarpperFunction.EquipmentTagWarpper(instance, "EquipmentTags", EquipmentTagsWarpType, EquipmentTagsWarpData);

            //WarpperFunction.ListOrArrayJsonWarpper(instance, "LiquidValidContainers", LiquidValidContainersWarpType, LiquidValidContainersWarpData, "", SrcPath);

            //WarpperFunction.CardDataWarpper(instance, "SpawningBlockedBy", SpawningBlockedByWarpType, SpawningBlockedByWarpData);

            //WarpperFunction.CardDataWarpper(instance, "CarriesOverTo", CarriesOverToWarpType, CarriesOverToWarpData);

            //WarpperFunction.AudioClipWarpper(instance, "WhenCreatedSounds", WhenCreatedSoundsWarpType, WhenCreatedSoundsWarpData);

            //WarpperFunction.AmbienceSettingsWarpper(instance, "Ambience", AmbienceWarpType, AmbienceWarpData);

            //WarpperFunction.ListOrArrayJsonWarpper(instance, "VisualEffects", VisualEffectsWarpType, VisualEffectsWarpData, "", SrcPath);

            //WarpperFunction.SpriteWarpper(instance, "LocationsBackground", LocationsBackgroundWarpType, LocationsBackgroundWarpData);

            //WarpperFunction.SpriteWarpper(instance, "BaseBackground", BaseBackgroundWarpType, BaseBackgroundWarpData);

            //WarpperFunction.WeatherSetWarpper(instance, "WeatherEffects", WeatherSetWarpType, WeatherSetWarpData);

            //WarpperFunction.DurabilityStatWarpper(instance, "SpoilageTime", SpoilageTimeWarpType, WSpoilageTimeWarpData);

            WarpperFunction.ClassWarpper(instance, "DismantleActions", DismantleActionsWarpType, DismantleActionsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(instance, "EnvironmentImprovements", EnvironmentImprovementsWarpType, EnvironmentImprovementsWarpData, SrcPath);
        }

        //public void WarpperReference(System.Object obj, string data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDataWarpper WarpperReference Single " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
        //    //if (ModLoader.AllGUIDDict.TryGetValue(data, out var ele) && ele is CardData)
        //    //{
        //    //    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    //    var field = obj.GetType().GetField(field_name, bindingFlags);
        //    //    field.SetValue(obj, ele);
        //    //}
        //}

        //public void WarpperReference(System.Object obj, List<string> data, string field_name)
        //{
        //    UnityEngine.Debug.Log("CardDataWarpper WarpperReference List " + obj.GetType().Name + "." + field_name);
        //    WarpperFunction.ObjectReferenceWarpper(obj, data, field_name, ModLoader.AllGUIDDict);
        //    //var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        //    //var field = obj.GetType().GetField(field_name, bindingFlags);
        //    //if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
        //    //{
        //    //    var target = field.GetValue(obj) as List<CardData>;
        //    //    foreach (var name in data)
        //    //        if (ModLoader.AllGUIDDict.TryGetValue(name, out var ele) && ele is CardData)
        //    //            target.Add(ele as CardData);
        //    //}
        //    //else if (field.FieldType.IsArray)
        //    //{
        //    //    var target = field.GetValue(obj) as CardData[];
        //    //    Array.Resize<CardData>(ref target, data.Count);
        //    //    for (int i = 0; i < data.Count; i++)
        //    //        if (ModLoader.AllGUIDDict.TryGetValue(data[i], out var ele) && ele is CardData)
        //    //            target[i] = ele as CardData;
        //    //    field.SetValue(obj, target);
        //    //}
        //}

        // Object Name
        public String ObjectName;

        // CardImage: Sprite
        public WarpperFunction.WarpType CardImageWarpType;
        public String CardImageWarpData;

        // CardBackground: Sprite
        public WarpperFunction.WarpType CardBackgroundWarpType;
        public String CardBackgroundWarpData;

        // CardTags: CardTag[]
        public WarpperFunction.WarpType CardTagsWarpType;
        public List<String> CardTagsWarpData;

        // EquipmentTags: EquipmentTag[]
        public WarpperFunction.WarpType EquipmentTagsWarpType;
        public List<String> EquipmentTagsWarpData;

        // LiquidValidContainers: CardOrTagRef[]
        public WarpperFunction.WarpType LiquidValidContainersWarpType;
        public List<String> LiquidValidContainersWarpData;

        // SpawningBlockedBy: CardData[]
        public WarpperFunction.WarpType SpawningBlockedByWarpType;
        public List<String> SpawningBlockedByWarpData;

        // CarriesOverTo: List<CardData>
        public WarpperFunction.WarpType CarriesOverToWarpType;
        public List<String> CarriesOverToWarpData;

        // WhenCreatedSounds: AudioClip[]
        public WarpperFunction.WarpType WhenCreatedSoundsWarpType;
        public List<String> WhenCreatedSoundsWarpData;

        // Ambience: AmbienceSettings
        public WarpperFunction.WarpType AmbienceWarpType;
        public String AmbienceWarpData;

        // VisualEffects: WeatherSpecialEffect[]
        public WarpperFunction.WarpType VisualEffectsWarpType;
        public List<String> VisualEffectsWarpData;

        // LocationsBackground: Sprite
        public WarpperFunction.WarpType LocationsBackgroundWarpType;
        public String LocationsBackgroundWarpData;

        // BaseBackground: Sprite
        public WarpperFunction.WarpType BaseBackgroundWarpType;
        public String BaseBackgroundWarpData;

        // WeatherEffects: Sprite
        public WarpperFunction.WarpType WeatherEffectsWarpType;
        public String WeatherEffectsWarpData;

        // SpoilageTime: DurabilityStat
        public WarpperFunction.WarpType SpoilageTimeWarpType;
        public String SpoilageTimeWarpData;

        // UsageDurability: DurabilityStat
        public WarpperFunction.WarpType UsageDurabilityWarpType;
        public String UsageDurabilityWarpData;

        // FuelCapacity: DurabilityStat
        public WarpperFunction.WarpType FuelCapacityWarpType;
        public String FuelCapacityWarpData;

        // Progress: DurabilityStat
        public WarpperFunction.WarpType ProgressWarpType;
        public String ProgressWarpData;

        // SpecialDurability1: DurabilityStat
        public WarpperFunction.WarpType SpecialDurability1WarpType;
        public String SpecialDurability1WarpData;

        // SpecialDurability2: DurabilityStat
        public WarpperFunction.WarpType SpecialDurability2WarpType;
        public String SpecialDurability2WarpData;

        // SpecialDurability3: DurabilityStat
        public WarpperFunction.WarpType SpecialDurability3WarpType;
        public String SpecialDurability3WarpData;

        // SpecialDurability4: DurabilityStat
        public WarpperFunction.WarpType SpecialDurability4WarpType;
        public String SpecialDurability4WarpData;

        // CardInteractions: CardOnCardAction[]
        public WarpperFunction.WarpType CardInteractionsWarpType;
        public List<String> CardInteractionsWarpData;

        // OnStatsChangeActions: FromStatChangeAction[]
        public WarpperFunction.WarpType OnStatsChangeActionsWarpType;
        public List<String> OnStatsChangeActionsWarpData;

        // DismantleActions: List<DismantleCardAction>
        public WarpperFunction.WarpType DismantleActionsWarpType;
        public List<String> DismantleActionsWarpData;

        // PassiveStatEffects: StatModifier[]
        public WarpperFunction.WarpType PassiveStatEffectsWarpType;
        public List<String> PassiveStatEffectsWarpData;

        // PassiveEffects: PassiveEffect[]
        public WarpperFunction.WarpType PassiveEffectsWarpType;
        public List<String> PassiveEffectsWarpData;

        // RemotePassiveEffects: RemotePassiveEffect[]
        public WarpperFunction.WarpType RemotePassiveEffectsWarpType;
        public List<String> RemotePassiveEffectsWarpData;

        // ActiveCounters: LocalTickCounterRef[]
        public WarpperFunction.WarpType ActiveCountersWarpType;
        public List<String> ActiveCountersWarpData;

        // LocalCounterEffects: LocalTickCounterRef[]
        public WarpperFunction.WarpType LocalCounterEffectsWarpType;
        public List<String> LocalCounterEffectsWarpData;

        // DroppedOnDestroy: CardsDropCollection[]
        public WarpperFunction.WarpType DroppedOnDestroyWarpType;
        public List<String> DroppedOnDestroyWarpData;

        // DefaultLiquidContained: LiquidDrop
        public WarpperFunction.WarpType DefaultLiquidContainedWarpType;
        public String DefaultLiquidContainedWarpData;

        // DefaultLiquidImage: Sprite
        public WarpperFunction.WarpType DefaultLiquidImageWarpType;
        public String DefaultLiquidImageWarpData;

        // LiquidImages: LiquidVisuals[]
        public WarpperFunction.WarpType LiquidImagesWarpType;
        public List<String> LiquidImagesWarpData;

        // ExclusivelyAcceptedLiquids: CardOrTagRef[]
        public WarpperFunction.WarpType ExclusivelyAcceptedLiquidsWarpType;
        public List<String> ExclusivelyAcceptedLiquidsWarpData;

        // ExclusivelyAcceptedLiquids: CardOrTagRef[]
        public WarpperFunction.WarpType NOTAcceptedLiquidsWarpType;
        public List<String> NOTAcceptedLiquidsWarpData;

        // ContainedLiquidTransform: CardData
        public WarpperFunction.WarpType ContainedLiquidTransformWarpType;
        public String ContainedLiquidTransformWarpData;

        // InventorySlots: CardData[]
        public WarpperFunction.WarpType InventorySlotsWarpType;
        public List<String> InventorySlotsWarpData;

        // InventoryFilter: CardFilter
        public WarpperFunction.WarpType InventoryFilterWarpType;
        public String InventoryFilterWarpData;

        // CookingConditions: CookingConditions
        public WarpperFunction.WarpType CookingConditionsWarpType;
        public String CookingConditionsWarpData;

        // CookingRecipes: CookingRecipe[]
        public WarpperFunction.WarpType CookingRecipesWarpType;
        public List<String> CookingRecipesWarpData;

        // CookingSprite: Sprite
        public WarpperFunction.WarpType CookingSpriteWarpType;
        public String CookingSpriteWarpData;

        // EffectsToInventoryContent: PassiveEffect[]
        public WarpperFunction.WarpType EffectsToInventoryContentWarpType;
        public List<String> EffectsToInventoryContentWarpData;

        // CardsOnBoard: List<CardOnBoardSubObjective>
        public WarpperFunction.WarpType CardsOnBoardWarpType;
        public List<String> CardsOnBoardWarpData;

        // TagsOnBoard: List<TagOnBoardSubObjective>
        public WarpperFunction.WarpType TagsOnBoardWarpType;
        public List<String> TagsOnBoardWarpData;

        // StatValues: List<StatSubObjective>
        public WarpperFunction.WarpType StatValuesWarpType;
        public List<String> StatValuesWarpData;

        // TimeValues: List<TimeObjective>
        public WarpperFunction.WarpType TimeValuesWarpType;
        public List<String> TimeValuesWarpData;

        // CompletedObjectives: List<ObjectiveSubObjective>
        public WarpperFunction.WarpType CompletedObjectivesWarpType;
        public List<String> CompletedObjectivesWarpData;

        // OnUnlockedLog: EndgameLog
        public WarpperFunction.WarpType OnUnlockedLogWarpType;
        public String OnUnlockedLogWarpData;

        // ExplicitBlueprintNeeded: CardData
        public WarpperFunction.WarpType ExplicitBlueprintNeededWarpType;
        public String ExplicitBlueprintNeededWarpData;

        // BlueprintCardConditions: CardOnBoardCondition[]
        public WarpperFunction.WarpType BlueprintCardConditionsWarpType;
        public List<String> BlueprintCardConditionsWarpData;

        // BlueprintTagConditions: TagOnBoardCondition[]
        public WarpperFunction.WarpType BlueprintTagConditionsWarpType;
        public List<String> BlueprintTagConditionsWarpData;

        // BlueprintStatConditions: StatValueTrigger[]
        public WarpperFunction.WarpType BlueprintStatConditionsWarpType;
        public List<String> BlueprintStatConditionsWarpData;

        // BuildingCardConditions: CardOnBoardCondition[]
        public WarpperFunction.WarpType BuildingCardConditionsWarpType;
        public List<String> BuildingCardConditionsWarpData;

        // BuildingTagConditions: TagOnBoardCondition[]
        public WarpperFunction.WarpType BuildingTagConditionsWarpType;
        public List<String> BuildingTagConditionsWarpData;

        // BlueprintStatConditions: StatValueTrigger[]
        public WarpperFunction.WarpType BuildingStatConditionsWarpType;
        public List<String> BuildingStatConditionsWarpData;

        // BlueprintStages: BlueprintStage[]
        public WarpperFunction.WarpType BlueprintStagesWarpType;
        public List<String> BlueprintStagesWarpData;

        // BuildSounds AudioClip[]
        public WarpperFunction.WarpType BuildSoundsWarpType;
        public List<String> BuildSoundsWarpData;

        // DeconstructSounds: AudioClip[]
        public WarpperFunction.WarpType DeconstructSoundsWarpType;
        public List<String> DeconstructSoundsWarpData;

        // BlueprintActionTags: ActionTag[]
        public WarpperFunction.WarpType BlueprintActionTagsWarpType;
        public List<String> BlueprintActionTagsWarpData;

        // BlueprintResult: CardDrop[]
        public WarpperFunction.WarpType BlueprintResultWarpType;
        public List<String> BlueprintResultWarpData;

        // BlueprintStatModifications: StatModifier[]
        public WarpperFunction.WarpType BlueprintStatModificationsWarpType;
        public List<String> BlueprintStatModificationsWarpData;

        // BlueprintCardModifications: ExtraDurabilityChange[]
        public WarpperFunction.WarpType BlueprintCardModificationsWarpType;
        public List<String> BlueprintCardModificationsWarpData;

        // BlueprintFinishedLog: EndgameLog
        public WarpperFunction.WarpType BlueprintFinishedLogWarpType;
        public String BlueprintFinishedLogWarpData;

        // ExplorationResults: ExplorationResult[]
        public WarpperFunction.WarpType ExplorationResultsWarpType;
        public List<String> ExplorationResultsWarpData;

        // EnvironmentImprovements: CardDataRef[]
        public WarpperFunction.WarpType EnvironmentImprovementsWarpType;
        public List<String> EnvironmentImprovementsWarpData;

        // EnvironmentDamages: CardDataRef[]
        public WarpperFunction.WarpType EnvironmentDamagesWarpType;
        public List<String> EnvironmentDamagesWarpData;

        // DefaultEnvCards: CardData[]
        public WarpperFunction.WarpType DefaultEnvCardsWarpType;
        public List<String> DefaultEnvCardsWarpData;
    }
}
