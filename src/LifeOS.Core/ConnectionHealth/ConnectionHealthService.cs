using LifeOS.Core.ConfigurationReadiness;
using LifeOS.Core.Forms;

namespace LifeOS.Core.ConnectionHealth;

public enum ConnectionCheckKind { ConfigurationReference = 0, AuthenticationBoundary = 10, ProviderRead = 20, ProviderWrite = 30 }
public enum ConnectionHealthState { NotConfigured = 0, ReadyForCredentialedTest = 10, Passed = 20, Degraded = 30, Failed = 40, Blocked = 50 }
public sealed record ConnectionHealthDraft(string? Capability, string? Owner, ConfigurationEnvironment Environment, ConnectionCheckKind Kind, string? SecretReferenceName, string? Notes);
public sealed record ConnectionHealthRecord(Guid Id, string Capability, string Owner, ConfigurationEnvironment Environment, ConnectionCheckKind Kind, string SecretReferenceName, string Notes, ConnectionHealthState State, string Observation, DateTimeOffset CreatedUtc, DateTimeOffset UpdatedUtc);

public static class ConnectionHealthService
{
    public static FormValidationResult Validate(ConnectionHealthDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Add(issues, FormValidation.Required("health-capability", draft.Capability, "Capability")); Add(issues, FormValidation.MaximumLength("health-capability", draft.Capability, "Capability", 100)); Add(issues, FormValidation.SingleLine("health-capability", draft.Capability, "Capability"));
        Add(issues, FormValidation.Required("health-owner", draft.Owner, "Owner")); Add(issues, FormValidation.MaximumLength("health-owner", draft.Owner, "Owner", 100)); Add(issues, FormValidation.SingleLine("health-owner", draft.Owner, "Owner"));
        Add(issues, FormValidation.MaximumLength("health-notes", draft.Notes, "Notes", 500));
        if (!Enum.IsDefined(draft.Environment)) issues.Add(new("health-environment", "environment", "Select a supported environment."));
        if (!Enum.IsDefined(draft.Kind)) issues.Add(new("health-kind", "check-kind", "Select a supported diagnostic kind."));
        string reference = Clean(draft.SecretReferenceName);
        if (reference.Length > 0 && !IsReferenceName(reference)) issues.Add(new("health-secret-reference", "reference-name", "Use an uppercase environment-variable reference name, never a secret value."));
        if (ConfigurationReadinessService.ContainsSecretLikeValue(draft.Notes)) issues.Add(new("health-notes", "secret-like-value", "Notes appear to contain a secret or credential value."));
        return new(issues);
    }

    public static ConnectionHealthRecord Create(ConnectionHealthDraft draft, DateTimeOffset now)
    {
        if (!Validate(draft).IsValid) throw new ArgumentException("The connection-health plan is invalid.", nameof(draft));
        string reference = Clean(draft.SecretReferenceName);
        return Normalize(new(Guid.NewGuid(), draft.Capability!.Trim(), draft.Owner!.Trim(), draft.Environment, draft.Kind, reference, Clean(draft.Notes), reference.Length == 0 ? ConnectionHealthState.NotConfigured : ConnectionHealthState.ReadyForCredentialedTest, "No credentialed test has run.", now, now));
    }

    public static ConnectionHealthRecord RecordObservation(ConnectionHealthRecord record, ConnectionHealthState state, string? observation, DateTimeOffset now)
    {
        ConnectionHealthRecord value = Normalize(record);
        if (state is not (ConnectionHealthState.Passed or ConnectionHealthState.Degraded or ConnectionHealthState.Failed or ConnectionHealthState.Blocked)) throw new ArgumentException("Select an observed health state.", nameof(state));
        string detail = Clean(observation);
        if (detail.Length == 0 || detail.Length > 240 || ConfigurationReadinessService.ContainsSecretLikeValue(detail)) throw new ArgumentException("A safe observation is required.", nameof(observation));
        return Normalize(value with { State = state, Observation = detail, UpdatedUtc = now });
    }

    public static ConnectionHealthRecord Normalize(ConnectionHealthRecord record)
    {
        if (record.Id == Guid.Empty || record.CreatedUtc == default || record.UpdatedUtc == default) throw new InvalidDataException("Connection-health identity and timestamps are required.");
        if (!Enum.IsDefined(record.Environment) || !Enum.IsDefined(record.Kind) || !Enum.IsDefined(record.State)) throw new InvalidDataException("Connection-health enum value is invalid.");
        string capability = Required(record.Capability, "capability", 100), owner = Required(record.Owner, "owner", 100), reference = Clean(record.SecretReferenceName), notes = Clean(record.Notes), observation = Required(record.Observation, "observation", 240);
        if (reference.Length > 0 && !IsReferenceName(reference)) throw new InvalidDataException("Connection-health reference name is invalid.");
        if (notes.Length > 500 || ConfigurationReadinessService.ContainsSecretLikeValue(notes) || ConfigurationReadinessService.ContainsSecretLikeValue(observation)) throw new InvalidDataException("Connection-health text is unsafe.");
        if (record.State == ConnectionHealthState.ReadyForCredentialedTest && reference.Length == 0) throw new InvalidDataException("Ready state requires a reference name.");
        return record with { Capability = capability, Owner = owner, SecretReferenceName = reference, Notes = notes, Observation = observation };
    }

    private static bool IsReferenceName(string value) => value.Length is >= 3 and <= 100 && char.IsUpper(value[0]) && value.All(c => char.IsUpper(c) || char.IsDigit(c) || c == '_');
    private static string Required(string? value, string label, int max) { string result = Clean(value); if (result.Length == 0 || result.Length > max || result.Any(char.IsControl)) throw new InvalidDataException($"Connection-health {label} is invalid."); return result; }
    private static string Clean(string? value) => string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue) { if (issue is not null) issues.Add(issue); }
}
