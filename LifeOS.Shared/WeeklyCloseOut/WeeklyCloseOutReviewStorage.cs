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
                return LoadFallback();
            }

            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return LoadFallback();
            }

            return JsonSerializer.Deserialize<List<WeeklyCloseOutReviewItem>>(json, JsonOptions)
                ?? LoadFallback();
        }
        catch
        {
            return LoadFallback();
        }
    }

    public static void Save(IEnumerable<WeeklyCloseOutReviewItem> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        Save(LoadFallback());
    }

    private static List<WeeklyCloseOutReviewItem> LoadFallback() =>
        LocalAppDataPath.IsPortfolioDemoMode ? WeeklyCloseOutReviewDemoData.Create() : [];
}
