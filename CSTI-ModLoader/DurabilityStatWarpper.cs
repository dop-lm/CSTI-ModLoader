using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class DurabilityStatWarpper : WarpperBase
    {
        public DurabilityStatWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(DurabilityStat obj)
        {
            WarpperFunction.ClassWarpper(obj, "OverrideIcon", OverrideIconWarpType, OverrideIconWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "OnZero", OnZeroWarpType, OnZeroWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "OnFull", OnFullWarpType, OnFullWarpData, SrcPath);
        }

        // OverrideIcon: Sprite
        public WarpperFunction.WarpType OverrideIconWarpType;
        public string OverrideIconWarpData;

        // OnZero: CardAction
        public WarpperFunction.WarpType OnZeroWarpType;
        public string OnZeroWarpData;

        // OnFull: CardAction
        public WarpperFunction.WarpType OnFullWarpType;
        public string OnFullWarpData;
    }
}
