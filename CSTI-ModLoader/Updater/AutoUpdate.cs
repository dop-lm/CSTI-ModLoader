using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModLoader.Updater;

public static class AutoUpdate
{
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

    public static async void UpdateModIfNecessary()
    {
        if (await CheckNeedUpdate())
        {
        }
        else
        {
            Debug.Log("无需更新ModLoader(no update need for ModLoader)");
        }
    }

    public static async Task<bool> CheckNeedUpdate()
    {
        var response = await WebRequest.Create(BaseDir + VersionFile).GetResponseAsync();
        var buf = new byte[response.ContentLength];
        if (response.GetResponseStream() is not { } responseStream) return false;
        _ = await responseStream.ReadAsync(buf, 0, buf.Length);
        // var s = Encoding.UTF8.GetString(buf,Encoding);
        // Debug.Log(s);
        // if (MyVersion.Parse(ModLoader.ModVersion) < MyVersion.Parse(s))
        // {
        //     return true;
        // }

        return false;
    }
}