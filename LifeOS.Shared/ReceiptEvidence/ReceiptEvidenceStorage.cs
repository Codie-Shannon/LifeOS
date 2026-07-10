using System.Text.Json;
using LifeOS.Core.ReceiptEvidence;

namespace LifeOS.Shared.ReceiptEvidence;

public static class ReceiptEvidenceStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LifeOS", "lifeos-receipt-evidence.json");

    public static List<ReceiptEvidenceItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                var defaults = ReceiptEvidenceDemoData.Create();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<ReceiptEvidenceItem>>(json, JsonOptions) ?? ReceiptEvidenceDemoData.Create();
        }
        catch
        {
            return ReceiptEvidenceDemoData.Create();
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
        Save(ReceiptEvidenceDemoData.Create());
    }
}
