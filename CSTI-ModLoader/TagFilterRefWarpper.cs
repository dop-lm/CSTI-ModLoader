using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class TagFilterRefWarpper : WarpperBase
    {
        public TagFilterRefWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref TagFilterRef obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "Tag", TagWarpType, TagWarpData, SrcPath);

            obj = (TagFilterRef)box;
        }

        // Object Name
        public String ObjectName;

        // Tag: CardTag
        public WarpperFunction.WarpType TagWarpType;
        public String TagWarpData;
    }
}
