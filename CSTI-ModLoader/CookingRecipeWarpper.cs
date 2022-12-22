using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CookingRecipeWarpper : WarpperBase
    {
        public CookingRecipeWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CookingRecipe obj)
        {
            WarpperFunction.ClassWarpper(obj, "CompatibleCards", CompatibleCardsWarpType, CompatibleCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CompatibleTags", CompatibleTagsWarpType, CompatibleTagsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "Conditions", ConditionsWarpType, ConditionsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CookerChanges", CookerChangesWarpType, CookerChangesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "IngredientChanges", IngredientChangesWarpType, IngredientChangesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CustomCompleteSound", CustomCompleteSoundWarpType, CustomCompleteSoundWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "Drops", DropsWarpType, DropsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "StatModifications", StatModificationsWarpType, StatModificationsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "DropsAsCollection", DropsAsCollectionWarpType, DropsAsCollectionWarpData, SrcPath);
        }

        // Object Name
        public String ObjectName;

        // CompatibleCards: CardData[]
        public WarpperFunction.WarpType CompatibleCardsWarpType;
        public List<string> CompatibleCardsWarpData;

        // CompatibleTags: CardTag[]
        public WarpperFunction.WarpType CompatibleTagsWarpType;
        public List<string> CompatibleTagsWarpData;

        // Conditions: GeneralCondition
        public WarpperFunction.WarpType ConditionsWarpType;
        public string ConditionsWarpData;

        // CookerChanges: CardStateChange
        public WarpperFunction.WarpType CookerChangesWarpType;
        public string CookerChangesWarpData;

        // IngredientChanges: CardStateChange
        public WarpperFunction.WarpType IngredientChangesWarpType;
        public string IngredientChangesWarpData;

        // CustomCompleteSound: AudioClip
        public WarpperFunction.WarpType CustomCompleteSoundWarpType;
        public string CustomCompleteSoundWarpData;

        // Drops: CardDrop[]
        public WarpperFunction.WarpType DropsWarpType;
        public string DropsWarpData;

        // StatModifications: StatModifier[]
        public WarpperFunction.WarpType StatModificationsWarpType;
        public string StatModificationsWarpData;

        // DropsAsCollection: CardsDropCollection[]
        public WarpperFunction.WarpType DropsAsCollectionWarpType;
        public string DropsAsCollectionWarpData;
    }
}
