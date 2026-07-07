using System.Text.Json;
using LifeOS.Core.TimesheetEvidence;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.TimesheetEvidence;

public static class TimesheetEvidenceStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("timesheet-evidence.json");

    public static List<TimesheetEvidenceEntry> Load()
    {
        if (!File.Exists(FilePath)) return [];

        var json = File.ReadAllText(FilePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<TimesheetEvidenceEntry>>(json, Options) ?? [];
    }

    public static void Save(IEnumerable<TimesheetEvidenceEntry> entries)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(entries.ToList(), Options));
    }
}
