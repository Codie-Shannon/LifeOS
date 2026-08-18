using System.Text.Json;
using LifeOS.Core.Agenda;
using LifeOS.Shared.Agenda;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups173To176AgendaPlanningTests
{
    [Fact]
    public void Capture_requires_title_and_next_action()
    {
        var result = AgendaService.Validate(Draft() with { Title = null, NextAction = null });
        Assert.False(result.IsValid);
        Assert.Contains(result.ForField("agenda-title"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("agenda-next-action"), issue => issue.Code == "required");
    }

    [Fact]
    public void Capture_requires_iso_date_and_bounded_single_line_text()
    {
        var result = AgendaService.Validate(Draft() with { DueDate = "18/08/2026", Title = "bad\nline" });
        Assert.Equal("date-format", Assert.Single(result.ForField("agenda-due-date")).Code);
        Assert.Equal("single-line", Assert.Single(result.ForField("agenda-title")).Code);
    }

    [Fact]
    public void Create_preserves_explicit_context_and_starts_planned()
    {
        DateTimeOffset now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);
        AgendaItem item = AgendaService.Create(Draft(), now);
        Assert.Equal(new DateOnly(2026, 8, 18), item.DueDate);
        Assert.Equal(AgendaItemStatus.Planned, item.Status);
        Assert.Equal(AgendaPressureLevel.High, item.PressureLevel);
        Assert.True(item.IsFixedCommitment);
    }

    [Fact]
    public void State_transitions_are_explicit_and_invalid_jump_is_rejected()
    {
        AgendaItem planned = AgendaService.Create(Draft(), DateTimeOffset.Now);
        AgendaItem active = AgendaService.Transition(planned, AgendaItemStatus.InProgress, DateTimeOffset.Now);
        AgendaItem complete = AgendaService.Transition(active, AgendaItemStatus.Completed, DateTimeOffset.Now);
        Assert.Equal(AgendaItemStatus.Completed, complete.Status);
        Assert.Throws<InvalidOperationException>(() => AgendaService.Transition(planned, AgendaItemStatus.Planned, DateTimeOffset.Now));
    }

    [Fact]
    public void Missing_repository_is_empty_without_creating_a_file()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "agenda-items.json");
        AgendaRepository repository = new(path); LocalStoreLoadResult<List<AgendaItem>> result = repository.LoadResult();
        Assert.Equal(LocalStoreLoadState.Empty, result.State); Assert.Empty(result.Value); Assert.False(File.Exists(path));
    }

    [Fact]
    public void Repository_round_trips_versioned_normalized_items()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "agenda-items.json"); AgendaRepository repository = new(path);
        repository.Save([AgendaService.Create(Draft() with { Title = "  Fixed review  " }, DateTimeOffset.Now)]);
        AgendaItem loaded = Assert.Single(repository.Load()); using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal("Fixed review", loaded.Title); Assert.Equal("agenda", json.RootElement.GetProperty("storeId").GetString()); Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
    }

    [Fact]
    public void Repository_rejects_invalid_item_without_overwriting_valid_state()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "agenda-items.json"); AgendaRepository repository = new(path);
        AgendaItem valid = AgendaService.Create(Draft(), DateTimeOffset.Now); repository.Save([valid]);
        Assert.Throws<InvalidDataException>(() => repository.Save([Clone(valid, title: "")])); Assert.Equal(valid.Id, Assert.Single(repository.Load()).Id);
    }

    [Fact]
    public void Trash_restore_is_recoverable_and_refuses_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "agenda-items.json"); AgendaRepository repository = new(path);
        repository.Save([AgendaService.Create(Draft(), DateTimeOffset.Now)]); LocalStoreTrashEntry trash = repository.MoveToTrash(); repository.RestoreTrash(trash.Id);
        Assert.Single(repository.Load()); Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static AgendaDraft Draft() => new("Dentist appointment", "2026-08-18", "14:30", AgendaItemType.Appointment, AgendaPressureLevel.High, "Confirm the local appointment details.", "Local only.", true);
    private static AgendaItem Clone(AgendaItem item, string title) => new() { Id = item.Id, Title = title, Type = item.Type, Status = item.Status, PressureLevel = item.PressureLevel, DueDate = item.DueDate, TimeText = item.TimeText, NextAction = item.NextAction, IsFixedCommitment = item.IsFixedCommitment, Notes = item.Notes, CreatedAt = item.CreatedAt, UpdatedAt = item.UpdatedAt };
    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lifeos-agenda-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); }
        public string Path { get; }
        public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true); }
    }
}
