using LifeOS.Core.Documents;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Documents;

public sealed class DocumentIntakeRepository
{
    private readonly VersionedJsonLocalStore<List<DocumentRecord>> _store;

    public DocumentIntakeRepository(
        string filePath,
        Func<List<DocumentRecord>>? emptyFactory = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<List<DocumentRecord>>(
            filePath,
            "document-intake",
            1,
            emptyFactory ?? (() => []),
            Normalize,
            utcNow: utcNow);
    }

    public LocalStoreLoadResult<List<DocumentRecord>> LoadResult() => _store.Load();
    public List<DocumentRecord> Load() => LoadResult().Value;

    public void Save(IEnumerable<DocumentRecord> records)
    {
        List<DocumentRecord> candidate = records.ToList();
        foreach (DocumentRecord record in candidate) DocumentIntegrity.Verify(record.Original);
        _store.Save(candidate);
    }

    public LocalStoreHealth Inspect() => _store.Inspect();
    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();
    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();
    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);

    private static List<DocumentRecord> Normalize(List<DocumentRecord> records)
    {
        foreach (DocumentRecord record in records) DocumentIntegrity.Verify(record.Original);
        return records
            .OrderBy(record => record.State == DocumentIntakeState.Rejected)
            .ThenByDescending(record => record.Original.ImportedUtc)
            .ToList();
    }
}

public static class DocumentIntakeStorage
{
    private const string FileName = "document-intake.json";
    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);
    public static List<DocumentRecord> Load() => Repository().Load();
    public static void Save(IEnumerable<DocumentRecord> records) => Repository().Save(records);
    public static LocalStoreHealth Inspect() => Repository().Inspect();
    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();
    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);
    public static void Reset() { if (File.Exists(FilePath)) Repository().MoveToTrash(); }

    private static DocumentIntakeRepository Repository() => new(
        FilePath,
        LocalAppDataPath.IsPortfolioDemoMode ? CreateProofRecords : () => []);

    private static List<DocumentRecord> CreateProofRecords()
    {
        (IReadOnlyList<DocumentRecord> records, _) = DocumentProofData.Build(
            new DateTimeOffset(2026, 7, 20, 12, 0, 0, TimeSpan.FromHours(12)));
        return records.ToList();
    }
}
