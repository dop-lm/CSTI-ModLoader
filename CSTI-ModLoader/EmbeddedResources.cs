using System.IO;

namespace ModLoader;

public static class EmbeddedResources
{
    public static Stream CSTIFonts => typeof(ModLoader).Assembly.GetManifestResourceStream("csti_fonts.bundle");
}