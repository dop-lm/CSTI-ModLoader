namespace ModLoader
{
    public static class StringHelper
    {
        public static string EscapeStr(this string s)
        {
            return s.Replace("\'", $"?u{(int) '\'':x}")
                .Replace("\"", $"?u{(int) '\"':x}")
                .Replace("[",$"?u{(int)'[':x}")
                .Replace("]",$"?u{(int)']':x}")
                .Replace("\\",$"?u{(int)'\\':x}")
                .Replace("\n",$"?u{(int)'\n':x}")
                .Replace("\t",$"?u{(int)'\t':x}");
        }
    }
}