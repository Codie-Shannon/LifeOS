using System.Text.Json;
using System.Text.Json.Serialization;

namespace LifeOS.Core.IntegrationInbox;

public static class IntegrationInboxV9Store
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "LifeOS",
        "integration-inbox-v9.json");

    public static IntegrationInboxV9State LoadOrCreate(
        DateTimeOffset nowUtc,
        string? path = null)
    {
        string resolvedPath = path ?? DefaultPath;

        if (!File.Exists(resolvedPath))
        {
            IntegrationInboxV9State seeded =
                IntegrationInboxV9Seed.CreateFictional(nowUtc);

            try
            {
                Save(seeded, resolvedPath);
            }
            catch (Exception exception) when (
                exception is IOException or
                UnauthorizedAccessException)
            {
                // The deterministic proof surface remains usable in memory.
            }

            return seeded;
        }

        try
        {
            string json = File.ReadAllText(resolvedPath);
            IntegrationInboxV9State? state =
                JsonSerializer.Deserialize<IntegrationInboxV9State>(
                    json,
                    SerializerOptions);

            return state?.Normalize() ??
                IntegrationInboxV9Seed.CreateFictional(nowUtc);
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            JsonException)
        {
            return IntegrationInboxV9Seed.CreateFictional(nowUtc);
        }
    }

    public static void Save(
        IntegrationInboxV9State state,
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
        string json = JsonSerializer.Serialize(
            state.Normalize(),
            SerializerOptions);

        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, resolvedPath, overwrite: true);
    }
}
