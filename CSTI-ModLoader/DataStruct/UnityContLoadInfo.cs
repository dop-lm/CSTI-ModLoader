using LitJson;

namespace ModLoader.DataStruct
{
    public class UnityContLoadInfo
    {
        public JsonData RawData;
        public string Name;
        public string Path;

        public UnityContLoadInfo(JsonData rawData, string name, string path)
        {
            RawData = rawData;
            Name = name;
            Path = path;
        }
    }

    public static class UnityContSerializeJsonDataReg
    {
        public static void RegContSerializeData(this string id,string path, JsonData data)
        {
            ModLoader.UnityContSerializeJsonData[id] = new UnityContLoadInfo(data, id, path);
        }
    }
}