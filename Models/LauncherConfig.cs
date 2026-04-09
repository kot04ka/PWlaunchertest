using System;
using System.Collections.Generic;
using System.Text;

namespace PWlaunchertest.Models
{
    public class LauncherConfig
    {
        public string GameExePath { get; set; } = string.Empty;
        public string InstallPath { get; set; } = string.Empty;
        public string BaseInstallManifestPath { get; set; } = "Data/base-install-manifest.json";
    }
}
