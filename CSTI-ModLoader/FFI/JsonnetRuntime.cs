using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ModLoader.FFI;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Byte64
{
    [FieldOffset(0)] public fixed byte Data[64];
}

public static class JsonnetRuntime
{
    [DllImport("Jsonnet4CSTIModLoader", EntryPoint = "JsonnetRuntime_add_pat")]
    public static extern void JsonnetRuntimeAddPat([MarshalAs(UnmanagedType.LPUTF8Str)] string pat);

    [DllImport("Jsonnet4CSTIModLoader", EntryPoint = "JsonnetRuntime_reg_global")]
    public static extern void JsonnetRuntimeRegGlobal([MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string val);

    [DllImport("Jsonnet4CSTIModLoader", EntryPoint = "JsonnetRuntime_read")]
    private static extern Byte64 JsonnetRuntimeRead(long key);

    [DllImport("Jsonnet4CSTIModLoader", EntryPoint = "JsonnetRuntime_eval")]
    private static extern long JsonnetRuntimeEval([MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string val);

    public static string JsonnetEval(string name, string val)
    {
        var eval = JsonnetRuntimeEval(name, val);
        if (eval < 0)
        {
            return "";
        }
        
        var bytes = new List<byte>(64);
        unsafe
        {
            while (true)
            {
                var runtimeRead = JsonnetRuntimeRead(eval);
                for (var i = 0; i < 64; i++)
                {
                    if (runtimeRead.Data[i] == 0)
                    {
                        goto ReadEnd;
                    }

                    bytes.Add(runtimeRead.Data[i]);
                }
            }
        }

        ReadEnd: ;
        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}