using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardFilterWarpper : WarpperBase
    {
        public CardFilterWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref CardFilter obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "AcceptedCards", AcceptedCardsWarpType, AcceptedCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "AcceptedTags", AcceptedTagsWarpType, AcceptedTagsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "NOTAcceptedCards", NOTAcceptedCardsWarpType, NOTAcceptedCardsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "NOTAcceptedTags", NOTAcceptedTagsWarpType, NOTAcceptedTagsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "CardFilters", CardFiltersWarpType, CardFiltersWarpData, SrcPath);

            obj = (CardFilter)box;
        }

        // Object Name
        public String ObjectName;

        // AcceptedCards: CardData[]
        public WarpperFunction.WarpType AcceptedCardsWarpType;
        public List<string> AcceptedCardsWarpData;

        // AcceptedTags: CardTag[]
        public WarpperFunction.WarpType AcceptedTagsWarpType;
        public List<string> AcceptedTagsWarpData;

        //  NOTAcceptedCards: CardData[]
        public WarpperFunction.WarpType NOTAcceptedCardsWarpType;
        public List<string> NOTAcceptedCardsWarpData;

        // NOTAcceptedTags: CardTag[]
        public WarpperFunction.WarpType NOTAcceptedTagsWarpType;
        public List<string> NOTAcceptedTagsWarpData;

        // CardFilters: CardFilterRef[]
        public WarpperFunction.WarpType CardFiltersWarpType;
        public List<string> CardFiltersWarpData;

        // TagFilters: TagFilterRef[]
        public WarpperFunction.WarpType TagFiltersWarpType;
        public List<string> TagFiltersWarpData;
    }
}
