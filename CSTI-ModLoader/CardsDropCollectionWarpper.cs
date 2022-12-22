using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardsDropCollectionWarpper : WarpperBase
    {
        public CardsDropCollectionWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CardsDropCollection obj)
        {
            WarpperFunction.ClassWarpper(obj, "StatsDropChanceModifiers", StatsDropChanceModifiersWarpType, StatsDropChanceModifiersWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CardDropChanceModifiers", CardDropChanceModifiersWarpType, CardDropChanceModifiersWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CreatedLiquid", CreatedLiquidWarpType, CreatedLiquidWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "DroppedCards", DroppedCardsWarpType, DroppedCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "StatModifications", StatModificationsWarpType, StatModificationsWarpData, SrcPath);
        }

        // StatsDropChanceModifiers: StatBasedDropChanceModifier[]
        public WarpperFunction.WarpType StatsDropChanceModifiersWarpType;
        public List<string> StatsDropChanceModifiersWarpData;

        // CardDropChanceModifiers: CardBasedDropChanceModifier[]
        public WarpperFunction.WarpType CardDropChanceModifiersWarpType;
        public List<string> CardDropChanceModifiersWarpData;

        // CreatedLiquid: LiquidDrop
        public WarpperFunction.WarpType CreatedLiquidWarpType;
        public string CreatedLiquidWarpData;

        // DroppedCards: CardDrop[]
        public WarpperFunction.WarpType DroppedCardsWarpType;
        public List<string> DroppedCardsWarpData;

        // StatModifications: ConditionalStatModifier[]
        public WarpperFunction.WarpType StatModificationsWarpType;
        public List<string> StatModificationsWarpData;
    }
}
