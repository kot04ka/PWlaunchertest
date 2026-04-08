using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PWlaunchertest.Services
{
    public static class GameLocatorService
    {
        private static readonly string[] PossibleExeNames =
        {
            "PrimeWorld.exe",
            "primeworld.exe"
        };

        private static readonly string[] AdditionalMarkers =
        {
            "bin",
            "data.pak",
            "heroes",
            "profiles",
            "Launcher",
            "Game"
        };

        public static string? FindInstalledGame(string? configuredPath = null)
        {
            string? resolvedConfigured = ResolveGamePath(configuredPath);
            if (!string.IsNullOrWhiteSpace(resolvedConfigured))
                return resolvedConfigured;

            foreach (string path in GetCommonPaths())
            {
                string? resolved = ResolveGamePath(path);
                if (!string.IsNullOrWhiteSpace(resolved))
                    return resolved;
            }

            foreach (string root in GetAvailableRoots())
            {
                string? found = SearchInRoot(root, 2);
                if (!string.IsNullOrWhiteSpace(found))
                    return found;
            }

            return null;
        }

        public static bool IsValidGamePath(string? path)
        {
            return !string.IsNullOrWhiteSpace(ResolveGamePath(path));
        }

        public static string? ResolveGamePath(string? inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
                return null;

            try
            {
                if (!Directory.Exists(inputPath))
                    return null;

                string normalized = Path.GetFullPath(inputPath);

                if (LooksLikeGameFolder(normalized))
                    return normalized;

                string[] childCandidates =
                {
                    Path.Combine(normalized, "Game"),
                    Path.Combine(normalized, "Launcher"),
                    Path.Combine(normalized, "game"),
                    Path.Combine(normalized, "launcher")
                };

                foreach (string candidate in childCandidates)
                {
                    if (LooksLikeGameFolder(candidate))
                        return candidate;
                }

                IEnumerable<string> subDirs;
                try
                {
                    subDirs = Directory.EnumerateDirectories(normalized);
                }
                catch
                {
                    return null;
                }

                foreach (string dir in subDirs)
                {
                    if (LooksLikeGameFolder(dir))
                        return dir;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static bool LooksLikeGameFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return false;

            bool hasExe = PossibleExeNames.Any(exe => File.Exists(Path.Combine(path, exe)));
            if (!hasExe)
                return false;

            bool hasAdditionalMarker = AdditionalMarkers.Any(marker =>
            {
                string full = Path.Combine(path, marker);
                return Directory.Exists(full) || File.Exists(full);
            });

            return hasAdditionalMarker || HasKnownGameStructure(path);
        }

        private static bool HasKnownGameStructure(string path)
        {
            string[] knownFiles =
            {
                Path.Combine(path, "bin"),
                Path.Combine(path, "profiles"),
                Path.Combine(path, "heroes"),
                Path.Combine(path, "data.pak")
            };

            return knownFiles.Any(p => Directory.Exists(p) || File.Exists(p));
        }

        private static IEnumerable<string> GetCommonPaths()
        {
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pfx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            return new[]
            {
                Path.Combine(pf, "Prime World"),
                Path.Combine(pf, "Prime World Classic"),
                Path.Combine(pfx86, "Prime World"),
                Path.Combine(pfx86, "Prime World Classic"),
                Path.Combine(pfx86, "Steam", "steamapps", "common", "Prime World Classic"),
                Path.Combine(pfx86, "Steam", "steamapps", "common", "Prime World"),
                Path.Combine(@"C:\Games", "Prime World"),
                Path.Combine(@"C:\Games", "Prime World Classic"),
                Path.Combine(@"D:\Games", "Prime World"),
                Path.Combine(@"D:\Games", "Prime World Classic"),
                @"C:\Prime World",
                @"C:\Prime World Classic",
                @"D:\Prime World",
                @"D:\Prime World Classic"
            };
        }

        private static IEnumerable<string> GetAvailableRoots()
        {
            try
            {
                return DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .Select(d => d.RootDirectory.FullName);
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private static string? SearchInRoot(string rootPath, int maxDepth)
        {
            try
            {
                return SearchRecursive(rootPath, 0, maxDepth);
            }
            catch
            {
                return null;
            }
        }

        private static string? SearchRecursive(string currentPath, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth)
                return null;

            string? resolved = ResolveGamePath(currentPath);
            if (!string.IsNullOrWhiteSpace(resolved))
                return resolved;

            IEnumerable<string> subDirectories;
            try
            {
                subDirectories = Directory.EnumerateDirectories(currentPath);
            }
            catch
            {
                return null;
            }

            foreach (string dir in subDirectories)
            {
                string dirName = Path.GetFileName(dir);
                if (IsIgnoredDirectory(dirName))
                    continue;

                string? found = SearchRecursive(dir, currentDepth + 1, maxDepth);
                if (!string.IsNullOrWhiteSpace(found))
                    return found;
            }

            return null;
        }

        private static bool IsIgnoredDirectory(string directoryName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                return true;

            string[] ignored =
            {
                "Windows",
                "ProgramData",
                "$Recycle.Bin",
                "System Volume Information",
                "Recovery",
                "PerfLogs",
                "Temp",
                "tmp",
                "AppData"
            };

            return ignored.Any(x => string.Equals(x, directoryName, StringComparison.OrdinalIgnoreCase));
        }
    }
}