using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class AmtFeedbackInfoWarpper : WarpperBase
    {
        public AmtFeedbackInfoWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(AmtFeedbackInfo obj)
        {
            WarpperFunction.ClassWarpper(obj, "Icon", IconWarpType, IconWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "NegativeIcon", NegativeIconWarpType, NegativeIconWarpData, SrcPath);
        }

        // Icon: Sprite
        public WarpperFunction.WarpType IconWarpType;
        public String IconWarpData;

        // NegativeIcon: Sprite
        public WarpperFunction.WarpType NegativeIconWarpType;
        public String NegativeIconWarpData;
    }
}
