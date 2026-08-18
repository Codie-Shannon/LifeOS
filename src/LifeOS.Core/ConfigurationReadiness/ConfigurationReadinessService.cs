using System.Text.RegularExpressions;
using LifeOS.Core.Forms;

namespace LifeOS.Core.ConfigurationReadiness;

public enum ConfigurationEnvironment { LocalDevelopment = 0, Test = 10, Staging = 15, Production = 20 }
public enum ConfigurationReadinessState { MissingOwnerInput = 0, ReadyForConfiguredTest = 10, Blocked = 20 }
public sealed record ConfigurationReadinessDraft(string? Capability, ConfigurationEnvironment Environment, string? Owner, string? SecretReferenceName, string? Notes);
public sealed record ConfigurationReadinessRecord(Guid Id, string Capability, ConfigurationEnvironment Environment, string Owner, string SecretReferenceName, string Notes, ConfigurationReadinessState State, DateTimeOffset CreatedUtc, DateTimeOffset UpdatedUtc);

public static partial class ConfigurationReadinessService
{
    public static FormValidationResult Validate(ConfigurationReadinessDraft draft)
    {
        List<FormFieldIssue> issues = []; Field(issues, "config-capability", draft.Capability, "Capability", 100, true); Field(issues, "config-owner", draft.Owner, "Owner", 100, true); Field(issues, "config-secret-reference", draft.SecretReferenceName, "Secret reference name", 100, false); Add(issues, FormValidation.MaximumLength("config-notes", draft.Notes, "Notes", 500)); if (!Enum.IsDefined(draft.Environment)) issues.Add(new("config-environment", "environment", "Select a supported configuration environment."));
        if (!string.IsNullOrWhiteSpace(draft.SecretReferenceName) && !ReferencePattern().IsMatch(draft.SecretReferenceName.Trim())) issues.Add(new("config-secret-reference", "reference-name", "Secret reference must be an uppercase environment-variable name, never a secret value."));
        if (ContainsSecretLikeValue(draft.Notes)) issues.Add(new("config-notes", "secret-like-value", "Notes appear to contain a secret or credential value. Store only a reference name."));
        return new(issues);
    }
    public static ConfigurationReadinessRecord Create(ConfigurationReadinessDraft draft, DateTimeOffset now)
    {
        if (!Validate(draft).IsValid) throw new ArgumentException("The configuration readiness record is invalid.", nameof(draft));
        string reference = Clean(draft.SecretReferenceName); return Normalize(new(Guid.NewGuid(), draft.Capability!.Trim(), draft.Environment, draft.Owner!.Trim(), reference, Clean(draft.Notes), reference.Length == 0 ? ConfigurationReadinessState.MissingOwnerInput : ConfigurationReadinessState.ReadyForConfiguredTest, now, now));
    }
    public static ConfigurationReadinessRecord SetBlocked(ConfigurationReadinessRecord record, bool blocked, DateTimeOffset now)
    {
        ConfigurationReadinessRecord value = Normalize(record); ConfigurationReadinessState next = blocked ? ConfigurationReadinessState.Blocked : value.SecretReferenceName.Length == 0 ? ConfigurationReadinessState.MissingOwnerInput : ConfigurationReadinessState.ReadyForConfiguredTest; if (next == value.State) throw new InvalidOperationException("The configuration state is already selected."); return value with { State = next, UpdatedUtc = now };
    }
    public static ConfigurationReadinessRecord Normalize(ConfigurationReadinessRecord record)
    {
        if (record.Id == Guid.Empty || record.CreatedUtc == default || record.UpdatedUtc == default) throw new InvalidDataException("Configuration identity and timestamps are required."); if (!Enum.IsDefined(record.Environment)) throw new InvalidDataException("Configuration environment is invalid."); string capability = Required(record.Capability, "capability", 100), owner = Required(record.Owner, "owner", 100), reference = Clean(record.SecretReferenceName), notes = Clean(record.Notes); if (reference.Length > 0 && !ReferencePattern().IsMatch(reference)) throw new InvalidDataException("The secret reference name is invalid."); if (notes.Length > 500 || ContainsSecretLikeValue(notes)) throw new InvalidDataException("Configuration notes are unsafe."); if (record.State == ConfigurationReadinessState.ReadyForConfiguredTest && reference.Length == 0) throw new InvalidDataException("Ready state requires a reference name."); return record with { Capability = capability, Owner = owner, SecretReferenceName = reference, Notes = notes };
    }
    public static bool ContainsSecretLikeValue(string? value) { if (string.IsNullOrWhiteSpace(value)) return false; string text = value.Trim(); return text.Contains("BEGIN PRIVATE KEY", StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(text, @"(?i)(password|secret|token|api[_-]?key)\s*[:=]\s*\S+"); }
    private static void Field(ICollection<FormFieldIssue> issues, string id, string? value, string label, int max, bool required) { if (required) Add(issues, FormValidation.Required(id, value, label)); Add(issues, FormValidation.MaximumLength(id, value, label, max)); Add(issues, FormValidation.SingleLine(id, value, label)); }
    private static string Required(string? value, string label, int max) { string result = Clean(value); if (result.Length == 0 || result.Length > max || result.Any(char.IsControl)) throw new InvalidDataException($"Configuration {label} is invalid."); return result; }
    private static string Clean(string? value) => string.IsNullOrWhiteSpace(value) ? "" : value.Trim(); private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue) { if (issue is not null) issues.Add(issue); }
    [GeneratedRegex("^[A-Z][A-Z0-9_]{2,99}$", RegexOptions.CultureInvariant)] private static partial Regex ReferencePattern();
}
