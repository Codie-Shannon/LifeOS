using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.MicrosoftProvider;

public sealed class MicrosoftFilesSyncService
{
    private readonly IMicrosoftFilesReader _reader;
    public MicrosoftFilesSyncService(IMicrosoftFilesReader reader) => _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    public async Task<MicrosoftFilesSyncResult> SyncAsync(
        MicrosoftProviderAccount account,
        MicrosoftFilesSelection selection,
        IntegrationInboxV9Service inbox,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(inbox);
        selection.Normalize();

        int imported = 0, reimported = 0, removed = 0, permissionLost = 0;
        List<string> errors = [];
        DateTimeOffset since = nowUtc.AddDays(-selection.LookbackDays);

        foreach (string driveId in selection.SelectedDriveIds)
        {
            try
            {
                IReadOnlyList<string?> folders = selection.SelectedFolderIds.Count == 0 ? [null] : selection.SelectedFolderIds.Cast<string?>().ToList();
                foreach (string? folderId in folders)
                {
                    IReadOnlyList<MicrosoftFileDescriptor> items = await _reader.GetDriveItemsAsync(driveId, folderId, since, selection.MaximumItemsPerSource, cancellationToken);
                    foreach (MicrosoftFileDescriptor item in items)
                    {
                        IntegrationCandidate candidate = inbox.Ingest(MicrosoftFilesCandidateFactory.CreateDraft(account, item), nowUtc);
                        if (item.SourceState == MicrosoftFileSourceState.SourceRemoved) { inbox.MarkSourceRemoved(candidate.Id, nowUtc); removed++; }
                        else if (item.SourceState == MicrosoftFileSourceState.PermissionLost) { permissionLost++; }
                        else if (candidate.CreatedUtc == nowUtc) imported++; else reimported++;
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add($"Drive {Redact(driveId)} failed: {ex.GetType().Name}");
            }
        }

        account.LastErrorUtc = errors.Count == 0 ? null : nowUtc;
        account.LastErrorCode = errors.Count == 0 ? string.Empty : "microsoft-files-partial-failure";
        account.LastErrorMessage = errors.Count == 0 ? string.Empty : string.Join("; ", errors);
        return new(imported, reimported, removed, permissionLost, errors, nowUtc);
    }

    private static string Redact(string value) => value.Length <= 6 ? "selected-source" : $"{value[..3]}...{value[^3..]}";
}
