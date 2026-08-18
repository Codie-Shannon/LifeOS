using LifeOS.Core.Forms;

namespace LifeOS.Core.LocalFirstSync;

public enum LocalAccountMode
{
    LocalOnly = 0,
    ManualTransfer = 10
}

public enum LocalSyncState
{
    LocalOnly = 0,
    ManualTransferReady = 10,
    Paused = 20
}

public sealed record LocalAccountDraft(string? DisplayName, string? DeviceLabel, LocalAccountMode Mode);

public sealed record LocalAccountSyncProfile
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string DeviceLabel { get; init; } = string.Empty;
    public LocalAccountMode Mode { get; init; }
    public LocalSyncState State { get; init; }
    public int PendingLocalChanges { get; init; }
    public DateTimeOffset? LastManualTransferUtc { get; init; }
    public bool ProviderConfigured { get; init; }
    public bool BackgroundSyncEnabled { get; init; }
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset UpdatedUtc { get; init; }
}

public static class LocalFirstSyncService
{
    public static FormValidationResult Validate(LocalAccountDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Field(issues, "local-account-name", draft.DisplayName, "Display name", 80);
        Field(issues, "local-device-label", draft.DeviceLabel, "Device label", 80);
        return new FormValidationResult(issues);
    }

    public static LocalAccountSyncProfile Create(LocalAccountDraft draft, DateTimeOffset now)
    {
        if (!Validate(draft).IsValid)
        {
            throw new ArgumentException("The local account profile is invalid.", nameof(draft));
        }

        return new LocalAccountSyncProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = draft.DisplayName!.Trim(),
            DeviceLabel = draft.DeviceLabel!.Trim(),
            Mode = draft.Mode,
            State = DefaultState(draft.Mode),
            ProviderConfigured = false,
            BackgroundSyncEnabled = false,
            CreatedUtc = now,
            UpdatedUtc = now
        };
    }

    public static LocalAccountSyncProfile RegisterLocalChange(LocalAccountSyncProfile profile, DateTimeOffset now)
    {
        LocalAccountSyncProfile normalized = Normalize(profile);
        if (normalized.State == LocalSyncState.Paused)
        {
            throw new InvalidOperationException("Local change registration is paused.");
        }

        return normalized with
        {
            PendingLocalChanges = checked(normalized.PendingLocalChanges + 1),
            UpdatedUtc = now
        };
    }

    public static LocalAccountSyncProfile RecordManualTransfer(LocalAccountSyncProfile profile, DateTimeOffset now)
    {
        LocalAccountSyncProfile normalized = Normalize(profile);
        if (normalized.Mode != LocalAccountMode.ManualTransfer || normalized.State == LocalSyncState.Paused)
        {
            throw new InvalidOperationException("A manual transfer checkpoint is not available in the current state.");
        }

        return normalized with
        {
            PendingLocalChanges = 0,
            LastManualTransferUtc = now,
            UpdatedUtc = now
        };
    }

    public static LocalAccountSyncProfile SetPaused(LocalAccountSyncProfile profile, bool paused, DateTimeOffset now)
    {
        LocalAccountSyncProfile normalized = Normalize(profile);
        LocalSyncState next = paused ? LocalSyncState.Paused : DefaultState(normalized.Mode);
        if (next == normalized.State)
        {
            throw new InvalidOperationException("The local sync state is already selected.");
        }

        return normalized with { State = next, UpdatedUtc = now };
    }

    public static LocalAccountSyncProfile Normalize(LocalAccountSyncProfile profile)
    {
        if (profile.Id == Guid.Empty) throw new InvalidDataException("The local account id is invalid.");
        string displayName = RequiredLine(profile.DisplayName, "display name", 80);
        string deviceLabel = RequiredLine(profile.DeviceLabel, "device label", 80);
        if (profile.PendingLocalChanges < 0) throw new InvalidDataException("Pending local changes cannot be negative.");
        if (profile.ProviderConfigured || profile.BackgroundSyncEnabled) throw new InvalidDataException("Provider and background sync require a future configured release.");
        if (profile.State != LocalSyncState.Paused && profile.State != DefaultState(profile.Mode)) throw new InvalidDataException("The local sync state does not match its mode.");
        if (profile.CreatedUtc == default || profile.UpdatedUtc == default) throw new InvalidDataException("Local account timestamps are required.");
        return profile with { DisplayName = displayName, DeviceLabel = deviceLabel };
    }

    private static LocalSyncState DefaultState(LocalAccountMode mode) => mode switch
    {
        LocalAccountMode.LocalOnly => LocalSyncState.LocalOnly,
        LocalAccountMode.ManualTransfer => LocalSyncState.ManualTransferReady,
        _ => throw new InvalidDataException("The local account mode is invalid.")
    };

    private static void Field(ICollection<FormFieldIssue> issues, string id, string? value, string label, int maximum)
    {
        Add(issues, FormValidation.Required(id, value, label));
        Add(issues, FormValidation.MaximumLength(id, value, label, maximum));
        Add(issues, FormValidation.SingleLine(id, value, label));
    }

    private static string RequiredLine(string? value, string label, int maximum)
    {
        string normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        if (normalized.Length == 0 || normalized.Length > maximum || normalized.Any(char.IsControl))
        {
            throw new InvalidDataException($"The local account {label} is invalid.");
        }
        return normalized;
    }

    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue)
    {
        if (issue is not null) issues.Add(issue);
    }
}
