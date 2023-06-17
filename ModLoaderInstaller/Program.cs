using System.IO;
using System.Runtime.InteropServices;
using EpicMorg.SteamPathsLib;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace ModLoaderInstaller;

internal static class Program
{
    private static bool HadInstall;

    private static void Main(string[] args)
    {
        var readAllText = File.ReadAllText(Path.Combine(Path.GetFullPath(SteamPathsUtil.GetSteamData().SteamPath),
            "config", "libraryfolders.vdf"));
        var vProperty = VdfConvert.Deserialize(readAllText);
        if (vProperty.Value is VObject vObject)
        {
            foreach (var (_, token) in vObject)
            {
                if (token is not VObject vObject1) continue;
                foreach (var (key, vToken) in vObject1)
                {
                    if (key != "path") continue;
                    var appPath = Path.Combine(vToken.ToString(), "steamapps", "common");
                    var enumerateDirectories = Directory.EnumerateDirectories(appPath);
                    foreach (var directory in enumerateDirectories)
                    {
                        if (Path.GetFileName(directory) != "Card Survival Tropical Island") continue;
                        InstallModLoader(directory);
                    }
                }
            }
        }

        if (!HadInstall)
        {
        }
    }

    private static void InstallModLoader(string gamePath)
    {
        HadInstall = true;
        var fastZip = new FastZip();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (
                !File.Exists(Path.Combine(gamePath, "winhttp.dll")) ||
                !Directory.Exists(Path.Combine(gamePath, "BepInEx")) ||
                !File.Exists(Path.Combine(gamePath, "doorstop_config.ini")) ||
                !Directory.Exists(Path.Combine(gamePath, "BepInEx", "core"))
            )
           )
        {
            fastZip.ExtractZip(ModLoaderPack.BepInEx_x64, gamePath,
                FastZip.Overwrite.Always, _ => true, ".*",
                ".*", true, true);
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (
                !Directory.Exists(Path.Combine(gamePath, "doorstop_libs")) ||
                !File.Exists(Path.Combine(gamePath, "run_bepinex.sh")) ||
                !Directory.Exists(Path.Combine(gamePath, "BepInEx")) ||
                !Directory.Exists(Path.Combine(gamePath, "BepInEx", "core"))
            )
           )
        {
            fastZip.ExtractZip(ModLoaderPack.BepInEx_unix, gamePath, FastZip.Overwrite.Always,
                _ => true, ".*", ".*",
                true, true);
        }

        var plugins = Path.Combine(gamePath, "BepInEx", "plugins");
        if (!Directory.Exists(plugins))
        {
            Directory.CreateDirectory(plugins);
        }

        var ModLoaderPlugins = Path.Combine(plugins, "ModLoader");
        if (!Directory.Exists(ModLoaderPlugins))
        {
            Directory.CreateDirectory(ModLoaderPlugins);
        }

        using var fileStreamCSTI_ChatTreeLoader =
            new FileStream(Path.Combine(ModLoaderPlugins, "CSTI-ChatTreeLoader.dll"), FileMode.Create);
        ModLoaderPack.CSTI_ChatTreeLoader.CopyTo(fileStreamCSTI_ChatTreeLoader);

        using var fileStreamDotNetZip = new FileStream(Path.Combine(ModLoaderPlugins, "DotNetZip.dll"),
            FileMode.Create);
        ModLoaderPack.DotNetZip.CopyTo(fileStreamDotNetZip);

        using var fileStreamLitJSON = new FileStream(Path.Combine(ModLoaderPlugins, "LitJSON.dll"),
            FileMode.Create);
        ModLoaderPack.LitJSON.CopyTo(fileStreamLitJSON);

        using var fileStreamModLoader = new FileStream(Path.Combine(ModLoaderPlugins, "ModLoader.dll"),
            FileMode.Create);
        ModLoaderPack.ModLoader.CopyTo(fileStreamModLoader);
    }
}