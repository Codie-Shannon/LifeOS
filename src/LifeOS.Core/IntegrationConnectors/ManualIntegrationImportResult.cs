using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors;

public sealed record ManualIntegrationImportResult(
    string ConnectorKey,
    IReadOnlyList<IntegrationPreviewItem> Previews,
    IReadOnlyList<ManualIntegrationImportError> Errors);
