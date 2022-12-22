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
        public void WarpperCustomSelf(CardData obj)
        {
            WarpperFunction.ClassWarpper(obj, "CardImage", CardImageWarpType, CardImageWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CardBackground", CardBackgroundWarpType, CardBackgroundWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CardTags", CardTagsWarpType, CardTagsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "EquipmentTags", EquipmentTagsWarpType, EquipmentTagsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "LiquidValidContainers", LiquidValidContainersWarpType, LiquidValidContainersWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "SpawningBlockedBy", SpawningBlockedByWarpType, SpawningBlockedByWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CarriesOverTo", CarriesOverToWarpType, CarriesOverToWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "WhenCreatedSounds", WhenCreatedSoundsWarpType, WhenCreatedSoundsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "Ambience", AmbienceWarpType, AmbienceWarpData, SrcPath);

            //WarpperFunction.ClassWarpper(obj, "VisualEffects", VisualEffectsWarpType, VisualEffectsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "LocationsBackground", LocationsBackgroundWarpType, LocationsBackgroundWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "BaseBackground", BaseBackgroundWarpType, BaseBackgroundWarpData, SrcPath);

            //WarpperFunction.ClassWarpper(obj, "WeatherEffects", WeatherEffectsWarpType, WeatherEffectsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "SpoilageTime", SpoilageTimeWarpType, SpoilageTimeWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "UsageDurability", UsageDurabilityWarpType, UsageDurabilityWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "FuelCapacity", FuelCapacityWarpType, FuelCapacityWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "Progress", ProgressWarpType, ProgressWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "SpecialDurability1", SpecialDurability1WarpType, SpecialDurability1WarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "SpecialDurability2", SpecialDurability2WarpType, SpecialDurability2WarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "SpecialDurability3", SpecialDurability3WarpType, SpecialDurability3WarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "SpecialDurability4", SpecialDurability4WarpType, SpecialDurability4WarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CardInteractions", CardInteractionsWarpType, CardInteractionsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "OnStatsChangeActions", OnStatsChangeActionsWarpType, OnStatsChangeActionsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "DismantleActions", DismantleActionsWarpType, DismantleActionsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "PassiveStatEffects", PassiveStatEffectsWarpType, PassiveStatEffectsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "PassiveEffects", PassiveEffectsWarpType, PassiveEffectsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RemotePassiveEffects", RemotePassiveEffectsWarpType, RemotePassiveEffectsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ActiveCounters", ActiveCountersWarpType, ActiveCountersWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "LocalCounterEffects", LocalCounterEffectsWarpType, LocalCounterEffectsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "DroppedOnDestroy", DroppedOnDestroyWarpType, DroppedOnDestroyWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "DefaultLiquidContained", DefaultLiquidContainedWarpType, DefaultLiquidContainedWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "DefaultLiquidImage", DefaultLiquidImageWarpType, DefaultLiquidImageWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "LiquidImages", LiquidImagesWarpType, LiquidImagesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ExclusivelyAcceptedLiquids", ExclusivelyAcceptedLiquidsWarpType, ExclusivelyAcceptedLiquidsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "NOTAcceptedLiquids", NOTAcceptedLiquidsWarpType, NOTAcceptedLiquidsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ContainedLiquidTransform", ContainedLiquidTransformWarpType, ContainedLiquidTransformWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "InventorySlots", InventorySlotsWarpType, InventorySlotsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "InventoryFilter", InventoryFilterWarpType, InventoryFilterWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CookingConditions", CookingConditionsWarpType, CookingConditionsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CookingRecipes", CookingRecipesWarpType, CookingRecipesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CookingSprite", CookingSpriteWarpType, CookingSpriteWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "EffectsToInventoryContent", EffectsToInventoryContentWarpType, EffectsToInventoryContentWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CardsOnBoard", CardsOnBoardWarpType, CardsOnBoardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "TagsOnBoard", TagsOnBoardWarpType, TagsOnBoardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "StatValues", StatValuesWarpType, StatValuesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "TimeValues", TimeValuesWarpType, TimeValuesWarpData, SrcPath);

            //WarpperFunction.ClassWarpper(obj, "CompletedObjectives", CompletedObjectivesWarpType, CompletedObjectivesWarpData, SrcPath);




            WarpperFunction.ClassWarpper(obj, "EnvironmentImprovements", EnvironmentImprovementsWarpType, EnvironmentImprovementsWarpData, SrcPath);
        }

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
