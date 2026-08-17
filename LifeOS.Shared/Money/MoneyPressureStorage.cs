using LifeOS.Core.Money;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Money;

public sealed class MoneyPressureRepository
{
    private readonly VersionedJsonLocalStore<MoneyPressureManualInput> _store;

    public MoneyPressureRepository(
        string filePath,
        Func<MoneyPressureManualInput>? emptyFactory = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<MoneyPressureManualInput>(
            filePath,
            "money-pressure",
            1,
            emptyFactory ?? (() => new MoneyPressureManualInput()),
            Normalize,
            utcNow: utcNow);
    }

    public LocalStoreLoadResult<MoneyPressureManualInput> LoadResult() => _store.Load();

    public MoneyPressureManualInput Load() => LoadResult().Value;

    public void Save(MoneyPressureManualInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!MoneyPressureInputService.Validate(input.ToDraft()).IsValid)
            throw new ArgumentException("The money-pressure snapshot is invalid.", nameof(input));
        _store.Save(input);
    }

    public LocalStoreHealth Inspect() => _store.Inspect();

    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();

    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();

    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);

    private static MoneyPressureManualInput Normalize(MoneyPressureManualInput input) => new()
    {
        CurrentBalance = decimal.Round(input.CurrentBalance, 2),
        PaidIncome = decimal.Round(input.PaidIncome, 2),
        PendingIncome = decimal.Round(input.PendingIncome, 2),
        BillsDue = decimal.Round(input.BillsDue, 2),
        DeductionsDue = decimal.Round(input.DeductionsDue, 2),
        FoodFuelBuffer = decimal.Round(input.FoodFuelBuffer, 2),
        EmergencyBuffer = decimal.Round(input.EmergencyBuffer, 2)
    };
}

public static class MoneyPressureStorage
{
    private const string FileName = "money-pressure-input.json";

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static MoneyPressureManualInput Load() => Repository().Load();

    public static void Save(MoneyPressureManualInput input) => Repository().Save(input);

    public static LocalStoreHealth Inspect() => Repository().Inspect();

    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repository().ListTrash();

    public static void RestoreTrash(string entryId) => Repository().RestoreTrash(entryId);

    public static void Reset()
    {
        if (File.Exists(FilePath)) Repository().MoveToTrash();
    }

    private static MoneyPressureRepository Repository() => new(
        FilePath,
        LocalAppDataPath.IsPortfolioDemoMode
            ? CreateDemoInput
            : () => new MoneyPressureManualInput());

    private static MoneyPressureManualInput CreateDemoInput() => new()
    {
        CurrentBalance = 120m,
        PaidIncome = 180m,
        PendingIncome = 320m,
        BillsDue = 65m,
        DeductionsDue = 15m,
        FoodFuelBuffer = 60m,
        EmergencyBuffer = 50m
    };
}
