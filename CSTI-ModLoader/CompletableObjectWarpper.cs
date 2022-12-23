using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class CompletableObjectWarpper : WarpperBase
    {
        public CompletableObjectWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(CompletableObject obj)
        {
            WarpperFunction.ClassWarpper(obj, "ActionObjectives", ActionObjectivesWarpType, ActionObjectivesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "CardsOnBoardObjectives", CardsOnBoardObjectivesWarpType, CardsOnBoardObjectivesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "TagsOnBoardObjectives", TagsOnBoardObjectivesWarpType, TagsOnBoardObjectivesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "StatsObjectives", StatsObjectivesWarpType, StatsObjectivesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "TimeObjectives", TimeObjectivesWarpType, TimeObjectivesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "NestedObjectives", NestedObjectivesWarpType, NestedObjectivesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "PlayedCharacter", PlayedCharacterWarpType, PlayedCharacterWarpData, SrcPath);
        }

        // ActionObjectives: List<ActionSubObjective>
        public WarpperFunction.WarpType ActionObjectivesWarpType;
        public List<String> ActionObjectivesWarpData;

        // CardsOnBoardObjectives: List<CardOnBoardSubObjective>
        public WarpperFunction.WarpType CardsOnBoardObjectivesWarpType;
        public List<String> CardsOnBoardObjectivesWarpData;

        // TagsOnBoardObjectives: List<TagOnBoardSubObjective>
        public WarpperFunction.WarpType TagsOnBoardObjectivesWarpType;
        public List<String> TagsOnBoardObjectivesWarpData;

        // StatsObjectives: List<StatSubObjective>
        public WarpperFunction.WarpType StatsObjectivesWarpType;
        public List<String> StatsObjectivesWarpData;

        // TimeObjectives: List<TimeObjective>
        public WarpperFunction.WarpType TimeObjectivesWarpType;
        public List<String> TimeObjectivesWarpData;

        // NestedObjectives: List<ObjectiveSubObjective>
        public WarpperFunction.WarpType NestedObjectivesWarpType;
        public List<String> NestedObjectivesWarpData;

        // PlayedCharacter: List<PlayerCharacter>
        public WarpperFunction.WarpType PlayedCharacterWarpType;
        public List<String> PlayedCharacterWarpData;
    }
}
