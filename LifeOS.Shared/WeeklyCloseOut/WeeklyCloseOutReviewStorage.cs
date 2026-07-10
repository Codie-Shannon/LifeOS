using System.Text.Json;
using LifeOS.Core.WeeklyCloseOut;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.WeeklyCloseOut;

public static class WeeklyCloseOutReviewStorage
{
    private const string FileName = "lifeos-weekly-close-out-review.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<WeeklyCloseOutReviewItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                var defaults = WeeklyCloseOutReviewDemoData.Create();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return WeeklyCloseOutReviewDemoData.Create();
            }

            return JsonSerializer.Deserialize<List<WeeklyCloseOutReviewItem>>(json, JsonOptions)
                ?? WeeklyCloseOutReviewDemoData.Create();
        }
        catch
        {
            return WeeklyCloseOutReviewDemoData.Create();
        }
    }

    public static void Save(IEnumerable<WeeklyCloseOutReviewItem> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        Save(WeeklyCloseOutReviewDemoData.Create());
    }
}
