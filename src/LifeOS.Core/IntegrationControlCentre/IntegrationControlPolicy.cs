namespace LifeOS.Core.IntegrationControlCentre;

public static class IntegrationConnectionStateMachine
{
    private static readonly IReadOnlyDictionary<IntegrationConnectionState, IntegrationConnectionState[]> AllowedTransitions =
        new Dictionary<IntegrationConnectionState, IntegrationConnectionState[]>
        {
            [IntegrationConnectionState.NotConnected] =
                [IntegrationConnectionState.Connecting, IntegrationConnectionState.Disconnected],
            [IntegrationConnectionState.Connecting] =
                [IntegrationConnectionState.Connected, IntegrationConnectionState.NeedsAttention, IntegrationConnectionState.Offline, IntegrationConnectionState.Disconnected],
            [IntegrationConnectionState.Connected] =
                [IntegrationConnectionState.Connecting, IntegrationConnectionState.NeedsAttention, IntegrationConnectionState.Expired, IntegrationConnectionState.Revoked, IntegrationConnectionState.Offline, IntegrationConnectionState.Disconnected],
            [IntegrationConnectionState.NeedsAttention] =
                [IntegrationConnectionState.Connecting, IntegrationConnectionState.Connected, IntegrationConnectionState.Expired, IntegrationConnectionState.Revoked, IntegrationConnectionState.Offline, IntegrationConnectionState.Disconnected],
            [IntegrationConnectionState.Expired] =
                [IntegrationConnectionState.Connecting, IntegrationConnectionState.Disconnected],
            [IntegrationConnectionState.Revoked] =
                [IntegrationConnectionState.Connecting, IntegrationConnectionState.Disconnected],
            [IntegrationConnectionState.Offline] =
                [IntegrationConnectionState.Connecting, IntegrationConnectionState.Connected, IntegrationConnectionState.NeedsAttention, IntegrationConnectionState.Disconnected],
            [IntegrationConnectionState.Disconnected] =
                [IntegrationConnectionState.Connecting, IntegrationConnectionState.NotConnected]
        };

    public static bool CanTransition(
        IntegrationConnectionState from,
        IntegrationConnectionState to)
    {
        if (from == to)
        {
            return true;
        }

        return AllowedTransitions.TryGetValue(from, out IntegrationConnectionState[]? allowed)
            && allowed.Contains(to);
    }

    public static void Transition(
        ConnectedIntegrationAccount account,
        IntegrationConnectionState to)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (!CanTransition(account.ConnectionState, to))
        {
            throw new InvalidOperationException(
                $"Connection state cannot move from {account.ConnectionState} to {to}.");
        }

        account.ConnectionState = to;
    }
}

public static class IntegrationFreshnessCalculator
{
    public static IntegrationFreshnessState Calculate(
        IntegrationConnectionState connectionState,
        DateTimeOffset? lastSuccessfulSyncUtc,
        DateTimeOffset nowUtc,
        TimeSpan freshFor,
        TimeSpan staleAfter)
    {
        return connectionState switch
        {
            IntegrationConnectionState.Revoked => IntegrationFreshnessState.Revoked,
            IntegrationConnectionState.Expired => IntegrationFreshnessState.Expired,
            IntegrationConnectionState.Offline => IntegrationFreshnessState.Offline,
            _ => CalculateFromTimestamp(lastSuccessfulSyncUtc, nowUtc, freshFor, staleAfter)
        };
    }

    private static IntegrationFreshnessState CalculateFromTimestamp(
        DateTimeOffset? lastSuccessfulSyncUtc,
        DateTimeOffset nowUtc,
        TimeSpan freshFor,
        TimeSpan staleAfter)
    {
        if (lastSuccessfulSyncUtc is null)
        {
            return IntegrationFreshnessState.NeverSynchronized;
        }

        TimeSpan age = nowUtc - lastSuccessfulSyncUtc.Value;

        if (age <= freshFor)
        {
            return IntegrationFreshnessState.Fresh;
        }

        return age <= staleAfter
            ? IntegrationFreshnessState.Ageing
            : IntegrationFreshnessState.Stale;
    }
}

public static class IntegrationCapabilityHealthCalculator
{
    public static IntegrationCapabilityHealthState Calculate(
        ConnectedIntegrationAccount account,
        IntegrationCapabilityStatus capability,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(capability);

        IntegrationFreshnessState freshness = IntegrationFreshnessCalculator.Calculate(
            account.ConnectionState,
            capability.LastSuccessfulSyncUtc,
            nowUtc,
            capability.FreshFor,
            capability.StaleAfter);

        if (account.ConnectionState == IntegrationConnectionState.Revoked)
        {
            return IntegrationCapabilityHealthState.Revoked;
        }

        if (account.ConnectionState == IntegrationConnectionState.Expired)
        {
            return IntegrationCapabilityHealthState.Expired;
        }

        if (account.ConnectionState == IntegrationConnectionState.Offline)
        {
            return IntegrationCapabilityHealthState.Offline;
        }

        bool permissionProblem = account.Permissions.Any(permission =>
            string.Equals(permission.CapabilityId, capability.CapabilityId, StringComparison.Ordinal)
            && permission.Requirement == IntegrationPermissionRequirement.Required
            && permission.State is IntegrationPermissionState.Missing or IntegrationPermissionState.Revoked);

        if (permissionProblem || !string.IsNullOrWhiteSpace(capability.LastErrorCode))
        {
            return IntegrationCapabilityHealthState.NeedsAttention;
        }

        return freshness switch
        {
            IntegrationFreshnessState.Fresh => IntegrationCapabilityHealthState.Healthy,
            IntegrationFreshnessState.Ageing => IntegrationCapabilityHealthState.Ageing,
            IntegrationFreshnessState.Stale => IntegrationCapabilityHealthState.Stale,
            IntegrationFreshnessState.Offline => IntegrationCapabilityHealthState.Offline,
            IntegrationFreshnessState.Expired => IntegrationCapabilityHealthState.Expired,
            IntegrationFreshnessState.Revoked => IntegrationCapabilityHealthState.Revoked,
            _ => IntegrationCapabilityHealthState.NotAvailable
        };
    }
}
