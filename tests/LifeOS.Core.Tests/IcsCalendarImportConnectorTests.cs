using LifeOS.Core.IntegrationConnectors;
using LifeOS.Core.IntegrationInbox;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class IcsCalendarImportConnectorTests
{
    [Fact]
    public void IcsImportCreatesCalendarPreview()
    {
        const string content = """
        BEGIN:VCALENDAR
        VERSION:2.0
        BEGIN:VEVENT
        UID:calendar-event-1
        DTSTART:20260710T090000
        DTEND:20260710T100000
        SUMMARY:Client planning call
        DESCRIPTION:Prep agenda before the call
        LOCATION:Zoom
        END:VEVENT
        END:VCALENDAR
        """;

        var result = IcsCalendarImportConnector.Import(content, "sample.ics");

        Assert.Empty(result.Errors);
        Assert.Single(result.Previews);

        var preview = result.Previews.Single();
        Assert.Equal("ics-import", result.ConnectorKey);
        Assert.Equal(IntegrationSourceKind.Calendar, preview.SourceKind);
        Assert.Equal("ics-import", preview.SourceLabel);
        Assert.Equal("calendar-event-1", preview.ExternalReference);
        Assert.Equal("Client planning call", preview.Title);
        Assert.Contains("Prep agenda", preview.Summary);
        Assert.Contains("Zoom", preview.Summary);
        Assert.Equal(IntegrationTargetKind.ItemState, preview.SuggestedTarget);
        Assert.Equal("sample.ics#VEVENT-1", preview.SourceEvidence);
        Assert.Equal("ics-import:calendar-event-1:20260710090000:noamount", preview.DuplicateKey);
        Assert.True(preview.IsReadOnlyPreview);
        Assert.True(preview.RequiresHumanReview);
    }

    [Fact]
    public void IcsImportReportsEventsWithoutSummary()
    {
        const string content = """
        BEGIN:VCALENDAR
        BEGIN:VEVENT
        UID:calendar-event-1
        DTSTART:20260710
        END:VEVENT
        END:VCALENDAR
        """;

        var result = IcsCalendarImportConnector.Import(content, "bad.ics");

        Assert.Empty(result.Previews);
        var error = Assert.Single(result.Errors);
        Assert.Equal(1, error.RowNumber);
        Assert.Contains("summary is required", error.Message);
    }

    [Fact]
    public void IcsImportCanBeDuplicateCheckedBeforeSave()
    {
        const string content = """
        BEGIN:VCALENDAR
        BEGIN:VEVENT
        UID:calendar-event-1
        DTSTART:20260710T090000
        SUMMARY:Client planning call
        END:VEVENT
        END:VCALENDAR
        """;

        var result = IcsCalendarImportConnector.Import(content, "sample.ics");

        var duplicateCount = IntegrationImportDuplicateDetector.MarkDuplicateSuspicions(
            [new IntegrationPreviewItem { DuplicateKey = "ics-import:calendar-event-1:20260710090000:noamount" }],
            result.Previews);

        Assert.Equal(1, duplicateCount);
        Assert.Equal(IntegrationPreviewStatus.DuplicateSuspected, result.Previews.Single().Status);
    }
}
