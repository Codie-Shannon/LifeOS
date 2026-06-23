using System.Text.Json;
using LifeOS.Core.PayLater;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.PayLater;

public static class PayLaterStorage
{
    private const string FileName = "pay-later-items.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<PayLaterItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return CreateDefaultItems();

            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json)) return CreateDefaultItems();

            return JsonSerializer.Deserialize<List<PayLaterItem>>(json, JsonOptions) ?? CreateDefaultItems();
        }
        catch
        {
            return CreateDefaultItems();
        }
    }

    public static void Save(IEnumerable<PayLaterItem> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }

    private static List<PayLaterItem> CreateDefaultItems()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new PayLaterItem
            {
                Name = "IRD account summary",
                Payee = "IRD",
                Amount = 311m,
                DueDate = today.AddDays(14),
                Status = PayLaterStatus.Planned,
                PressureLevel = PayLaterPressureLevel.High,
                Notes = "Default starter item. Update amount/date/status as needed."
            }
        ];
    }
}
