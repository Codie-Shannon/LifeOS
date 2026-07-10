using LifeOS.Core.IntegrationConnectors;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class IntegrationImportAuditTests
{
    [Fact]
    public void ManualImportAuditCapturesCountsAndFileIdentity()
    {
        const string content =
            "externalReference,title,summary,amount,currency,date,suggestedTarget,suggestedAction\r\n" +
            "good-row,Valid imported preview,This row imports,42.00,NZD,2026-07-10,EvidenceVault,Review imported evidence\r\n" +
            "bad-row,,,12.50,NZD,2026-07-10,BillsPayments,Review skipped row warning";

        var result = ManualIntegrationImportConnector.ImportCsv(content, @"C:\Projects\LifeOS\docs\integrations\audit.csv");
        var audit = IntegrationImportAudit.CreateManualImportEntry(result, @"C:\Projects\LifeOS\docs\integrations\audit.csv", ".csv", content);

        Assert.Equal("manual-csv", audit.ConnectorKey);
        Assert.Equal("CSV", audit.FileKind);
        Assert.Equal(@"C:\Projects\LifeOS\docs\integrations\audit.csv", audit.SourceFilePath);
        Assert.Equal("audit.csv", audit.SourceFileName);
        Assert.Equal(1, audit.ImportedCount);
        Assert.Equal(1, audit.SkippedRowCount);
        Assert.Equal(2, audit.TotalRowsSeen);
        Assert.Single(audit.PreviewIds);
        Assert.Single(audit.Errors);
        Assert.Equal(64, audit.FileSha256.Length);
    }

    [Fact]
    public void ManualImportAuditRequiresSourceFilePath()
    {
        var result = ManualIntegrationImportConnector.ImportCsv("title\r\nOne row", "one.csv");

        Assert.Throws<ArgumentException>(() =>
            IntegrationImportAudit.CreateManualImportEntry(result, "", ".csv", "title\r\nOne row"));
    }
}
