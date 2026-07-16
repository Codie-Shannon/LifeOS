namespace LifeOS.Core.IntegrationControlCentre;

public interface IIntegrationProviderAdapter
{
    string ProviderId { get; }

    Task<IReadOnlyList<ConnectorIdentity>> DiscoverAccountsAsync(
        CancellationToken cancellationToken = default);

    Task<ConnectorOperationResult> VerifyIdentityAsync(
        string providerAccountId,
        CancellationToken cancellationToken = default);

    Task<ConnectorCapabilityReadResult> ReadCapabilityAsync(
        string providerAccountId,
        string capabilityId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
}

public sealed class FictionalIntegrationProviderAdapter : IIntegrationProviderAdapter
{
    private readonly IReadOnlyList<ConnectorIdentity> _identities;

    public FictionalIntegrationProviderAdapter(
        string providerId,
        IReadOnlyList<ConnectorIdentity> identities)
    {
        ProviderId = providerId;
        _identities = identities;
    }

    public string ProviderId { get; }

    public Task<IReadOnlyList<ConnectorIdentity>> DiscoverAccountsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_identities);
    }

    public Task<ConnectorOperationResult> VerifyIdentityAsync(
        string providerAccountId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool exists = _identities.Any(identity =>
            string.Equals(identity.ProviderAccountId, providerAccountId, StringComparison.Ordinal));

        return Task.FromResult(new ConnectorOperationResult(
            exists,
            exists ? "Fictional identity verified." : "Fictional identity was not found.",
            DateTimeOffset.UtcNow));
    }

    public Task<ConnectorCapabilityReadResult> ReadCapabilityAsync(
        string providerAccountId,
        string capabilityId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool exists = _identities.Any(identity =>
            string.Equals(identity.ProviderAccountId, providerAccountId, StringComparison.Ordinal));

        return Task.FromResult(new ConnectorCapabilityReadResult(
            capabilityId,
            exists,
            nowUtc.AddMinutes(-2),
            nowUtc,
            exists ? 3 : 0,
            exists ? "Fictional bounded read completed." : "Fictional account was not found."));
    }
}
