using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class StatBasedDropChanceModifierWarpper : WarpperBase
    {
        public StatBasedDropChanceModifierWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref StatBasedDropChanceModifier obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "Stat", StatWarpType, StatWarpData, SrcPath);

            obj = (StatBasedDropChanceModifier)box;
        }

        // Object Name
        public String ObjectName;

        // Stat: GameStat
        public WarpperFunction.WarpType StatWarpType;
        public String StatWarpData;
    }
}
