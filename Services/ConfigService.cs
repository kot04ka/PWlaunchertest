using System;
using System.IO;
using System.Text.Json;
using PWlaunchertest.Models;

namespace PWlaunchertest.Services
{
    public static class ConfigService
    {
        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "config.json");

        public static LauncherConfig Load()
        {
            try
            {
                string? directory = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(ConfigPath))
                {
                    LauncherConfig defaultConfig = new LauncherConfig();
                    Save(defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(ConfigPath);
                LauncherConfig? config = JsonSerializer.Deserialize<LauncherConfig>(json);

                return config ?? new LauncherConfig();
            }
            catch
            {
                return new LauncherConfig();
            }
        }

        public static void Save(LauncherConfig config)
        {
            try
            {
                string? directory = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
            }
        }
    }
}