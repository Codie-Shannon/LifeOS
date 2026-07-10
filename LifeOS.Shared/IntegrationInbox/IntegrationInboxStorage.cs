using System.Text.Json;
using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Shared.IntegrationInbox;

public static class IntegrationInboxStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LifeOS", "lifeos-integration-inbox.json");

    public static List<IntegrationPreviewItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                var demo = IntegrationInboxDemoData.Create();
                Save(demo);
                return demo;
            }

            return JsonSerializer.Deserialize<List<IntegrationPreviewItem>>(
                File.ReadAllText(FilePath), Options) ?? [];
        }
        catch
        {
            return IntegrationInboxDemoData.Create();
        }
    }

    public static void Save(IEnumerable<IntegrationPreviewItem> items)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(items, Options));
    }

    public static List<IntegrationPreviewItem> Reset()
    {
        var demo = IntegrationInboxDemoData.Create();
        Save(demo);
        return demo;
    }
}
