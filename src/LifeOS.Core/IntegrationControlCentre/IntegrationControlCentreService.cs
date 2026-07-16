namespace LifeOS.Core.IntegrationControlCentre;

public sealed class IntegrationControlCentreService
{
    public IntegrationControlCentreService(IntegrationControlCentreState state)
    {
        State = state.Normalize();
    }

    public IntegrationControlCentreState State { get; }

    public ConnectedIntegrationAccount GetRequiredAccount(string accountId) =>
        State.Accounts.Single(account =>
            string.Equals(account.Id, accountId, StringComparison.Ordinal));

    public IntegrationProviderProfile GetRequiredProvider(string providerId) =>
        State.Providers.Single(provider =>
            string.Equals(provider.Id, providerId, StringComparison.Ordinal));

    public void Refresh(string accountId, DateTimeOffset nowUtc)
    {
        ConnectedIntegrationAccount account = GetRequiredAccount(accountId);

        if (account.ConnectionState is
            IntegrationConnectionState.Revoked or
            IntegrationConnectionState.Expired or
            IntegrationConnectionState.NotConnected or
            IntegrationConnectionState.Connecting or
            IntegrationConnectionState.Disconnected)
        {
            AppendAudit(
                account,
                null,
                ConnectorAuditAction.RefreshFailed,
                "Refresh was blocked because a connected and verified account is required.",
                succeeded: false,
                nowUtc);
            return;
        }

        account.LastSyncAttemptUtc = nowUtc;
        account.LastSuccessfulSyncUtc = nowUtc;
        account.LastErrorUtc = null;
        account.LastErrorCode = null;
        account.LastErrorMessage = null;
        account.NextEligibleSyncUtc = nowUtc.AddMinutes(15);

        foreach (IntegrationCapabilityStatus capability in account.CapabilityStatuses)
        {
            bool requiredPermissionProblem = account.Permissions.Any(permission =>
                permission.CapabilityId == capability.CapabilityId
                && permission.Requirement == IntegrationPermissionRequirement.Required
                && permission.State != IntegrationPermissionState.Granted);

            capability.LastSyncAttemptUtc = nowUtc;
            capability.NextEligibleSyncUtc = nowUtc.AddMinutes(15);

            if (requiredPermissionProblem)
            {
                capability.LastErrorUtc = nowUtc;
                capability.LastErrorCode = "permission-required";
                capability.LastErrorMessage = "Required fictional permission is not granted.";
            }
            else
            {
                capability.LastSuccessfulSyncUtc = nowUtc;
                capability.LastErrorUtc = null;
                capability.LastErrorCode = null;
                capability.LastErrorMessage = null;
            }
        }

        account.ConnectionState = account.CapabilityStatuses.Any(status =>
            !string.IsNullOrWhiteSpace(status.LastErrorCode))
            ? IntegrationConnectionState.NeedsAttention
            : IntegrationConnectionState.Connected;

        AppendAudit(
            account,
            null,
            ConnectorAuditAction.RefreshCompleted,
            "Manual fictional refresh completed; capability health was recalculated.",
            succeeded: true,
            nowUtc);
    }

    public void Reconnect(string accountId, DateTimeOffset nowUtc)
    {
        ConnectedIntegrationAccount account = GetRequiredAccount(accountId);

        IntegrationConnectionStateMachine.Transition(
            account,
            IntegrationConnectionState.Connecting);

        AppendAudit(
            account,
            null,
            ConnectorAuditAction.ReconnectStarted,
            "Reconnect review confirmed for fictional account.",
            succeeded: true,
            nowUtc);

        IntegrationConnectionStateMachine.Transition(
            account,
            IntegrationConnectionState.Connected);

        account.LastErrorUtc = null;
        account.LastErrorCode = null;
        account.LastErrorMessage = null;
        account.LastSuccessfulSyncUtc = nowUtc;
        account.NextEligibleSyncUtc = nowUtc.AddMinutes(15);

        foreach (IntegrationCapabilityStatus capability in account.CapabilityStatuses)
        {
            capability.LastSyncAttemptUtc = nowUtc;
            capability.LastSuccessfulSyncUtc = nowUtc;
            capability.LastErrorUtc = null;
            capability.LastErrorCode = null;
            capability.LastErrorMessage = null;
            capability.NextEligibleSyncUtc = nowUtc.AddMinutes(15);
        }

        AppendAudit(
            account,
            null,
            ConnectorAuditAction.Reconnected,
            "Fictional identity, permission catalogue and first safe read were re-verified.",
            succeeded: true,
            nowUtc.AddMilliseconds(1));
    }

    public void Revoke(string accountId, DateTimeOffset nowUtc)
    {
        ConnectedIntegrationAccount account = GetRequiredAccount(accountId);
        IntegrationConnectionStateMachine.Transition(account, IntegrationConnectionState.Revoked);

        foreach (IntegrationPermissionGrant permission in account.Permissions.Where(permission =>
                     permission.State == IntegrationPermissionState.Granted))
        {
            permission.State = IntegrationPermissionState.Revoked;
            permission.ChangedUtc = nowUtc;
        }

        account.LastErrorUtc = nowUtc;
        account.LastErrorCode = "consent-revoked";
        account.LastErrorMessage = "Fictional provider consent was revoked.";
        account.NextEligibleSyncUtc = null;

        AppendAudit(
            account,
            null,
            ConnectorAuditAction.Revoked,
            "Fictional consent was revoked. No accepted LifeOS records were deleted.",
            succeeded: true,
            nowUtc);
    }

    public void Disconnect(
        string accountId,
        DisconnectRetentionChoice retentionChoice,
        DateTimeOffset nowUtc)
    {
        ConnectedIntegrationAccount account = GetRequiredAccount(accountId);

        if (account.ConnectionState != IntegrationConnectionState.Disconnected)
        {
            IntegrationConnectionStateMachine.Transition(
                account,
                IntegrationConnectionState.Disconnected);
        }

        account.NextEligibleSyncUtc = null;

        string retentionSummary = retentionChoice switch
        {
            DisconnectRetentionChoice.KeepAcceptedLifeOsRecords =>
                "Accepted LifeOS records were kept; provider links remain reviewable.",
            DisconnectRetentionChoice.ArchiveProviderLinks =>
                "Accepted records were kept and provider links were archived.",
            _ =>
                "Unaccepted imported candidates were selected for removal; accepted records remain protected."
        };

        AppendAudit(
            account,
            null,
            ConnectorAuditAction.Disconnected,
            retentionSummary,
            succeeded: true,
            nowUtc);
    }

    public void ChangePermission(
        string accountId,
        string permissionId,
        IntegrationPermissionState newState,
        DateTimeOffset nowUtc)
    {
        ConnectedIntegrationAccount account = GetRequiredAccount(accountId);
        IntegrationPermissionGrant permission = account.Permissions.Single(candidate =>
            string.Equals(candidate.Id, permissionId, StringComparison.Ordinal));

        permission.State = newState;
        permission.ChangedUtc = nowUtc;

        AppendAudit(
            account,
            permission.CapabilityId,
            ConnectorAuditAction.PermissionChanged,
            $"Permission {permission.DisplayName} changed to {newState}.",
            succeeded: true,
            nowUtc);
    }

    private void AppendAudit(
        ConnectedIntegrationAccount account,
        string? capabilityId,
        ConnectorAuditAction action,
        string summary,
        bool succeeded,
        DateTimeOffset timestampUtc)
    {
        State.AuditEntries.Add(new ConnectorAuditEntry
        {
            Sequence = State.NextAuditSequence++,
            TimestampUtc = timestampUtc,
            ProviderId = account.ProviderId,
            AccountId = account.Id,
            CapabilityId = capabilityId,
            Action = action,
            Summary = summary,
            Succeeded = succeeded
        });
    }
}
