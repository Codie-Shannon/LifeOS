using System.Text.Json;
using LifeOS.Core.Money;
using LifeOS.Shared.Money;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups153To156MoneyPressureTests
{
    [Fact]
    public void Ordinary_manual_input_defaults_to_honest_zero_values()
    {
        MoneyPressureManualInput input = new();

        Assert.Equal(0m, input.CurrentBalance);
        Assert.Equal(0m, input.PaidIncome);
        Assert.Equal(0m, input.PendingIncome);
        Assert.Equal(0m, input.BillsDue);
        Assert.Equal(0m, input.FoodFuelBuffer);
    }

    [Fact]
    public void Validation_allows_overdraft_but_rejects_negative_commitments()
    {
        var valid = MoneyPressureInputService.Validate(Draft(currentBalance: -250m));
        var invalid = MoneyPressureInputService.Validate(Draft(billsDue: -1m));

        Assert.True(valid.IsValid);
        Assert.Equal("amount-range", Assert.Single(invalid.ForField("money-bills-due")).Code);
    }

    [Fact]
    public void Pending_income_is_visible_but_excluded_from_safe_to_spend()
    {
        MoneyPressureSummary summary = MoneyPressureInputService.Calculate(
            Draft(currentBalance: 200m, paidIncome: 100m, pendingIncome: 500m,
                billsDue: 40m, deductionsDue: 10m, foodFuel: 50m, emergency: 25m),
            new DateOnly(2026, 8, 18));

        Assert.Equal(500m, summary.PendingIncome);
        Assert.Equal(175m, summary.SafeToSpend);
        Assert.Contains(summary.Reasons, reason => reason.Contains("not counted as safe", StringComparison.Ordinal));
    }

    [Fact]
    public void Missing_repository_returns_zero_state_without_writing()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "money-pressure-input.json");
        MoneyPressureRepository repository = new(path);

        LocalStoreLoadResult<MoneyPressureManualInput> result = repository.LoadResult();

        Assert.Equal(LocalStoreLoadState.Empty, result.State);
        Assert.Equal(0m, result.Value.CurrentBalance);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Repository_rejects_invalid_snapshot_before_write()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "money-pressure-input.json");
        MoneyPressureRepository repository = new(path);
        MoneyPressureManualInput invalid = Input(Draft(billsDue: -2m));

        Assert.Throws<ArgumentException>(() => repository.Save(invalid));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Repository_round_trips_versioned_rounded_snapshot()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "money-pressure-input.json");
        MoneyPressureRepository repository = new(path);
        repository.Save(Input(Draft(currentBalance: 123.456m)));

        MoneyPressureManualInput loaded = repository.Load();
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        Assert.Equal(123.46m, loaded.CurrentBalance);
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("money-pressure", document.RootElement.GetProperty("storeId").GetString());
        Assert.Equal(LocalStoreHealthState.Healthy, repository.Inspect().State);
    }

    [Fact]
    public void Legacy_plain_json_is_migrated_and_preserved()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "money-pressure-input.json");
        File.WriteAllText(path, JsonSerializer.Serialize(Input(Draft(currentBalance: 90m))));
        MoneyPressureRepository repository = new(path);

        LocalStoreLoadResult<MoneyPressureManualInput> result = repository.LoadResult();

        Assert.Equal(LocalStoreLoadState.MigratedLegacy, result.State);
        Assert.Equal(90m, result.Value.CurrentBalance);
        Assert.NotNull(result.PreservedPath);
        Assert.True(File.Exists(result.PreservedPath));
    }

    [Fact]
    public void Repository_trash_restore_refuses_overwrite_and_recovers_snapshot()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "money-pressure-input.json");
        MoneyPressureRepository repository = new(path);
        repository.Save(Input(Draft(currentBalance: 75m)));
        LocalStoreTrashEntry trash = repository.MoveToTrash();

        repository.RestoreTrash(trash.Id);

        Assert.Equal(75m, repository.Load().CurrentBalance);
        Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static MoneyPressureDraft Draft(
        decimal currentBalance = 0m,
        decimal paidIncome = 0m,
        decimal pendingIncome = 0m,
        decimal billsDue = 0m,
        decimal deductionsDue = 0m,
        decimal foodFuel = 0m,
        decimal emergency = 0m) => new(
            currentBalance,
            paidIncome,
            pendingIncome,
            billsDue,
            deductionsDue,
            foodFuel,
            emergency);

    private static MoneyPressureManualInput Input(MoneyPressureDraft draft) => new()
    {
        CurrentBalance = draft.CurrentBalance,
        PaidIncome = draft.PaidIncome,
        PendingIncome = draft.PendingIncome,
        BillsDue = draft.BillsDue,
        DeductionsDue = draft.DeductionsDue,
        FoodFuelBuffer = draft.FoodFuelBuffer,
        EmergencyBuffer = draft.EmergencyBuffer
    };

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "lifeos-money-pressure-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }
}
