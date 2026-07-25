namespace LifeOS.Core.GuardedProviders;

public enum ProviderCapability
{
    Read,
    Import,
    Draft,
    Write
}

public enum ProviderReviewState
{
    New,
    Deferred,
    Accepted,
    Rejected,
    Linked,
    Duplicate,
    Conflict
}

public sealed record ProviderPermission(
    string ProviderId,
    ProviderCapability Capability,
    bool Enabled,
    DateTimeOffset? GrantedAt,
    string GrantedBy);

public sealed record ProviderContract(
    string Id,
    string Name,
    string SetupState,
    IReadOnlyList<ProviderCapability> Capabilities,
    string ManualSetupNote);

public sealed record ProviderCandidate(
    string Id,
    string ProviderId,
    string SourceReference,
    DateTimeOffset SourceTimestamp,
    string RecordType,
    string Title,
    string Fingerprint,
    ProviderReviewState State,
    string? LinkedRecordId,
    string? ConflictReason);

public sealed record ProviderAudit(
    DateTimeOffset At,
    string ProviderId,
    string Action,
    string CandidateId,
    string Detail);

public sealed class GuardedProviderService
{
    public bool EmergencyStopEnabled { get; private set; }

    public ProviderCandidate ImportPreview(
        ProviderContract contract,
        ProviderPermission permission,
        string sourceReference,
        DateTimeOffset sourceTimestamp,
        string recordType,
        string title,
        string fingerprint,
        IEnumerable<ProviderCandidate> existing)
    {
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(permission);
        if (EmergencyStopEnabled)
        {
            throw new InvalidOperationException("Provider intake is stopped globally.");
        }
        if (permission.ProviderId != contract.Id ||
            permission.Capability != ProviderCapability.Import ||
            !permission.Enabled)
        {
            throw new InvalidOperationException("Explicit import permission is required.");
        }

        ProviderCandidate? duplicate = existing.FirstOrDefault(candidate =>
            string.Equals(candidate.ProviderId, contract.Id, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(candidate.Fingerprint, fingerprint, StringComparison.Ordinal));

        return new ProviderCandidate(
            Guid.NewGuid().ToString("N"),
            contract.Id,
            sourceReference,
            sourceTimestamp,
            recordType,
            title,
            fingerprint,
            duplicate is null ? ProviderReviewState.New : ProviderReviewState.Duplicate,
            duplicate?.LinkedRecordId,
            duplicate is null ? null : $"Matches candidate {duplicate.Id}.");
    }

    public ProviderCandidate Defer(ProviderCandidate candidate) =>
        candidate.State is ProviderReviewState.New or ProviderReviewState.Conflict
            ? candidate with { State = ProviderReviewState.Deferred }
            : throw new InvalidOperationException("This candidate cannot be deferred.");

    public ProviderCandidate Reject(ProviderCandidate candidate) =>
        candidate.State is ProviderReviewState.Accepted or ProviderReviewState.Linked
            ? throw new InvalidOperationException("Accepted or linked candidates require an explicit unlink workflow.")
            : candidate with { State = ProviderReviewState.Rejected };

    public ProviderCandidate Accept(ProviderCandidate candidate) =>
        candidate.State is ProviderReviewState.New or ProviderReviewState.Deferred
            ? candidate with { State = ProviderReviewState.Accepted }
            : throw new InvalidOperationException("Only new or deferred candidates can be accepted.");

    public ProviderCandidate Link(ProviderCandidate candidate, string authoritativeRecordId)
    {
        if (candidate.State != ProviderReviewState.Accepted)
        {
            throw new InvalidOperationException("A candidate must be accepted before it can be linked.");
        }
        if (string.IsNullOrWhiteSpace(authoritativeRecordId))
        {
            throw new ArgumentException("An authoritative record is required.", nameof(authoritativeRecordId));
        }

        return candidate with
        {
            State = ProviderReviewState.Linked,
            LinkedRecordId = authoritativeRecordId.Trim()
        };
    }

    public void RequireWritePermission(
        string providerId,
        IReadOnlyCollection<ProviderPermission> permissions)
    {
        if (EmergencyStopEnabled)
        {
            throw new InvalidOperationException("External writes are stopped globally.");
        }

        bool allowed = permissions.Any(permission =>
            permission.ProviderId == providerId &&
            permission.Capability == ProviderCapability.Write &&
            permission.Enabled &&
            permission.GrantedAt is not null);
        if (!allowed)
        {
            throw new InvalidOperationException("Provider writes are disabled until explicitly granted.");
        }
    }

    public void SetEmergencyStop(bool enabled) => EmergencyStopEnabled = enabled;

    public ProviderAudit Audit(
        ProviderCandidate candidate,
        string action,
        string detail,
        DateTimeOffset at) =>
        new(at, candidate.ProviderId, action, candidate.Id, detail);
}
