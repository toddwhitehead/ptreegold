using Microsoft.Extensions.Configuration;
using PTreeGold.Models;

namespace PTreeGold.Services;

public class ConfigService
{
    public AppSettings Load(string settingsPath = "appsettings.json")
    {
        string resolvedPath = File.Exists(settingsPath)
            ? settingsPath
            : Path.Combine(AppContext.BaseDirectory, settingsPath);

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(resolvedPath, optional: false, reloadOnChange: false)
            .Build();

        var settings = config.Get<AppSettings>() ?? new AppSettings();

        // Normalise keys to lowercase for case-insensitive lookup
        settings.Patterns = settings.Patterns
            .ToDictionary(
                kv => kv.Key.ToLowerInvariant(),
                kv => kv.Value,
                StringComparer.OrdinalIgnoreCase);

        return settings;
    }
}
