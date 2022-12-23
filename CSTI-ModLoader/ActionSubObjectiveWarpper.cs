using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ActionSubObjectiveWarpper : WarpperBase
    {
        public ActionSubObjectiveWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ActionSubObjective obj)
        {
            WarpperFunction.ClassWarpper(obj, "OnCard", OnCardWarpType, OnCardWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "LastActionReport", LastActionReportWarpType, LastActionReportWarpData, SrcPath);
        }

        // OnCard: CardData
        public WarpperFunction.WarpType OnCardWarpType;
        public string OnCardWarpData;

        // LastActionReport: ActionReport
        public WarpperFunction.WarpType LastActionReportWarpType;
        public string LastActionReportWarpData;
    }
}
