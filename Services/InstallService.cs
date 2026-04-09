using System;
using System.IO;
using System.Threading.Tasks;
using PWlaunchertest.Models;

namespace PWlaunchertest.Services
{
    public class InstallService
    {
        public async Task<string> InstallBaseGameAsync(
            string installPath,
            BaseInstallManifest manifest,
            IProgress<string>? statusProgress = null,
            IProgress<double>? overallProgress = null)
        {
            if (string.IsNullOrWhiteSpace(installPath))
                throw new InvalidOperationException("Не выбран путь установки.");

            if (!Directory.Exists(installPath))
                Directory.CreateDirectory(installPath);

            if (manifest.Files == null || manifest.Files.Count == 0)
                throw new InvalidOperationException("Manifest пустой.");

            int totalFiles = manifest.Files.Count;

            for (int i = 0; i < totalFiles; i++)
            {
                InstallFileEntry file = manifest.Files[i];

                if (string.IsNullOrWhiteSpace(file.RelativePath))
                    throw new InvalidOperationException("В manifest найден файл без RelativePath.");

                if (string.IsNullOrWhiteSpace(file.DownloadUrl))
                    throw new InvalidOperationException($"Для файла {file.RelativePath} не указан DownloadUrl.");

                string destinationPath = Path.Combine(installPath, file.RelativePath);

                statusProgress?.Report($"Скачивание: {file.RelativePath}");

                double basePercent = (double)i / totalFiles * 100.0;
                double stepWeight = 100.0 / totalFiles;

                Progress<double> perFileProgress = new Progress<double>(filePercent =>
                {
                    double overall = basePercent + (filePercent / 100.0 * stepWeight);
                    overallProgress?.Report(overall);
                });

                await DownloadService.DownloadFileAsync(file.DownloadUrl, destinationPath, perFileProgress);
            }

            string exePath = Path.Combine(installPath, manifest.GameExeRelativePath);

            if (!File.Exists(exePath))
                throw new FileNotFoundException("После установки не найден исполняемый файл игры.", exePath);

            statusProgress?.Report("Установка завершена");
            overallProgress?.Report(100);

            return exePath;
        }
    }
}