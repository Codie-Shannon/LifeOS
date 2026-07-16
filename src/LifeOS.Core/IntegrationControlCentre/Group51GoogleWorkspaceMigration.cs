namespace LifeOS.Core.IntegrationControlCentre;

public static class Group51GoogleWorkspaceMigration
{
    private static readonly string[] CapabilityIds =
    [
        "gmail",
        "google-calendar",
        "google-drive",
        "google-contacts",
        "google-tasks"
    ];

    public static IntegrationControlCentreState Apply(
        IntegrationControlCentreState state,
        DateTimeOffset nowUtc)
    {
        IntegrationProviderProfile? provider = state.Providers.FirstOrDefault(
            item => item.Id is "google" or "fictional-google");

        if (provider is null)
        {
            provider = new IntegrationProviderProfile();
            state.Providers.Add(provider);
        }

        provider.Id = "google";
        provider.DisplayName = "Google Workspace";
        provider.Description =
            "One complete LifeOS Google Cloud project for Gmail, Calendar, Drive, Contacts and Tasks read-only.";
        provider.IsFictional = false;
        provider.Capabilities =
        [
            Capability("gmail", "Gmail", "Bounded label and date-window message/thread metadata."),
            Capability("google-calendar", "Google Calendar", "Selected-calendar and date-window event metadata."),
            Capability("google-drive", "Google Drive", "Selected-folder metadata with no automatic body download."),
            Capability("google-contacts", "Google Contacts", "People API read-only contact candidates."),
            Capability("google-tasks", "Google Tasks", "Selected-list read-only task candidates.")
        ];

        ConnectedIntegrationAccount? account = state.Accounts.FirstOrDefault(
            item => item.ProviderId is "google" or "fictional-google" ||
                    item.Id == "alex-personal");

        if (account is null)
        {
            account = new ConnectedIntegrationAccount();
            state.Accounts.Add(account);
        }

        account.Id = "google-personal";
        account.ProviderId = "google";
        account.DisplayName = "Codie Shannon";
        account.ProviderIdentity = "Identity verified · address redacted";
        account.Classification = IntegrationAccountClassification.Personal;
        account.ConnectionState = IntegrationConnectionState.Connected;
        account.LastSyncAttemptUtc = nowUtc.AddMinutes(-9);
        account.LastSuccessfulSyncUtc = nowUtc.AddMinutes(-9);
        account.LastErrorUtc = null;
        account.LastErrorCode = null;
        account.LastErrorMessage = null;
        account.NextEligibleSyncUtc = nowUtc.AddMinutes(21);
        account.Permissions = CapabilityIds
            .Select(id => new IntegrationPermissionGrant
            {
                Id = $"{id}-readonly",
                DisplayName = $"{DisplayName(id)} read-only",
                CapabilityId = id,
                Requirement = IntegrationPermissionRequirement.Optional,
                State = IntegrationPermissionState.Granted,
                ChangedUtc = nowUtc.AddMinutes(-30)
            })
            .ToList();
        account.CapabilityStatuses = CapabilityIds
            .Select(id => new IntegrationCapabilityStatus
            {
                CapabilityId = id,
                LastSyncAttemptUtc = nowUtc.AddMinutes(-9),
                LastSuccessfulSyncUtc = nowUtc.AddMinutes(-9),
                NextEligibleSyncUtc = nowUtc.AddMinutes(21),
                FreshFor = TimeSpan.FromHours(2),
                StaleAfter = TimeSpan.FromHours(8)
            })
            .ToList();

        return state.Normalize();
    }

    private static IntegrationProviderCapabilityDefinition Capability(
        string id,
        string name,
        string description) => new()
        {
            Id = id,
            DisplayName = name,
            Description = description,
            IsPlanned = false
        };

    private static string DisplayName(string id) => id switch
    {
        "gmail" => "Gmail",
        "google-calendar" => "Google Calendar",
        "google-drive" => "Google Drive",
        "google-contacts" => "Google Contacts",
        "google-tasks" => "Google Tasks",
        _ => id
    };
}
