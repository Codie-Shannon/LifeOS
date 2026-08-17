using System.Text.Json;
using LifeOS.Core.Forms;
using LifeOS.Core.WorkSessions;
using LifeOS.Shared.Storage;
using LifeOS.Shared.WorkSessions;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups149To152WorkTimeTests
{
    [Fact]
    public void Session_requires_client_project_and_description()
    {
        FormValidationResult result = WorkSessionService.Validate(Draft(
            clientProject: " ",
            description: null));

        Assert.False(result.IsValid);
        Assert.Equal("required", Assert.Single(result.ForField("work-client-project")).Code);
        Assert.Equal("required", Assert.Single(result.ForField("work-description")).Code);
    }

    [Fact]
    public void Session_rejects_impossible_hours_rate_and_multiline_identity()
    {
        FormValidationResult result = WorkSessionService.Validate(Draft(
            clientProject: "Client\nPrivate",
            hours: 25m,
            hourlyRate: -1m));

        Assert.Contains(result.ForField("work-client-project"), issue => issue.Code == "single-line");
        Assert.Equal("hours-range", Assert.Single(result.ForField("work-hours")).Code);
        Assert.Equal("rate-range", Assert.Single(result.ForField("work-hourly-rate")).Code);
    }

    [Fact]
    public void Create_trims_values_and_normalizes_non_billable_session()
    {
        DateTime recordedAt = new(2026, 8, 18, 9, 30, 0, DateTimeKind.Local);

        WorkSession session = WorkSessionService.Create(Draft(
            clientProject: " Internal systems ",
            hourlyRate: 500m,
            billable: false,
            status: WorkSessionStatus.Completed,
            description: " Maintenance ",
            notes: " Local only "), recordedAt);

        Assert.Equal("Internal systems", session.ClientOrProject);
        Assert.Equal("Maintenance", session.Description);
        Assert.False(session.IsBillable);
        Assert.Equal(0m, session.HourlyRate);
        Assert.Equal(WorkSessionStatus.NonBillable, session.Status);
        Assert.Equal(recordedAt, session.CreatedAt);
    }

    [Fact]
    public void Billable_status_changes_are_explicit_and_do_not_mutate_source()
    {
        WorkSession original = WorkSessionService.Create(Draft(), DateTime.Now);
        DateTime changedAt = original.UpdatedAt.AddMinutes(1);

        WorkSession changed = WorkSessionService.ChangeStatus(
            original,
            WorkSessionStatus.Paid,
            changedAt);

        Assert.Equal(WorkSessionStatus.Completed, original.Status);
        Assert.Equal(WorkSessionStatus.Paid, changed.Status);
        Assert.Equal(changedAt, changed.UpdatedAt);
        Assert.Equal(original.Id, changed.Id);
    }

    [Fact]
    public void Summary_keeps_paid_and_unpaid_values_distinct()
    {
        WorkSession unpaid = WorkSessionService.Create(Draft(hours: 2m, hourlyRate: 50m), DateTime.Now);
        WorkSession paid = WorkSessionService.ChangeStatus(
            WorkSessionService.Create(Draft(hours: 1m, hourlyRate: 80m), DateTime.Now),
            WorkSessionStatus.Paid,
            DateTime.Now);
        WorkSession nonBillable = WorkSessionService.Create(Draft(hours: 3m, billable: false), DateTime.Now);

        WorkSessionSummary summary = WorkSessionCalculator.Calculate([unpaid, paid, nonBillable]);

        Assert.Equal(6m, summary.TotalHours);
        Assert.Equal(3m, summary.BillableHours);
        Assert.Equal(180m, summary.BillableValue);
        Assert.Equal(80m, summary.PaidValue);
        Assert.Equal(100m, summary.UnpaidBillableValue);
    }

    [Fact]
    public void Missing_repository_returns_honest_empty_state_without_writing()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "work-sessions.json");
        WorkSessionRepository repository = new(path);

        LocalStoreLoadResult<List<WorkSession>> result = repository.LoadResult();

        Assert.Equal(LocalStoreLoadState.Empty, result.State);
        Assert.Empty(result.Value);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Repository_round_trips_versioned_normalized_sessions()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "work-sessions.json");
        WorkSessionRepository repository = new(path);
        WorkSession session = WorkSessionService.Create(Draft(clientProject: " Client "), DateTime.Now);

        repository.Save([session]);
        WorkSession loaded = Assert.Single(repository.Load());
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        Assert.Equal("Client", loaded.ClientOrProject);
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("work-sessions", document.RootElement.GetProperty("storeId").GetString());
        Assert.Equal(LocalStoreHealthState.Healthy, repository.Inspect().State);
    }

    [Fact]
    public void Repository_trash_restore_refuses_overwrite_and_recovers_session()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "work-sessions.json");
        WorkSessionRepository repository = new(path);
        repository.Save([WorkSessionService.Create(Draft(), DateTime.Now)]);
        LocalStoreTrashEntry trash = repository.MoveToTrash();

        repository.RestoreTrash(trash.Id);

        Assert.Single(repository.Load());
        Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static WorkSessionDraft Draft(
        string? clientProject = "Client",
        decimal hours = 1m,
        decimal hourlyRate = 100m,
        bool billable = true,
        WorkSessionStatus status = WorkSessionStatus.Completed,
        string? description = "Delivered a local work slice",
        string? notes = "Notes") => new(
            clientProject,
            new DateOnly(2026, 8, 18),
            hours,
            hourlyRate,
            billable,
            status,
            description,
            notes);

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "lifeos-work-time-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }
}
