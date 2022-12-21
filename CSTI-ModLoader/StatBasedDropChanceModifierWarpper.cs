using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class StatBasedDropChanceModifierWarpper
    {
        public void Warpper(StatInterruptionCondition instance)
        {
            //if (StatWarpType != WarpperFunction.WarpType.NONE)
            //    WarpperFunction.UniqueIDScriptableWarpper(instance, "Stat", StatWarpType, StatWarpData);
        }

        // Object Name
        public String ObjectName;

        // Stat: GameStat
        public WarpperFunction.WarpType StatWarpType;
        public String StatWarpData;
    }
}
