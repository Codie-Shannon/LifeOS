using LifeOS.Core.ConfigurationReadiness;
using LifeOS.Core.Forms;
using LifeOS.Core.ProviderAdapters;

namespace LifeOS.Core.ProviderConsent;

public enum ProviderConsentState { Proposed = 0, Approved = 10, Paused = 20, Revoked = 30, Expired = 40 }
public sealed record ProviderConsentDraft(string? ProfileName, ProviderFamily Family, string? Owner, ConfigurationEnvironment Environment, ProviderCapability Capabilities, string? Purpose, DateOnly? ExpiresOn, string? Notes);
public sealed record ProviderConsentRecord(Guid Id, string ProfileName, ProviderFamily Family, string Owner, ConfigurationEnvironment Environment, ProviderCapability Capabilities, string Purpose, DateOnly? ExpiresOn, string Notes, ProviderConsentState State, DateTimeOffset CreatedUtc, DateTimeOffset UpdatedUtc);

public static class ProviderConsentService
{
    private const ProviderCapability KnownCapabilities = ProviderCapability.CalendarRead | ProviderCapability.EmailRead | ProviderCapability.FileRead | ProviderCapability.DraftProposal | ProviderCapability.ProviderWrite;

    public static FormValidationResult Validate(ProviderConsentDraft draft, DateOnly today)
    {
        List<FormFieldIssue> issues = [];
        Field(issues, "consent-name", draft.ProfileName, "Profile name", 100, true);
        Field(issues, "consent-owner", draft.Owner, "Owner", 100, true);
        Field(issues, "consent-purpose", draft.Purpose, "Purpose", 240, true);
        Add(issues, FormValidation.MaximumLength("consent-notes", draft.Notes, "Notes", 500));
        if (!Enum.IsDefined(draft.Family)) issues.Add(new("consent-family", "family", "Select a supported provider family."));
        if (!Enum.IsDefined(draft.Environment)) issues.Add(new("consent-environment", "environment", "Select a supported environment."));
        if (draft.Capabilities == ProviderCapability.None || (draft.Capabilities & ~KnownCapabilities) != 0) issues.Add(new("consent-capabilities", "capabilities", "Select at least one supported capability."));
        if (draft.ExpiresOn is { } expiry && expiry < today) issues.Add(new("consent-expiry", "expiry", "Expiry cannot be in the past."));
        if (ConfigurationReadinessService.ContainsSecretLikeValue(draft.Purpose) || ConfigurationReadinessService.ContainsSecretLikeValue(draft.Notes)) issues.Add(new("consent-notes", "secret-like-value", "Purpose or notes appear to contain a secret or credential value."));
        return new(issues);
    }

    public static ProviderConsentRecord Create(ProviderConsentDraft draft, DateOnly today, DateTimeOffset now)
    {
        if (!Validate(draft, today).IsValid) throw new ArgumentException("The provider-consent profile is invalid.", nameof(draft));
        return Normalize(new(Guid.NewGuid(), draft.ProfileName!.Trim(), draft.Family, draft.Owner!.Trim(), draft.Environment, draft.Capabilities, draft.Purpose!.Trim(), draft.ExpiresOn, Clean(draft.Notes), ProviderConsentState.Proposed, now, now), today);
    }

    public static ProviderConsentRecord SetState(ProviderConsentRecord record, ProviderConsentState state, DateOnly today, DateTimeOffset now)
    {
        ProviderConsentRecord value = Normalize(record, today);
        if (!Enum.IsDefined(state)) throw new ArgumentException("Select a supported consent state.", nameof(state));
        if (state == value.State) throw new InvalidOperationException("The consent state is already selected.");
        if (value.State == ProviderConsentState.Revoked && state != ProviderConsentState.Proposed) throw new InvalidOperationException("Revoked consent must be reproposed before review.");
        if (state == ProviderConsentState.Approved && value.ExpiresOn is { } expiry && expiry < today) throw new InvalidOperationException("Expired consent cannot be approved.");
        return Normalize(value with { State = state, UpdatedUtc = now }, today);
    }

    public static ProviderConsentRecord RefreshExpiry(ProviderConsentRecord record, DateOnly today)
    {
        ProviderConsentRecord value = Normalize(record, today, applyExpiry: false);
        return value.ExpiresOn is { } expiry && expiry < today && value.State is not (ProviderConsentState.Revoked or ProviderConsentState.Expired)
            ? value with { State = ProviderConsentState.Expired }
            : value;
    }

    public static bool ProviderAccessEnabled(ProviderConsentRecord record) => false;

    public static ProviderConsentRecord Normalize(ProviderConsentRecord record, DateOnly? today = null, bool applyExpiry = true)
    {
        if (record.Id == Guid.Empty || record.CreatedUtc == default || record.UpdatedUtc == default) throw new InvalidDataException("Consent identity and timestamps are required.");
        if (!Enum.IsDefined(record.Family) || !Enum.IsDefined(record.Environment) || !Enum.IsDefined(record.State) || record.Capabilities == ProviderCapability.None || (record.Capabilities & ~KnownCapabilities) != 0) throw new InvalidDataException("Consent enum or capability value is invalid.");
        string name = Required(record.ProfileName, "profile name", 100), owner = Required(record.Owner, "owner", 100), purpose = Required(record.Purpose, "purpose", 240), notes = Clean(record.Notes);
        if (notes.Length > 500 || ConfigurationReadinessService.ContainsSecretLikeValue(purpose) || ConfigurationReadinessService.ContainsSecretLikeValue(notes)) throw new InvalidDataException("Consent text is unsafe.");
        ProviderConsentRecord value = record with { ProfileName = name, Owner = owner, Purpose = purpose, Notes = notes };
        return applyExpiry && today is { } date ? RefreshExpiry(value, date) : value;
    }

    private static void Field(ICollection<FormFieldIssue> issues, string id, string? value, string label, int max, bool required) { if (required) Add(issues, FormValidation.Required(id, value, label)); Add(issues, FormValidation.MaximumLength(id, value, label, max)); Add(issues, FormValidation.SingleLine(id, value, label)); }
    private static string Required(string? value, string label, int max) { string result = Clean(value); if (result.Length == 0 || result.Length > max || result.Any(char.IsControl)) throw new InvalidDataException($"Consent {label} is invalid."); return result; }
    private static string Clean(string? value) => string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue) { if (issue is not null) issues.Add(issue); }
}
