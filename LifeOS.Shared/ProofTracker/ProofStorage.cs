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
                Project = "Workshop Proof Project",
                Title = "Local proof workflow screenshot set",
                Type = ProofType.Screenshot,
                Status = ProofStatus.Ready,
                Date = today,
                Description = "Demo screenshots that can support a client-safe work summary.",
                LinkOrPath = "docs/screenshot-groups/group-03-paid-work-money-proof/",
                Notes = "Fictional demo data. Replace with real proof only in private local use."
            },
            new ProofItem
            {
                Project = "Door Invoice OCR Proof",
                Title = "Invoice extraction review notes",
                Type = ProofType.Documentation,
                Status = ProofStatus.Ready,
                Date = today.AddDays(-1),
                Description = "Review notes showing what was tested and what still requires human approval.",
                LinkOrPath = "docs/release-notes/v1.7.md",
                Notes = "Fictional demo data. Conservative wording only."
            },
            new ProofItem
            {
                Project = "Portfolio Review Lead",
                Title = "Accepted portfolio summary proof",
                Type = ProofType.CaseStudy,
                Status = ProofStatus.Accepted,
                Date = today.AddDays(-4),
                Description = "A fictional accepted proof brick for case-study tracking.",
                LinkOrPath = "README.md",
                Notes = "Fictional demo data."
            }
        ];
    }
}
