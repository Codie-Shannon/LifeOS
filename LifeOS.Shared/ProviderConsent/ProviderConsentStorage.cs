using LifeOS.Core.ConfigurationReadiness;
using LifeOS.Core.ProviderAdapters;
using LifeOS.Core.ProviderConsent;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.ProviderConsent;

public sealed class ProviderConsentRepository
{
    private readonly VersionedJsonLocalStore<List<ProviderConsentRecord>> _store;
    private readonly Func<DateOnly> _today;
    public ProviderConsentRepository(string path, Func<List<ProviderConsentRecord>>? empty = null, Func<DateOnly>? today = null, Func<DateTimeOffset>? utcNow = null) { _today = today ?? (() => DateOnly.FromDateTime(DateTime.Today)); _store = new(path, "provider-consent-profiles", 1, empty ?? (() => []), Normalize, utcNow: utcNow); }
    public LocalStoreLoadResult<List<ProviderConsentRecord>> LoadResult() => _store.Load(); public List<ProviderConsentRecord> Load() => LoadResult().Value; public void Save(IEnumerable<ProviderConsentRecord> values) => _store.Save(values.ToList()); public LocalStoreHealth Inspect() => _store.Inspect(); public IReadOnlyList<LocalStoreTrashEntry> ListTrash() => _store.ListTrash(); public LocalStoreTrashEntry MoveToTrash() => _store.MoveToTrash(); public void RestoreTrash(string id) => _store.RestoreTrash(id);
    private List<ProviderConsentRecord> Normalize(List<ProviderConsentRecord> values) => values.Select(value => ProviderConsentService.Normalize(value, _today())).OrderBy(value => value.State).ThenBy(value => value.ProfileName, StringComparer.OrdinalIgnoreCase).ToList();
}

public static class ProviderConsentStorage
{
    public static string FilePath => LocalAppDataPath.GetFilePath("provider-consent-profiles.json"); private static ProviderConsentRepository Repo() => new(FilePath, LocalAppDataPath.IsPortfolioDemoMode ? Demo : () => []); public static List<ProviderConsentRecord> Load() => Repo().Load(); public static void Save(IEnumerable<ProviderConsentRecord> values) => Repo().Save(values); public static LocalStoreHealth Inspect() => Repo().Inspect(); public static IReadOnlyList<LocalStoreTrashEntry> ListTrash() => Repo().ListTrash(); public static void RestoreTrash(string id) => Repo().RestoreTrash(id); public static void Reset() { if (File.Exists(FilePath)) Repo().MoveToTrash(); }
    private static List<ProviderConsentRecord> Demo() { DateOnly today = DateOnly.FromDateTime(DateTime.Today); DateTimeOffset now = DateTimeOffset.UtcNow; ProviderConsentRecord proposed = ProviderConsentService.Create(new("Fictional calendar review", ProviderFamily.Google, "Integration owner", ConfigurationEnvironment.Test, ProviderCapability.CalendarRead | ProviderCapability.DraftProposal, "Review fictional calendar candidates for planning.", today.AddDays(30), "No provider access is active."), today, now.AddHours(-2)); ProviderConsentRecord approved = ProviderConsentService.SetState(proposed with { Id = Guid.NewGuid(), ProfileName = "Fictional file review", Family = ProviderFamily.Microsoft, Capabilities = ProviderCapability.FileRead, Purpose = "Review fictional file candidates." }, ProviderConsentState.Approved, today, now.AddHours(-1)); return [proposed, approved]; }
}
