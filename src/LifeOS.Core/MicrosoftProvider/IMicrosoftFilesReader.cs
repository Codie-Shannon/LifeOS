namespace LifeOS.Core.MicrosoftProvider;

public interface IMicrosoftFilesReader
{
    Task<IReadOnlyList<MicrosoftDriveDescriptor>> GetDrivesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MicrosoftSiteDescriptor>> GetSitesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MicrosoftLibraryDescriptor>> GetLibrariesAsync(string siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MicrosoftFileDescriptor>> GetDriveItemsAsync(string driveId, string? folderId, DateTimeOffset modifiedSinceUtc, int maximumItems, CancellationToken cancellationToken = default);
}
