namespace LifeOS.Core.IntegrationConnectors.GoogleWorkspace;

public interface IGoogleWorkspaceReader
{
    Task<GoogleWorkspaceIdentity> GetIdentityAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GoogleGmailRecord>> GetGmailAsync(
        DateTimeOffset sinceUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GoogleCalendarRecord>> GetCalendarAsync(
        DateTimeOffset sinceUtc,
        DateTimeOffset untilUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GoogleDriveRecord>> GetDriveAsync(
        IReadOnlyList<string> selectedFolderIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GoogleContactRecord>> GetContactsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GoogleTaskRecord>> GetTasksAsync(
        IReadOnlyList<string> selectedListIds,
        CancellationToken cancellationToken = default);
}
