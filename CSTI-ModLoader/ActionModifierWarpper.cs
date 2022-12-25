using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{

    public class ActionModifierWarpper : WarpperBase
    {
        public ActionModifierWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ActionModifier obj)
        {
            WarpperFunction.ClassWarpper(obj, "AppliesTo", AppliesToWarpType, AppliesToWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "AddedStatModifiers", AddedStatModifiersWarpType, AddedStatModifiersWarpData, SrcPath);
        }

        // AppliesTo: List<ActionTag>
        public WarpperFunction.WarpType AppliesToWarpType;
        public List<string> AppliesToWarpData;

        // AddedStatModifiers: StatModifier[]
        public WarpperFunction.WarpType AddedStatModifiersWarpType;
        public List<string> AddedStatModifiersWarpData;
    }
}
