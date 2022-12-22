using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class AmbienceSettingsWarpper : WarpperBase
    {
        public AmbienceSettingsWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(AmbienceSettings obj)
        {
            WarpperFunction.ClassWarpper(obj, "BackgroundSound", BackgroundSoundWarpType, BackgroundSoundWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RandomNoises", RandomNoisesWarpType, RandomNoisesWarpData, SrcPath);
        }

        // BackgroundSound: AudioClip
        public WarpperFunction.WarpType BackgroundSoundWarpType;
        public string BackgroundSoundWarpData;

        // RandomNoises: AudioClip[]
        public WarpperFunction.WarpType RandomNoisesWarpType;
        public List<string> RandomNoisesWarpData;
    }
}
