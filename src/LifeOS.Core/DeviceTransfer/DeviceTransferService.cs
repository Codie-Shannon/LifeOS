using LifeOS.Core.Forms;

namespace LifeOS.Core.DeviceTransfer;

public enum DeviceTransferState { Duplicate = 0, ConflictReview = 10, Resolved = 20, Rejected = 30 }
public enum DeviceTransferResolution { None = 0, KeepLocal = 10, KeepIncomingCandidate = 20, KeepBothCandidates = 30, RejectIncoming = 40 }

public sealed record DeviceTransferDraft(string? SourceDevice, string? DestinationDevice, string? RecordKey, string? LocalFingerprint, string? IncomingFingerprint);

public sealed record DeviceTransferReview
{
    public Guid Id { get; init; }
    public string SourceDevice { get; init; } = string.Empty;
    public string DestinationDevice { get; init; } = string.Empty;
    public string RecordKey { get; init; } = string.Empty;
    public string LocalFingerprint { get; init; } = string.Empty;
    public string IncomingFingerprint { get; init; } = string.Empty;
    public DeviceTransferState State { get; init; }
    public DeviceTransferResolution Resolution { get; init; }
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset UpdatedUtc { get; init; }
}

public static class DeviceTransferService
{
    public static FormValidationResult Validate(DeviceTransferDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Field(issues, "transfer-source", draft.SourceDevice, "Source device", 80);
        Field(issues, "transfer-destination", draft.DestinationDevice, "Destination device", 80);
        Field(issues, "transfer-record-key", draft.RecordKey, "Record key", 120);
        Fingerprint(issues, "transfer-local-fingerprint", draft.LocalFingerprint, "Local fingerprint");
        Fingerprint(issues, "transfer-incoming-fingerprint", draft.IncomingFingerprint, "Incoming fingerprint");
        return new FormValidationResult(issues);
    }

    public static DeviceTransferReview Create(DeviceTransferDraft draft, DateTimeOffset now)
    {
        if (!Validate(draft).IsValid) throw new ArgumentException("The transfer review is invalid.", nameof(draft));
        string local = draft.LocalFingerprint!.Trim().ToLowerInvariant();
        string incoming = draft.IncomingFingerprint!.Trim().ToLowerInvariant();
        return new DeviceTransferReview
        {
            Id = Guid.NewGuid(), SourceDevice = draft.SourceDevice!.Trim(), DestinationDevice = draft.DestinationDevice!.Trim(), RecordKey = draft.RecordKey!.Trim(),
            LocalFingerprint = local, IncomingFingerprint = incoming,
            State = local == incoming ? DeviceTransferState.Duplicate : DeviceTransferState.ConflictReview,
            Resolution = DeviceTransferResolution.None, CreatedUtc = now, UpdatedUtc = now
        };
    }

    public static DeviceTransferReview Resolve(DeviceTransferReview review, DeviceTransferResolution resolution, DateTimeOffset now)
    {
        DeviceTransferReview normalized = Normalize(review);
        if (normalized.State != DeviceTransferState.ConflictReview || resolution == DeviceTransferResolution.None) throw new InvalidOperationException("Only an unresolved conflict can receive an explicit decision.");
        return normalized with { State = resolution == DeviceTransferResolution.RejectIncoming ? DeviceTransferState.Rejected : DeviceTransferState.Resolved, Resolution = resolution, UpdatedUtc = now };
    }

    public static DeviceTransferReview Normalize(DeviceTransferReview review)
    {
        if (review.Id == Guid.Empty) throw new InvalidDataException("The transfer review id is invalid.");
        string source = RequiredLine(review.SourceDevice, "source device", 80), destination = RequiredLine(review.DestinationDevice, "destination device", 80), key = RequiredLine(review.RecordKey, "record key", 120);
        string local = RequiredFingerprint(review.LocalFingerprint, "local fingerprint"), incoming = RequiredFingerprint(review.IncomingFingerprint, "incoming fingerprint");
        if (review.CreatedUtc == default || review.UpdatedUtc == default) throw new InvalidDataException("Transfer review timestamps are required.");
        if (review.State == DeviceTransferState.Duplicate && (local != incoming || review.Resolution != DeviceTransferResolution.None)) throw new InvalidDataException("Duplicate transfer state is inconsistent.");
        if (review.State == DeviceTransferState.ConflictReview && (local == incoming || review.Resolution != DeviceTransferResolution.None)) throw new InvalidDataException("Conflict transfer state is inconsistent.");
        if (review.State is DeviceTransferState.Resolved or DeviceTransferState.Rejected && review.Resolution == DeviceTransferResolution.None) throw new InvalidDataException("Resolved transfer state requires a decision.");
        return review with { SourceDevice = source, DestinationDevice = destination, RecordKey = key, LocalFingerprint = local, IncomingFingerprint = incoming };
    }

    private static void Field(ICollection<FormFieldIssue> issues, string id, string? value, string label, int maximum) { Add(issues, FormValidation.Required(id, value, label)); Add(issues, FormValidation.MaximumLength(id, value, label, maximum)); Add(issues, FormValidation.SingleLine(id, value, label)); }
    private static void Fingerprint(ICollection<FormFieldIssue> issues, string id, string? value, string label) { Add(issues, FormValidation.Required(id, value, label)); if (!string.IsNullOrWhiteSpace(value) && (value.Trim().Length != 64 || value.Trim().Any(character => !Uri.IsHexDigit(character)))) issues.Add(new FormFieldIssue(id, "sha256", $"{label} must be a 64-character SHA-256 fingerprint.")); }
    private static string RequiredLine(string? value, string label, int maximum) { string normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim(); if (normalized.Length == 0 || normalized.Length > maximum || normalized.Any(char.IsControl)) throw new InvalidDataException($"The transfer {label} is invalid."); return normalized; }
    private static string RequiredFingerprint(string? value, string label) { string normalized = RequiredLine(value, label, 64).ToLowerInvariant(); if (normalized.Length != 64 || normalized.Any(character => !Uri.IsHexDigit(character))) throw new InvalidDataException($"The transfer {label} is invalid."); return normalized; }
    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue) { if (issue is not null) issues.Add(issue); }
}
