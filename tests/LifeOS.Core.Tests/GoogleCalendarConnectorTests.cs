using LifeOS.Core.IntegrationConnectors;
using LifeOS.Core.IntegrationConnectors.GoogleCalendar;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Shared.IntegrationConnectors.GoogleCalendar;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class GoogleCalendarConnectorTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 11, 12, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void Maps_external_event_to_untrusted_read_only_preview()
    {
        var item = GoogleCalendarPreviewMapper.Map(Event("evt-1"));

        Assert.Equal("google-calendar", item.ConnectorKey);
        Assert.Equal("evt-1", item.ExternalReference);
        Assert.Equal(IntegrationPreviewStatus.New, item.Status);
        Assert.Equal(IntegrationTrustState.Untrusted, item.TrustState);
        Assert.True(item.IsReadOnlyPreview);
        Assert.True(item.RequiresHumanReview);
        Assert.Equal(IntegrationTargetKind.ItemState, item.SuggestedTarget);
        Assert.Equal("calendar-primary", item.ProviderContainerId);
        Assert.Equal(Now.LocalDateTime.AddHours(2), item.EndsAt);
    }

    [Fact]
    public void Duplicate_key_is_stable_for_same_provider_event()
    {
        var first = GoogleCalendarPreviewMapper.Map(Event("evt-1"));
        var second = GoogleCalendarPreviewMapper.Map(Event("evt-1"));
        Assert.Equal(first.DuplicateKey, second.DuplicateKey);
    }

    [Fact]
    public async Task Repeated_refresh_is_marked_duplicate_suspected()
    {
        var provider = new FakeProvider([Event("evt-1")]);
        var service = new GoogleCalendarRefreshService(provider, () => Now);
        var range = new CalendarRefreshRange(Now, Now.AddDays(7));
        var first = await service.RefreshAsync(range);
        var second = await service.RefreshAsync(range);

        var count = IntegrationImportDuplicateDetector.MarkDuplicateSuspicions(first.Previews, second.Previews);
        Assert.Equal(1, count);
        Assert.Equal(IntegrationPreviewStatus.DuplicateSuspected, second.Previews[0].Status);
        Assert.Throws<InvalidOperationException>(() => IntegrationInboxReviewEngine.Accept(second.Previews[0], "blind accept"));
    }

    [Fact]
    public void Range_rejects_more_than_31_days()
    {
        var range = new CalendarRefreshRange(Now, Now.AddDays(32));
        Assert.Throws<ArgumentException>(() => range.Validate(Now));
    }

    [Fact]
    public async Task Empty_provider_response_returns_empty_preview_set()
    {
        var service = new GoogleCalendarRefreshService(new FakeProvider([]), () => Now);
        var result = await service.RefreshAsync(new CalendarRefreshRange(Now, Now.AddDays(1)));
        Assert.Empty(result.Previews);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Malformed_event_is_skipped_without_automatic_state_change()
    {
        var malformed = Event("evt-bad") with { Title = "" };
        var service = new GoogleCalendarRefreshService(new FakeProvider([malformed]), () => Now);
        var result = await service.RefreshAsync(new CalendarRefreshRange(Now, Now.AddDays(1)));
        Assert.Empty(result.Previews);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Provider_failure_is_not_hidden()
    {
        var service = new GoogleCalendarRefreshService(new ThrowingProvider(), () => Now);
        await Assert.ThrowsAsync<HttpRequestException>(() => service.RefreshAsync(new CalendarRefreshRange(Now, Now.AddDays(1))));
    }

    [Fact]
    public void Registry_exposes_read_only_google_calendar_action_policy()
    {
        var connector = IntegrationConnectorRegistry.GetRequired("google-calendar");
        Assert.Contains(IntegrationConnectorScope.ReadMetadata, connector.Scopes);
        Assert.Contains(IntegrationConnectorScope.ReadContent, connector.Scopes);
        Assert.Equal(IntegrationConnectorActionPolicy.PreviewOnly, connector.ActionPolicy);
        Assert.False(connector.AllowsExternalMutation);
    }

    private static ExternalCalendarEvent Event(string id) => new(
        id, "Safe sample appointment", Now, Now.AddHours(2), "Sample location", "Sample description",
        "calendar-primary", "Primary calendar", "Connected Google account", "", Now);

    private sealed class FakeProvider(IReadOnlyList<ExternalCalendarEvent> events) : IReadOnlyCalendarProvider
    {
        public Task<IReadOnlyList<ExternalCalendarEvent>> GetEventsAsync(CalendarRefreshRange range, CancellationToken cancellationToken = default)
            => Task.FromResult(events);
    }

    private sealed class ThrowingProvider : IReadOnlyCalendarProvider
    {
        public Task<IReadOnlyList<ExternalCalendarEvent>> GetEventsAsync(CalendarRefreshRange range, CancellationToken cancellationToken = default)
            => throw new HttpRequestException("Provider unavailable.");
    }

    [Fact]
    public void Lifecycle_store_sanitizes_secret_bearing_error_text()
    {
        var sanitized = GoogleCalendarLifecycleStore.SanitizeError(new InvalidOperationException(
            "token refresh failed client_secret=super-secret access_token=ya29.private https://oauth2.googleapis.com/token"));
        Assert.DoesNotContain("super-secret", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("ya29.private", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("oauth2.googleapis.com", sanitized, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("401 unauthorized", GoogleCalendarLifecycleState.AuthenticationExpired)]
    [InlineData("invalid_grant access revoked", GoogleCalendarLifecycleState.AccessRevoked)]
    [InlineData("network unavailable", GoogleCalendarLifecycleState.RefreshFailed)]
    public void Lifecycle_failure_classification_is_explicit(string message, GoogleCalendarLifecycleState expected)
    {
        Assert.Equal(expected, GoogleCalendarLifecycleStore.ClassifyFailure(new InvalidOperationException(message)));
    }

    [Fact]
    public void Lifecycle_snapshot_defaults_to_manual_recovery_without_automation()
    {
        var snapshot = new GoogleCalendarLifecycleSnapshot();
        Assert.Null(snapshot.LastRefreshAttemptAt);
        Assert.Null(snapshot.LastSuccessfulRefreshAt);
        Assert.Equal("No connector action has been recorded yet.", snapshot.LastResultSummary);
    }

    [Fact]
    public void Lifecycle_audit_entry_is_sanitized_and_does_not_create_preview_links()
    {
        var entry = IntegrationImportAudit.CreateConnectorLifecycleEntry(
            "google-calendar", "refresh-failure", "Sanitized provider failure.");
        Assert.Equal("CONNECTOR-LIFECYCLE", entry.FileKind);
        Assert.Empty(entry.PreviewIds);
        Assert.Equal(0, entry.ImportedCount);
        Assert.Equal("refresh-failure", entry.Action);
    }

}
