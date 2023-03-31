using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using JetBrains.Annotations;
using UnityEngine;

namespace ModLoader
{
    public static class ResourceLoadHelper
    {
        public static Task<(List<(byte[] dat, string name)> sprites, string modName)> LoadPictures(string ModName,
            [NotNull] string[] pictures)
        {
            var wait = new List<(Task<byte[]>, string)>();
            foreach (var picture in pictures)
            {
                if (!picture.EndsWith(".jpg") && !picture.EndsWith(".jpeg") && !picture.EndsWith(".png"))
                    continue;
                var sprite_name = Path.GetFileNameWithoutExtension(picture);
                wait.Add((LoadFile(picture), sprite_name));
            }

            return Task.Run(async () =>
            {
                var dat = new List<(byte[] dat, string name)>();
                foreach (var (task, name) in wait)
                {
                    dat.Add((await task, name));
                }

                return (dat, ModName);
            });
        }

        public static Task<(List<(byte[] dat, string pat, Type type)> uniqueObjs, string modName)> LoadUniqueObjs(
            string ModName,
            [NotNull] string dir, Assembly GameSourceAssembly, ModInfo Info)
        {
            var wait = new List<(Task<byte[]>, string, Type)>();
            var subclasses = from type in GameSourceAssembly.GetTypes()
                where type.IsSubclassOf(typeof(UniqueIDScriptable))
                select type;
            foreach (var type in subclasses)
            {
                if (!Directory.Exists(Path.Combine(dir, type.Name)))
                    continue;
                if (Info.ModEditorVersion.IsNullOrWhiteSpace())
                {
                    Debug.LogWarningFormat("{0} Only Support Editor Mod", ModName);
                }
                else
                {
                    foreach (var file in Directory.EnumerateFiles(Path.Combine(dir, type.Name), "*.json",
                                 SearchOption.AllDirectories))
                    {
                        wait.Add((LoadFile(file), file, type));
                    }
                }
            }

            return Task.Run(async () =>
            {
                var dat = new List<(byte[] dat, string pat, Type type)>();
                foreach (var (task, file, type) in wait)
                {
                    dat.Add((await task, file, type));
                }

                return (dat, ModName);
            });
        }

        public static Task<byte[]> LoadFile(string pat)
        {
            async Task<byte[]> loadInner()
            {
                using var f = new FileStream(pat, FileMode.Open);
                using var ms = new MemoryStream();
                await f.CopyToAsync(ms);
                return ms.ToArray();
            }

            return Task.Run(loadInner);
        }
    }

    public class SimpleOnce
    {
        private int OnceStat;

        public bool DoOnce()
        {
            return Interlocked.CompareExchange(ref OnceStat, 1, 0) == 0;
        }
    }
}