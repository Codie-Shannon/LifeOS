using System.Text.Json.Serialization;

namespace LifeOS.Core.MicrosoftProvider;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MicrosoftFileSourceKind { OneDrive, SharePoint }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MicrosoftFileSourceState { Current, Moved, Renamed, SourceRemoved, PermissionLost }

public sealed record MicrosoftDriveDescriptor(string Id, string Name, string DriveType, string WebUrl);
public sealed record MicrosoftSiteDescriptor(string Id, string Name, string WebUrl);
public sealed record MicrosoftLibraryDescriptor(string Id, string SiteId, string Name, string WebUrl);

public sealed record MicrosoftFileDescriptor(
    string Id,
    string Name,
    bool IsFolder,
    long SizeBytes,
    DateTimeOffset CreatedUtc,
    DateTimeOffset ModifiedUtc,
    string WebUrl,
    string ParentReference,
    string OwnerDisplayName,
    string ETag,
    string DriveId,
    string SiteId,
    string LibraryId,
    MicrosoftFileSourceKind SourceKind,
    MicrosoftFileSourceState SourceState);

public sealed class MicrosoftFilesSelection
{
    public List<string> SelectedDriveIds { get; set; } = [];
    public List<string> SelectedFolderIds { get; set; } = [];
    public List<string> SelectedSiteIds { get; set; } = [];
    public List<string> SelectedLibraryIds { get; set; } = [];
    public int LookbackDays { get; set; } = 90;
    public int MaximumItemsPerSource { get; set; } = 200;

    public MicrosoftFilesSelection Normalize()
    {
        SelectedDriveIds = NormalizeIds(SelectedDriveIds);
        SelectedFolderIds = NormalizeIds(SelectedFolderIds);
        SelectedSiteIds = NormalizeIds(SelectedSiteIds);
        SelectedLibraryIds = NormalizeIds(SelectedLibraryIds);
        LookbackDays = Math.Clamp(LookbackDays, 1, 730);
        MaximumItemsPerSource = Math.Clamp(MaximumItemsPerSource, 1, 1000);
        return this;
    }

    private static List<string> NormalizeIds(IEnumerable<string>? values) =>
        (values ?? []).Select(x => x.Trim()).Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
}

public sealed record MicrosoftFilesSyncResult(
    int Imported,
    int Reimported,
    int SourceRemoved,
    int PermissionLost,
    IReadOnlyList<string> Errors,
    DateTimeOffset CompletedUtc)
{
    public bool Succeeded => Errors.Count == 0;
    public bool PartiallySucceeded => Imported + Reimported > 0 && Errors.Count > 0;
}
