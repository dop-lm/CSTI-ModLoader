﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class StatModifierWarpper : WarpperBase
    {
        public StatModifierWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(ref StatModifier obj)
        {
            if (StatWarpType == WarpperFunction.WarpType.REFERENCE)
            {
                if (ModLoader.AllGUIDDict.TryGetValue(StatWarpData, out var ele) && ele is GameStat)
                {
                    obj.Stat = ele as GameStat;
                }
            }
        }

        // Object Name
        public String ObjectName;

        // Stat: GameStat
        public WarpperFunction.WarpType StatWarpType;
        public String StatWarpData;
    }
}