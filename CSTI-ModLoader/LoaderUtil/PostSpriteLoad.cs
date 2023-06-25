using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ModLoader.LoaderUtil
{
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
                PostSetQueue[id] = new Queue<PostSetter>(new[] {new PostSetter(setter, o, id)});
            }
        }

        public static readonly Dictionary<string, Queue<PostSetter>> PostSetQueue = new();

        public static readonly Queue<Task<(List<(byte[] dat, string name)> sprites, string modName)>>
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
                yield return new WaitForSeconds(0.1f);
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
                foreach (var (dat, name) in sprites)
                {
                    var startTime = DateTime.Now;
                    if ((startTime - loadStartTime).TotalMilliseconds > 20)
                    {
                        loadStartTime = startTime;
                        yield return null;
                    }

                    var t2d = new Texture2D(0, 0, TextureFormat.RGBA32, 0, false);
                    t2d.LoadImage(dat);

                    if (!ModLoader.TexCompatibilityMode.Value)
                    {
                        t2d.Compress(false);
                    }

                    var sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height),
                        Vector2.zero);
                    sprite.name = name;
                    if (!ModLoader.SpriteDict.ContainsKey(name))
                    {
                        ModLoader.SpriteDict.Add(name, sprite);
                        if (!PostSetQueue.TryGetValue(name, out var postSetters)) continue;
                        while (postSetters.Count > 0)
                        {
                            var postSetter = postSetters.Dequeue();
                            postSetter.Set();
                        }
                    }
                    else
                        Debug.LogWarningFormat("{0} SpriteDict Same Key was Add {1}", modName, name);
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

            ModLoader.ShowLoadSuccess += 0.5f;

            while (ShouldCompress.Count > 0)
            {
                var timeStart = DateTime.Now;
                while ((DateTime.Now - timeStart).TotalMilliseconds < 20 && ShouldCompress.Count > 0)
                {
                    var texture2D = ShouldCompress.Dequeue();
                    texture2D.Compress(false);
                }

                yield return null;
            }
        }
    }
}