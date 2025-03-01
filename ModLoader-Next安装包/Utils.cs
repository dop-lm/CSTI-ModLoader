using Gameloop.Vdf.Linq;

namespace ModLoader_Next安装包;

public static class Utils
{
    public static void Deconstruct<TK, TV>(this KeyValuePair<TK, TV> keyValuePair, out TK key, out TV value)
    {
        key = keyValuePair.Key;
        value = keyValuePair.Value;
    }
}