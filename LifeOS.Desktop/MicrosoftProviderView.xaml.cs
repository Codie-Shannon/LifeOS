using System.Net;
using System.Net.Http;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using LifeOS.Core.IntegrationControlCentre;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Core.MicrosoftProvider;

namespace LifeOS.Desktop;

public partial class MicrosoftProviderView : UserControl
{
    private readonly MicrosoftTokenStore _tokenStore = new();
    private MicrosoftProviderConfiguration _configuration;
    private MicrosoftProviderService _service;
    private string? _selectedAccountId;
    private bool _operationRunning;

    public MicrosoftProviderView(bool compactDensity)
    {
        InitializeComponent();

        _configuration =
            MicrosoftProviderStore.LoadConfiguration();
        _service = new MicrosoftProviderService(
            MicrosoftProviderStore.LoadState());

        ClassificationComboBox.ItemsSource =
            Enum.GetValues<IntegrationAccountClassification>();
        ClassificationComboBox.SelectedItem =
            IntegrationAccountClassification.Personal;

        LoadConfigurationIntoUi();
        ApplyDensity(compactDensity);
        RefreshAll();
    }

    public event EventHandler? BackRequested;

    public event Action<int>? ReviewCountChanged;

    public void ApplyDensity(bool compactDensity)
    {
        RootGrid.MinHeight = compactDensity ? 760 : 820;
    }

    private void LoadConfigurationIntoUi()
    {
        ClientIdTextBox.Text = _configuration.ClientId;
        TenantTextBox.Text = _configuration.Tenant;
        RedirectPortTextBox.Text =
            _configuration.RedirectPort.ToString();
        MailLookbackTextBox.Text =
            _configuration.MailLookbackDays.ToString();
        CalendarLookbackTextBox.Text =
            _configuration.CalendarLookbackDays.ToString();
        CalendarLookaheadTextBox.Text =
            _configuration.CalendarLookaheadDays.ToString();
        RedirectUriText.Text =
            $"Desktop redirect URI: {_configuration.RedirectUri}";
    }

    private MicrosoftProviderConfiguration ReadConfigurationFromUi()
    {
        MicrosoftProviderConfiguration configuration = new()
        {
            ClientId = ClientIdTextBox.Text,
            Tenant = TenantTextBox.Text,
            RedirectPort = ParseInt(
                RedirectPortTextBox.Text,
                _configuration.RedirectPort),
            MailLookbackDays = ParseInt(
                MailLookbackTextBox.Text,
                _configuration.MailLookbackDays),
            CalendarLookbackDays = ParseInt(
                CalendarLookbackTextBox.Text,
                _configuration.CalendarLookbackDays),
            CalendarLookaheadDays = ParseInt(
                CalendarLookaheadTextBox.Text,
                _configuration.CalendarLookaheadDays),
            MaximumMailItemsPerFolder =
                _configuration.MaximumMailItemsPerFolder,
            MaximumCalendarItems =
                _configuration.MaximumCalendarItems,
            SelectedMailFolderIds =
                _configuration.SelectedMailFolderIds.ToList(),
            SelectedCalendarIds =
                _configuration.SelectedCalendarIds.ToList()
        };

        return configuration.Normalize();
    }

    private void SaveConfigurationButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            _configuration = ReadConfigurationFromUi();
            MicrosoftProviderStore.SaveConfiguration(
                _configuration);

            _service.AppendAudit(
                accountId: null,
                MicrosoftProviderAuditAction.ConfigurationSaved,
                succeeded: true,
                "Microsoft public-client configuration was saved outside Git.",
                DateTimeOffset.UtcNow);
            SaveState();

            ConfigurationStatusText.Text =
                _configuration.Validate().Count == 0
                    ? "Configuration saved. The client ID is public configuration; no client secret is used or stored."
                    : string.Join(
                        " ",
                        _configuration.Validate());
            LoadConfigurationIntoUi();
            RefreshAll();
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException)
        {
            ConfigurationStatusText.Text =
                $"Configuration could not be saved: {exception.Message}";
        }
    }

    private async void ConnectButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!BeginOperation())
        {
            return;
        }

        try
        {
            _configuration = ReadConfigurationFromUi();
            IReadOnlyList<string> errors =
                _configuration.Validate();

            if (errors.Count > 0)
            {
                ActionStatusText.Text =
                    string.Join(" ", errors);
                return;
            }

            MicrosoftProviderStore.SaveConfiguration(
                _configuration);

            IntegrationAccountClassification classification =
                ClassificationComboBox.SelectedItem is
                    IntegrationAccountClassification selected
                    ? selected
                    : IntegrationAccountClassification.Personal;

            _service.AppendAudit(
                accountId: null,
                MicrosoftProviderAuditAction.ConsentStarted,
                succeeded: true,
                "Browser consent started for Group 48 read-only scopes.",
                DateTimeOffset.UtcNow);
            SaveState();

            using HttpClient httpClient = new();
            MicrosoftAuthorizationResult authorization =
                await new MicrosoftOAuthPkceClient(httpClient)
                    .ConnectAsync(_configuration);

            IReadOnlyCollection<string> grantedScopes =
                authorization.Token.Scope.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            MicrosoftProviderAccount account =
                _service.UpsertConnectedAccount(
                    authorization.Identity,
                    classification,
                    grantedScopes,
                    authorization.Token.ExpiresUtc,
                    DateTimeOffset.UtcNow);

            authorization.Token.AccountId = account.Id;
            _tokenStore.Save(authorization.Token);
            _selectedAccountId = account.Id;

            SaveState();
            ActionStatusText.Text =
                "Microsoft identity verified. Mail and Calendar remain read-only and will enter the Integration Inbox.";
            RefreshAll();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or
            HttpRequestException or
            TimeoutException or
            TaskCanceledException)
        {
            ActionStatusText.Text =
                $"Microsoft connection stopped: {exception.Message}";
        }
        finally
        {
            EndOperation();
        }
    }

    private async void VerifyIdentityButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedAccountId) ||
            !BeginOperation())
        {
            return;
        }

        try
        {
            MicrosoftTokenRecord token =
                await GetUsableTokenAsync(
                    _selectedAccountId);

            using HttpClient graphHttp = new();
            MicrosoftGraphIdentity identity =
                await new MicrosoftGraphClient(
                    graphHttp,
                    token.AccessToken)
                .GetIdentityAsync();

            MicrosoftProviderAccount account =
                _service.GetRequiredAccount(
                    _selectedAccountId);

            _service.UpsertConnectedAccount(
                identity,
                account.Classification,
                token.Scope.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries),
                token.ExpiresUtc,
                DateTimeOffset.UtcNow);

            SaveState();
            ActionStatusText.Text =
                "Microsoft identity was verified with a safe /me read.";
            RefreshAll();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or
            HttpRequestException or
            MicrosoftGraphException or
            TaskCanceledException)
        {
            ActionStatusText.Text =
                $"Identity verification needs attention: {exception.Message}";
        }
        finally
        {
            EndOperation();
        }
    }

    private async void SyncButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedAccountId) ||
            !BeginOperation())
        {
            return;
        }

        try
        {
            MicrosoftTokenRecord token =
                await GetUsableTokenAsync(
                    _selectedAccountId);

            IntegrationInboxV9State inboxState =
                IntegrationInboxV9Store.LoadOrCreate(
                    DateTimeOffset.UtcNow);

            using HttpClient graphHttp = new();
            MicrosoftProviderSyncResult result =
                await _service.SyncAsync(
                    _selectedAccountId,
                    _configuration,
                    new MicrosoftGraphClient(
                        graphHttp,
                        token.AccessToken),
                    inboxState,
                    DateTimeOffset.UtcNow);

            IntegrationInboxV9Store.Save(inboxState);
            SaveState();

            ActionStatusText.Text =
                $"{result.SanitizedMessage} " +
                $"{result.MailCandidates} mail, " +
                $"{result.CalendarCandidates} calendar, " +
                $"{result.Suggestions} reviewable suggestions.";

            ReviewCountChanged?.Invoke(
                new IntegrationInboxV9Service(inboxState)
                    .GetReviewCount());
            RefreshAll();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or
            HttpRequestException or
            MicrosoftGraphException or
            TaskCanceledException or
            IOException or
            UnauthorizedAccessException)
        {
            ActionStatusText.Text =
                $"Microsoft sync needs attention: {exception.Message}";
            RefreshAll();
        }
        finally
        {
            EndOperation();
        }
    }

    private void DisconnectButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_selectedAccountId))
        {
            return;
        }

        MessageBoxResult confirmation = MessageBox.Show(
            "Disconnect this Microsoft account and delete its local DPAPI-protected token? Accepted LifeOS records and review history will be retained.",
            "Disconnect Microsoft account",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        _tokenStore.Delete(_selectedAccountId);
        _service.MarkDisconnected(
            _selectedAccountId,
            DateTimeOffset.UtcNow);
        SaveState();
        ActionStatusText.Text =
            "Microsoft account disconnected. Accepted LifeOS records were retained.";
        RefreshAll();
    }

    private async Task<MicrosoftTokenRecord> GetUsableTokenAsync(
        string accountId)
    {
        MicrosoftTokenRecord token =
            _tokenStore.Load(accountId) ??
            throw new InvalidOperationException(
                "No secure local token exists. Reconnect this account.");

        if (token.ExpiresUtc >
            DateTimeOffset.UtcNow.AddMinutes(2))
        {
            return token;
        }

        using HttpClient httpClient = new();
        MicrosoftTokenRecord refreshed =
            await new MicrosoftOAuthPkceClient(httpClient)
                .RefreshAsync(
                    _configuration,
                    token);

        _tokenStore.Save(refreshed);
        _service.MarkTokenRefreshed(
            accountId,
            refreshed.ExpiresUtc,
            DateTimeOffset.UtcNow);
        SaveState();
        return refreshed;
    }

    private void AccountListBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (AccountListBox.SelectedItem is
            not MicrosoftAccountView selected)
        {
            return;
        }

        _selectedAccountId = selected.Id;
        ShowSelectedAccount();
        PopulateCapabilities();
    }

    private void RefreshAll()
    {
        IReadOnlyList<string> validation =
            _configuration.Validate();

        RegistrationStateText.Text =
            validation.Count == 0
                ? "Configured"
                : "Setup required";
        AccountCountText.Text =
            _service.State.Accounts.Count.ToString();
        MailCountText.Text =
            _service.State.Accounts.Sum(account =>
                account.LastMailCandidateCount).ToString();
        CalendarCountText.Text =
            _service.State.Accounts.Sum(account =>
                account.LastCalendarCandidateCount).ToString();

        MicrosoftAccountView[] accounts =
            _service.State.Accounts
                .OrderBy(account => account.Classification)
                .ThenBy(account => account.DisplayName)
                .Select(account => new MicrosoftAccountView(
                    account.Id,
                    account.DisplayName,
                    account.RedactedIdentity,
                    $"{FormatEnum(account.Classification)} · " +
                    $"{FormatEnum(account.ConnectionState)}"))
                .ToArray();

        AccountListBox.ItemsSource = accounts;
        AccountListBox.SelectedItem =
            accounts.FirstOrDefault(account =>
                string.Equals(
                    account.Id,
                    _selectedAccountId,
                    StringComparison.Ordinal)) ??
            accounts.FirstOrDefault();

        PopulateCapabilities();
        PopulateAudit();
        ShowSelectedAccount();
        RedirectUriText.Text =
            $"Desktop redirect URI: {_configuration.RedirectUri}";
    }

    private void ShowSelectedAccount()
    {
        if (string.IsNullOrWhiteSpace(_selectedAccountId))
        {
            SelectedAccountTitleText.Text =
                "Select or connect a Microsoft account";
            SelectedAccountDetailText.Text =
                "No token or live provider read is attempted until an explicit foreground action.";
            return;
        }

        MicrosoftProviderAccount account =
            _service.GetRequiredAccount(
                _selectedAccountId);

        SelectedAccountTitleText.Text =
            account.DisplayName;
        SelectedAccountDetailText.Text =
            $"Identity: {account.RedactedIdentity}\n" +
            $"Classification: {FormatEnum(account.Classification)}\n" +
            $"Connection: {FormatEnum(account.ConnectionState)}\n" +
            $"Token expiry: {FormatTimestamp(account.TokenExpiresUtc)}\n" +
            $"Last Mail sync: {FormatTimestamp(account.LastMailSyncUtc)}\n" +
            $"Last Calendar sync: {FormatTimestamp(account.LastCalendarSyncUtc)}\n" +
            $"Last result: {account.LastMailCandidateCount} mail, " +
            $"{account.LastCalendarCandidateCount} calendar, " +
            $"{account.LastSuggestionCount} suggestions\n" +
            (string.IsNullOrWhiteSpace(account.LastErrorMessage)
                ? "No current provider error."
                : $"Needs attention: {account.LastErrorMessage}");
    }

    private void PopulateCapabilities()
    {
        MicrosoftProviderAccount? account =
            string.IsNullOrWhiteSpace(_selectedAccountId)
                ? _service.State.Accounts.FirstOrDefault()
                : _service.State.Accounts.FirstOrDefault(candidate =>
                    candidate.Id == _selectedAccountId);

        CapabilityItems.ItemsSource =
            Enum.GetValues<MicrosoftProviderCapability>()
                .Select(capability =>
                {
                    bool delivered =
                        capability is
                            MicrosoftProviderCapability.OutlookMail or
                            MicrosoftProviderCapability.Calendar;

                    MicrosoftProviderPermissionState state =
                        account?.Permissions.GetValueOrDefault(
                            capability,
                            MicrosoftProviderPermissionState.NotRequested) ??
                        MicrosoftProviderPermissionState.NotRequested;

                    return new MicrosoftCapabilityView(
                        FormatEnum(capability),
                        delivered
                            ? "Group 48 read-only"
                            : "Later group",
                        FormatEnum(state),
                        delivered
                            ? "Requested only when the user connects Group 48 Mail/Calendar."
                            : "Recorded in the single Microsoft registration; permission not requested.");
                })
                .ToArray();
    }

    private void PopulateAudit()
    {
        AuditItems.ItemsSource =
            _service.State.AuditEntries
                .OrderByDescending(entry => entry.Sequence)
                .Take(12)
                .Select(entry => new MicrosoftAuditView(
                    entry.TimestampUtc.ToLocalTime()
                        .ToString("yyyy-MM-dd HH:mm:ss"),
                    FormatEnum(entry.Action),
                    entry.Succeeded ? "Success" : "Review",
                    entry.Summary))
                .ToArray();
    }

    private void SaveState()
    {
        MicrosoftProviderStore.SaveState(
            _service.State);
        MicrosoftControlCentreBridge.Synchronize(
            _service.State,
            DateTimeOffset.UtcNow);
    }

    private bool BeginOperation()
    {
        if (_operationRunning)
        {
            return false;
        }

        _operationRunning = true;
        IsEnabled = false;
        ActionStatusText.Text =
            "Working in the foreground. No background polling or external write is enabled.";
        return true;
    }

    private void EndOperation()
    {
        _operationRunning = false;
        IsEnabled = true;
    }

    private void BackButton_Click(
        object sender,
        RoutedEventArgs e) =>
        BackRequested?.Invoke(this, EventArgs.Empty);

    private static int ParseInt(
        string text,
        int fallback) =>
        int.TryParse(text, out int parsed)
            ? parsed
            : fallback;

    private static string FormatTimestamp(
        DateTimeOffset? value) =>
        value.HasValue
            ? value.Value.ToLocalTime()
                .ToString("yyyy-MM-dd HH:mm:ss")
            : "Never";

    private static string FormatEnum<T>(T value)
        where T : struct, Enum =>
        string.Concat(
            value.ToString()
                .Select((character, index) =>
                    index > 0 &&
                    char.IsUpper(character)
                        ? $" {character}"
                        : character.ToString()));

    private sealed record MicrosoftAccountView(
        string Id,
        string DisplayName,
        string Identity,
        string StateLine);

    private sealed record MicrosoftCapabilityView(
        string Capability,
        string Delivery,
        string Permission,
        string Note);

    private sealed record MicrosoftAuditView(
        string Timestamp,
        string Action,
        string Result,
        string Summary);
}
