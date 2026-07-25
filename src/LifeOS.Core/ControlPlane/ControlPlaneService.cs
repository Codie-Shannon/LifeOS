using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LifeOS.Core.ControlPlane;

public enum SensitiveCategory
{
    Money,
    Health,
    Family,
    LegalAndPrivateDocuments,
    Communications,
    ExternalAi
}

public sealed record CategoryPermission(
    SensitiveCategory Category,
    bool Enabled,
    DateTimeOffset? GrantedAt,
    string ChangePath);

public sealed record PrivacyProfile(
    IReadOnlyList<CategoryPermission> Permissions,
    bool SanitizedCrashReports,
    int CrashRetentionDays,
    bool HideSensitivePreviews);

public sealed record BackupEnvelope(
    int SchemaVersion,
    DateTimeOffset CreatedAt,
    string Payload,
    string Sha256,
    bool ContainsCredentials);

public sealed record ControlPlaneState(
    bool EmergencyStop,
    IReadOnlySet<string> DisconnectedProviders,
    IReadOnlyList<string> Audit,
    bool UndoAvailable);

public sealed class ControlPlaneService
{
    private static readonly HashSet<string> CredentialFieldNames = new(
        new[] { "password", "token", "secret", "credential", "apikey", "accesstoken", "refreshtoken" },
        StringComparer.OrdinalIgnoreCase);

    public bool CanAccess(PrivacyProfile profile, SensitiveCategory category) =>
        profile.Permissions.Any(permission =>
            permission.Category == category &&
            permission.Enabled &&
            permission.GrantedAt is not null);

    public PrivacyProfile SetPermission(
        PrivacyProfile profile,
        SensitiveCategory category,
        bool enabled,
        DateTimeOffset at)
    {
        int matches = profile.Permissions.Count(permission => permission.Category == category);
        if (matches != 1)
        {
            throw new InvalidOperationException(
                $"Expected exactly one permission record for {category}; found {matches}.");
        }

        CategoryPermission[] permissions = profile.Permissions
            .Select(permission => permission.Category == category
                ? permission with { Enabled = enabled, GrantedAt = enabled ? at : null }
                : permission)
            .ToArray();
        return profile with { Permissions = permissions };
    }

    public BackupEnvelope Export<T>(T state, DateTimeOffset at)
    {
        string payload = JsonSerializer.Serialize(state);
        using JsonDocument document = JsonDocument.Parse(payload);
        if (ContainsCredentialField(document.RootElement))
        {
            throw new InvalidOperationException(
                "Backup payload contains credential-like fields and cannot be exported.");
        }

        return new BackupEnvelope(
            1,
            at,
            payload,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))),
            false);
    }

    public T ValidateAndRestore<T>(BackupEnvelope envelope)
    {
        if (envelope.SchemaVersion != 1)
        {
            throw new InvalidOperationException(
                $"Unsupported backup schema version: {envelope.SchemaVersion}.");
        }

        if (envelope.ContainsCredentials)
        {
            throw new InvalidOperationException("Backups cannot contain credentials.");
        }
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(envelope.Payload)));
        if (!string.Equals(hash, envelope.Sha256, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Backup integrity validation failed.");
        }

        return JsonSerializer.Deserialize<T>(envelope.Payload)
            ?? throw new InvalidOperationException("Backup payload could not be restored.");
    }

    public ControlPlaneState EmergencyStop(
        IEnumerable<string> providers,
        DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(providers);
        string[] disconnected = providers
            .Where(provider => !string.IsNullOrWhiteSpace(provider))
            .Select(provider => provider.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(provider => provider, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return new ControlPlaneState(
            true,
            new HashSet<string>(disconnected, StringComparer.OrdinalIgnoreCase),
            new[] { $"{at:O} Emergency Stop enabled; {disconnected.Length} providers disconnected." },
            true);
    }

    public ControlPlaneState UndoEmergencyStop(
        ControlPlaneState state,
        DateTimeOffset at) =>
        !state.UndoAvailable
            ? throw new InvalidOperationException("No reversible Emergency Stop action is available.")
            : state with
            {
                EmergencyStop = false,
                Audit = state.Audit.Append($"{at:O} Emergency Stop cleared by explicit user action. Providers remain disconnected.").ToArray(),
                UndoAvailable = false
            };

    private static bool ContainsCredentialField(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                string normalizedName = new(
                    property.Name.Where(char.IsLetterOrDigit).ToArray());
                if (CredentialFieldNames.Contains(normalizedName) ||
                    ContainsCredentialField(property.Value))
                {
                    return true;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray().Any(ContainsCredentialField);
        }

        return false;
    }
}
