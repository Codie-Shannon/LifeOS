using System.Text.Json;
using LifeOS.Core.RelationshipRadar;
using LifeOS.Shared.RelationshipRadar;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups181To184RelationshipContextTests
{
    [Fact] public void Capture_requires_name_context_and_next_action()
    {
        var result = RelationshipRadarService.Validate(Draft() with { Name = null, RoleOrContext = null, NextAction = null });
        Assert.False(result.IsValid);
        Assert.Contains(result.ForField("relationship-name"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("relationship-context"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("relationship-next-action"), issue => issue.Code == "required");
    }

    [Fact] public void Capture_requires_iso_dates_and_bounded_single_line_text()
    {
        var result = RelationshipRadarService.Validate(Draft() with { LastContactDate = "18/08/2026", Name = "bad\nline", NextAction = new string('x', 201) });
        Assert.Equal("date-format", Assert.Single(result.ForField("relationship-last-contact")).Code);
        Assert.Equal("single-line", Assert.Single(result.ForField("relationship-name")).Code);
        Assert.Equal("maximum-length", Assert.Single(result.ForField("relationship-next-action")).Code);
    }

    [Fact] public void Create_respects_waiting_and_do_not_chase_state()
    {
        DateTimeOffset now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);
        RelationshipRadarProfile waiting = RelationshipRadarService.Create(Draft() with { WaitingOn = RelationshipWaitingOn.Them }, now);
        RelationshipRadarProfile protectedProfile = RelationshipRadarService.Create(Draft() with { DoNotChase = true }, now);
        Assert.Equal(RelationshipRadarStatus.WaitingOnThem, waiting.Status);
        Assert.Equal(RelationshipRadarStatus.DoNotChaseYet, protectedProfile.Status);
        Assert.True(protectedProfile.DoNotChase);
    }

    [Fact] public void Transitions_are_explicit_and_invalid_same_state_fails_closed()
    {
        DateTimeOffset now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);
        RelationshipRadarProfile active = RelationshipRadarService.Create(Draft(), now);
        RelationshipRadarProfile parked = RelationshipRadarService.Transition(active, RelationshipRadarStatus.Parked, now.AddMinutes(1));
        Assert.Equal(RelationshipRadarStatus.Parked, parked.Status);
        Assert.Throws<InvalidOperationException>(() => RelationshipRadarService.Transition(parked, RelationshipRadarStatus.Parked, now.AddMinutes(2)));
    }

    [Fact] public void Missing_repository_is_empty_without_creating_a_file()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "relationship-radar.json"); RelationshipRadarRepository repository = new(path);
        LocalStoreLoadResult<List<RelationshipRadarProfile>> result = repository.LoadResult();
        Assert.Equal(LocalStoreLoadState.Empty, result.State); Assert.Empty(result.Value); Assert.False(File.Exists(path));
    }

    [Fact] public void Repository_round_trips_versioned_normalized_profiles()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "relationship-radar.json"); RelationshipRadarRepository repository = new(path);
        repository.Save([RelationshipRadarService.Create(Draft() with { Name = "  Project Reviewer  " }, DateTimeOffset.UtcNow)]);
        RelationshipRadarProfile loaded = Assert.Single(repository.Load()); using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal("Project Reviewer", loaded.Name); Assert.Equal("relationship-radar", json.RootElement.GetProperty("storeId").GetString()); Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
    }

    [Fact] public void Repository_rejects_invalid_profile_without_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "relationship-radar.json"); RelationshipRadarRepository repository = new(path);
        RelationshipRadarProfile valid = RelationshipRadarService.Create(Draft(), DateTimeOffset.UtcNow); repository.Save([valid]);
        valid.Name = string.Empty; Assert.Throws<InvalidDataException>(() => repository.Save([valid])); Assert.Equal("Project Reviewer", Assert.Single(repository.Load()).Name);
    }

    [Fact] public void Trash_restore_is_recoverable_and_refuses_overwrite()
    {
        using TemporaryDirectory temporary = new(); string path = Path.Combine(temporary.Path, "relationship-radar.json"); RelationshipRadarRepository repository = new(path);
        repository.Save([RelationshipRadarService.Create(Draft(), DateTimeOffset.UtcNow)]); LocalStoreTrashEntry trash = repository.MoveToTrash();
        repository.RestoreTrash(trash.Id); Assert.Single(repository.Load()); Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static RelationshipRadarDraft Draft() => new("Project Reviewer", "Workshop proof review", RelationshipWaitingOn.Unknown, "2026-08-17", "2026-08-21", "Workshop Proof Project", "Send a concise update with the proof link.", "Local-only context.", false);
    private sealed class TemporaryDirectory : IDisposable { public TemporaryDirectory() { Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "lifeos-relationship-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path); } public string Path { get; } public void Dispose() { if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true); } }
}
