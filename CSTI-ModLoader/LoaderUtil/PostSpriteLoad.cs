using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JeremyAnsel.Media.Dds;
using UnityEngine;
using static ModLoader.ResourceLoadHelper;

namespace ModLoader.LoaderUtil
{
    public class ImageEntry
    {
        public readonly string ImgPath;
        public readonly string DdsPath;
        public readonly string Name;

        public ImageEntry(string imgPath)
        {
            ImgPath = imgPath;
            Name = Path.GetFileNameWithoutExtension(imgPath);
            var ddsPath = Path.ChangeExtension(imgPath, "dds");
            if (File.Exists(ddsPath))
            {
                DdsPath = ddsPath;
                return;
            }

            var fId = imgPath.Substring(imgPath.LastIndexOf(PicturePat, StringComparison.Ordinal)).Substring(8);
            var resourcePat = imgPath.Substring(0,
                imgPath.LastIndexOf(ResourcePat, StringComparison.Ordinal) - 1);
            ddsPath = Path.Combine(resourcePat, ResourcePat, "Dxt", Path.ChangeExtension(fId, "dds"));
            if (File.Exists(ddsPath))
            {
                DdsPath = ddsPath;
            }
        }
    }

    public static class PostSpriteLoad
    {
        public class PostSetter
        {
            private readonly Action<object, object> SetterFunc;
            private readonly object ToSetObj;
            private readonly string SpriteId;

            public PostSetter(Action<object, object> setterFunc, object toSetObj, string spriteId)
            {
                SetterFunc = setterFunc;
                ToSetObj = toSetObj;
                SpriteId = spriteId;
            }

            public void Set()
            {
                if (ModLoader.SpriteDict.TryGetValue(SpriteId, out var sprite))
                {
                    SetterFunc(ToSetObj, sprite);
                }
            }
        }

        public static void PostSetEnQueue(this object o, Action<object, object> setter, string id)
        {
            if (PostSetQueue.TryGetValue(id, out var queue))
            {
                queue.Enqueue(new PostSetter(setter, o, id));
            }
            else
            {
                PostSetQueue[id] = new Queue<PostSetter>(new[] { new PostSetter(setter, o, id) });
            }
        }

        public static readonly Dictionary<string, Queue<PostSetter>> PostSetQueue = new();

        public static readonly Queue<Task<(List<(byte[] dat, ImageEntry entry)> sprites, string modName)>>
            SpriteLoadQueue = new();

        public static bool NoMoreSpriteLoadQueue;

        public static bool CanEnd;
        public static bool BeginCompress;
        public static CoroutineController Controller;

        public static readonly Queue<Texture2D> ShouldCompress = new();

        public static void ToCompress(this Texture2D texture2D)
        {
            ShouldCompress.Enqueue(texture2D);
        }

        public static IEnumerator CompressOnLate()
        {
            while (!BeginCompress)
            {
                yield return null;
            }

            while (SpriteLoadQueue.Count > 0 || !NoMoreSpriteLoadQueue)
            {
                var task = SpriteLoadQueue.Dequeue();
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                var (sprites, modName) = task.Result;
                var loadStartTime = DateTime.Now;
                foreach (var (dat, imageEntry) in sprites)
                {
                    var startTime = DateTime.Now;
                    if ((startTime - loadStartTime).TotalMilliseconds > 25)
                    {
                        loadStartTime = startTime;
                        yield return null;
                    }

                    Texture2D t2d;
                    Rect spriteSize;
                    if (imageEntry.DdsPath != null)
                    {
                        var dds = DdsFile.FromStream(new MemoryStream(dat));
                        spriteSize = new Rect(0, 0, dds.Width, dds.Height);
                        var rawSize = new Vector2Int((dds.Width & 0b11) == 0 ? dds.Width : ((dds.Width >> 2) + 1) << 2,
                            (dds.Height & 0b11) == 0 ? dds.Height : ((dds.Height >> 2) + 1) << 2);
                        t2d = dds.PixelFormat.FourCC switch
                        {
                            DdsFourCC.DXT1 => new Texture2D(rawSize.x, rawSize.y, TextureFormat.DXT1, 0, false),
                            DdsFourCC.DXT5 => new Texture2D(rawSize.x, rawSize.y, TextureFormat.DXT5, 0, false),
                            _ => new Texture2D(dds.Width, dds.Height, TextureFormat.RGBA32, 0, false)
                        };
                        t2d.GetRawTextureData<byte>().CopyFrom(dds.Data);
                        t2d.Apply();
                        dds = null;
                    }
                    else
                    {
                        t2d = new Texture2D(0, 0, TextureFormat.RGBA32, 0, false);
                        t2d.LoadImage(dat);
                        spriteSize = new Rect(0, 0, t2d.width, t2d.height);
                    }

                    if (imageEntry.DdsPath != null && !ModLoader.TexCompatibilityMode.Value)
                    {
                        t2d.Compress(false);
                    }

                    var sprite = Sprite.Create(t2d, spriteSize,
                        Vector2.zero);
                    sprite.name = imageEntry.Name;
                    if (!ModLoader.SpriteDict.ContainsKey(imageEntry.Name))
                    {
                        ModLoader.SpriteDict.Add(imageEntry.Name, sprite);
                        if (!PostSetQueue.TryGetValue(imageEntry.Name, out var postSetters)) continue;
                        while (postSetters.Count > 0)
                        {
                            var postSetter = postSetters.Dequeue();
                            postSetter.Set();
                        }

                        PostSetQueue.Remove(imageEntry.Name);
                    }
                    else
                        Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", modName, imageEntry);
                }
            }

            while (!CanEnd)
            {
                yield return null;
            }

            foreach (var (_, queue) in PostSetQueue)
            {
                while (queue.Count > 0)
                {
                    var postSetter = queue.Dequeue();
                    postSetter.Set();
                }
            }

            var graphicsArray = Resources.FindObjectsOfTypeAll<CardGraphics>();
            foreach (var graphics in graphicsArray.Where(graphics => graphics && graphics.CardLogic))
            {
                try
                {
                    graphics.Setup(graphics.CardLogic);
                }
                catch (Exception e)
                {
                    ModLoader.Instance.CommonLogger.LogError(e);
                }
            }

            ModLoader.ShowLoadSuccess += 1.2f;

            while (ShouldCompress.Count > 0)
            {
                var timeStart = DateTime.Now;
                while ((DateTime.Now - timeStart).TotalMilliseconds < 25 && ShouldCompress.Count > 0)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var texture2D = ShouldCompress.Dequeue();
                        texture2D.Compress(false);
                    }
                }

                yield return null;
            }
        }
    }
}