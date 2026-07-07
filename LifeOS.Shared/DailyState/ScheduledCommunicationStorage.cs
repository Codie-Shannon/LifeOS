using System.Text.Json;
using LifeOS.Core.DailyState;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.DailyState;

public static class ScheduledCommunicationStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("scheduled-communications.json");

    public static List<ScheduledCommunicationItem> Load()
    {
        if (!File.Exists(FilePath)) return [];

        var json = File.ReadAllText(FilePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<ScheduledCommunicationItem>>(json, Options) ?? [];
    }

    public static void Save(IEnumerable<ScheduledCommunicationItem> items)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(items.ToList(), Options));
    }
}
