using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{

    public class RemotePassiveEffectWarpper : WarpperBase
    {
        public RemotePassiveEffectWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref RemotePassiveEffect obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "AppliesTo", AppliesToWarpType, AppliesToWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "Effect", EffectWarpType, EffectWarpData, SrcPath);

            obj = (RemotePassiveEffect)box;
        }

        // AppliesTo: CardOrTagRef[]
        public WarpperFunction.WarpType AppliesToWarpType;
        public List<string> AppliesToWarpData;

        // Effect: PassiveEffect
        public WarpperFunction.WarpType EffectWarpType;
        public string EffectWarpData;
    }
}
