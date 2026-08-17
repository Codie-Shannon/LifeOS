using System.Text.Json;
using LifeOS.Core.Projects;
using LifeOS.Shared.Projects;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups145To148ProjectsTests
{
    [Fact]
    public void Project_requires_name_and_next_action()
    {
        var result = ProjectService.Validate(new ProjectDraft(
            " ", null, ProjectStatus.Active, null, null, null, null));

        Assert.False(result.IsValid);
        Assert.Equal("required", Assert.Single(result.ForField("project-name")).Code);
        Assert.Equal("required", Assert.Single(result.ForField("project-next-action")).Code);
    }

    [Fact]
    public void Project_rejects_overlong_or_multiline_single_line_fields()
    {
        var result = ProjectService.Validate(new ProjectDraft(
            new string('p', 121),
            null,
            ProjectStatus.Active,
            "First\nSecond",
            null,
            "proof\nprivate",
            null));

        Assert.Contains(result.ForField("project-name"), issue => issue.Code == "maximum-length");
        Assert.Contains(result.ForField("project-next-action"), issue => issue.Code == "single-line");
        Assert.Contains(result.ForField("project-evidence"), issue => issue.Code == "single-line");
    }

    [Fact]
    public void Create_trims_values_and_preserves_supplied_clock()
    {
        DateTimeOffset now = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);

        ProjectRecord project = ProjectService.Create(new ProjectDraft(
            " Project Alpha ",
            " Summary ",
            ProjectStatus.Active,
            " Ship local slice ",
            new DateOnly(2026, 8, 25),
            " proof/alpha ",
            " Notes "), now);

        Assert.Equal("Project Alpha", project.Name);
        Assert.Equal("Ship local slice", project.NextAction);
        Assert.Equal("proof/alpha", project.EvidenceReference);
        Assert.Equal(now, project.CreatedUtc);
        Assert.Equal(now, project.UpdatedUtc);
    }

    [Fact]
    public void Overview_excludes_archived_and_counts_open_due_projects()
    {
        DateOnly today = new(2026, 8, 18);
        ProjectRecord[] projects =
        [
            Project("Active", ProjectStatus.Active, today.AddDays(2)),
            Project("Waiting", ProjectStatus.Waiting, today.AddDays(8)),
            Project("Blocked", ProjectStatus.Blocked, today.AddDays(-1)),
            Project("Done", ProjectStatus.Completed, today),
            Project("Archived", ProjectStatus.Archived, today)
        ];

        ProjectOverview overview = ProjectService.Calculate(projects, today);

        Assert.Equal(4, overview.Visible);
        Assert.Equal(1, overview.Active);
        Assert.Equal(1, overview.Waiting);
        Assert.Equal(1, overview.Blocked);
        Assert.Equal(2, overview.DueNextSevenDays);
        Assert.Equal(1, overview.Completed);
        Assert.Equal(1, overview.Archived);
    }

    [Fact]
    public void Archive_and_restore_are_explicit_status_transitions()
    {
        ProjectRecord original = Project("Alpha", ProjectStatus.Active, null);
        DateTimeOffset archivedAt = DateTimeOffset.UtcNow;

        ProjectRecord archived = ProjectService.ChangeStatus(
            original,
            ProjectStatus.Archived,
            archivedAt);
        ProjectRecord restored = ProjectService.ChangeStatus(
            archived,
            ProjectStatus.Backlog,
            archivedAt.AddMinutes(1));

        Assert.Equal(original.Id, archived.Id);
        Assert.Equal(ProjectStatus.Archived, archived.Status);
        Assert.Equal(ProjectStatus.Backlog, restored.Status);
        Assert.True(restored.UpdatedUtc > archived.UpdatedUtc);
    }

    [Fact]
    public void Missing_repository_returns_honest_empty_state_without_writing()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "projects.json");
        ProjectRepository repository = new(path);

        LocalStoreLoadResult<List<ProjectRecord>> result = repository.LoadResult();

        Assert.Equal(LocalStoreLoadState.Empty, result.State);
        Assert.Empty(result.Value);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Repository_round_trips_versioned_project_records()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "projects.json");
        ProjectRepository repository = new(path);
        repository.Save([Project("Alpha", ProjectStatus.Active, new DateOnly(2026, 8, 25))]);

        ProjectRecord loaded = Assert.Single(repository.Load());
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));

        Assert.Equal("Alpha", loaded.Name);
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("projects", document.RootElement.GetProperty("storeId").GetString());
        Assert.Equal(LocalStoreHealthState.Healthy, repository.Inspect().State);
    }

    [Fact]
    public void Repository_trash_restore_refuses_overwrite_and_recovers_record()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "projects.json");
        ProjectRepository repository = new(path);
        repository.Save([Project("Alpha", ProjectStatus.Active, null)]);
        LocalStoreTrashEntry trash = repository.MoveToTrash();

        repository.RestoreTrash(trash.Id);

        Assert.Equal("Alpha", Assert.Single(repository.Load()).Name);
        Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static ProjectRecord Project(
        string name,
        ProjectStatus status,
        DateOnly? dueDate) => new()
    {
        Name = name,
        Summary = "Summary",
        Status = status,
        NextAction = "Next action",
        DueDate = dueDate,
        CreatedUtc = DateTimeOffset.UtcNow,
        UpdatedUtc = DateTimeOffset.UtcNow
    };

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "lifeos-project-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }
}
