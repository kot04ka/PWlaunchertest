using System;
using System.Collections.Generic;
using System.Text;

namespace PWlaunchertest.Models
{
    public class InstallFileEntry
    {
        public string RelativePath { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
