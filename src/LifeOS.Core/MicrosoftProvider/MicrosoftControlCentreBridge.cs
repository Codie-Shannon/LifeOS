using LifeOS.Core.IntegrationControlCentre;

namespace LifeOS.Core.MicrosoftProvider;

public static class MicrosoftControlCentreBridge
{
    private const string ProviderId = "microsoft";
    private const string FictionalProviderId = "fictional-microsoft";

    public static IntegrationControlCentreState Synchronize(
        MicrosoftProviderState microsoftState,
        DateTimeOffset nowUtc,
        string? controlCentrePath = null)
    {
        ArgumentNullException.ThrowIfNull(microsoftState);

        IntegrationControlCentreState control =
            IntegrationControlCentreStore.LoadOrCreate(
                nowUtc,
                controlCentrePath);

        control.Providers.RemoveAll(provider =>
            string.Equals(
                provider.Id,
                FictionalProviderId,
                StringComparison.Ordinal));

        control.Accounts.RemoveAll(account =>
            string.Equals(
                account.ProviderId,
                FictionalProviderId,
                StringComparison.Ordinal));

        IntegrationProviderProfile? provider =
            control.Providers.FirstOrDefault(candidate =>
                string.Equals(
                    candidate.Id,
                    ProviderId,
                    StringComparison.Ordinal));

        provider ??= new IntegrationProviderProfile
        {
            Id = ProviderId
        };

        if (!control.Providers.Contains(provider))
        {
            control.Providers.Add(provider);
        }

        provider.DisplayName = "Microsoft 365";
        provider.Description =
            "Single complete LifeOS Microsoft registration. " +
            "Group 48 delivers read-only Outlook Mail and Calendar.";
        provider.IsFictional = false;
        provider.Capabilities =
        [
            Capability("outlook-mail", "Outlook Mail", "Group 48 read-only metadata and attachment metadata."),
            Capability("calendar", "Microsoft Calendar", "Group 48 read-only bounded calendar view."),
            Capability("contacts-people", "Contacts / People", "Planned; permission not requested."),
            Capability("onedrive", "OneDrive", "Planned for Group 49; permission not requested."),
            Capability("sharepoint", "SharePoint", "Planned for Group 49; permission not requested."),
            Capability("teams", "Teams", "Planned for a later group; permission not requested."),
            Capability("power-bi", "Power BI", "Planned; permission not requested."),
            Capability("power-automate", "Power Automate", "Planned; permission not requested.")
        ];

        foreach (MicrosoftProviderAccount microsoft in
                 microsoftState.Accounts)
        {
            ConnectedIntegrationAccount? account =
                control.Accounts.FirstOrDefault(candidate =>
                    string.Equals(
                        candidate.Id,
                        microsoft.Id,
                        StringComparison.Ordinal));

            account ??= new ConnectedIntegrationAccount
            {
                Id = microsoft.Id
            };

            if (!control.Accounts.Contains(account))
            {
                control.Accounts.Add(account);
            }

            account.ProviderId = ProviderId;
            account.DisplayName = microsoft.DisplayName;
            account.ProviderIdentity = microsoft.RedactedIdentity;
            account.Classification = microsoft.Classification;
            account.ConnectionState = microsoft.ConnectionState;
            account.LastSyncAttemptUtc =
                Max(microsoft.LastMailSyncUtc, microsoft.LastCalendarSyncUtc);
            account.LastSuccessfulSyncUtc =
                account.ConnectionState == IntegrationConnectionState.Connected
                    ? account.LastSyncAttemptUtc
                    : null;
            account.LastErrorUtc = microsoft.LastErrorUtc;
            account.LastErrorCode = EmptyToNull(microsoft.LastErrorCode);
            account.LastErrorMessage = EmptyToNull(microsoft.LastErrorMessage);
            account.NextEligibleSyncUtc =
                account.ConnectionState == IntegrationConnectionState.Connected
                    ? nowUtc.AddMinutes(15)
                    : null;
            account.Permissions = CreatePermissions(microsoft, nowUtc);
            account.CapabilityStatuses =
                CreateCapabilityStatuses(microsoft);
        }

        foreach (MicrosoftProviderAuditEntry entry in
                 microsoftState.AuditEntries
                     .OrderBy(entry => entry.Sequence)
                     .TakeLast(20))
        {
            string marker = $"Microsoft provider audit {entry.Sequence}: ";
            if (control.AuditEntries.Any(existing =>
                    existing.Summary.StartsWith(
                        marker,
                        StringComparison.Ordinal)))
            {
                continue;
            }

            control.AuditEntries.Add(new ConnectorAuditEntry
            {
                Sequence = control.NextAuditSequence++,
                TimestampUtc = entry.TimestampUtc,
                ProviderId = ProviderId,
                AccountId = entry.AccountId,
                Action = MapAuditAction(entry.Action),
                Summary = marker + entry.Summary,
                Succeeded = entry.Succeeded
            });
        }

        IntegrationControlCentreStore.Save(
            control,
            controlCentrePath);
        return control.Normalize();
    }

    private static List<IntegrationPermissionGrant> CreatePermissions(
        MicrosoftProviderAccount account,
        DateTimeOffset nowUtc) =>
    [
        Permission(
            "microsoft-user-read",
            "Basic identity",
            "identity",
            IntegrationPermissionRequirement.Required,
            IntegrationPermissionState.Granted,
            account.LastIdentityVerificationUtc ?? nowUtc),
        Permission(
            "microsoft-mail-read",
            "Mail read",
            "outlook-mail",
            IntegrationPermissionRequirement.Optional,
            MapPermission(account, MicrosoftProviderCapability.OutlookMail),
            account.LastIdentityVerificationUtc),
        Permission(
            "microsoft-calendar-read",
            "Calendar read",
            "calendar",
            IntegrationPermissionRequirement.Optional,
            MapPermission(account, MicrosoftProviderCapability.Calendar),
            account.LastIdentityVerificationUtc),
        Permission(
            "microsoft-contacts-read",
            "Contacts / People read",
            "contacts-people",
            IntegrationPermissionRequirement.Optional,
            IntegrationPermissionState.NotRequested,
            null),
        Permission(
            "microsoft-files-read",
            "OneDrive read",
            "onedrive",
            IntegrationPermissionRequirement.Optional,
            IntegrationPermissionState.NotRequested,
            null),
        Permission(
            "microsoft-sites-read",
            "SharePoint read",
            "sharepoint",
            IntegrationPermissionRequirement.Optional,
            IntegrationPermissionState.NotRequested,
            null),
        Permission(
            "microsoft-teams-read",
            "Teams read",
            "teams",
            IntegrationPermissionRequirement.Optional,
            IntegrationPermissionState.NotRequested,
            null),
        Permission(
            "microsoft-power-bi-read",
            "Power BI read",
            "power-bi",
            IntegrationPermissionRequirement.Optional,
            IntegrationPermissionState.NotRequested,
            null),
        Permission(
            "microsoft-power-automate",
            "Power Automate",
            "power-automate",
            IntegrationPermissionRequirement.Optional,
            IntegrationPermissionState.NotRequested,
            null)
    ];

    private static List<IntegrationCapabilityStatus>
        CreateCapabilityStatuses(
            MicrosoftProviderAccount account) =>
    [
        Status(
            "outlook-mail",
            account.LastMailSyncUtc,
            account.LastMailSyncUtc,
            account.LastErrorUtc,
            account.LastErrorCode,
            account.LastErrorMessage),
        Status(
            "calendar",
            account.LastCalendarSyncUtc,
            account.LastCalendarSyncUtc,
            account.LastErrorUtc,
            account.LastErrorCode,
            account.LastErrorMessage),
        Status("contacts-people", null, null, null, null, null),
        Status("onedrive", null, null, null, null, null),
        Status("sharepoint", null, null, null, null, null),
        Status("teams", null, null, null, null, null),
        Status("power-bi", null, null, null, null, null),
        Status("power-automate", null, null, null, null, null)
    ];

    private static IntegrationPermissionState MapPermission(
        MicrosoftProviderAccount account,
        MicrosoftProviderCapability capability) =>
        account.Permissions.GetValueOrDefault(
            capability,
            MicrosoftProviderPermissionState.NotRequested) switch
        {
            MicrosoftProviderPermissionState.Granted =>
                IntegrationPermissionState.Granted,
            MicrosoftProviderPermissionState.Missing =>
                IntegrationPermissionState.Missing,
            MicrosoftProviderPermissionState.Revoked =>
                IntegrationPermissionState.Revoked,
            _ =>
                IntegrationPermissionState.NotRequested
        };

    private static ConnectorAuditAction MapAuditAction(
        MicrosoftProviderAuditAction action) =>
        action switch
        {
            MicrosoftProviderAuditAction.ConsentStarted =>
                ConnectorAuditAction.ConsentReviewed,
            MicrosoftProviderAuditAction.IdentityVerified =>
                ConnectorAuditAction.IdentityVerified,
            MicrosoftProviderAuditAction.InitialMailReadVerified or
            MicrosoftProviderAuditAction.InitialCalendarReadVerified =>
                ConnectorAuditAction.InitialSyncCompleted,
            MicrosoftProviderAuditAction.SyncCompleted =>
                ConnectorAuditAction.RefreshCompleted,
            MicrosoftProviderAuditAction.SyncPartiallyCompleted or
            MicrosoftProviderAuditAction.SyncFailed =>
                ConnectorAuditAction.RefreshFailed,
            MicrosoftProviderAuditAction.TokenRefreshed =>
                ConnectorAuditAction.Reconnected,
            MicrosoftProviderAuditAction.ConsentRevoked =>
                ConnectorAuditAction.Revoked,
            MicrosoftProviderAuditAction.Disconnected =>
                ConnectorAuditAction.Disconnected,
            _ =>
                ConnectorAuditAction.Discovered
        };

    private static IntegrationProviderCapabilityDefinition Capability(
        string id,
        string displayName,
        string description) =>
        new()
        {
            Id = id,
            DisplayName = displayName,
            Description = description,
            IsPlanned = true
        };

    private static IntegrationPermissionGrant Permission(
        string id,
        string displayName,
        string capabilityId,
        IntegrationPermissionRequirement requirement,
        IntegrationPermissionState state,
        DateTimeOffset? changedUtc) =>
        new()
        {
            Id = id,
            DisplayName = displayName,
            CapabilityId = capabilityId,
            Requirement = requirement,
            State = state,
            ChangedUtc = changedUtc
        };

    private static IntegrationCapabilityStatus Status(
        string capabilityId,
        DateTimeOffset? attemptUtc,
        DateTimeOffset? successUtc,
        DateTimeOffset? errorUtc,
        string? errorCode,
        string? errorMessage) =>
        new()
        {
            CapabilityId = capabilityId,
            LastSyncAttemptUtc = attemptUtc,
            LastSuccessfulSyncUtc = successUtc,
            LastErrorUtc = errorUtc,
            LastErrorCode = EmptyToNull(errorCode),
            LastErrorMessage = EmptyToNull(errorMessage),
            NextEligibleSyncUtc = successUtc?.AddMinutes(15),
            FreshFor = TimeSpan.FromHours(2),
            StaleAfter = TimeSpan.FromHours(8)
        };

    private static DateTimeOffset? Max(
        DateTimeOffset? left,
        DateTimeOffset? right) =>
        left is null
            ? right
            : right is null
                ? left
                : left > right
                    ? left
                    : right;

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
}
