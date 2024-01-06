using System.Reflection;
using BepInEx.Preloader.Patching;

// ReSharper disable once CheckNamespace
namespace Doorstop
{
    public static class Entrypoint
    {
        public static void Start()
        {
            var assembly = Assembly.GetAssembly(typeof(AssemblyPatcher));
            var type = assembly.GetType("BepInEx.Preloader.Entrypoint");
            var Main = type.GetMethod("Main", BindingFlags.Default | BindingFlags.Static | BindingFlags.Public);
            Main!.Invoke(null, new object[] { });
        }
    }
}