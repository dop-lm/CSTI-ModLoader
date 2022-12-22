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
            object box = obj;
            
            WarpperFunction.ClassWarpper(box, "TriggerCards", TriggerCardsWarpType, TriggerCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "TriggerTags", TriggerTagsWarpType, TriggerTagsWarpData, SrcPath);

            obj = (CardInteractionTrigger)box;
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
