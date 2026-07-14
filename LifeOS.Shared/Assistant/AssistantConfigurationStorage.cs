using System.Text.Json;
using LifeOS.Core.Assistant;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Assistant;

public static class AssistantConfigurationStorage
{
    private const string FileName = "assistant-configuration.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly HashSet<AssistantSourceType> DefaultEnabled =
    [
        AssistantSourceType.CommandCentre, AssistantSourceType.FollowUps, AssistantSourceType.WorkPipeline,
        AssistantSourceType.Timeline, AssistantSourceType.Evidence
    ];

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static AssistantConfiguration Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return CreateDefault();
            var value = JsonSerializer.Deserialize<AssistantConfiguration>(File.ReadAllText(FilePath), JsonOptions);
            return Normalise(value ?? CreateDefault());
        }
        catch { return AssistantConfiguration.Disabled; }
    }

    public static void Save(AssistantConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var temp = FilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(Normalise(configuration), JsonOptions));
        File.Move(temp, FilePath, true);
    }

    public static AssistantConfiguration CreateDefault() => new(false,
        Enum.GetValues<AssistantSourceType>()
            .Select(source => new AssistantSourcePermission(source, DefaultEnabled.Contains(source)))
            .ToArray());

    private static AssistantConfiguration Normalise(AssistantConfiguration value)
    {
        var permissions = Enum.GetValues<AssistantSourceType>()
            .Select(source => new AssistantSourcePermission(source, value.Sources.FirstOrDefault(p => p.Source == source)?.Enabled ?? DefaultEnabled.Contains(source)))
            .ToArray();
        return value with { Sources = permissions, MaximumRecords = Math.Clamp(value.MaximumRecords, 1, 50), MaximumQuestionCharacters = Math.Clamp(value.MaximumQuestionCharacters, 50, 1000) };
    }
}
