using System.Text.Json;
using LifeOS.Core.Agenda;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Agenda;

public static class AgendaStorage
{
    private const string FileName = "agenda-items.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<AgendaItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return CreateDefaultItems();

            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json)) return CreateDefaultItems();

            return JsonSerializer.Deserialize<List<AgendaItem>>(json, JsonOptions) ?? CreateDefaultItems();
        }
        catch
        {
            return CreateDefaultItems();
        }
    }

    public static void Save(IEnumerable<AgendaItem> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }

    private static List<AgendaItem> CreateDefaultItems()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new AgendaItem
            {
                Title = "Review weekly pressure",
                Type = AgendaItemType.Task,
                Status = AgendaItemStatus.Planned,
                PressureLevel = AgendaPressureLevel.Normal,
                DueDate = today,
                TimeText = "Morning",
                IsFixedCommitment = false,
                Notes = "Default starter item. Replace with real weekly pressure tasks."
            },
            new AgendaItem
            {
                Title = "Follow up on paid work",
                Type = AgendaItemType.FollowUp,
                Status = AgendaItemStatus.Planned,
                PressureLevel = AgendaPressureLevel.High,
                DueDate = today.AddDays(2),
                TimeText = "Any time",
                IsFixedCommitment = false,
                Notes = "Demo item for v0.2."
            }
        ];
    }
}
