using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ExtraDurabilityChangeWarpper : WarpperBase
    {
        public ExtraDurabilityChangeWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ExtraDurabilityChange obj)
        {
            WarpperFunction.ClassWarpper(obj, "AffectedCards", AffectedCardsWarpType, AffectedCardsWarpData, SrcPath);
            WarpperFunction.ClassWarpper(obj, "AffectedTags", AffectedTagsWarpType, AffectedTagsWarpData, SrcPath);
            WarpperFunction.ClassWarpper(obj, "NOTAffectedThings", NOTAffectedThingsWarpType, NOTAffectedThingsWarpData, SrcPath);
            WarpperFunction.ClassWarpper(obj, "SendToEnvironment", SendToEnvironmentWarpType, SendToEnvironmentWarpData, SrcPath);
        }

        // Object Name
        public String ObjectName;

        // AffectedCards: List<CardData>
        public WarpperFunction.WarpType AffectedCardsWarpType;
        public List<string> AffectedCardsWarpData;

        // AffectedTags: CardTag[]
        public WarpperFunction.WarpType AffectedTagsWarpType;
        public List<string> AffectedTagsWarpData;

        // NOTAffectedThings: CardOrTagRef[]
        public WarpperFunction.WarpType NOTAffectedThingsWarpType;
        public List<string> NOTAffectedThingsWarpData;

        // SendToEnvironment: EnvironmentCardDataRef[]
        public WarpperFunction.WarpType SendToEnvironmentWarpType;
        public List<string> SendToEnvironmentWarpData;
    }
}
