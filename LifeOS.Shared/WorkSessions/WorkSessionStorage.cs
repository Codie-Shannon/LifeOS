using System.Text.Json;
using LifeOS.Core.WorkSessions;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.WorkSessions;

public static class WorkSessionStorage
{
    private const string FileName = "work-sessions.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<WorkSession> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return CreateDefaultSessions();

            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json)) return CreateDefaultSessions();

            return JsonSerializer.Deserialize<List<WorkSession>>(json, JsonOptions) ?? CreateDefaultSessions();
        }
        catch
        {
            return CreateDefaultSessions();
        }
    }

    public static void Save(IEnumerable<WorkSession> sessions)
    {
        var json = JsonSerializer.Serialize(sessions, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }

    private static List<WorkSession> CreateDefaultSessions()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new WorkSession
            {
                ClientOrProject = "AIE",
                Date = today,
                Hours = 1.5m,
                HourlyRate = 35m,
                IsBillable = true,
                Status = WorkSessionStatus.Completed,
                Description = "Example paid-work discovery/proof session.",
                Notes = "Default starter data. Replace with real tracked sessions."
            }
        ];
    }
}
