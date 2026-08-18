using LifeOS.Core.DeviceTransfer;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.DeviceTransfer;

public sealed class DeviceTransferRepository
{
    private readonly VersionedJsonLocalStore<List<DeviceTransferReview>> _store;
    public DeviceTransferRepository(string filePath, Func<List<DeviceTransferReview>>? emptyFactory = null, Func<DateTimeOffset>? utcNow = null) { _store = new VersionedJsonLocalStore<List<DeviceTransferReview>>(filePath, "device-transfer-review", 1, emptyFactory ?? (() => []), Normalize, utcNow: utcNow); }
    public LocalStoreLoadResult<List<DeviceTransferReview>> LoadResult() => _store.Load();
    public List<DeviceTransferReview> Load() => LoadResult().Value;
    public void Save(IEnumerable<DeviceTransferReview> reviews) => _store.Save(reviews.ToList());
    public LocalStoreHealth Inspect() => _store.Inspect();
    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();
    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();
    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);
    private static List<DeviceTransferReview> Normalize(List<DeviceTransferReview> reviews) => reviews.Select(DeviceTransferService.Normalize).OrderBy(review => review.State != DeviceTransferState.ConflictReview).ThenByDescending(review => review.UpdatedUtc).ToList();
}

public static class DeviceTransferStorage
{
    private const string FileName = "device-transfer-review.json";
    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);
    private static DeviceTransferRepository Repository() => new(FilePath, LocalAppDataPath.IsPortfolioDemoMode ? Demo : () => []);
    public static List<DeviceTransferReview> Load() => Repository().Load();
    public static void Save(IEnumerable<DeviceTransferReview> reviews) => Repository().Save(reviews);
    public static LocalStoreHealth Inspect() => Repository().Inspect();
    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();
    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);
    public static void Reset() { if (File.Exists(FilePath)) Repository().MoveToTrash(); }
    private static List<DeviceTransferReview> Demo() { DateTimeOffset now = DateTimeOffset.UtcNow; return [DeviceTransferService.Create(new DeviceTransferDraft("Fictional Laptop", "Fictional Desktop", "project/demo-1", new string('a', 64), new string('b', 64)), now.AddHours(-2)), DeviceTransferService.Create(new DeviceTransferDraft("Fictional Phone", "Fictional Desktop", "note/demo-2", new string('c', 64), new string('c', 64)), now.AddHours(-1))]; }
}
