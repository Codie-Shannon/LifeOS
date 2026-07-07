using System.Text.Json;
using LifeOS.Core.DailyState;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.DailyState;

public static class DailyStateStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("daily-state-items.json");

    public static List<DailyStateItem> Load()
    {
        if (!File.Exists(FilePath)) return [];

        var json = File.ReadAllText(FilePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<DailyStateItem>>(json, Options) ?? [];
    }

    public static void Save(IEnumerable<DailyStateItem> items)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(items.ToList(), Options));
    }
}
