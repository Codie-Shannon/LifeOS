using LifeOS.Core.GuardedProviders;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups73To76GuardedProviderTests
{
    private readonly GuardedProviderService _service = new();
    private readonly ProviderContract _contract = new(
        "google-calendar",
        "Google Calendar",
        "Manual setup ready",
        new[] { ProviderCapability.Read, ProviderCapability.Import },
        "Connect from Settings when required.");
    private readonly ProviderPermission _importPermission = new(
        "google-calendar",
        ProviderCapability.Import,
        true,
        new DateTimeOffset(2026, 7, 27, 8, 0, 0, TimeSpan.FromHours(12)),
        "user");

    [Fact]
    public void Imported_provider_data_stays_a_candidate()
    {
        ProviderCandidate candidate = Import("fingerprint-1", Array.Empty<ProviderCandidate>());

        Assert.Equal(ProviderReviewState.New, candidate.State);
        Assert.Null(candidate.LinkedRecordId);
        Assert.Equal("fictional://calendar/event-1", candidate.SourceReference);
    }

    [Fact]
    public void Duplicate_fingerprints_are_never_silently_merged()
    {
        ProviderCandidate original = Import("fingerprint-1", Array.Empty<ProviderCandidate>());
        ProviderCandidate duplicate = Import("fingerprint-1", new[] { original });

        Assert.Equal(ProviderReviewState.Duplicate, duplicate.State);
        Assert.Contains(original.Id, duplicate.ConflictReason);
    }

    [Fact]
    public void Linking_requires_acceptance_and_an_authoritative_record()
    {
        ProviderCandidate candidate = Import("fingerprint-1", Array.Empty<ProviderCandidate>());

        Assert.Throws<InvalidOperationException>(() => _service.Link(candidate, "agenda-1"));

        ProviderCandidate linked = _service.Link(_service.Accept(candidate), "agenda-1");
        Assert.Equal(ProviderReviewState.Linked, linked.State);
        Assert.Equal("agenda-1", linked.LinkedRecordId);
    }

    [Fact]
    public void Writes_are_disabled_without_an_explicit_grant()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _service.RequireWritePermission("google-calendar", new[] { _importPermission }));
    }

    [Fact]
    public void Emergency_stop_blocks_imports_and_writes()
    {
        _service.SetEmergencyStop(true);

        Assert.Throws<InvalidOperationException>(() => Import("fingerprint-1", Array.Empty<ProviderCandidate>()));
        Assert.Throws<InvalidOperationException>(() =>
            _service.RequireWritePermission("google-calendar", new[]
            {
                new ProviderPermission("google-calendar", ProviderCapability.Write, true, DateTimeOffset.Now, "user")
            }));
    }

    private ProviderCandidate Import(string fingerprint, IEnumerable<ProviderCandidate> existing) =>
        _service.ImportPreview(
            _contract,
            _importPermission,
            "fictional://calendar/event-1",
            new DateTimeOffset(2026, 7, 27, 9, 0, 0, TimeSpan.FromHours(12)),
            "calendar-event",
            "Fictional delivery review",
            fingerprint,
            existing);
}
