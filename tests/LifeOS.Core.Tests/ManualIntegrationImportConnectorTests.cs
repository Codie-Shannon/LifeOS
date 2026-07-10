using LifeOS.Core.IntegrationConnectors;
using LifeOS.Core.IntegrationInbox;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class ManualIntegrationImportConnectorTests
{
    [Fact]
    public void CsvImportCreatesManualPreviewItems()
    {
        const string content =
            "externalReference,title,summary,amount,currency,date,suggestedTarget,suggestedAction\r\n" +
            "bill-1,Power bill,Imported from CSV,91.45,NZD,2026-07-10,BillsPayments,Review bill";

        var result = ManualIntegrationImportConnector.ImportCsv(content, "sample.csv");

        Assert.Empty(result.Errors);
        Assert.Single(result.Previews);

        var preview = result.Previews.Single();
        Assert.Equal("manual-csv", preview.SourceLabel);
        Assert.Equal(IntegrationSourceKind.ManualImport, preview.SourceKind);
        Assert.Equal("bill-1", preview.ExternalReference);
        Assert.Equal("Power bill", preview.Title);
        Assert.Equal(91.45m, preview.Amount);
        Assert.Equal("NZD", preview.Currency);
        Assert.Equal(IntegrationTargetKind.BillsPayments, preview.SuggestedTarget);
        Assert.Equal("sample.csv#1", preview.SourceEvidence);
        Assert.Equal("manual-csv:bill-1:20260710:91.45", preview.DuplicateKey);
        Assert.True(preview.IsReadOnlyPreview);
        Assert.True(preview.RequiresHumanReview);
    }

    [Fact]
    public void JsonImportCreatesManualPreviewItems()
    {
        const string content = """
        [
          {
            "id": "task-7",
            "title": "Follow up supplier",
            "notes": "Imported from JSON",
            "target": "FollowUps",
            "action": "Check whether quote arrived"
          }
        ]
        """;

        var result = ManualIntegrationImportConnector.ImportJson(content, "sample.json");

        Assert.Empty(result.Errors);
        Assert.Single(result.Previews);

        var preview = result.Previews.Single();
        Assert.Equal("manual-json", result.ConnectorKey);
        Assert.Equal("manual-json", preview.SourceLabel);
        Assert.Equal("task-7", preview.ExternalReference);
        Assert.Equal("Follow up supplier", preview.Title);
        Assert.Equal("Imported from JSON", preview.Summary);
        Assert.Equal(IntegrationTargetKind.FollowUps, preview.SuggestedTarget);
        Assert.Equal("Check whether quote arrived", preview.SuggestedAction);
    }

    [Fact]
    public void CsvImportUsesRowReferenceWhenExternalReferenceMissing()
    {
        const string content = "title,amount\r\nReceipt candidate,12.50";

        var result = ManualIntegrationImportConnector.ImportCsv(content, "receipt.csv");

        var preview = Assert.Single(result.Previews);
        Assert.Equal("row-1", preview.ExternalReference);
        Assert.Equal("manual-csv:row-1:nodate:12.50", preview.DuplicateKey);
    }

    [Fact]
    public void ImportReportsRowsWithoutTitle()
    {
        const string content =
            "externalReference,amount\r\n" +
            "missing-title,12.50";

        var result = ManualIntegrationImportConnector.ImportCsv(content, "bad.csv");

        Assert.Empty(result.Previews);
        var error = Assert.Single(result.Errors);
        Assert.Equal(1, error.RowNumber);
        Assert.Contains("Title is required", error.Message);
    }

    [Fact]
    public void CsvParserHandlesQuotedCommas()
    {
        const string content = "id,title,summary\r\n1,\"Receipt, pharmacy\",\"Painkillers, vitamins\"";

        var result = ManualIntegrationImportConnector.ImportCsv(content, "quoted.csv");

        var preview = Assert.Single(result.Previews);
        Assert.Equal("Receipt, pharmacy", preview.Title);
        Assert.Equal("Painkillers, vitamins", preview.Summary);
    }
}
