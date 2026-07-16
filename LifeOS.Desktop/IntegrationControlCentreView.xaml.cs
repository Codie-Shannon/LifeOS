using System.IO;
using System.Windows;
using System.Windows.Controls;
using LifeOS.Core.IntegrationControlCentre;

namespace LifeOS.Desktop;

public partial class IntegrationControlCentreView : UserControl
{
    private readonly string _storePath;
    private readonly IntegrationControlCentreService _service;
    private string? _selectedAccountId;

    public event EventHandler? BackRequested;

    public IntegrationControlCentreView(bool compactDensity)
    {
        InitializeComponent();

        _storePath = IntegrationControlCentreStore.DefaultPath;
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        IntegrationControlCentreState state = IntegrationControlCentreStore.LoadOrCreate(
            nowUtc,
            _storePath);
        state = Group49ControlCentreMigration.Apply(state, nowUtc);
        state = Group50ControlCentreMigration.Apply(state, nowUtc);
        state = Group51GoogleWorkspaceMigration.Apply(state, nowUtc);
        try { IntegrationControlCentreStore.Save(state, _storePath); } catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { }
        _service = new IntegrationControlCentreService(state);

        ApplyDensity(compactDensity);
        RefreshView();
    }

    private void RefreshView(string? preferredAccountId = null)
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        IReadOnlyList<ProviderView> providers = _service.State.Providers
            .Select(provider => CreateProviderView(provider, nowUtc))
            .ToArray();
        IReadOnlyList<AccountView> accounts = _service.State.Accounts
            .Select(account => CreateAccountView(account, nowUtc))
            .ToArray();

        ProviderItems.ItemsSource = providers;
        OverviewAccountItems.ItemsSource = accounts;
        AccountListBox.ItemsSource = accounts;

        ProviderCountText.Text = providers.Count.ToString();
        AccountCountText.Text = accounts.Count.ToString();

        IntegrationCapabilityHealthState[] allHealth = _service.State.Accounts
            .SelectMany(account => account.CapabilityStatuses.Select(status =>
                IntegrationCapabilityHealthCalculator.Calculate(account, status, nowUtc)))
            .ToArray();

        int healthy = allHealth.Count(health => health == IntegrationCapabilityHealthState.Healthy);
        int attention = allHealth.Count(health => health is
            IntegrationCapabilityHealthState.NeedsAttention or
            IntegrationCapabilityHealthState.Stale or
            IntegrationCapabilityHealthState.Expired or
            IntegrationCapabilityHealthState.Revoked or
            IntegrationCapabilityHealthState.Offline);

        CapabilityHealthText.Text = $"{healthy} healthy";
        AttentionCountText.Text = attention.ToString();

        string? accountId = preferredAccountId
            ?? _selectedAccountId
            ?? accounts.FirstOrDefault()?.Id;

        AccountView? selected = accounts.FirstOrDefault(account =>
            string.Equals(account.Id, accountId, StringComparison.Ordinal));

        AccountListBox.SelectedItem = selected;
        ShowAccount(selected?.Id, nowUtc);
        AuditItems.ItemsSource = CreateAuditViews();
    }

    private ProviderView CreateProviderView(
        IntegrationProviderProfile provider,
        DateTimeOffset nowUtc)
    {
        ConnectedIntegrationAccount[] accounts = _service.State.Accounts
            .Where(account => account.ProviderId == provider.Id)
            .ToArray();
        int healthy = accounts
            .SelectMany(account => account.CapabilityStatuses.Select(status =>
                IntegrationCapabilityHealthCalculator.Calculate(account, status, nowUtc)))
            .Count(health => health == IntegrationCapabilityHealthState.Healthy);
        int accountCount = accounts.Length;

        string healthyLabel = healthy == 1 ? "1 healthy capability" : $"{healthy} healthy capabilities";

        return new ProviderView(
            provider.DisplayName,
            provider.Description,
            $"{accountCount} account{(accountCount == 1 ? string.Empty : "s")} · {healthyLabel}",
            $"Capabilities: {string.Join(" · ", provider.Capabilities.Select(capability => capability.DisplayName))}");
    }

    private AccountView CreateAccountView(
        ConnectedIntegrationAccount account,
        DateTimeOffset nowUtc)
    {
        string providerName = _service.GetRequiredProvider(account.ProviderId).DisplayName;
        IntegrationFreshnessState freshness = IntegrationFreshnessCalculator.Calculate(
            account.ConnectionState,
            account.LastSuccessfulSyncUtc,
            nowUtc,
            TimeSpan.FromHours(2),
            TimeSpan.FromHours(8));

        return new AccountView(
            account.Id,
            account.DisplayName,
            providerName,
            account.ProviderIdentity,
            FormatClassification(account.Classification),
            FormatEnum(account.ConnectionState),
            $"{FormatClassification(account.Classification)} · {providerName}",
            $"Account freshness: {FormatEnum(freshness)} · last success {FormatTimestamp(account.LastSuccessfulSyncUtc)}");
    }

    private void ShowAccount(string? accountId, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            _selectedAccountId = null;
            CapabilityItems.ItemsSource = null;
            PermissionItems.ItemsSource = null;
            return;
        }

        _selectedAccountId = accountId;
        ConnectedIntegrationAccount account = _service.GetRequiredAccount(accountId);
        IntegrationProviderProfile provider = _service.GetRequiredProvider(account.ProviderId);

        SelectedAccountNameText.Text = account.DisplayName;
        SelectedAccountIdentityText.Text = $"{provider.DisplayName} · {account.ProviderIdentity}";
        SelectedAccountClassText.Text = FormatClassification(account.Classification);
        SelectedAccountStateText.Text = $"Connection: {FormatEnum(account.ConnectionState)}";
        SelectedAccountSyncText.Text =
            $"Last attempt {FormatTimestamp(account.LastSyncAttemptUtc)} · Last success {FormatTimestamp(account.LastSuccessfulSyncUtc)} · Next eligible {FormatTimestamp(account.NextEligibleSyncUtc)}";

        RefreshActionButton.IsEnabled = account.ConnectionState is
            IntegrationConnectionState.Connected or
            IntegrationConnectionState.NeedsAttention or
            IntegrationConnectionState.Offline;
        ReconnectActionButton.IsEnabled = account.ConnectionState != IntegrationConnectionState.Connecting;
        RevokeActionButton.IsEnabled = account.ConnectionState is
            IntegrationConnectionState.Connected or
            IntegrationConnectionState.NeedsAttention;
        DisconnectActionButton.IsEnabled = account.ConnectionState is not
            IntegrationConnectionState.Disconnected and not
            IntegrationConnectionState.NotConnected;

        CapabilityItems.ItemsSource = provider.Capabilities
            .Select(capability => CreateCapabilityView(account, capability, nowUtc))
            .ToArray();
        PermissionItems.ItemsSource = account.Permissions
            .Select(permission => new PermissionView(
                permission.DisplayName,
                permission.CapabilityId,
                FormatEnum(permission.Requirement),
                FormatEnum(permission.State)))
            .ToArray();
    }

    private CapabilityView CreateCapabilityView(
        ConnectedIntegrationAccount account,
        IntegrationProviderCapabilityDefinition capability,
        DateTimeOffset nowUtc)
    {
        IntegrationCapabilityStatus? status = account.CapabilityStatuses.FirstOrDefault(candidate =>
            string.Equals(candidate.CapabilityId, capability.Id, StringComparison.Ordinal));

        if (status is null)
        {
            return new CapabilityView(
                capability.Id,
                capability.DisplayName,
                "Not available",
                "Never synchronized",
                "Last success: never",
                capability.Description);
        }

        IntegrationFreshnessState freshness = IntegrationFreshnessCalculator.Calculate(
            account.ConnectionState,
            status.LastSuccessfulSyncUtc,
            nowUtc,
            status.FreshFor,
            status.StaleAfter);
        IntegrationCapabilityHealthState health = IntegrationCapabilityHealthCalculator.Calculate(
            account,
            status,
            nowUtc);
        string detail = string.IsNullOrWhiteSpace(status.LastErrorMessage)
            ? capability.Description
            : status.LastErrorMessage;

        return new CapabilityView(
            capability.Id,
            capability.DisplayName,
            FormatEnum(health),
            FormatEnum(freshness),
            $"Last success: {FormatTimestamp(status.LastSuccessfulSyncUtc)}",
            detail);
    }

    private IReadOnlyList<AuditView> CreateAuditViews()
    {
        return _service.State.AuditEntries
            .OrderByDescending(entry => entry.Sequence)
            .Select(entry =>
            {
                string account = _service.State.Accounts
                    .FirstOrDefault(candidate => candidate.Id == entry.AccountId)
                    ?.DisplayName
                    ?? entry.AccountId
                    ?? "Provider";

                return new AuditView(
                    entry.TimestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                    FormatEnum(entry.Action),
                    account,
                    entry.Summary,
                    entry.Succeeded ? "Success" : "Review");
            })
            .ToArray();
    }

    private void AccountListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AccountListBox.SelectedItem is AccountView account)
        {
            ShowAccount(account.Id, DateTimeOffset.UtcNow);
            ActionStatusText.Text = string.Empty;
        }
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAccountId is null)
        {
            return;
        }

        _service.Refresh(_selectedAccountId, DateTimeOffset.UtcNow);
        SaveAndRefresh("Manual refresh completed. Capability health remains explicit.");
    }

    private void ReconnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAccountId is null)
        {
            return;
        }

        ConnectedIntegrationAccount account = _service.GetRequiredAccount(_selectedAccountId);
        IntegrationConnectionReviewDialog dialog = new(
            account.DisplayName,
            account.ConnectionState,
            IntegrationConnectionReviewMode.Reconnect);
        SetDialogOwner(dialog);

        if (dialog.ShowDialog() != true)
        {
            ActionStatusText.Text = "Reconnect review cancelled. No state changed.";
            return;
        }

        _service.Reconnect(_selectedAccountId, DateTimeOffset.UtcNow);
        SaveAndRefresh("Reconnect completed against fictional proof state. Identity and first safe read were re-verified.");
    }

    private void RevokeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAccountId is null)
        {
            return;
        }

        ConnectedIntegrationAccount account = _service.GetRequiredAccount(_selectedAccountId);
        IntegrationConnectionReviewDialog dialog = new(
            account.DisplayName,
            account.ConnectionState,
            IntegrationConnectionReviewMode.Revoke);
        SetDialogOwner(dialog);

        if (dialog.ShowDialog() != true)
        {
            ActionStatusText.Text = "Revoke review cancelled. No state changed.";
            return;
        }

        _service.Revoke(_selectedAccountId, DateTimeOffset.UtcNow);
        SaveAndRefresh("Fictional consent revoked. Accepted LifeOS records were not silently deleted.");
    }

    private void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAccountId is null)
        {
            return;
        }

        ConnectedIntegrationAccount account = _service.GetRequiredAccount(_selectedAccountId);
        IntegrationDisconnectReviewDialog dialog = new(account.DisplayName);
        SetDialogOwner(dialog);

        if (dialog.ShowDialog() != true || dialog.SelectedChoice is null)
        {
            ActionStatusText.Text = "Disconnect review cancelled. No state changed.";
            return;
        }

        _service.Disconnect(
            _selectedAccountId,
            dialog.SelectedChoice.Value,
            DateTimeOffset.UtcNow);
        SaveAndRefresh($"Disconnected with retention choice: {FormatEnum(dialog.SelectedChoice.Value)}.");
    }

    private void SaveAndRefresh(string status)
    {
        string? accountId = _selectedAccountId;

        try
        {
            IntegrationControlCentreStore.Save(_service.State, _storePath);
            RefreshView(accountId);
            ActionStatusText.Text = status;
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException)
        {
            RefreshView(accountId);
            ActionStatusText.Text = "State changed in memory, but the local Integration Control Centre file could not be saved.";
        }
    }

    public void ApplyDensity(bool compactDensity)
    {
        ControlTabs.Height = compactDensity ? 520 : 560;
    }

    private void SetDialogOwner(Window dialog)
    {
        Window? owner = Window.GetWindow(this) ?? Application.Current?.MainWindow;
        if (owner is { IsVisible: true })
        {
            dialog.Owner = owner;
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e) =>
        BackRequested?.Invoke(this, EventArgs.Empty);

    private static string FormatTimestamp(DateTimeOffset? timestampUtc) =>
        timestampUtc is null
            ? "never"
            : timestampUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    private static string FormatClassification(IntegrationAccountClassification classification) =>
        classification == IntegrationAccountClassification.FamilyHousehold
            ? "Family / Household"
            : FormatEnum(classification);

    private static string FormatEnum<T>(T value)
        where T : struct, Enum
    {
        string text = value.ToString();
        return string.Concat(text.Select((character, index) =>
            index > 0 && char.IsUpper(character)
                ? $" {character}"
                : character.ToString()));
    }

    private sealed record ProviderView(
        string DisplayName,
        string Description,
        string Summary,
        string CapabilitySummary);

    private sealed record AccountView(
        string Id,
        string DisplayName,
        string ProviderName,
        string Identity,
        string Classification,
        string ConnectionState,
        string ClassificationAndProvider,
        string FreshnessSummary);

    private sealed record CapabilityView(
        string CapabilityId,
        string DisplayName,
        string Health,
        string Freshness,
        string LastSuccess,
        string Detail);

    private sealed record PermissionView(
        string DisplayName,
        string Capability,
        string Requirement,
        string State);

    private sealed record AuditView(
        string Timestamp,
        string Action,
        string Account,
        string Summary,
        string Result);
}
