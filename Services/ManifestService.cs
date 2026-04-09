using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using PWlaunchertest.Models;

namespace PWlaunchertest.Services
{
    public static class ManifestService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<BaseInstallManifest> LoadBaseInstallManifestAsync(string manifestUrl)
        {
            if (string.IsNullOrWhiteSpace(manifestUrl))
                throw new InvalidOperationException("Не указан путь к manifest базовой установки.");

            string json;

            if (Uri.TryCreate(manifestUrl, UriKind.Absolute, out Uri? uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                json = await _httpClient.GetStringAsync(manifestUrl);
            }
            else
            {
                string localPath = Path.GetFullPath(manifestUrl);

                if (!File.Exists(localPath))
                    throw new FileNotFoundException("Manifest базовой установки не найден.", localPath);

                json = await File.ReadAllTextAsync(localPath);
            }

            BaseInstallManifest? manifest = JsonSerializer.Deserialize<BaseInstallManifest>(json);

            if (manifest == null)
                throw new InvalidOperationException("Не удалось прочитать manifest базовой установки.");

            if (manifest.Files == null || manifest.Files.Count == 0)
                throw new InvalidOperationException("Manifest не содержит файлов для установки.");

            return manifest;
        }
    }
}