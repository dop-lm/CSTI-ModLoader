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
            WarpperFunction.ClassWarpper(obj, "DroppedCards", DroppedCardsWarpType, DroppedCardsWarpData, SrcPath);
        }

        // Object Name
        public String ObjectName;

        // DroppedCards: CardDrop[]
        public WarpperFunction.WarpType DroppedCardsWarpType;
        public List<string> DroppedCardsWarpData;
    }
}
