using System.Text.Json;
using LifeOS.Core.FollowUps;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.FollowUps;

public static class FollowUpStorage
{
    private const string FileName = "follow-ups.json";

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<FollowUpItem> Load() => Store().Load().Value;

    public static LocalStoreHealth Inspect() => Store().Inspect();

    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Store().ListTrash();

    public static void RestoreTrash(string entryId) => Store().RestoreTrash(entryId);

    private static List<FollowUpItem> LoadFallback() =>
        LocalAppDataPath.IsPortfolioDemoMode ? CreateDefaultFollowUps() : [];

    public static void Save(IEnumerable<FollowUpItem> items)
    {
        Store().Save(items.ToList());
    }

    public static void Reset()
    {
        if (File.Exists(FilePath)) Store().MoveToTrash();
    }

    private static VersionedJsonLocalStore<List<FollowUpItem>> Store() => new(
        FilePath,
        "follow-ups",
        1,
        LoadFallback);

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
