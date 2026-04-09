using System.Collections.Generic;

namespace PWlaunchertest.Models
{
    public class BaseInstallManifest
    {
        public string Version { get; set; } = string.Empty;
        public string GameExeRelativePath { get; set; } = "Launcher/PWClassic.exe";
        public List<InstallFileEntry> Files { get; set; } = new();
    }
}