using LifeOS.Core.Projects;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Projects;

public sealed class ProjectRepository
{
    private readonly VersionedJsonLocalStore<List<ProjectRecord>> _store;

    public ProjectRepository(
        string filePath,
        Func<List<ProjectRecord>>? emptyFactory = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _store = new VersionedJsonLocalStore<List<ProjectRecord>>(
            filePath,
            "projects",
            1,
            emptyFactory ?? (() => []),
            Normalize,
            utcNow: utcNow);
    }

    public LocalStoreLoadResult<List<ProjectRecord>> LoadResult() => _store.Load();

    public List<ProjectRecord> Load() => LoadResult().Value;

    public void Save(IEnumerable<ProjectRecord> projects) =>
        _store.Save(projects.ToList());

    public LocalStoreHealth Inspect() => _store.Inspect();

    public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash();

    public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash();

    public void RestoreTrash(string entryId) => _store.RestoreTrash(entryId);

    private static List<ProjectRecord> Normalize(List<ProjectRecord> projects) => projects
        .Select(ProjectService.Normalize)
        .OrderBy(project => project.Status == ProjectStatus.Archived)
        .ThenBy(project => project.Status == ProjectStatus.Completed)
        .ThenBy(project => project.DueDate ?? DateOnly.MaxValue)
        .ThenBy(project => project.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();
}

public static class ProjectStorage
{
    private const string FileName = "projects.json";

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<ProjectRecord> Load() => Repository().Load();

    public static void Save(IEnumerable<ProjectRecord> projects) =>
        Repository().Save(projects);

    public static LocalStoreHealth Inspect() => Repository().Inspect();

    public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() =>
        Repository().ListTrash();

    public static void RestoreTrash(string entryId) =>
        Repository().RestoreTrash(entryId);

    public static void Reset()
    {
        if (File.Exists(FilePath)) Repository().MoveToTrash();
    }

    private static ProjectRepository Repository() => new(
        FilePath,
        LocalAppDataPath.IsPortfolioDemoMode ? CreateProofProjects : () => []);

    private static List<ProjectRecord> CreateProofProjects()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return
        [
            ProjectService.Create(new ProjectDraft(
                "Fictional delivery portal",
                "Portfolio-only example showing an active delivery project.",
                ProjectStatus.Active,
                "Review the fictional proof checklist.",
                DateOnly.FromDateTime(DateTime.Today).AddDays(5),
                "portfolio-proof/delivery-portal",
                "Fictional demo data."), now),
            ProjectService.Create(new ProjectDraft(
                "Fictional archive migration",
                "Portfolio-only example showing blocked work.",
                ProjectStatus.Blocked,
                "Wait for the fictional source archive.",
                null,
                string.Empty,
                "Fictional demo data."), now)
        ];
    }
}
