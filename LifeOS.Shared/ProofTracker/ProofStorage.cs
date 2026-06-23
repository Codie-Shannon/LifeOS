using System.Text.Json;
using LifeOS.Core.ProofTracker;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.ProofTracker;

public static class ProofStorage
{
    private const string FileName = "proof-items.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<ProofItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return CreateDefaultItems();

            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json)) return CreateDefaultItems();

            return JsonSerializer.Deserialize<List<ProofItem>>(json, JsonOptions) ?? CreateDefaultItems();
        }
        catch
        {
            return CreateDefaultItems();
        }
    }

    public static void Save(IEnumerable<ProofItem> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }

    private static List<ProofItem> CreateDefaultItems()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new ProofItem
            {
                Project = "LifeOS",
                Title = "LifeOS v0.1 release",
                Type = ProofType.Release,
                Status = ProofStatus.Accepted,
                Date = today.AddDays(-1),
                Description = "Desktop shell proof with Money Pressure, Follow-Ups, and Command Centre.",
                LinkOrPath = "README.md",
                Notes = "Default starter proof item."
            }
        ];
    }
}
