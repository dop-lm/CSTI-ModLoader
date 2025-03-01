using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

namespace ModLoader_Next安装包;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public string GamePathStr = "";

    public MainWindow()
    {
        InitializeComponent();
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var readAllText = File.ReadAllText(Path.Combine(
                    Path.GetFullPath((registryKey?.GetValue("SteamPath") ?? "").ToString()!),
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
                                if (Path.GetFileName(directory) != "Card Survival Fantasy Forest") continue;
                                GamePathStr = directory;
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

        GamePath.Text = GamePathStr;
    }

    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (File.Exists(Path.Combine(GamePath.Text, "Card Survival - Fantasy Forest.exe")))
        {
            InstallModLoader(GamePath.Text);
            RunResult.Text = "modloader安装完成";
            ResultBorder.BorderBrush = new SolidColorBrush(Colors.Aqua);
            ResultBorder.BorderThickness = new Thickness(1);
        }
        else
        {
            RunResult.Text = "游戏路径不正确";
            ResultBorder.BorderBrush = new SolidColorBrush(Colors.Red);
            ResultBorder.BorderThickness = new Thickness(2);
        }

        e.Handled = false;
    }

    private static void InstallModLoader(string gamePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (
                !File.Exists(Path.Combine(gamePath, "winhttp.dll")) ||
                !Directory.Exists(Path.Combine(gamePath, "BepInEx")) ||
                !File.Exists(Path.Combine(gamePath, "doorstop_config.ini")) ||
                !Directory.Exists(Path.Combine(gamePath, "BepInEx", "core"))
            )
           )
        {
            new ZipArchive(ModLoaderPack.BepInEx_x64, ZipArchiveMode.Read).ExtractToDirectory(gamePath);
        }

        var plugins = Path.Combine(gamePath, "BepInEx", "plugins");
        if (!Directory.Exists(plugins)) Directory.CreateDirectory(plugins);

        var modLoaderPlugins = Path.Combine(plugins, "ModLoader");
        if (!Directory.Exists(modLoaderPlugins)) Directory.CreateDirectory(modLoaderPlugins);


        // using var fileStreamDotNetZip = new FileStream(Path.Combine(ModLoaderPlugins, "DotNetZip.dll"),
        //     FileMode.Create);
        // ModLoaderPack.DotNetZip.CopyTo(fileStreamDotNetZip);

        using var fileStreamLitJson = new FileStream(Path.Combine(modLoaderPlugins, "LitJSON.dll"),
            FileMode.Create);
        ModLoaderPack.LitJSON.CopyTo(fileStreamLitJson);

        using var fileStreamModLoader = new FileStream(Path.Combine(modLoaderPlugins, "ModLoader.dll"),
            FileMode.Create);
        ModLoaderPack.ModLoader.CopyTo(fileStreamModLoader);
    }
}