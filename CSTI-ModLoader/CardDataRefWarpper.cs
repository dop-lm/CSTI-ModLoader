using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardDataRefWarpper : WarpperBase
    {
        public CardDataRefWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CardDataRef obj)
        {
            WarpperFunction.ClassWarpper(obj, "Card", CardWarpType, CardWarpData, SrcPath);
        }

        // Card: CardData
        public WarpperFunction.WarpType CardWarpType;
        public String CardWarpData;
    }
}
