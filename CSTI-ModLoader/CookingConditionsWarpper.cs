using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CookingConditionsWarpper : WarpperBase
    {
        public CookingConditionsWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref CookingConditions obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "CookingPausedSound", CookingPausedSoundWarpType, CookingPausedSoundWarpData, SrcPath);

            obj = (CookingConditions)box;
        }

        // CookingPausedSound: AudioClip
        public WarpperFunction.WarpType CookingPausedSoundWarpType;
        public String CookingPausedSoundWarpData;
    }
}
