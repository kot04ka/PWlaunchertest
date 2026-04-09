using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PWlaunchertest.Services
{
    public static class GameLocatorService
    {
        private const string GameExeName = "PWClassic.exe";

        private static readonly string[] RequiredFiles =
        {
            "nw.dll",
            "resources.pak"
        };

        private static readonly string[] RequiredFolders =
        {
            "locales"
        };

        public static string? FindInstalledGameExe(string? configuredExePath = null)
        {
            string? resolvedConfigured = ResolveGameExePath(configuredExePath);
            if (!string.IsNullOrWhiteSpace(resolvedConfigured))
                return resolvedConfigured;

            foreach (string path in GetCommonPaths())
            {
                string? resolved = ResolveGameExePath(path);
                if (!string.IsNullOrWhiteSpace(resolved))
                    return resolved;
            }

            return null;
        }

        public static bool IsValidGameExe(string? exePath)
        {
            return !string.IsNullOrWhiteSpace(ResolveGameExePath(exePath));
        }

        public static string? ResolveGameExePath(string? inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
                return null;

            try
            {
                if (File.Exists(inputPath))
                {
                    if (string.Equals(Path.GetFileName(inputPath), GameExeName, StringComparison.OrdinalIgnoreCase)
                        && IsValidLauncherFolder(Path.GetDirectoryName(inputPath)!))
                    {
                        return Path.GetFullPath(inputPath);
                    }

                    return null;
                }

                if (!Directory.Exists(inputPath))
                    return null;

                string fullDirectory = Path.GetFullPath(inputPath);

                if (IsValidLauncherFolder(fullDirectory))
                    return Path.Combine(fullDirectory, GameExeName);

                string launcherSubFolder = Path.Combine(fullDirectory, "Launcher");
                if (IsValidLauncherFolder(launcherSubFolder))
                    return Path.Combine(launcherSubFolder, GameExeName);

                foreach (string subDir in Directory.EnumerateDirectories(fullDirectory))
                {
                    if (IsValidLauncherFolder(subDir))
                        return Path.Combine(subDir, GameExeName);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool IsValidLauncherFolder(string? folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return false;

            if (!Directory.Exists(folderPath))
                return false;

            string exePath = Path.Combine(folderPath, GameExeName);
            if (!File.Exists(exePath))
                return false;

            bool hasRequiredFiles = RequiredFiles.All(file => File.Exists(Path.Combine(folderPath, file)));
            bool hasRequiredFolders = RequiredFolders.All(dir => Directory.Exists(Path.Combine(folderPath, dir)));

            return hasRequiredFiles && hasRequiredFolders;
        }

        public static bool IsGameInstalledInDirectory(string? path)
        {
            return !string.IsNullOrWhiteSpace(ResolveGameExePath(path));
        }

        private static IEnumerable<string> GetCommonPaths()
        {
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pfx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            return new[]
            {
                Path.Combine(pfx86, "Steam", "steamapps", "common", "Prime World Classic"),
                Path.Combine(pfx86, "Steam", "steamapps", "common", "Prime World Classic", "Launcher"),
                Path.Combine(pf, "Prime World Classic"),
                Path.Combine(pf, "Prime World Classic", "Launcher"),
                Path.Combine(pfx86, "Prime World Classic"),
                Path.Combine(pfx86, "Prime World Classic", "Launcher"),
                Path.Combine(@"C:\Games", "Prime World Classic"),
                Path.Combine(@"C:\Games", "Prime World Classic", "Launcher"),
                Path.Combine(@"D:\Games", "Prime World Classic"),
                Path.Combine(@"D:\Games", "Prime World Classic", "Launcher"),
                @"C:\Prime World Classic",
                @"C:\Prime World Classic\Launcher",
                @"D:\Prime World Classic",
                @"D:\Prime World Classic\Launcher"
            };
        }
    }
}