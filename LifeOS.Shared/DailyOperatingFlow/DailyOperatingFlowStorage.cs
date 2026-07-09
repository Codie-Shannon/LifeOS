using System.Text.Json;
using LifeOS.Core.DailyOperatingFlow;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.DailyOperatingFlow;

public static class DailyOperatingFlowStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("daily-operating-flow.json");

    public static List<DailyOperatingFlowBlock> Load()
    {
        if (!File.Exists(FilePath))
        {
            return DailyOperatingFlowDemoData.CreateDefaultBlocks();
        }

        var json = File.ReadAllText(FilePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<DailyOperatingFlowBlock>>(json, Options) ?? [];
    }

    public static void Save(IEnumerable<DailyOperatingFlowBlock> blocks)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(blocks.ToList(), Options));
    }

    public static void ResetToDemoData()
    {
        Save(DailyOperatingFlowDemoData.CreateDefaultBlocks());
    }
}
