using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class PassiveEffectWarpper : WarpperBase
    {
        public PassiveEffectWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref PassiveEffect obj)
        {
            //if (StatWarpType == WarpperFunction.WarpType.REFERENCE)
            //{
            //    if (ModLoader.AllGUIDDict.TryGetValue(StatWarpData, out var ele) && ele is GameStat)
            //    {
            //        obj.Stat = ele as GameStat;
            //    }
            //}
        }

        // Object Name
        public String ObjectName;

        // Conditions: GeneralCondition
        public WarpperFunction.WarpType ConditionsWarpType;
        public String ConditionsWarpData;

        // StatModifiers: StatModifier[]
        public WarpperFunction.WarpType StatModifiersWarpType;
        public List<String> StatModifiersWarpData;
    }
}
