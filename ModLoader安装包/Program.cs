using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
// using ICSharpCode.SharpZipLib.Zip;
using SharpCompress.Archives;
using Microsoft.Win32;


namespace ModLoader安装包
{
    internal static class Program
    {
        private static bool HadInstall;
        private static readonly string GamePathTxt = "gamePath.txt";

        private static void Main(string[] args)
        {
            args = Array.Empty<string>();

            if (!HadInstall &&
                Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Card Survival - Tropical Island_Data")))
            {
                InstallModLoader(Environment.CurrentDirectory, args.Length == 0);
            }

            var gamePath = "";
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using var registryKey = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
                    var readAllText = File.ReadAllText(Path.Combine(
                        Path.GetFullPath((registryKey?.GetValue("SteamPath") ?? "").ToString()),
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
                                    gamePath = directory;
                                    InstallModLoader(directory, args.Length == 0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var appDataDir = "";
            try
            {
                appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                appDataDir = Path.Combine(Path.GetDirectoryName(appDataDir) ?? "", "LocalLow", "WinterSpring Games");
                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }

                appDataDir = Path.Combine(appDataDir, "Card Survival - Tropical Island");
                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                appDataDir = "";
            }

            if (!HadInstall && appDataDir != "" && File.Exists(Path.Combine(appDataDir, GamePathTxt)))
            {
                gamePath = File.ReadAllText(Path.Combine(appDataDir, GamePathTxt)).Trim();

                if (Directory.Exists(gamePath) &&
                    Directory.Exists(Path.Combine(gamePath, "Card Survival - Tropical Island_Data")))
                {
                    InstallModLoader(gamePath, args.Length == 0);
                }
            }

            while (!HadInstall)
            {
                Console.WriteLine(CultureInfo.CurrentCulture.EnglishName.Contains("Chinese")
                    ? "无法搜索到游戏路径，请手动输入游戏路径(绝对路径)(exit = 退出):"
                    : "Unable to search the game path, please enter the game path manually(Absolute path)(exit = exit):");
                var readLine = Console.ReadLine()?.Trim();
                if (readLine?.StartsWith("exit", true, CultureInfo.CurrentCulture) is true)
                {
                    return;
                }

                if (readLine == null)
                {
                    continue;
                }

                readLine = Path.Combine(readLine.Trim().Split('\\', '/'));
                if (!Directory.Exists(readLine) ||
                    !Directory.Exists(Path.Combine(readLine, "Card Survival - Tropical Island_Data")))
                {
                    Console.WriteLine(CultureInfo.CurrentCulture.EnglishName.Contains("Chinese")
                        ? "输入路径错误"
                        : "Wrong input path");
                    continue;
                }

                gamePath = readLine;
                if (appDataDir != "")
                {
                    File.WriteAllText(Path.Combine(appDataDir, GamePathTxt), readLine);
                }

                InstallModLoader(readLine, args.Length == 0);
            }

            if (args.Length != 0)
            {
                foreach (var s in args)
                {
                    var filePat = s.Trim();
                    if (!File.Exists(filePat))
                    {
                        continue;
                    }

                    try
                    {
                        ArchiveFactory.Open(filePat)
                            .ExtractArch(Path.Combine(gamePath, "BepInEx", "plugins"), _ => true);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            Console.WriteLine(CultureInfo.CurrentCulture.EnglishName.Contains("Chinese")
                ? "安装已完成"
                : "Installation is complete");
            Console.WriteLine(CultureInfo.CurrentCulture.EnglishName.Contains("Chinese")
                ? "按下回车关闭界面"
                : "Press enter to close the screen");
            Console.ReadKey();
        }

        private static void InstallModLoader(string gamePath, bool updateModLoader)
        {
            HadInstall = true;
            // var fastZip = new FastZip();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (
                    !File.Exists(Path.Combine(gamePath, "winhttp.dll")) ||
                    !Directory.Exists(Path.Combine(gamePath, "BepInEx")) ||
                    !File.Exists(Path.Combine(gamePath, "doorstop_config.ini")) ||
                    !Directory.Exists(Path.Combine(gamePath, "BepInEx", "core"))
                )
               )
            {
                // fastZip.ExtractZip(ModLoaderPack.BepInEx_x64, gamePath, FastZip.Overwrite.Always,
                //     _ => true, ".*", ".*",
                //     true, true);
                SharpCompress.Archives.ArchiveFactory.Open(ModLoaderPack.BepInEx_x64).ExtractArch(gamePath, _ => true);
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

            if (!updateModLoader)
            {
                return;
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

            using var fileStreamUpdateHistory = new FileStream(
                Path.Combine(ModLoaderPlugins, CultureInfo.CurrentCulture.EnglishName.Contains("Chinese")
                    ? "更新历史.txt"
                    : "UpdateHistory.txt"),
                FileMode.Create);
            ModLoaderPack.UpdateHistory.CopyTo(fileStreamUpdateHistory);
        }
    }
}