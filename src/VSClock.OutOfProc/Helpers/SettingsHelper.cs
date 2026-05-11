using Newtonsoft.Json;
using VSClock.OutOfProc.Models;
using VSClock.OutOfProc.Services;

namespace VSClock.OutOfProc.Helpers;

public static class SettingsHelper
{
    private static readonly string _globalSettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "VSClock");

    private static readonly string _globalSettingsFile = Path.Combine(_globalSettingsFolder, "VSClock.json");

    private static GlobalSettings? _globalSettings;

    /// <summary>
    /// Save global settings to disk.
    /// </summary>
    public static async Task SaveGlobalSettings(
        GlobalSettings settings,
        OutputWindowService outputWindowService)
    {
        try
        {
            if (!Directory.Exists(_globalSettingsFolder))
            {
                Directory.CreateDirectory(_globalSettingsFolder);
            }

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            using var writer = new StreamWriter(_globalSettingsFile, false);

            await writer.WriteAsync(json);

            _globalSettings = settings;
        }
        catch (Exception e)
        {
            await outputWindowService.WriteException("Failed to save global settings.", e);
        }
    }


    /// <summary>
    /// Loads saved global settings from disk.
    /// </summary>
    public static async Task<GlobalSettings> LoadGlobalSettings(OutputWindowService outputWindowService)
    {
        try
        {
            if (!File.Exists(_globalSettingsFile))
            {
                return new();
            }

            using var reader = new StreamReader(_globalSettingsFile);
            var json = await reader.ReadToEndAsync();
            var settings = JsonConvert.DeserializeObject<GlobalSettings>(json);

            _globalSettings = settings ?? new();

            return _globalSettings;
        }
        catch (Exception e)
        {
            await outputWindowService.WriteException("Failed to load global settings.", e);
        }

        return new();
    }

    /// <summary>
    /// Loads cached global settings from memory or loads them from disk if not cached.
    /// </summary>
    /// <returns></returns>
    public static async Task<GlobalSettings> GetGlobalSettings(OutputWindowService outputWindowService)
        => _globalSettings ?? await LoadGlobalSettings(outputWindowService);
}
