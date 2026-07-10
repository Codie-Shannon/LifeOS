using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors;

public sealed record ManualIntegrationImportPreviewSummary(
    string ConnectorKey,
    string SourceFileName,
    string FileKind,
    int PreviewCount,
    int DuplicateSuspectedCount,
    int SkippedRowCount,
    decimal PreviewMoney,
    IReadOnlyList<ManualIntegrationImportError> Errors)
{
    public static ManualIntegrationImportPreviewSummary Create(
        ManualIntegrationImportResult result,
        string sourceFilePath,
        string fileKind)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ManualIntegrationImportPreviewSummary(
            result.ConnectorKey,
            Path.GetFileName(sourceFilePath),
            fileKind.Trim().TrimStart('.').ToUpperInvariant(),
            result.Previews.Count,
            result.Previews.Count(preview => preview.Status == IntegrationPreviewStatus.DuplicateSuspected),
            result.Errors.Count,
            result.Previews.Sum(preview => preview.Amount ?? 0m),
            result.Errors.ToArray());
    }
}
