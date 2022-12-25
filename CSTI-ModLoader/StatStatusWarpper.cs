using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class StatStatusWarpper : WarpperBase
    {
        public StatStatusWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(StatStatus obj)
        {
            WarpperFunction.ClassWarpper(obj, "StatusLog", StatusLogWarpType, StatusLogWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "Icon", IconWarpType, IconWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "EffectsOnStats", EffectsOnStatsWarpType, EffectsOnStatsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "EffectsOnActions", EffectsOnActionsWarpType, EffectsOnActionsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "AlertSounds", AlertSoundsWarpType, AlertSoundsWarpData, SrcPath);
        }

        // StatusLog: StatusEndgameLog
        public WarpperFunction.WarpType StatusLogWarpType;
        public string StatusLogWarpData;

        // Icon: Sprite
        public WarpperFunction.WarpType IconWarpType;
        public string IconWarpData;

        // EffectsOnStats: StatModifier[]
        public WarpperFunction.WarpType EffectsOnStatsWarpType;
        public List<string> EffectsOnStatsWarpData;

        // EffectsOnActions: ActionModifier[]
        public WarpperFunction.WarpType EffectsOnActionsWarpType;
        public List<string> EffectsOnActionsWarpData;

        // AlertSounds: AudioClip[]
        public WarpperFunction.WarpType AlertSoundsWarpType;
        public List<string> AlertSoundsWarpData;

        //// AlertSettings: StatusAlertSettings
        //public WarpperFunction.WarpType AlertSettingsWarpType;
        //public string AlertSettingsWarpData;
    }
}
