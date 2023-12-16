using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace ModLoader.Updater;

public static class AutoUpdate
{
    public class MyRef<T>(T @ref)
    {
        public T Ref = @ref;
    }

    public class MyVersion(List<int> versions)
    {
        public List<int> Versions = versions;

        public static MyVersion Parse(string s)
        {
            var versions = s.Split('.').Select(int.Parse);
            return new MyVersion(versions.ToList());
        }

        public static bool operator <(MyVersion version1, MyVersion version2)
        {
            for (var i = 0; i < Math.Min(version1.Versions.Count, version2.Versions.Count); i++)
            {
                if (version1.Versions[i] < version2.Versions[i])
                {
                    return true;
                }
            }

            return false;
        }

        public static bool operator >(MyVersion version1, MyVersion version2)
        {
            for (var i = 0; i < Math.Min(version1.Versions.Count, version2.Versions.Count); i++)
            {
                if (version1.Versions[i] > version2.Versions[i])
                {
                    return true;
                }
            }

            return false;
        }
    }

    public const string BaseDir = "https://gitee.com/typeunknown/my-cstimods/raw/master/ModLoader/";
    public const string VersionFile = "version.txt";
    public const string CSTI_ChatTreeLoader = "CSTI-ChatTreeLoader.dll";
    public const string CSTI_LuaActionSupport = "CSTI_LuaActionSupport.dll";
    public const string DotNetZip = "DotNetZip.dll";
    public const string Jsonnet4CSTIModLoader = "Jsonnet4CSTIModLoader.dll";
    public const string LitJSON = "LitJSON.dll";
    public const string ModLoader = "ModLoader.dll";
    public const string libJsonnet4CSTIModLoader_mac = "libJsonnet4CSTIModLoader.dylib";
    public const string libJsonnet4CSTIModLoader_linux = "libJsonnet4CSTIModLoader.so";
    public const string liblua54_mac = "liblua54.dylib";
    public const string liblua54_linux = "liblua54.so";
    public const string lua54_w64 = "lua54.dll";
    public const string lua54_w32 = "x86/lua54.dll";
    public const string 更新历史 = "更新历史.txt";

    public static IEnumerator UpdateModIfNecessary()
    {
        var stat = new MyRef<bool?>(null);
        yield return ModLoaderInstance.StartCoroutine(CheckNeedUpdate(stat));
        if (stat.Ref is true)
        {
            Debug.Log("ModLoader需要更新");
            stat.Ref = false;
            yield return ModLoaderInstance.StartCoroutine(DoUpdate(stat));
            switch (stat.Ref)
            {
                case true:
                    Debug.Log("更新成功");
                    break;
                case false:
                    Debug.LogWarning("更新失败");
                    break;
            }
        }
        else if (stat.Ref is false)
        {
            Debug.Log("无需更新ModLoader(no update need for ModLoader)");
        }
        else
        {
            Debug.LogWarning("更新检测过程出错");
        }
    }

    public static IEnumerator DownloadAll(string baseUrl, List<string> urls, MyRef<List<MemoryStream>> results)
    {
        foreach (var url in urls)
        {
            var webRequest = WebRequest.Create(baseUrl + url).GetResponseAsync();
            while (!webRequest.IsCompleted)
            {
                if (webRequest.IsFaulted || webRequest.IsCanceled)
                {
                    results.Ref = null;
                    yield break;
                }

                yield return null;
            }

            if (webRequest.IsCompleted && results.Ref != null)
            {
                var buf = new MemoryStream();
                webRequest.Result.GetResponseStream()!.CopyTo(buf);
                buf.Seek(0, SeekOrigin.Begin);
                results.Ref.Add(buf);
            }
            else
            {
                Debug.LogWarning($"下载 {url} 失败");
                results.Ref = null;
                yield break;
            }
        }
    }

    public static IEnumerator DoUpdate(MyRef<bool?> stat)
    {
        const string OldVersion = "OldVersion";
        const string NewVersion = "NewVersion";

        var ModLoaderLocation = ModLoaderInstance.Info.Location;
        var ModLoaderDir = Path.GetDirectoryName(ModLoaderLocation);
        if (!Directory.Exists(Path.Combine(ModLoaderDir!, NewVersion)))
        {
            Directory.CreateDirectory(Path.Combine(ModLoaderDir!, NewVersion));
        }

        if (!Environment.Is64BitOperatingSystem)
        {
            Debug.LogError("不支持32位系统");
            yield break;
        }

        var results = new MyRef<List<MemoryStream>>([]);
        List<string> urls = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            urls =
            [
                ModLoader, CSTI_ChatTreeLoader, CSTI_LuaActionSupport, DotNetZip, Jsonnet4CSTIModLoader, LitJSON,
                lua54_w64, 更新历史
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            urls =
            [
                ModLoader, CSTI_ChatTreeLoader, CSTI_LuaActionSupport, DotNetZip, libJsonnet4CSTIModLoader_linux,
                LitJSON,
                liblua54_linux, 更新历史
            ];
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            urls =
            [
                ModLoader, CSTI_ChatTreeLoader, CSTI_LuaActionSupport, DotNetZip, libJsonnet4CSTIModLoader_mac, LitJSON,
                liblua54_mac, 更新历史
            ];
        }

        if (urls == null)
        {
            Debug.LogError("未知的操作系统");
            yield break;
        }

        yield return ModLoaderInstance.StartCoroutine(DownloadAll(BaseDir,
            urls, results));

        if (results.Ref == null)
        {
            Debug.LogError("更新中发生网络错误");
            yield break;
        }

        foreach (var (file, response) in urls.Zip(results.Ref, (s, response) => (file: s, response)))
        {
            using var fileStream = new FileStream(Path.Combine(ModLoaderDir!, NewVersion, file), FileMode.Create);
            response.CopyTo(fileStream);
        }

        if (Directory.Exists(Path.Combine(ModLoaderDir, OldVersion)))
        {
            Directory.Delete(Path.Combine(ModLoaderDir, OldVersion), true);
        }

        Directory.CreateDirectory(Path.Combine(ModLoaderDir, OldVersion));
        foreach (var file in urls)
        {
            if (File.Exists(Path.Combine(ModLoaderDir, file)))
            {
                File.Move(Path.Combine(ModLoaderDir, file), Path.Combine(ModLoaderDir, OldVersion, file));
            }

            File.Move(Path.Combine(ModLoaderDir, NewVersion, file), Path.Combine(ModLoaderDir, file));
        }

        Directory.Delete(Path.Combine(ModLoaderDir, NewVersion), true);
        ModLoaderInstance.ModLoaderUpdated = true;
        stat.Ref = true;
    }

    public static IEnumerator CheckNeedUpdate(MyRef<bool?> stat)
    {
        var response_task = WebRequest.Create(BaseDir + VersionFile).GetResponseAsync();
        while (!response_task.IsCompleted)
        {
            if (response_task.IsFaulted || response_task.IsCanceled) yield break;
            yield return null;
        }

        var response = response_task.Result;
        var buf = new byte[response.ContentLength];
        if (response.GetResponseStream() is not { } responseStream) yield break;
        var readAsync = responseStream.ReadAsync(buf, 0, buf.Length);
        while (!readAsync.IsCompleted)
        {
            if (readAsync.IsFaulted || readAsync.IsCanceled) yield break;
            yield return null;
        }

        var s = Encoding.UTF8.GetString(buf);
        Debug.Log(s);
        stat.Ref = MyVersion.Parse(ModVersion) < MyVersion.Parse(s);
    }

    private static async Task<bool?> CheckNeedUpdate_debug()
    {
        var response = await WebRequest.Create(BaseDir + VersionFile).GetResponseAsync();
        var buf = new byte[response.ContentLength];
        if (response.GetResponseStream() is not { } responseStream) return null;
        _ = await responseStream.ReadAsync(buf, 0, buf.Length);
        var s = Encoding.UTF8.GetString(buf);
        CommonLogger.LogInfo(s);
        if (MyVersion.Parse(ModVersion) < MyVersion.Parse(s))
        {
            return true;
        }

        return false;
    }
}