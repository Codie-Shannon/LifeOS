using System.Text.Json;
using LifeOS.Core.FollowUps;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.FollowUps;

public static class FollowUpStorage
{
    private const string FileName = "follow-ups.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<FollowUpItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return CreateDefaultFollowUps();
            }

            var json = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateDefaultFollowUps();
            }

            return JsonSerializer.Deserialize<List<FollowUpItem>>(json, JsonOptions)
                ?? CreateDefaultFollowUps();
        }
        catch
        {
            return CreateDefaultFollowUps();
        }
    }

    public static void Save(IEnumerable<FollowUpItem> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }

    private static List<FollowUpItem> CreateDefaultFollowUps()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new FollowUpItem
            {
                PersonOrOrganisation = "JV Systems",
                Context = "Development environment/payment setup",
                NextAction = "Wait for payment/time tracking confirmation before continuing work",
                FollowUpDate = today.AddDays(2),
                Status = FollowUpStatus.Waiting,
                Priority = FollowUpPriority.High,
                IsMoneyLinked = true,
                Notes = "Demo/private workflow example. Replace with real or anonymised data as needed."
            },
            new FollowUpItem
            {
                PersonOrOrganisation = "Total Door Systems",
                Context = "OCR invoice/bill extraction proof",
                NextAction = "Wait for scope/payment confirmation before building further proof work",
                FollowUpDate = today.AddDays(4),
                Status = FollowUpStatus.Waiting,
                Priority = FollowUpPriority.High,
                IsMoneyLinked = true,
                Notes = "Demo/private workflow example. Replace with real or anonymised data as needed."
            },
            new FollowUpItem
            {
                PersonOrOrganisation = "OSHE / Vanessa",
                Context = "OnboardingFlow phased scope discussion",
                NextAction = "Follow up mid/late July if she has not come back first",
                FollowUpDate = new DateOnly(2026, 7, 20),
                Status = FollowUpStatus.Parked,
                Priority = FollowUpPriority.Normal,
                IsMoneyLinked = true,
                Notes = "Warm lead parked correctly."
            }
        ];
    }
}