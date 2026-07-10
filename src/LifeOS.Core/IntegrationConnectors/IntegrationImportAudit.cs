using System.Security.Cryptography;
using System.Text;
using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors;

public static class IntegrationImportAudit
{
    public static IntegrationImportAuditEntry CreateManualImportEntry(
        ManualIntegrationImportResult result,
        string sourceFilePath,
        string fileKind,
        string fileContent)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            throw new ArgumentException("Source file path is required for import audit.", nameof(sourceFilePath));
        }

        if (string.IsNullOrWhiteSpace(fileKind))
        {
            throw new ArgumentException("File kind is required for import audit.", nameof(fileKind));
        }

        return new IntegrationImportAuditEntry
        {
            ConnectorKey = result.ConnectorKey,
            FileKind = fileKind.Trim().TrimStart('.').ToUpperInvariant(),
            SourceFilePath = sourceFilePath.Trim(),
            SourceFileName = Path.GetFileName(sourceFilePath.Trim()),
            FileSha256 = HashContent(fileContent),
            ImportedCount = result.Previews.Count,
            SkippedRowCount = result.Errors.Count,
            DuplicateSuspectedCount = result.Previews.Count(preview => preview.Status == IntegrationPreviewStatus.DuplicateSuspected),
            TotalRowsSeen = result.Previews.Count + result.Errors.Count,
            PreviewIds = result.Previews.Select(preview => preview.Id).ToArray(),
            Errors = result.Errors.ToArray(),
            ImportedAt = DateTime.Now
        };
    }

    private static string HashContent(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
