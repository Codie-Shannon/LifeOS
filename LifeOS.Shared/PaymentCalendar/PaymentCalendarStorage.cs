using System.Text.Json;
using LifeOS.Core.PaymentCalendar;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.PaymentCalendar;

public static class PaymentCalendarStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("lifeos-payment-calendar.json");

    public static PaymentCalendarPlan Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return PaymentCalendarDemoData.CreateDefaultPlan();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? PaymentCalendarDemoData.CreateDefaultPlan()
                : JsonSerializer.Deserialize<PaymentCalendarPlan>(json, Options) ?? PaymentCalendarDemoData.CreateDefaultPlan();
        }
        catch
        {
            return PaymentCalendarDemoData.CreateDefaultPlan();
        }
    }

    public static void Save(PaymentCalendarPlan plan)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(plan, Options));
    }

    public static void ResetToDemoData()
    {
        Save(PaymentCalendarDemoData.CreateDefaultPlan());
    }
}
