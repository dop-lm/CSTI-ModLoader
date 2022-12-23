using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ActionReportWarpper : WarpperBase
    {
        public ActionReportWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref ActionReport obj)
        {
            object box = obj;

            WarpperFunction.ClassWarpper(box, "FromCard", FromCardWarpType, FromCardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(box, "Action", ActionWarpType, ActionWarpData, SrcPath);

            obj = (ActionReport)box;
        }

        // FromCard: CardData
        public WarpperFunction.WarpType FromCardWarpType;
        public string FromCardWarpData;

        // Action: CardAction
        public WarpperFunction.WarpType ActionWarpType;
        public string ActionWarpData;
    }
}
