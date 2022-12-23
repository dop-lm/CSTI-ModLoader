using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class ObjectiveWarpper : CompletableObjectWarpper
    {
        public ObjectiveWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(Objective obj)
        {
            base.WarpperCustomSelf(obj);

            WarpperFunction.ClassWarpper(obj, "ObjectiveLog", ObjectiveLogWarpType, ObjectiveLogWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "OnCompleteActions", OnCompleteActionsWarpType, OnCompleteActionsWarpData, SrcPath);
        }

        // ObjectiveLog: EndgameLog
        public WarpperFunction.WarpType ObjectiveLogWarpType;
        public String ObjectiveLogWarpData;

        // OnCompleteActions: List<CardAction>
        public WarpperFunction.WarpType OnCompleteActionsWarpType;
        public List<String> OnCompleteActionsWarpData;
    }
}
