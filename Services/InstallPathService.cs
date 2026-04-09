using System;
using System.IO;
using PWlaunchertest.Services;

namespace PWlaunchertest.Services
{
    public enum InstallPathCheckResult
    {
        EmptyFolder,
        ExistingGameFolder,
        NonEmptyUnknownFolder,
        InvalidPath
    }

    public static class InstallPathService
    {
        public static InstallPathCheckResult CheckInstallPath(string? selectedPath)
        {
            if (string.IsNullOrWhiteSpace(selectedPath))
                return InstallPathCheckResult.InvalidPath;

            try
            {
                if (!Directory.Exists(selectedPath))
                    return InstallPathCheckResult.InvalidPath;

                string[] files = Directory.GetFiles(selectedPath);
                string[] directories = Directory.GetDirectories(selectedPath);

                bool isEmpty = files.Length == 0 && directories.Length == 0;
                if (isEmpty)
                    return InstallPathCheckResult.EmptyFolder;

                string? resolvedGameExe = GameLocatorService.ResolveGameExePath(selectedPath);
                if (!string.IsNullOrWhiteSpace(resolvedGameExe))
                    return InstallPathCheckResult.ExistingGameFolder;

                return InstallPathCheckResult.NonEmptyUnknownFolder;
            }
            catch
            {
                return InstallPathCheckResult.InvalidPath;
            }
        }

        public static string NormalizeInstallPath(string path)
        {
            return Path.GetFullPath(path.Trim());
        }
    }
}