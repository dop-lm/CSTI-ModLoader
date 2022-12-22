using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardInteractionTriggerWarpper : WarpperBase
    {
        public CardInteractionTriggerWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref CardInteractionTrigger obj)
        {
            if (TriggerCardsWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                Array.Resize<CardData>(ref obj.TriggerCards, TriggerCardsWarpData.Count);
                for(int i = 0; i < TriggerCardsWarpData.Count; i++)
                {
                    if (ModLoader.AllGUIDDict.TryGetValue(TriggerCardsWarpData[i], out var ele) && ele is CardData)
                    {
                        obj.TriggerCards[i] = ele as CardData;
                    }
                }
            }

            if (TriggerTagsWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                Array.Resize<CardTag>(ref obj.TriggerTags, TriggerTagsWarpData.Count);
                for (int i = 0; i < TriggerTagsWarpData.Count; i++)
                {
                    if (ModLoader.CardTagDict.TryGetValue(TriggerTagsWarpData[i], out var ele))
                    {
                        obj.TriggerTags[i] = ele as CardTag;
                    }
                }
            }
        }

        // Object Name
        public String ObjectName;

        // TriggerCards: CardData[]
        public WarpperFunction.WarpType TriggerCardsWarpType;
        public List<string> TriggerCardsWarpData;

        // TriggerTags: CardTag[]
        public WarpperFunction.WarpType TriggerTagsWarpType;
        public List<string> TriggerTagsWarpData;
    }
}
