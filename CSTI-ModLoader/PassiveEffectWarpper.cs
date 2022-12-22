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
            object box = obj;

            WarpperFunction.ClassWarpper(box, "Conditions", ConditionsWarpType, ConditionsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "StatModifiers", StatModifiersWarpType, StatModifiersWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "GeneratedLiquid", GeneratedLiquidWarpType, GeneratedLiquidWarpData, SrcPath);

            obj = (PassiveEffect)box;
        }

        // Conditions: GeneralCondition
        public WarpperFunction.WarpType ConditionsWarpType;
        public String ConditionsWarpData;

        // StatModifiers: StatModifier[]
        public WarpperFunction.WarpType StatModifiersWarpType;
        public List<String> StatModifiersWarpData;

        // GeneratedLiquid: LiquidDrop
        public WarpperFunction.WarpType GeneratedLiquidWarpType;
        public String GeneratedLiquidWarpData;
    }
}
