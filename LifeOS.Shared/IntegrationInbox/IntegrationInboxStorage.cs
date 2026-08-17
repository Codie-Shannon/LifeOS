using System.Text.Json;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.IntegrationInbox;

public static class IntegrationInboxStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("lifeos-integration-inbox.json");

    public static List<IntegrationPreviewItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return LoadFallback();
            }

            return JsonSerializer.Deserialize<List<IntegrationPreviewItem>>(
                File.ReadAllText(FilePath), Options) ?? [];
        }
        catch
        {
            return LoadFallback();
        }
    }

    public static void Save(IEnumerable<IntegrationPreviewItem> items)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(items, Options));
    }

    public static List<IntegrationPreviewItem> Reset()
    {
        List<IntegrationPreviewItem> items = LoadFallback();
        Save(items);
        return items;
    }

    private static List<IntegrationPreviewItem> LoadFallback() =>
        LocalAppDataPath.IsPortfolioDemoMode ? IntegrationInboxDemoData.Create() : [];
}
