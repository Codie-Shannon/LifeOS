using System.Text.Json;
using LifeOS.Core.WeeklyCloseOut;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.WeeklyCloseOut;

public static class WeeklyCloseOutStorage
{
    private const string FileName = "weekly-close-out.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<WeeklyCloseOutEntry> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return CreateDefaultEntries();

            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json)) return CreateDefaultEntries();

            return JsonSerializer.Deserialize<List<WeeklyCloseOutEntry>>(json, JsonOptions) ?? CreateDefaultEntries();
        }
        catch
        {
            return CreateDefaultEntries();
        }
    }

    public static void Save(IEnumerable<WeeklyCloseOutEntry> entries)
    {
        var json = JsonSerializer.Serialize(entries, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }

    private static List<WeeklyCloseOutEntry> CreateDefaultEntries()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1);

        return
        [
            new WeeklyCloseOutEntry
            {
                WeekStart = weekStart,
                WhatGotDone = "LifeOS v0.2 planning started.",
                WhatMoved = "Agenda, Pay Later, and Weekly Close-Out are becoming real modules.",
                StillWaitingOn = "Client samples/replies where applicable.",
                NextWeekPressure = "Keep building controlled proof without scope creep.",
                Notes = "Default starter close-out. Replace with real weekly notes."
            }
        ];
    }
}
