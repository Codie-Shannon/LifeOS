using LifeOS.Core.MicrosoftProvider;

namespace LifeOS.Core.IntegrationControlCentre;

public static class Group49ControlCentreMigration
{
    public static IntegrationControlCentreState Apply(IntegrationControlCentreState state, DateTimeOffset nowUtc)
    {
        IntegrationProviderProfile? provider = state.Providers.FirstOrDefault(p => string.Equals(p.Id, "microsoft", StringComparison.Ordinal));
        if (provider is not null)
        {
            provider.Description = "Single Microsoft provider identity for read-only Mail, Calendar, OneDrive and SharePoint metadata.";
            foreach (IntegrationProviderCapabilityDefinition capability in provider.Capabilities)
            {
                if (capability.Id == "onedrive") capability.Description = "Group 49 selected-drive and selected-folder metadata sync; no automatic body download.";
                if (capability.Id == "sharepoint") capability.Description = "Group 49 selected-site and selected-library metadata sync; no write actions.";
            }
        }

        foreach (ConnectedIntegrationAccount account in state.Accounts.Where(a => a.ProviderId == "microsoft"))
        {
            foreach (IntegrationPermissionGrant permission in account.Permissions)
            {
                if (permission.CapabilityId is "onedrive" or "sharepoint")
                {
                    permission.State = IntegrationPermissionState.Granted;
                    permission.ChangedUtc ??= nowUtc.AddMinutes(-20);
                }
            }

            foreach (IntegrationCapabilityStatus status in account.CapabilityStatuses)
            {
                if (status.CapabilityId is "onedrive" or "sharepoint")
                {
                    status.LastSyncAttemptUtc ??= nowUtc.AddMinutes(-18);
                    status.LastSuccessfulSyncUtc ??= nowUtc.AddMinutes(-18);
                    status.LastErrorUtc = null;
                    status.LastErrorCode = null;
                    status.LastErrorMessage = null;
                }
            }
        }
        return state.Normalize();
    }
}
