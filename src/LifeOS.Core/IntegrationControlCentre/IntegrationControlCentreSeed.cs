namespace LifeOS.Core.IntegrationControlCentre;

public static class IntegrationControlCentreSeed
{
    public static IntegrationControlCentreState CreateFictional(DateTimeOffset nowUtc)
    {
        IntegrationControlCentreState state = new()
        {
            Providers =
            [
                new IntegrationProviderProfile
                {
                    Id = "fictional-microsoft",
                    DisplayName = "Microsoft 365 (fictional)",
                    Description = "Proof-only provider profile. No live Microsoft account or credential is used in Group 46.",
                    IsFictional = true,
                    Capabilities =
                    [
                        Capability("mail", "Outlook Mail", "Read-only message metadata boundary for a later group."),
                        Capability("calendar", "Microsoft Calendar", "Read-only event boundary for a later group."),
                        Capability("contacts", "People / Contacts", "Planned provider capability; permission not requested."),
                        Capability("files", "OneDrive / SharePoint", "Planned provider capability; feature surface not started."),
                        Capability("teams", "Teams", "Planned provider capability; feature surface not started.")
                    ]
                },
                new IntegrationProviderProfile
                {
                    Id = "fictional-google",
                    DisplayName = "Google Workspace (fictional)",
                    Description = "Proof-only provider profile used to prove personal/work separation and lifecycle behavior.",
                    IsFictional = true,
                    Capabilities =
                    [
                        Capability("mail", "Gmail", "Read-only message boundary."),
                        Capability("calendar", "Google Calendar", "Read-only calendar boundary."),
                        Capability("contacts", "Google Contacts", "Planned; permission not requested."),
                        Capability("files", "Google Drive", "Planned; permission not requested.")
                    ]
                },
                new IntegrationProviderProfile
                {
                    Id = "local-connectors",
                    DisplayName = "Local connectors",
                    Description = "Local file and fixture connectors without external authentication.",
                    IsFictional = true,
                    Capabilities =
                    [
                        Capability("files", "Local file intake", "Review-first local evidence intake."),
                        Capability("calendar", "ICS import", "Review-first local calendar fixture intake.")
                    ]
                }
            ],
            Accounts =
            [
                new ConnectedIntegrationAccount
                {
                    Id = "northstar-work",
                    ProviderId = "fictional-microsoft",
                    DisplayName = "Northstar Operations",
                    ProviderIdentity = "work-account@example.invalid",
                    Classification = IntegrationAccountClassification.Work,
                    ConnectionState = IntegrationConnectionState.NeedsAttention,
                    LastSyncAttemptUtc = nowUtc.AddMinutes(-18),
                    LastSuccessfulSyncUtc = nowUtc.AddMinutes(-18),
                    LastErrorUtc = nowUtc.AddHours(-1),
                    LastErrorCode = "permission-required",
                    LastErrorMessage = "Fictional Files permission requires review.",
                    NextEligibleSyncUtc = nowUtc.AddMinutes(12),
                    Permissions =
                    [
                        Permission("identity-basic", "Basic identity", "identity", IntegrationPermissionRequirement.Required, IntegrationPermissionState.Granted, nowUtc.AddDays(-14)),
                        Permission("mail-read", "Mail metadata read", "mail", IntegrationPermissionRequirement.Optional, IntegrationPermissionState.Granted, nowUtc.AddDays(-14)),
                        Permission("calendar-read", "Calendar read", "calendar", IntegrationPermissionRequirement.Optional, IntegrationPermissionState.Granted, nowUtc.AddDays(-14)),
                        Permission("contacts-read", "Contacts read", "contacts", IntegrationPermissionRequirement.Optional, IntegrationPermissionState.NotRequested, null),
                        Permission("files-read", "Files read", "files", IntegrationPermissionRequirement.Required, IntegrationPermissionState.Missing, nowUtc.AddHours(-1)),
                        Permission("teams-read", "Teams read", "teams", IntegrationPermissionRequirement.Optional, IntegrationPermissionState.NotRequested, null)
                    ],
                    CapabilityStatuses =
                    [
                        Status("mail", nowUtc.AddMinutes(-18), nowUtc.AddMinutes(-18), null, null, null, nowUtc.AddMinutes(12), TimeSpan.FromHours(2), TimeSpan.FromHours(8)),
                        Status("calendar", nowUtc.AddHours(-11), nowUtc.AddHours(-11), null, null, null, nowUtc.AddMinutes(12), TimeSpan.FromHours(2), TimeSpan.FromHours(8)),
                        Status("contacts", null, null, null, null, null, null, TimeSpan.FromHours(2), TimeSpan.FromHours(8)),
                        Status("files", nowUtc.AddHours(-1), nowUtc.AddDays(-2), nowUtc.AddHours(-1), "permission-required", "Required fictional Files permission is missing.", null, TimeSpan.FromHours(2), TimeSpan.FromHours(8)),
                        Status("teams", null, null, null, null, null, null, TimeSpan.FromHours(2), TimeSpan.FromHours(8))
                    ]
                },
                new ConnectedIntegrationAccount
                {
                    Id = "alex-personal",
                    ProviderId = "fictional-google",
                    DisplayName = "Alex Morgan",
                    ProviderIdentity = "personal-account@example.invalid",
                    Classification = IntegrationAccountClassification.Personal,
                    ConnectionState = IntegrationConnectionState.Connected,
                    LastSyncAttemptUtc = nowUtc.AddMinutes(-42),
                    LastSuccessfulSyncUtc = nowUtc.AddMinutes(-42),
                    NextEligibleSyncUtc = nowUtc.AddMinutes(18),
                    Permissions =
                    [
                        Permission("identity-basic", "Basic identity", "identity", IntegrationPermissionRequirement.Required, IntegrationPermissionState.Granted, nowUtc.AddDays(-7)),
                        Permission("mail-read", "Mail metadata read", "mail", IntegrationPermissionRequirement.Optional, IntegrationPermissionState.Granted, nowUtc.AddDays(-7)),
                        Permission("calendar-read", "Calendar read", "calendar", IntegrationPermissionRequirement.Optional, IntegrationPermissionState.Granted, nowUtc.AddDays(-7)),
                        Permission("contacts-read", "Contacts read", "contacts", IntegrationPermissionRequirement.Optional, IntegrationPermissionState.Revoked, nowUtc.AddDays(-1)),
                        Permission("files-read", "Files read", "files", IntegrationPermissionRequirement.Optional, IntegrationPermissionState.NotRequested, null)
                    ],
                    CapabilityStatuses =
                    [
                        Status("mail", nowUtc.AddMinutes(-42), nowUtc.AddMinutes(-42), null, null, null, nowUtc.AddMinutes(18), TimeSpan.FromHours(2), TimeSpan.FromHours(8)),
                        Status("calendar", nowUtc.AddHours(-5), nowUtc.AddHours(-5), null, null, null, nowUtc.AddMinutes(18), TimeSpan.FromHours(2), TimeSpan.FromHours(8)),
                        Status("contacts", nowUtc.AddDays(-1), nowUtc.AddDays(-1), nowUtc.AddDays(-1), "permission-revoked", "Optional fictional Contacts permission is revoked.", null, TimeSpan.FromHours(2), TimeSpan.FromHours(8)),
                        Status("files", null, null, null, null, null, null, TimeSpan.FromHours(2), TimeSpan.FromHours(8))
                    ]
                }
            ],
            AuditEntries =
            [
                Audit(1, nowUtc.AddDays(-14), "fictional-microsoft", "northstar-work", ConnectorAuditAction.Discovered, "Fictional work account discovered.", true),
                Audit(2, nowUtc.AddDays(-14).AddMinutes(1), "fictional-microsoft", "northstar-work", ConnectorAuditAction.ConsentReviewed, "Required and optional fictional permissions reviewed.", true),
                Audit(3, nowUtc.AddDays(-14).AddMinutes(2), "fictional-microsoft", "northstar-work", ConnectorAuditAction.IdentityVerified, "Fictional work identity verified.", true),
                Audit(4, nowUtc.AddDays(-14).AddMinutes(3), "fictional-microsoft", "northstar-work", ConnectorAuditAction.InitialSyncCompleted, "First safe fictional read completed.", true),
                Audit(5, nowUtc.AddDays(-7), "fictional-google", "alex-personal", ConnectorAuditAction.Connected, "Fictional personal account connected separately.", true),
                Audit(6, nowUtc.AddDays(-1), "fictional-google", "alex-personal", ConnectorAuditAction.PermissionChanged, "Optional Contacts permission changed to Revoked.", true),
                Audit(7, nowUtc.AddHours(-1), "fictional-microsoft", "northstar-work", ConnectorAuditAction.RefreshFailed, "Files capability requires permission review; other capability freshness was retained.", false),
                Audit(8, nowUtc.AddMinutes(-18), "fictional-microsoft", "northstar-work", ConnectorAuditAction.RefreshCompleted, "Mail refreshed successfully while Files remained Needs Attention.", true)
            ],
            NextAuditSequence = 9
        };

        return state.Normalize();
    }

    private static IntegrationProviderCapabilityDefinition Capability(
        string id,
        string displayName,
        string description) => new()
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
        DateTimeOffset? changedUtc) => new()
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
        DateTimeOffset? lastAttemptUtc,
        DateTimeOffset? lastSuccessUtc,
        DateTimeOffset? lastErrorUtc,
        string? lastErrorCode,
        string? lastErrorMessage,
        DateTimeOffset? nextEligibleUtc,
        TimeSpan freshFor,
        TimeSpan staleAfter) => new()
        {
            CapabilityId = capabilityId,
            LastSyncAttemptUtc = lastAttemptUtc,
            LastSuccessfulSyncUtc = lastSuccessUtc,
            LastErrorUtc = lastErrorUtc,
            LastErrorCode = lastErrorCode,
            LastErrorMessage = lastErrorMessage,
            NextEligibleSyncUtc = nextEligibleUtc,
            FreshFor = freshFor,
            StaleAfter = staleAfter
        };

    private static ConnectorAuditEntry Audit(
        long sequence,
        DateTimeOffset timestampUtc,
        string providerId,
        string accountId,
        ConnectorAuditAction action,
        string summary,
        bool succeeded) => new()
        {
            Sequence = sequence,
            TimestampUtc = timestampUtc,
            ProviderId = providerId,
            AccountId = accountId,
            Action = action,
            Summary = summary,
            Succeeded = succeeded
        };
}
