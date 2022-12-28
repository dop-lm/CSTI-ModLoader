using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CharacterPerkWarpper : CompletableObjectWarpper
    {
        public CharacterPerkWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CharacterPerk obj)
        {
            base.WarpperCustomSelf(obj);

            WarpperFunction.ClassWarpper(obj, "PerkIcon", PerkIconWarpType, PerkIconWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "OverrideEnvironment", OverrideEnvironmentWarpType, OverrideEnvironmentWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "OverrideWeather", OverrideWeatherWarpType, OverrideWeatherWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "AddedCards", AddedCardsWarpType, AddedCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "EquippedCards", EquippedCardsWarpType, EquippedCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "StartingStatModifiers", StartingStatModifiersWarpType, StartingStatModifiersWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "PassiveStatModifiers", PassiveStatModifiersWarpType, PassiveStatModifiersWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "ActionModifiers", ActionModifiersWarpType, ActionModifiersWarpData, SrcPath);
        }

        // PerkGroup: PerkGroup
        public String CharacterPerkPerkGroup;

        // PerkIcon: Sprite
        public WarpperFunction.WarpType PerkIconWarpType;
        public String PerkIconWarpData;

        // OverrideEnvironment: CardData
        public WarpperFunction.WarpType OverrideEnvironmentWarpType;
        public String OverrideEnvironmentWarpData;

        // OverrideWeather: CardData
        public WarpperFunction.WarpType OverrideWeatherWarpType;
        public String OverrideWeatherWarpData;

        // AddedCards: CardData[]
        public WarpperFunction.WarpType AddedCardsWarpType;
        public List<String> AddedCardsWarpData;

        // EquippedCards: CardData[]
        public WarpperFunction.WarpType EquippedCardsWarpType;
        public List<String> EquippedCardsWarpData;

        // StartingStatModifiers: StatModifier[]
        public WarpperFunction.WarpType StartingStatModifiersWarpType;
        public List<String> StartingStatModifiersWarpData;

        // PassiveStatModifiers: StatModifier[]
        public WarpperFunction.WarpType PassiveStatModifiersWarpType;
        public List<String> PassiveStatModifiersWarpData;

        // ActionModifiers: ActionModifier[]
        public WarpperFunction.WarpType ActionModifiersWarpType;
        public List<String> ActionModifiersWarpData;
    }
}
