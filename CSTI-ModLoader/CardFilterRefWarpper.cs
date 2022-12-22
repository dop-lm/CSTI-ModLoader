using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardFilterRefWarpper : WarpperBase
    {
        public CardFilterRefWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref CardFilterRef obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "Card", CardWarpType, CardWarpData, SrcPath);

            obj = (CardFilterRef)box;
        }

        // Card: CardData
        public WarpperFunction.WarpType CardWarpType;
        public String CardWarpData;
    }
}
