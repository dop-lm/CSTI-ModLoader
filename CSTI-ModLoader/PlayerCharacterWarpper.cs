﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class PlayerCharacterWarpper : WarpperBase
    {
        public PlayerCharacterWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(PlayerCharacter obj)
        {
            WarpperFunction.ClassWarpper(obj, "CharacterPortrait", CharacterPortraitWarpType, CharacterPortraitWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "Environment", EnvironmentWarpType, EnvironmentWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "Locations", LocationsWarpType, LocationsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "BaseAndItemCards", BaseAndItemCardsWarpType, BaseAndItemCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "StartingClothes", StartingClothesWarpType, StartingClothesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "InitialStatModifiers", InitialStatModifiersWarpType, InitialStatModifiersWarpData, SrcPath);

            //WarpperFunction.ClassWarpper(obj, "Journal", JournalWarpType, JournalWarpData, SrcPath);

            //WarpperFunction.ClassWarpper(obj, "Guide", GuideWarpType, GuideWarpData, SrcPath);

            //WarpperFunction.ClassWarpper(obj, "EasyPackage", EasyPackageWarpType, EasyPackageWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CharacterPerks", CharacterPerksWarpType, CharacterPerksWarpData, SrcPath);

        }

        // CharacterPortrait: Sprite
        public WarpperFunction.WarpType CharacterPortraitWarpType;
        public String CharacterPortraitWarpData;

        // Environment: CardData
        public WarpperFunction.WarpType EnvironmentWarpType;
        public String EnvironmentWarpData;

        // Weather: CardData
        public WarpperFunction.WarpType WeatherWarpType;
        public String WeatherWarpData;

        // Locations: CardData[]
        public WarpperFunction.WarpType LocationsWarpType;
        public List<String> LocationsWarpData;

        // BaseAndItemCards: CardData[]
        public WarpperFunction.WarpType BaseAndItemCardsWarpType;
        public List<String> BaseAndItemCardsWarpData;

        // StartingClothes: CardData[]
        public WarpperFunction.WarpType StartingClothesWarpType;
        public List<String> StartingClothesWarpData;

        // InitialStatModifiers: StatModifier[]
        public WarpperFunction.WarpType InitialStatModifiersWarpType;
        public List<String> InitialStatModifiersWarpData;

        // Journal: ContentDisplayer
        public WarpperFunction.WarpType JournalWarpType;
        public String JournalWarpData;

        // Guide: ContentDisplayer
        public WarpperFunction.WarpType GuideWarpType;
        public String GuideWarpData;

        // EasyPackage: GameModifierPackage
        public WarpperFunction.WarpType EasyPackageWarpType;
        public String EasyPackageWarpData;

        // CharacterPerks: List<CharacterPerk>
        public WarpperFunction.WarpType CharacterPerksWarpType;
        public List<String> CharacterPerksWarpData;
    }
}