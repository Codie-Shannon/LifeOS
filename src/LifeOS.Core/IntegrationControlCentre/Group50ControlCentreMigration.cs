namespace LifeOS.Core.IntegrationControlCentre;

public static class Group50ControlCentreMigration
{
    public static IntegrationControlCentreState Apply(
        IntegrationControlCentreState state,
        DateTimeOffset nowUtc)
    {
        IntegrationProviderProfile? provider = state.Providers.FirstOrDefault(
            item => string.Equals(item.Id, "microsoft", StringComparison.Ordinal));

        if (provider is not null)
        {
            provider.Description =
                "Single Microsoft provider identity for read-only Mail, Calendar, Files and Teams context.";

            IntegrationProviderCapabilityDefinition? teams =
                provider.Capabilities.FirstOrDefault(capability =>
                    capability.Id == "teams");

            if (teams is not null)
            {
                teams.Description =
                    "Group 50 selected-team, selected-channel and bounded message/meeting context; no Teams writes.";
            }
        }

        foreach (ConnectedIntegrationAccount account in
                 state.Accounts.Where(item => item.ProviderId == "microsoft"))
        {
            foreach (IntegrationPermissionGrant permission in account.Permissions)
            {
                if (permission.CapabilityId == "teams")
                {
                    permission.State = IntegrationPermissionState.Granted;
                    permission.ChangedUtc ??= nowUtc.AddMinutes(-30);
                }
            }

            foreach (IntegrationCapabilityStatus status in
                     account.CapabilityStatuses)
            {
                if (status.CapabilityId == "teams")
                {
                    status.LastSyncAttemptUtc ??= nowUtc.AddMinutes(-12);
                    status.LastSuccessfulSyncUtc ??= nowUtc.AddMinutes(-12);
                    status.LastErrorUtc = null;
                    status.LastErrorCode = null;
                    status.LastErrorMessage = null;
                }
            }
        }

        return state.Normalize();
    }
}
