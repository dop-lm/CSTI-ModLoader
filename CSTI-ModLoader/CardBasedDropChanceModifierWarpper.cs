using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CardBasedDropChanceModifierWarpper : WarpperBase
    {
        public CardBasedDropChanceModifierWarpper(string SrcPath) : base(SrcPath) { }
        public void Warpper(ref CardBasedDropChanceModifier instance)
        {


        }

        // Object Name
        public String ObjectName;

        // CardOnBoard: CardData 
        public WarpperFunction.WarpType CardOnBoardWarpType;
        public String CardOnBoardWarpData;

        // TagOnBoard: CardTag 
        public WarpperFunction.WarpType TagOnBoardWarpType;
        public String TagOnBoardWarpData;

        // AddedStatModifiers: StatBasedDropChanceModifier[] 
        public WarpperFunction.WarpType AddedStatModifiersWarpType;
        public List<String> AddedStatModifiersWarpData;
    }
}
