using System;
using System.IO;
using System.Text.Json;

namespace HomeCareManager.Core.Configuration
{
    public static class DatabaseConfiguration
    {
        private const string ConnectionStringEnvironmentVariable = "HOMECAREMANAGER_CONNECTION_STRING";

        public const string DefaultConnectionString =
            "datasource=127.0.0.1;" +
            "port=3306;" +
            "username=root;password=;" +
            "database=homecaremanager";

        public static string GetConnectionString()
        {
            string? environmentValue = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(environmentValue))
            {
                return environmentValue;
            }

            foreach (string settingsPath in GetSettingsPaths())
            {
                string? configuredValue = TryReadConnectionString(settingsPath);
                if (!string.IsNullOrWhiteSpace(configuredValue))
                {
                    return configuredValue;
                }
            }

            return DefaultConnectionString;
        }

        private static string[] GetSettingsPaths()
        {
            return new[]
            {
                Path.Combine(AppContext.BaseDirectory, "appsettings.Development.json"),
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")
            };
        }

        private static string? TryReadConnectionString(string settingsPath)
        {
            if (!File.Exists(settingsPath))
            {
                return null;
            }

            try
            {
                using FileStream stream = File.OpenRead(settingsPath);
                using JsonDocument document = JsonDocument.Parse(stream);

                if (!document.RootElement.TryGetProperty("ConnectionStrings", out JsonElement connectionStrings))
                {
                    return null;
                }

                if (connectionStrings.TryGetProperty("HomeCareManager", out JsonElement homeCareManager))
                {
                    return homeCareManager.GetString();
                }

                if (connectionStrings.TryGetProperty("DefaultConnection", out JsonElement defaultConnection))
                {
                    return defaultConnection.GetString();
                }
            }
            catch (JsonException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }

            return null;
        }
    }
}
