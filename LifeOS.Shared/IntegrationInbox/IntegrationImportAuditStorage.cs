using System.Text.Json;
using LifeOS.Core.IntegrationConnectors;

namespace LifeOS.Shared.IntegrationInbox;

public static class IntegrationImportAuditStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LifeOS", "lifeos-integration-import-audit.json");

    public static List<IntegrationImportAuditEntry> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<IntegrationImportAuditEntry>>(
                File.ReadAllText(FilePath), Options) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static void Save(IEnumerable<IntegrationImportAuditEntry> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(entries, Options));
    }

    public static List<IntegrationImportAuditEntry> Append(IntegrationImportAuditEntry entry)
    {
        var entries = Load();
        entries.Insert(0, entry);
        Save(entries);

        return entries;
    }
}
