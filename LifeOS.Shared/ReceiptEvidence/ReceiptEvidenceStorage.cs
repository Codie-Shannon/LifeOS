using System.Text.Json;
using LifeOS.Core.ReceiptEvidence;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.ReceiptEvidence;

public static class ReceiptEvidenceStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("lifeos-receipt-evidence.json");

    public static List<ReceiptEvidenceItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return LoadFallback();
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<ReceiptEvidenceItem>>(json, JsonOptions) ?? LoadFallback();
        }
        catch
        {
            return LoadFallback();
        }
    }

    public static void Save(IEnumerable<ReceiptEvidenceItem> items)
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(FilePath, JsonSerializer.Serialize(items, JsonOptions));
    }

    public static void Reset()
    {
        Save(LoadFallback());
    }

    private static List<ReceiptEvidenceItem> LoadFallback() =>
        LocalAppDataPath.IsPortfolioDemoMode ? ReceiptEvidenceDemoData.Create() : [];
}
