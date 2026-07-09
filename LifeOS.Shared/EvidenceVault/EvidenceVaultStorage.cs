using System.Text.Json;
using LifeOS.Core.EvidenceVault;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.EvidenceVault;

public static class EvidenceVaultStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("evidence-vault.json");

    public static List<EvidenceVaultItem> Load()
    {
        if (!File.Exists(FilePath)) return [];

        var json = File.ReadAllText(FilePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<EvidenceVaultItem>>(json, Options) ?? [];
    }

    public static void Save(IEnumerable<EvidenceVaultItem> items)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(items.ToList(), Options));
    }

    public static void ResetToDemoData()
    {
        Save(EvidenceVaultDemoData.CreateDefaultItems());
    }
}
