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

    public static IntegrationImportAuditEntry CreateConnectorLifecycleEntry(
        string connectorKey,
        string action,
        string summary,
        int receivedCount = 0,
        int duplicateCount = 0,
        int skippedCount = 0)
    {
        if (string.IsNullOrWhiteSpace(connectorKey)) throw new ArgumentException("Connector key is required.", nameof(connectorKey));
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Lifecycle action is required.", nameof(action));

        var safeSummary = summary?.Trim() ?? string.Empty;
        if (safeSummary.Length > 320) safeSummary = safeSummary[..320] + "…";

        return new IntegrationImportAuditEntry
        {
            ConnectorKey = connectorKey.Trim(),
            Action = action.Trim(),
            Summary = safeSummary,
            FileKind = "CONNECTOR-LIFECYCLE",
            SourceFilePath = "local://connector-lifecycle",
            SourceFileName = action.Trim(),
            FileSha256 = HashContent($"{connectorKey}|{action}|{safeSummary}"),
            ImportedCount = receivedCount,
            DuplicateSuspectedCount = duplicateCount,
            SkippedRowCount = skippedCount,
            TotalRowsSeen = receivedCount + skippedCount,
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
