using System.Text.Json;
using System.Text.Json.Serialization;

namespace LifeOS.Core.MicrosoftProvider;

public static class MicrosoftProviderStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string RootPath => Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "LifeOS");

    public static string ConfigurationPath =>
        Path.Combine(RootPath, "microsoft-provider-config.json");

    public static string StatePath =>
        Path.Combine(RootPath, "microsoft-provider-state.json");

    public static MicrosoftProviderConfiguration LoadConfiguration(
        string? path = null)
    {
        string resolvedPath = path ?? ConfigurationPath;

        try
        {
            if (!File.Exists(resolvedPath))
            {
                return new MicrosoftProviderConfiguration().Normalize();
            }

            string json = File.ReadAllText(resolvedPath);
            return (JsonSerializer.Deserialize<MicrosoftProviderConfiguration>(
                json,
                SerializerOptions) ??
                new MicrosoftProviderConfiguration()).Normalize();
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            JsonException)
        {
            return new MicrosoftProviderConfiguration().Normalize();
        }
    }

    public static void SaveConfiguration(
        MicrosoftProviderConfiguration configuration,
        string? path = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        SaveJson(configuration.Normalize(), path ?? ConfigurationPath);
    }

    public static MicrosoftProviderState LoadState(string? path = null)
    {
        string resolvedPath = path ?? StatePath;

        try
        {
            if (!File.Exists(resolvedPath))
            {
                return new MicrosoftProviderState().Normalize();
            }

            string json = File.ReadAllText(resolvedPath);
            return (JsonSerializer.Deserialize<MicrosoftProviderState>(
                json,
                SerializerOptions) ??
                new MicrosoftProviderState()).Normalize();
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            JsonException)
        {
            return new MicrosoftProviderState().Normalize();
        }
    }

    public static void SaveState(
        MicrosoftProviderState state,
        string? path = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        SaveJson(state.Normalize(), path ?? StatePath);
    }

    private static void SaveJson<T>(T value, string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = path + ".tmp";
        string json = JsonSerializer.Serialize(value, SerializerOptions);
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, path, overwrite: true);
    }
}
