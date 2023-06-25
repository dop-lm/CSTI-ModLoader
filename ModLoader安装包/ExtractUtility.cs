using System;
using System.Collections.Generic;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace ModLoader安装包
{
    public static class ExtractUtility
    {
        public static void ExtractArch(this IArchive archive, string targetDir, Func<string, bool> extract)
        {
            var extractionOptions = new ExtractionOptions
            {
                Overwrite = true, ExtractFullPath = true, PreserveFileTime = true
            };
            foreach (var archiveEntry in archive.Entries)
            {
                if (extract(archiveEntry.Key))
                {
                    archiveEntry.WriteToDirectory(targetDir, extractionOptions);
                }
            }
        }

        public static void Deconstruct<TKey, TVal>(this KeyValuePair<TKey, TVal> keyValue, out TKey key, out TVal val)
        {
            key = keyValue.Key;
            val = keyValue.Value;
        }
    }
}