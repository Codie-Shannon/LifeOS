using System.Text.Json;
using System.Text.Json.Serialization;

namespace LifeOS.Core.IntegrationControlCentre;

public static class IntegrationControlCentreStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LifeOS",
        "integration-control-centre.json");

    public static IntegrationControlCentreState LoadOrCreate(
        DateTimeOffset nowUtc,
        string? path = null)
    {
        string resolvedPath = path ?? DefaultPath;

        if (!File.Exists(resolvedPath))
        {
            IntegrationControlCentreState seeded = IntegrationControlCentreSeed.CreateFictional(nowUtc);

            try
            {
                Save(seeded, resolvedPath);
            }
            catch (Exception exception) when (
                exception is IOException or
                UnauthorizedAccessException)
            {
                // The proof surface remains usable in memory when local persistence is unavailable.
            }

            return seeded;
        }

        try
        {
            string json = File.ReadAllText(resolvedPath);
            IntegrationControlCentreState? state = JsonSerializer.Deserialize<IntegrationControlCentreState>(
                json,
                SerializerOptions);

            return state?.Normalize() ?? IntegrationControlCentreSeed.CreateFictional(nowUtc);
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            JsonException)
        {
            return IntegrationControlCentreSeed.CreateFictional(nowUtc);
        }
    }

    public static void Save(
        IntegrationControlCentreState state,
        string? path = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        string resolvedPath = path ?? DefaultPath;
        string? directory = Path.GetDirectoryName(resolvedPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = resolvedPath + ".tmp";
        string json = JsonSerializer.Serialize(state.Normalize(), SerializerOptions);
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, resolvedPath, overwrite: true);
    }
}
