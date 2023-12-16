using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using JetBrains.Annotations;
using ModLoader.FFI;
using ModLoader.LoaderUtil;
using UnityEngine;

namespace ModLoader;

public static class ResourceLoadHelper
{
    public static readonly string ResourcePat = "Resource";
    public static readonly string PicturePat = "Picture";

    public static Task<(List<(byte[] dat, ImageEntry entry)> sprites, string modName)> LoadPictures(string ModName,
        [NotNull] string[] pictures)
    {
        var wait = (from picture in pictures
            where picture.EndsWith(".jpg") || picture.EndsWith(".jpeg") || picture.EndsWith(".png")
            let imageEntry = new ImageEntry(picture)
            select (LoadFile(imageEntry), imageEntry)).ToList();

        return Task.Run(async () =>
        {
            var dat = new List<(byte[] dat, ImageEntry entry)>();
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
        var libPath = Path.Combine(dir, "JsonnetLib");
        if (Directory.Exists(libPath))
        {
            JsonnetRuntime.JsonnetRuntimeAddPat(libPath);
        }

        var wait = new List<(Task<byte[]>, string, Type)>();
        var subclasses = from type in GameSourceAssembly.GetTypes()
            where type.IsSubclassOf(typeof(UniqueIDScriptable))
            select type;
        foreach (var type in subclasses)
        {
            if (!Directory.Exists(ModLoader.CombinePaths(dir, type.Name)))
                continue;
            if (Info.ModEditorVersion.IsNullOrWhiteSpace())
            {
                Debug.LogWarningFormat("{0} Only Support Editor Mod", ModName);
            }
            else
            {
                wait.AddRange(Directory
                    .EnumerateFiles(Path.Combine(dir, type.Name), "*.json", SearchOption.AllDirectories)
                    .Select(file => (LoadFile(file), file, type)));
                wait.AddRange(Directory
                    .EnumerateFiles(Path.Combine(dir, type.Name), "*.jsonnet", SearchOption.AllDirectories)
                    .Select(file => (LoadFile(file), file, type)));
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

    private static Task<byte[]> LoadFile(string pat)
    {
        return Task.Run(loadInner);

        async Task<byte[]> loadInner()
        {
            using var f = new FileStream(pat, FileMode.Open);
            using var ms = new MemoryStream();
            await f.CopyToAsync(ms);
            return ms.ToArray();
        }
    }

    private static Task<byte[]> LoadFile(ImageEntry entry)
    {
        return Task.Run(loadInner);

        async Task<byte[]> loadInner()
        {
            using var f = entry.DdsPath != null
                ? new FileStream(entry.DdsPath, FileMode.Open)
                : new FileStream(entry.ImgPath, FileMode.Open);
            using var ms = new MemoryStream();
            await f.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}

public class SimpleOnce
{
    private long OnceStat;

    public bool DoOnce()
    {
        return Interlocked.CompareExchange(ref OnceStat, 1, 0) == 0;
    }

    public bool SetDone()
    {
        return Interlocked.CompareExchange(ref OnceStat, 2, 1) == 1;
    }

    public bool Done()
    {
        return Interlocked.Read(ref OnceStat) == 2;
    }
}