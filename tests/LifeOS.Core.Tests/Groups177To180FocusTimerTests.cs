using System.Text.Json;
using LifeOS.Core.FocusTimers;
using LifeOS.Shared.FocusTimers;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups177To180FocusTimerTests
{
    [Fact] public void Capture_requires_title_area_and_next_action()
    {
        var result = FocusTimerService.Validate(Draft() with { Title = null, Area = null, NextAction = null }); Assert.False(result.IsValid);
        Assert.Contains(result.ForField("focus-title"), issue => issue.Code == "required"); Assert.Contains(result.ForField("focus-area"), issue => issue.Code == "required"); Assert.Contains(result.ForField("focus-next-action"), issue => issue.Code == "required");
    }

    [Fact] public void Capture_requires_bounded_target_and_single_line_text()
    {
        var result = FocusTimerService.Validate(Draft() with { TargetMinutes = "721", Title = "bad\nline" });
        Assert.Equal("range", Assert.Single(result.ForField("focus-target")).Code); Assert.Equal("single-line", Assert.Single(result.ForField("focus-title")).Code);
    }

    [Fact] public void Create_preserves_context_and_never_auto_starts()
    {
        DateTimeOffset now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero); FocusTimerRecord record = FocusTimerService.Create(Draft(), now);
        Assert.Equal(FocusTimerState.Planned, record.State); Assert.Null(record.SegmentStartedUtc); Assert.Equal(25, record.TargetMinutes); Assert.Equal(now, record.CreatedUtc);
    }

    [Fact] public void Pause_excludes_paused_time_and_invalid_jump_fails_closed()
    {
        DateTimeOffset now = new(2026, 8, 18, 1, 0, 0, TimeSpan.Zero); FocusTimerRecord planned = FocusTimerService.Create(Draft(), now);
        FocusTimerRecord running = FocusTimerService.Transition(planned, FocusTimerState.Running, now.AddMinutes(1)); FocusTimerRecord paused = FocusTimerService.Transition(running, FocusTimerState.Paused, now.AddMinutes(11));
        Assert.Equal(TimeSpan.FromMinutes(10), FocusTimerService.Duration(paused, now.AddHours(1))); Assert.Throws<InvalidOperationException>(() => FocusTimerService.Transition(planned, FocusTimerState.Completed, now.AddMinutes(1)));
    }

    [Fact] public void Missing_repository_is_empty_without_creating_a_file()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "focus-timers.json"); FocusTimerRepository repository = new(path); LocalStoreLoadResult<List<FocusTimerRecord>> result = repository.LoadResult();
        Assert.Equal(LocalStoreLoadState.Empty, result.State); Assert.Empty(result.Value); Assert.False(File.Exists(path));
    }

    [Fact] public void Repository_round_trips_versioned_normalized_records()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "focus-timers.json"); FocusTimerRepository repository = new(path); repository.Save([FocusTimerService.Create(Draft() with { Title = "  Deep work  " }, DateTimeOffset.UtcNow)]);
        FocusTimerRecord loaded = Assert.Single(repository.Load()); using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path)); Assert.Equal("Deep work", loaded.Title); Assert.Equal("focus-timers", json.RootElement.GetProperty("storeId").GetString()); Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
    }

    [Fact] public void Repository_rejects_inconsistent_running_state_without_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "focus-timers.json"); FocusTimerRepository repository = new(path); FocusTimerRecord valid = FocusTimerService.Create(Draft(), DateTimeOffset.UtcNow); repository.Save([valid]);
        Assert.Throws<InvalidDataException>(() => repository.Save([valid with { State = FocusTimerState.Running }])); Assert.Equal(valid.Id, Assert.Single(repository.Load()).Id);
    }

    [Fact] public void Trash_restore_is_recoverable_and_refuses_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "focus-timers.json"); FocusTimerRepository repository = new(path); repository.Save([FocusTimerService.Create(Draft(), DateTimeOffset.UtcNow)]);
        LocalStoreTrashEntry trash = repository.MoveToTrash(); repository.RestoreTrash(trash.Id); Assert.Single(repository.Load()); Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static FocusTimerDraft Draft() => new("Deep work", "Product", FocusTimerKind.Work, "25", "Review the build checkpoint.", "Local only.");
    private sealed class TemporaryDirectory : IDisposable { public TemporaryDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lifeos-focus-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); } public string Path { get; } public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true); } }
}
