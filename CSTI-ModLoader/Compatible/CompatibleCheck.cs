using System.Linq;
using CSTI_LuaActionSupport;
using HarmonyLib;
using UnityEngine;

namespace ModLoader.Compatible;

public static class CompatibleCheck
{
    public static void MainCheck()
    {
        var GameManager_RemoveCard = AccessTools.Method(typeof(GameManager), "RemoveCard");
        var GameManager_ChangeEnvironment = AccessTools.Method(typeof(GameManager), "ChangeEnvironment");
        if (GameManager_RemoveCard == null)
        {
            Debug.LogError("[！致命错误！]无法检测到GameManager.RemoveCard");
        }
        else
        {
            var patchInfo = Harmony.GetPatchInfo(GameManager_RemoveCard);
            if (patchInfo != null)
            {
                foreach (var infoPrefix in patchInfo.Prefixes)
                {
                    var assembly = infoPrefix.PatchMethod.DeclaringType?.Assembly;
                    if (assembly == null) continue;
                    if (infoPrefix.PatchMethod.ReturnType == typeof(void)) continue;
                    if (assembly != typeof(CompatibleCheck).Assembly &&
                        assembly != typeof(LuaSupportRuntime).Assembly &&
                        assembly.GetName().Name != "CSTI-ChatTreeLoader" &&
                        infoPrefix.PatchMethod.DeclaringType?.Namespace?.StartsWith("MakeItSimple") is false)
                    {
                        Debug.LogWarning(
                            "GameManager.RemoveCard被其他模组以特定方式(pre)" +
                            $"|{infoPrefix.PatchMethod.DeclaringType?.Namespace}.{infoPrefix.PatchMethod.DeclaringType?.Name}.{infoPrefix.PatchMethod.Name}|" +
                            "注入，这可能会导致错误");
                    }
                }

                foreach (var infoPostfix in patchInfo.Postfixes)
                {
                    var assembly = infoPostfix.PatchMethod.DeclaringType?.Assembly;
                    if (assembly == null) continue;
                    if (infoPostfix.PatchMethod.ReturnType == typeof(void) &&
                        infoPostfix.PatchMethod.GetParameters().All(pInfo =>
                            pInfo.Name != "__result" || pInfo is {IsOut: false, ParameterType.IsByRef: false}))
                        continue;
                    if (assembly != typeof(CompatibleCheck).Assembly &&
                        assembly != typeof(LuaSupportRuntime).Assembly &&
                        assembly.GetName().Name != "CSTI-ChatTreeLoader" &&
                        infoPostfix.PatchMethod.DeclaringType?.Namespace?.StartsWith("MakeItSimple") is false)
                    {
                        Debug.LogWarning(
                            "GameManager.RemoveCard被其他模组以特定方式(post)" +
                            $"|{infoPostfix.PatchMethod.DeclaringType?.Namespace}.{infoPostfix.PatchMethod.DeclaringType?.Name}.{infoPostfix.PatchMethod.Name}|" +
                            "注入，这可能会导致错误");
                    }
                }
            }
        }

        if (GameManager_ChangeEnvironment == null)
        {
            Debug.LogError("[！致命错误！]无法检测到GameManager.ChangeEnvironment");
        }
        else
        {
            var patchInfo = Harmony.GetPatchInfo(GameManager_ChangeEnvironment);
            if (patchInfo != null)
            {
                foreach (var infoPrefix in patchInfo.Prefixes)
                {
                    var assembly = infoPrefix.PatchMethod.DeclaringType?.Assembly;
                    if (assembly == null) continue;
                    if (infoPrefix.PatchMethod.ReturnType == typeof(void)) continue;
                    if (assembly != typeof(CompatibleCheck).Assembly &&
                        assembly != typeof(LuaSupportRuntime).Assembly &&
                        assembly.GetName().Name != "CSTI-ChatTreeLoader" &&
                        infoPrefix.PatchMethod.DeclaringType?.Namespace?.StartsWith("MakeItSimple") is false)
                    {
                        Debug.LogWarning(
                            "GameManager.ChangeEnvironment被其他模组以特定方式(pre)" +
                            $"|{infoPrefix.PatchMethod.DeclaringType?.Namespace}.{infoPrefix.PatchMethod.DeclaringType?.Name}.{infoPrefix.PatchMethod.Name}|" +
                            "注入，这可能会导致错误");
                    }
                }

                foreach (var infoPostfix in patchInfo.Postfixes)
                {
                    var assembly = infoPostfix.PatchMethod.DeclaringType?.Assembly;
                    if (assembly == null) continue;
                    if (infoPostfix.PatchMethod.ReturnType == typeof(void) &&
                        infoPostfix.PatchMethod.GetParameters().All(pInfo =>
                            pInfo.Name != "__result" || pInfo is {IsOut: false, ParameterType.IsByRef: false}))
                        continue;
                    if (assembly != typeof(CompatibleCheck).Assembly &&
                        assembly != typeof(LuaSupportRuntime).Assembly &&
                        assembly.GetName().Name != "CSTI-ChatTreeLoader" &&
                        infoPostfix.PatchMethod.DeclaringType?.Namespace?.StartsWith("MakeItSimple") is false)
                    {
                        Debug.LogWarning(
                            "GameManager.ChangeEnvironment被其他模组以特定方式(post)" +
                            $"|{infoPostfix.PatchMethod.DeclaringType?.Namespace}.{infoPostfix.PatchMethod.DeclaringType?.Name}.{infoPostfix.PatchMethod.Name}|" +
                            "注入，这可能会导致错误");
                    }
                }
            }
        }
    }
}