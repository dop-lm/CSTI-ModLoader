using System.IO;

namespace ModLoader安装包
{
    public static class ModLoaderPack
    {
        public static Stream BepInEx_x64 =>
            typeof(ModLoaderPack).Assembly.GetManifestResourceStream("ModLoaderInstaller.BepInEx_x64_5.4.21.0.zip");

        public static Stream BepInEx_unix =>
            typeof(ModLoaderPack).Assembly.GetManifestResourceStream("ModLoaderInstaller.BepInEx_unix_5.4.21.0.zip");

        public static Stream CSTI_ChatTreeLoader =>
            typeof(ModLoaderPack).Assembly.GetManifestResourceStream("ModLoaderInstaller.ChatTreeLoader.dll");

        public static Stream DotNetZip =>
            typeof(ModLoaderPack).Assembly.GetManifestResourceStream("ModLoaderInstaller.DotNetZip.dll");

        public static Stream LitJSON =>
            typeof(ModLoaderPack).Assembly.GetManifestResourceStream("ModLoaderInstaller.LitJSON.dll");

        public static Stream ModLoader =>
            typeof(ModLoaderPack).Assembly.GetManifestResourceStream("ModLoaderInstaller.ModLoader.dll");

        public static Stream UpdateHistory =>
            typeof(ModLoaderPack).Assembly.GetManifestResourceStream("ModLoaderInstaller.更新历史.txt");
    }
}