using System.Text.Json;
using LifeOS.Core.Life;
using LifeOS.Shared.Life;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups165To168LifeRoutinesTests
{
    [Fact]
    public void Capture_requires_date_title_area_and_next_action()
    {
        var result = LifeRoutineService.Validate(Draft() with { Date = null, Title = null, Area = null, NextAction = null });
        Assert.False(result.IsValid);
        Assert.Contains(result.ForField("life-date"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("life-title"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("life-area"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("life-next-action"), issue => issue.Code == "required");
    }

    [Fact]
    public void Capture_requires_iso_date_and_bounded_single_line_text()
    {
        var result = LifeRoutineService.Validate(Draft() with { Date = "18/08/2026", Title = "bad\nline" });
        Assert.Equal("date-format", Assert.Single(result.ForField("life-date")).Code);
        Assert.Equal("single-line", Assert.Single(result.ForField("life-title")).Code);
    }

    [Fact]
    public void Create_preserves_explicit_local_fields_and_starts_planned()
    {
        DateTimeOffset now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);
        LifeRoutineRecord record = LifeRoutineService.Create(Draft(), now);
        Assert.Equal(new DateOnly(2026, 8, 18), record.Date);
        Assert.Equal(LifeRoutineState.Planned, record.State);
        Assert.Equal(LifeRoutinePressure.High, record.Pressure);
        Assert.True(record.Pinned);
        Assert.Equal(now, record.CreatedUtc);
    }

    [Fact]
    public void State_transitions_are_explicit_and_invalid_jump_is_rejected()
    {
        LifeRoutineRecord planned = LifeRoutineService.Create(Draft(), DateTimeOffset.UtcNow);
        LifeRoutineRecord active = LifeRoutineService.Transition(planned, LifeRoutineState.Active, DateTimeOffset.UtcNow);
        LifeRoutineRecord done = LifeRoutineService.Transition(active, LifeRoutineState.Done, DateTimeOffset.UtcNow);
        Assert.Equal(LifeRoutineState.Done, done.State);
        Assert.Throws<InvalidOperationException>(() => LifeRoutineService.Transition(planned, LifeRoutineState.Archived, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Missing_repository_is_empty_without_creating_a_file()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "life-routines.json");
        LifeRoutineRepository repository = new(path); LocalStoreLoadResult<List<LifeRoutineRecord>> result = repository.LoadResult();
        Assert.Equal(LocalStoreLoadState.Empty, result.State); Assert.Empty(result.Value); Assert.False(File.Exists(path));
    }

    [Fact]
    public void Repository_round_trips_versioned_normalized_records()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "life-routines.json");
        LifeRoutineRepository repository = new(path); repository.Save([LifeRoutineService.Create(Draft() with { Title = "  Renewal  " }, DateTimeOffset.UtcNow)]);
        LifeRoutineRecord loaded = Assert.Single(repository.Load()); using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal("Renewal", loaded.Title); Assert.Equal("life-routines", json.RootElement.GetProperty("storeId").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32()); Assert.Equal(LocalStoreHealthState.Healthy, repository.Inspect().State);
    }

    [Fact]
    public void Repository_rejects_invalid_record_without_overwriting_valid_state()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "life-routines.json");
        LifeRoutineRepository repository = new(path); LifeRoutineRecord valid = LifeRoutineService.Create(Draft(), DateTimeOffset.UtcNow); repository.Save([valid]);
        Assert.Throws<InvalidDataException>(() => repository.Save([valid with { NextAction = "" }]));
        Assert.Equal(valid.Id, Assert.Single(repository.Load()).Id);
    }

    [Fact]
    public void Trash_restore_is_recoverable_and_refuses_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "life-routines.json");
        LifeRoutineRepository repository = new(path); repository.Save([LifeRoutineService.Create(Draft(), DateTimeOffset.UtcNow)]);
        LocalStoreTrashEntry trash = repository.MoveToTrash(); repository.RestoreTrash(trash.Id);
        Assert.Single(repository.Load()); Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static LifeRoutineDraft Draft() => new("2026-08-18", "Vehicle renewal", "Personal admin", LifeRoutineKind.PersonalAdmin, LifeRoutinePressure.High, "Review the renewal date.", "18:00", "Local only.", true);
    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lifeos-life-routine-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); }
        public string Path { get; }
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true); }
    }
}
