using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.MicrosoftProvider;

public static class MicrosoftFilesCandidateFactory
{
    public static FileDocumentCandidateDraft CreateDraft(MicrosoftProviderAccount account, MicrosoftFileDescriptor item)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(item);

        string extension = item.IsFolder ? "folder" : Path.GetExtension(item.Name).TrimStart('.');
        return new FileDocumentCandidateDraft
        {
            ProviderId = "microsoft",
            ProviderDisplayName = "Microsoft 365",
            AccountId = account.Id,
            AccountDisplayName = account.DisplayName,
            ExternalId = item.Id,
            CapabilityId = item.SourceKind == MicrosoftFileSourceKind.OneDrive ? "onedrive-files" : "sharepoint-files",
            RawReference = $"microsoft-graph://drives/{item.DriveId}/items/{item.Id}",
            SourceTimestampUtc = item.ModifiedUtc,
            Summary = $"Source-backed {item.SourceKind} {(item.IsFolder ? "folder" : "file")} metadata imported read-only. No file body was downloaded.",
            Name = item.Name,
            Extension = extension,
            SizeBytes = item.SizeBytes,
            ModifiedUtc = item.ModifiedUtc,
            WebReference = item.WebUrl,
            AdditionalFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["source-kind"] = item.SourceKind.ToString(),
                ["source-state"] = item.SourceState.ToString(),
                ["created"] = item.CreatedUtc.ToString("O"),
                ["parent"] = item.ParentReference,
                ["owner"] = item.OwnerDisplayName,
                ["etag"] = item.ETag,
                ["drive"] = item.DriveId,
                ["site"] = item.SiteId,
                ["library"] = item.LibraryId,
                ["body-downloaded"] = "false"
            }
        };
    }
}
