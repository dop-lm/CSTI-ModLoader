using System.IO;
using BepInEx;
using UnityEngine;

namespace ModLoader.ExportUtil;

public static class ExportUI
{
    public static string ModPath = "";
    public static string ModExportPath = "";
    public static bool ForAndroid;

    public static void ModExportUIWindow()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("是否兼容安卓: " + (ForAndroid ? "<兼容>" : "<不兼容>"))) ForAndroid = !ForAndroid;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("源模组路径：", GUILayout.ExpandWidth(false));
        ModPath = GUILayout.TextField(ModPath);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("模组打包导出路径(默认为源模组路径的上级)：", GUILayout.ExpandWidth(false));
        ModExportPath = GUILayout.TextField(ModExportPath);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("执行导出"))
        {
            var initExportArch = ExportAll.InitExportArch(ModPath);
            if (initExportArch == null)
            {
                GUILayout.EndVertical();
                return;
            }

            initExportArch.ForAndroid = ForAndroid;

            if (ModExportPath.IsNullOrWhiteSpace() || !Directory.Exists(ModExportPath))
            {
                initExportArch.CollectAllToPat(Path.GetDirectoryName(ModPath));
            }
            else
            {
                initExportArch.CollectAllToPat(ModExportPath);
            }
        }

        GUILayout.EndVertical();
    }
}