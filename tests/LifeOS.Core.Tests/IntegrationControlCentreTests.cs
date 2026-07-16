using System.Text.Json;
using LifeOS.Core.IntegrationControlCentre;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class IntegrationControlCentreTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 16, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FictionalSeed_CreatesPermanentProviderAndAccountModel()
    {
        IntegrationControlCentreState state = IntegrationControlCentreSeed.CreateFictional(Now);

        Assert.Equal(3, state.Providers.Count);
        Assert.Equal(2, state.Accounts.Count);
        Assert.All(state.Providers, provider => Assert.True(provider.IsFictional));
        Assert.Contains(state.Providers, provider =>
            provider.Id == "fictional-microsoft" &&
            provider.Capabilities.Any(capability => capability.Id == "mail") &&
            provider.Capabilities.Any(capability => capability.Id == "calendar"));
        Assert.Contains(state.Accounts, account => account.Classification == IntegrationAccountClassification.Personal);
        Assert.Contains(state.Accounts, account => account.Classification == IntegrationAccountClassification.Work);
    }

    [Fact]
    public void Store_RoundTripsProviderAccountCapabilityAndAuditState()
    {
        string path = CreateTemporaryPath();

        try
        {
            IntegrationControlCentreState expected = IntegrationControlCentreSeed.CreateFictional(Now);
            IntegrationControlCentreStore.Save(expected, path);

            IntegrationControlCentreState actual = IntegrationControlCentreStore.LoadOrCreate(Now, path);

            Assert.Equal(expected.SchemaVersion, actual.SchemaVersion);
            Assert.Equal(expected.Providers.Count, actual.Providers.Count);
            Assert.Equal(expected.Accounts.Count, actual.Accounts.Count);
            Assert.Equal(expected.AuditEntries.Count, actual.AuditEntries.Count);
            Assert.Equal(
                expected.Accounts.Single(account => account.Id == "northstar-work").Permissions.Count,
                actual.Accounts.Single(account => account.Id == "northstar-work").Permissions.Count);
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    [Fact]
    public void CorruptStore_FailsClosedToFictionalSeed()
    {
        string path = CreateTemporaryPath();

        try
        {
            File.WriteAllText(path, "{ not valid json");

            IntegrationControlCentreState state = IntegrationControlCentreStore.LoadOrCreate(Now, path);

            Assert.NotEmpty(state.Providers);
            Assert.All(state.Providers, provider => Assert.True(provider.IsFictional));
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    [Theory]
    [InlineData(IntegrationConnectionState.NotConnected, IntegrationConnectionState.Connecting, true)]
    [InlineData(IntegrationConnectionState.Connected, IntegrationConnectionState.Connecting, true)]
    [InlineData(IntegrationConnectionState.Connected, IntegrationConnectionState.Revoked, true)]
    [InlineData(IntegrationConnectionState.Revoked, IntegrationConnectionState.Connecting, true)]
    [InlineData(IntegrationConnectionState.Disconnected, IntegrationConnectionState.Connecting, true)]
    [InlineData(IntegrationConnectionState.NotConnected, IntegrationConnectionState.Connected, false)]
    [InlineData(IntegrationConnectionState.Revoked, IntegrationConnectionState.Connected, false)]
    [InlineData(IntegrationConnectionState.Disconnected, IntegrationConnectionState.Revoked, false)]
    public void ConnectionStateMachine_ValidatesTransitions(
        IntegrationConnectionState from,
        IntegrationConnectionState to,
        bool expected)
    {
        Assert.Equal(expected, IntegrationConnectionStateMachine.CanTransition(from, to));
    }

    [Fact]
    public void ConnectionStateMachine_IsIdempotentForSameState()
    {
        ConnectedIntegrationAccount account = new()
        {
            ConnectionState = IntegrationConnectionState.Connected
        };

        IntegrationConnectionStateMachine.Transition(account, IntegrationConnectionState.Connected);

        Assert.Equal(IntegrationConnectionState.Connected, account.ConnectionState);
    }

    [Fact]
    public void PermissionCatalogue_SeparatesRequirementFromConsentState()
    {
        ConnectedIntegrationAccount account = IntegrationControlCentreSeed
            .CreateFictional(Now)
            .Accounts
            .Single(candidate => candidate.Id == "northstar-work");

        Assert.Contains(account.Permissions, permission =>
            permission.Requirement == IntegrationPermissionRequirement.Required &&
            permission.State == IntegrationPermissionState.Granted);
        Assert.Contains(account.Permissions, permission =>
            permission.Requirement == IntegrationPermissionRequirement.Required &&
            permission.State == IntegrationPermissionState.Missing);
        Assert.Contains(account.Permissions, permission =>
            permission.Requirement == IntegrationPermissionRequirement.Optional &&
            permission.State == IntegrationPermissionState.NotRequested);
    }

    [Fact]
    public void CapabilityHealth_IsCalculatedPerCapability()
    {
        IntegrationControlCentreState state = IntegrationControlCentreSeed.CreateFictional(Now);
        ConnectedIntegrationAccount account = state.Accounts.Single(candidate => candidate.Id == "northstar-work");
        IntegrationCapabilityStatus mail = account.CapabilityStatuses.Single(status => status.CapabilityId == "mail");
        IntegrationCapabilityStatus calendar = account.CapabilityStatuses.Single(status => status.CapabilityId == "calendar");
        IntegrationCapabilityStatus files = account.CapabilityStatuses.Single(status => status.CapabilityId == "files");

        Assert.Equal(
            IntegrationCapabilityHealthState.Healthy,
            IntegrationCapabilityHealthCalculator.Calculate(account, mail, Now));
        Assert.Equal(
            IntegrationCapabilityHealthState.Stale,
            IntegrationCapabilityHealthCalculator.Calculate(account, calendar, Now));
        Assert.Equal(
            IntegrationCapabilityHealthState.NeedsAttention,
            IntegrationCapabilityHealthCalculator.Calculate(account, files, Now));
    }

    [Theory]
    [InlineData(IntegrationConnectionState.Connected, -1, IntegrationFreshnessState.Fresh)]
    [InlineData(IntegrationConnectionState.Connected, -4, IntegrationFreshnessState.Ageing)]
    [InlineData(IntegrationConnectionState.Connected, -12, IntegrationFreshnessState.Stale)]
    [InlineData(IntegrationConnectionState.Offline, -1, IntegrationFreshnessState.Offline)]
    [InlineData(IntegrationConnectionState.Expired, -1, IntegrationFreshnessState.Expired)]
    [InlineData(IntegrationConnectionState.Revoked, -1, IntegrationFreshnessState.Revoked)]
    public void FreshnessCalculator_CoversLifecycleAndAgeStates(
        IntegrationConnectionState connectionState,
        int hoursFromNow,
        IntegrationFreshnessState expected)
    {
        IntegrationFreshnessState actual = IntegrationFreshnessCalculator.Calculate(
            connectionState,
            Now.AddHours(hoursFromNow),
            Now,
            TimeSpan.FromHours(2),
            TimeSpan.FromHours(8));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Reconnect_ReverifiesStateAndCreatesOrderedAuditEntries()
    {
        IntegrationControlCentreState state = IntegrationControlCentreSeed.CreateFictional(Now);
        IntegrationControlCentreService service = new(state);
        ConnectedIntegrationAccount account = service.GetRequiredAccount("northstar-work");
        long startingSequence = state.NextAuditSequence;

        service.Reconnect(account.Id, Now.AddMinutes(5));

        Assert.Equal(IntegrationConnectionState.Connected, account.ConnectionState);
        Assert.Null(account.LastErrorCode);
        Assert.Equal(startingSequence + 2, state.NextAuditSequence);
        Assert.Equal(ConnectorAuditAction.ReconnectStarted, state.AuditEntries[^2].Action);
        Assert.Equal(ConnectorAuditAction.Reconnected, state.AuditEntries[^1].Action);
        Assert.True(state.AuditEntries[^2].Sequence < state.AuditEntries[^1].Sequence);
    }

    [Theory]
    [InlineData(DisconnectRetentionChoice.KeepAcceptedLifeOsRecords, "kept")]
    [InlineData(DisconnectRetentionChoice.ArchiveProviderLinks, "archived")]
    [InlineData(DisconnectRetentionChoice.RemoveUnacceptedImportedCandidates, "Unaccepted")]
    public void Disconnect_RecordsExplicitRetentionChoice(
        DisconnectRetentionChoice choice,
        string expectedText)
    {
        IntegrationControlCentreState state = IntegrationControlCentreSeed.CreateFictional(Now);
        IntegrationControlCentreService service = new(state);

        service.Disconnect("alex-personal", choice, Now.AddMinutes(1));

        ConnectedIntegrationAccount account = service.GetRequiredAccount("alex-personal");
        ConnectorAuditEntry audit = state.AuditEntries[^1];
        Assert.Equal(IntegrationConnectionState.Disconnected, account.ConnectionState);
        Assert.Contains(expectedText, audit.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(ConnectorAuditAction.Disconnected, audit.Action);
    }

    [Fact]
    public void Disconnect_IsSafeToRerun()
    {
        IntegrationControlCentreState state = IntegrationControlCentreSeed.CreateFictional(Now);
        IntegrationControlCentreService service = new(state);

        service.Disconnect(
            "alex-personal",
            DisconnectRetentionChoice.KeepAcceptedLifeOsRecords,
            Now.AddMinutes(1));
        service.Disconnect(
            "alex-personal",
            DisconnectRetentionChoice.ArchiveProviderLinks,
            Now.AddMinutes(2));

        Assert.Equal(
            IntegrationConnectionState.Disconnected,
            service.GetRequiredAccount("alex-personal").ConnectionState);
        Assert.Equal(2, state.AuditEntries.Count(entry =>
            entry.Action == ConnectorAuditAction.Disconnected &&
            entry.AccountId == "alex-personal"));
    }

    [Fact]
    public void Revocation_IsVisibleAndDoesNotDeleteAccounts()
    {
        IntegrationControlCentreState state = IntegrationControlCentreSeed.CreateFictional(Now);
        IntegrationControlCentreService service = new(state);
        int accountCount = state.Accounts.Count;

        service.Revoke("northstar-work", Now.AddMinutes(2));

        ConnectedIntegrationAccount account = service.GetRequiredAccount("northstar-work");
        Assert.Equal(accountCount, state.Accounts.Count);
        Assert.Equal(IntegrationConnectionState.Revoked, account.ConnectionState);
        Assert.DoesNotContain(account.Permissions, permission =>
            permission.State == IntegrationPermissionState.Granted);
        Assert.Equal("consent-revoked", account.LastErrorCode);
    }

    [Fact]
    public async Task FictionalAdapter_ProvidesDeterministicDiscoveryAndReadContracts()
    {
        ConnectorIdentity identity = new(
            "fictional-account",
            "Fictional Person",
            "fi***@example.invalid",
            IntegrationAccountClassification.Other);
        FictionalIntegrationProviderAdapter adapter = new(
            "fictional-provider",
            [identity]);

        IReadOnlyList<ConnectorIdentity> discovered = await adapter.DiscoverAccountsAsync();
        ConnectorOperationResult verified = await adapter.VerifyIdentityAsync("fictional-account");
        ConnectorCapabilityReadResult read = await adapter.ReadCapabilityAsync(
            "fictional-account",
            "mail",
            Now);

        Assert.Single(discovered);
        Assert.True(verified.Succeeded);
        Assert.True(read.Succeeded);
        Assert.Equal(3, read.RecordCount);
        Assert.Equal(Now, read.ImportedTimestampUtc);
    }

    [Fact]
    public void SerializedState_ContainsNoCredentialOrTokenFields()
    {
        IntegrationControlCentreState state = IntegrationControlCentreSeed.CreateFictional(Now);
        string json = JsonSerializer.Serialize(state).ToLowerInvariant();

        Assert.DoesNotContain("clientsecret", json);
        Assert.DoesNotContain("access_token", json);
        Assert.DoesNotContain("refresh_token", json);
        Assert.DoesNotContain("authorizationcode", json);
        Assert.DoesNotContain("password", json);
    }

    private static string CreateTemporaryPath() => Path.Combine(
        Path.GetTempPath(),
        $"lifeos-integration-control-centre-{Guid.NewGuid():N}.json");

    private static void DeleteIfPresent(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        string temporaryPath = path + ".tmp";
        if (File.Exists(temporaryPath))
        {
            File.Delete(temporaryPath);
        }
    }
}
