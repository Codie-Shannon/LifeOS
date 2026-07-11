namespace LifeOS.Core.IntegrationConnectors;

public sealed class IntegrationImportAuditEntry
{
    public string Action { get; init; } = "manual-import";
    public string Summary { get; init; } = string.Empty;
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ConnectorKey { get; init; } = string.Empty;
    public string FileKind { get; init; } = string.Empty;
    public string SourceFilePath { get; init; } = string.Empty;
    public string SourceFileName { get; init; } = string.Empty;
    public string FileSha256 { get; init; } = string.Empty;
    public int ImportedCount { get; init; }
    public int SkippedRowCount { get; init; }
    public int DuplicateSuspectedCount { get; init; }
    public int TotalRowsSeen { get; init; }
    public IReadOnlyList<Guid> PreviewIds { get; init; } = [];
    public IReadOnlyList<ManualIntegrationImportError> Errors { get; init; } = [];
    public DateTime ImportedAt { get; init; } = DateTime.Now;
}
