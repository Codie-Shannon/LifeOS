using System.Text.Json;
using LifeOS.Core.CommandCentrePressure;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.CommandCentrePressure;

public static class CommandCentrePressurePolicyStorage
{
    private const string FileName = "lifeos-command-centre-pressure-policy.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static CommandCentrePressurePolicy Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                var defaults = CreateDefault();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<CommandCentrePressurePolicy>(json, JsonOptions)
                ?? CreateDefault();
        }
        catch
        {
            return CreateDefault();
        }
    }

    public static void Save(CommandCentrePressurePolicy policy)
    {
        var json = JsonSerializer.Serialize(policy, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        Save(CreateDefault());
    }

    public static CommandCentrePressurePolicy CreateDefault()
    {
        return new CommandCentrePressurePolicy
        {
            LowWeight = 1,
            NormalWeight = 3,
            HighWeight = 7,
            CriticalWeight = 12,
            NormalScore = 5,
            HighScore = 16,
            CriticalScore = 30,
            MaximumTopSignals = 8,
            SuppressWaitingOnOthers = true,
            RequireReviewForUntrusted = true
        };
    }
}
