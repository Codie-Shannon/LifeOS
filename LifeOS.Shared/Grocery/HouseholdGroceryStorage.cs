using LifeOS.Core.Grocery;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Grocery;

public sealed class HouseholdGroceryRepository
{
    private readonly VersionedJsonLocalStore<HouseholdGroceryState> _store;

    public HouseholdGroceryRepository(
        string filePath,
        Func<HouseholdGroceryState>? emptyFactory = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<HouseholdGroceryState>(
            filePath,
            "household-grocery",
            1,
            emptyFactory ?? (() => HouseholdGroceryState.Empty),
            HouseholdGroceryCaptureService.Normalize,
            utcNow: utcNow);
    }

    public LocalStoreLoadResult<HouseholdGroceryState> LoadResult() => _store.Load();
    public HouseholdGroceryState Load() => LoadResult().Value;
    public void Save(HouseholdGroceryState state) => _store.Save(state);
    public LocalStoreHealth Inspect() => _store.Inspect();
    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();
    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();
    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);
}

public static class HouseholdGroceryStorage
{
    private const string FileName = "household-grocery.json";

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);
    public static HouseholdGroceryState Load() => Repository().Load();
    public static void Save(HouseholdGroceryState state) => Repository().Save(state);
    public static LocalStoreHealth Inspect() => Repository().Inspect();
    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();
    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);
    public static void Reset() { if (File.Exists(FilePath)) Repository().MoveToTrash(); }

    private static HouseholdGroceryRepository Repository() => new(
        FilePath,
        LocalAppDataPath.IsPortfolioDemoMode ? CreateProofState : () => HouseholdGroceryState.Empty);

    private static HouseholdGroceryState CreateProofState()
    {
        (IReadOnlyList<GroceryItem> items, IReadOnlyList<GroceryList> lists,
            IReadOnlyList<RecurringEssential> essentials) = GroceryProofData.Build();
        return new HouseholdGroceryState(items.ToList(), lists.ToList(), essentials.ToList());
    }
}
