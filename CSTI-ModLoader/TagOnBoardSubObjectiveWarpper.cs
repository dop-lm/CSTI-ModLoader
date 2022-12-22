using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class TagOnBoardSubObjectiveWarpper : WarpperBase
    {
        public TagOnBoardSubObjectiveWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(TagOnBoardSubObjective obj)
        {
            WarpperFunction.ClassWarpper(obj, "Tag", TagWarpType, TagWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "OnlyInEnvironment", OnlyInEnvironmentWarpType, OnlyInEnvironmentWarpData, SrcPath);
        }

        // Object Name
        public String ObjectName;

        // Tag: CardTag
        public WarpperFunction.WarpType TagWarpType;
        public string TagWarpData;

        // OnlyInEnvironment: CardData
        public WarpperFunction.WarpType OnlyInEnvironmentWarpType;
        public string OnlyInEnvironmentWarpData;
    }
}
