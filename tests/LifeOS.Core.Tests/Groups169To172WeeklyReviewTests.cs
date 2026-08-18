using System.Text.Json;
using LifeOS.Core.WeeklyReview;
using LifeOS.Shared.Storage;
using LifeOS.Shared.WeeklyReview;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups169To172WeeklyReviewTests
{
    [Fact]
    public void Capture_requires_week_done_and_next_week_focus()
    {
        var result = WeeklyReviewService.Validate(Draft() with { WeekStart = null, WhatGotDone = null, NextWeekFocus = null });
        Assert.False(result.IsValid);
        Assert.Contains(result.ForField("weekly-review-week"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("weekly-review-done"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("weekly-review-focus"), issue => issue.Code == "required");
    }

    [Fact]
    public void Capture_requires_iso_date_and_bounded_text()
    {
        var result = WeeklyReviewService.Validate(Draft() with { WeekStart = "17/08/2026", NextWeekFocus = new string('x', 501) });
        Assert.Equal("date-format", Assert.Single(result.ForField("weekly-review-week")).Code);
        Assert.Equal("maximum-length", Assert.Single(result.ForField("weekly-review-focus")).Code);
    }

    [Fact]
    public void Create_preserves_explicit_fields_and_starts_draft()
    {
        DateTimeOffset now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);
        WeeklyReviewRecord record = WeeklyReviewService.Create(Draft(), now);
        Assert.Equal(new DateOnly(2026, 8, 17), record.WeekStart);
        Assert.Equal(WeeklyReviewState.Draft, record.State);
        Assert.Equal(WeeklyReviewPressure.High, record.Pressure);
        Assert.Equal(now, record.CreatedUtc);
    }

    [Fact]
    public void State_transitions_are_explicit_and_invalid_jump_is_rejected()
    {
        WeeklyReviewRecord draft = WeeklyReviewService.Create(Draft(), DateTimeOffset.UtcNow);
        WeeklyReviewRecord ready = WeeklyReviewService.Transition(draft, WeeklyReviewState.Ready, DateTimeOffset.UtcNow);
        WeeklyReviewRecord closed = WeeklyReviewService.Transition(ready, WeeklyReviewState.Closed, DateTimeOffset.UtcNow);
        Assert.Equal(WeeklyReviewState.Closed, closed.State);
        Assert.Throws<InvalidOperationException>(() => WeeklyReviewService.Transition(draft, WeeklyReviewState.Closed, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Missing_repository_is_empty_without_creating_a_file()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "weekly-review.json");
        WeeklyReviewRepository repository = new(path); LocalStoreLoadResult<List<WeeklyReviewRecord>> result = repository.LoadResult();
        Assert.Equal(LocalStoreLoadState.Empty, result.State); Assert.Empty(result.Value); Assert.False(File.Exists(path));
    }

    [Fact]
    public void Repository_round_trips_versioned_normalized_records()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "weekly-review.json");
        WeeklyReviewRepository repository = new(path); repository.Save([WeeklyReviewService.Create(Draft() with { WhatGotDone = "  Delivered milestone  " }, DateTimeOffset.UtcNow)]);
        WeeklyReviewRecord loaded = Assert.Single(repository.Load()); using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal("Delivered milestone", loaded.WhatGotDone); Assert.Equal("weekly-review", json.RootElement.GetProperty("storeId").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32()); Assert.Equal(LocalStoreHealthState.Healthy, repository.Inspect().State);
    }

    [Fact]
    public void Repository_rejects_invalid_record_without_overwriting_valid_state()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "weekly-review.json");
        WeeklyReviewRepository repository = new(path); WeeklyReviewRecord valid = WeeklyReviewService.Create(Draft(), DateTimeOffset.UtcNow); repository.Save([valid]);
        Assert.Throws<InvalidDataException>(() => repository.Save([valid with { NextWeekFocus = "" }]));
        Assert.Equal(valid.Id, Assert.Single(repository.Load()).Id);
    }

    [Fact]
    public void Trash_restore_is_recoverable_and_refuses_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "weekly-review.json");
        WeeklyReviewRepository repository = new(path); repository.Save([WeeklyReviewService.Create(Draft(), DateTimeOffset.UtcNow)]);
        LocalStoreTrashEntry trash = repository.MoveToTrash(); repository.RestoreTrash(trash.Id);
        Assert.Single(repository.Load()); Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static WeeklyReviewDraft Draft() => new("2026-08-17", "Delivered the milestone.", "Moved one review.", "Waiting on a sample.", WeeklyReviewPressure.High, "Protect focused delivery time.", "Local notes only.");
    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lifeos-weekly-review-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); }
        public string Path { get; }
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true); }
    }
}
