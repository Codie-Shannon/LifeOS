using System.Net;
using LifeOS.Core.IntegrationControlCentre;
using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.MicrosoftProvider;

public sealed class MicrosoftProviderService
{
    public MicrosoftProviderService(MicrosoftProviderState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        State = state.Normalize();
    }

    public MicrosoftProviderState State { get; }

    public MicrosoftProviderAccount UpsertConnectedAccount(
        MicrosoftGraphIdentity identity,
        IntegrationAccountClassification classification,
        IReadOnlyCollection<string> grantedScopes,
        DateTimeOffset tokenExpiresUtc,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(grantedScopes);

        string id = $"microsoft-{identity.Id}";
        MicrosoftProviderAccount? account =
            State.Accounts.FirstOrDefault(candidate =>
                string.Equals(
                    candidate.Id,
                    id,
                    StringComparison.Ordinal));

        account ??= new MicrosoftProviderAccount
        {
            Id = id,
            MicrosoftUserId = identity.Id
        };

        if (!State.Accounts.Contains(account))
        {
            State.Accounts.Add(account);
        }

        account.DisplayName = string.IsNullOrWhiteSpace(identity.DisplayName)
            ? "Microsoft account"
            : identity.DisplayName.Trim();
        account.RedactedIdentity = RedactIdentity(
            string.IsNullOrWhiteSpace(identity.Mail)
                ? identity.UserPrincipalName
                : identity.Mail);
        account.TenantId = identity.TenantId;
        account.Classification = classification;
        account.ConnectionState = IntegrationConnectionState.Connected;
        account.TokenExpiresUtc = tokenExpiresUtc;
        account.LastIdentityVerificationUtc = nowUtc;
        account.LastErrorUtc = null;
        account.LastErrorCode = string.Empty;
        account.LastErrorMessage = string.Empty;

        HashSet<string> scopes = grantedScopes.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        account.Permissions[MicrosoftProviderCapability.OutlookMail] =
            scopes.Contains("Mail.Read")
                ? MicrosoftProviderPermissionState.Granted
                : MicrosoftProviderPermissionState.Missing;
        account.Permissions[MicrosoftProviderCapability.Calendar] =
            scopes.Contains("Calendars.Read")
                ? MicrosoftProviderPermissionState.Granted
                : MicrosoftProviderPermissionState.Missing;

        foreach (MicrosoftProviderCapability capability in
                 new[]
                 {
                     MicrosoftProviderCapability.ContactsPeople,
                     MicrosoftProviderCapability.OneDrive,
                     MicrosoftProviderCapability.SharePoint,
                     MicrosoftProviderCapability.Teams,
                     MicrosoftProviderCapability.PowerBI,
                     MicrosoftProviderCapability.PowerAutomate
                 })
        {
            account.Permissions[capability] =
                MicrosoftProviderPermissionState.NotRequested;
        }

        if (account.Permissions[MicrosoftProviderCapability.OutlookMail] !=
                MicrosoftProviderPermissionState.Granted ||
            account.Permissions[MicrosoftProviderCapability.Calendar] !=
                MicrosoftProviderPermissionState.Granted)
        {
            account.ConnectionState =
                IntegrationConnectionState.NeedsAttention;
        }

        AppendAudit(
            account.Id,
            MicrosoftProviderAuditAction.IdentityVerified,
            succeeded: true,
            "Microsoft identity was verified and Group 48 scopes were evaluated.",
            nowUtc);

        return account;
    }

    public async Task<MicrosoftProviderSyncResult> SyncAsync(
        string accountId,
        MicrosoftProviderConfiguration configuration,
        MicrosoftGraphClient graphClient,
        IntegrationInboxV9State inboxState,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(graphClient);
        ArgumentNullException.ThrowIfNull(inboxState);

        MicrosoftProviderAccount account = GetRequiredAccount(accountId);
        IntegrationInboxV9Service inbox =
            new(inboxState);

        int mailCandidates = 0;
        int calendarCandidates = 0;
        int suggestions = 0;
        List<string> failures = [];

        if (account.Permissions[MicrosoftProviderCapability.OutlookMail] ==
            MicrosoftProviderPermissionState.Granted)
        {
            try
            {
                DateTimeOffset fromUtc =
                    nowUtc.AddDays(-configuration.MailLookbackDays);

                foreach (string folderId in
                         configuration.SelectedMailFolderIds)
                {
                    IReadOnlyList<MicrosoftGraphMessage> messages =
                        await graphClient.GetMessagesAsync(
                            folderId,
                            fromUtc,
                            configuration.MaximumMailItemsPerFolder,
                            cancellationToken);

                    foreach (MicrosoftGraphMessage message in messages)
                    {
                        if (message.IsRemoved)
                        {
                            MarkSourceRemovedIfPresent(
                                inbox,
                                account.Id,
                                message.Id,
                                nowUtc);
                            continue;
                        }

                        inbox.Ingest(
                            MicrosoftCandidateFactory.CreateMessageDraft(
                                account,
                                message),
                            nowUtc);
                        mailCandidates++;

                        foreach (
                            GenericProviderRecordCandidateDraft suggestion in
                            MicrosoftCandidateFactory.CreateMessageSuggestions(
                                account,
                                message))
                        {
                            inbox.Ingest(suggestion, nowUtc);
                            suggestions++;
                        }
                    }
                }

                account.LastMailSyncUtc = nowUtc;
                account.LastMailCandidateCount = mailCandidates;
                AppendAudit(
                    account.Id,
                    MicrosoftProviderAuditAction.InitialMailReadVerified,
                    true,
                    $"Read-only Outlook Mail sync produced {mailCandidates} review candidates.",
                    nowUtc);
            }
            catch (Exception exception) when (
                exception is MicrosoftGraphException or
                HttpRequestException or
                TaskCanceledException)
            {
                failures.Add(
                    $"Mail: {SanitizeException(exception)}");
            }
        }
        else
        {
            failures.Add("Mail: Mail.Read is not granted.");
        }

        if (account.Permissions[MicrosoftProviderCapability.Calendar] ==
            MicrosoftProviderPermissionState.Granted)
        {
            try
            {
                DateTimeOffset startUtc =
                    nowUtc.AddDays(-configuration.CalendarLookbackDays);
                DateTimeOffset endUtc =
                    nowUtc.AddDays(configuration.CalendarLookaheadDays);

                foreach (string calendarId in
                         configuration.SelectedCalendarIds)
                {
                    IReadOnlyList<MicrosoftGraphCalendarEvent> events =
                        await graphClient.GetCalendarEventsAsync(
                            calendarId,
                            startUtc,
                            endUtc,
                            configuration.MaximumCalendarItems,
                            cancellationToken);

                    foreach (
                        MicrosoftGraphCalendarEvent calendarEvent in events)
                    {
                        if (calendarEvent.IsRemoved)
                        {
                            MarkSourceRemovedIfPresent(
                                inbox,
                                account.Id,
                                calendarEvent.Id,
                                nowUtc);
                            continue;
                        }

                        inbox.Ingest(
                            MicrosoftCandidateFactory.CreateCalendarDraft(
                                account,
                                calendarEvent),
                            nowUtc);
                        calendarCandidates++;

                        inbox.Ingest(
                            MicrosoftCandidateFactory.CreateScheduleSuggestion(
                                account,
                                calendarEvent),
                            nowUtc);
                        suggestions++;
                    }
                }

                account.LastCalendarSyncUtc = nowUtc;
                account.LastCalendarCandidateCount =
                    calendarCandidates;
                AppendAudit(
                    account.Id,
                    MicrosoftProviderAuditAction.InitialCalendarReadVerified,
                    true,
                    $"Read-only Microsoft Calendar sync produced {calendarCandidates} review candidates.",
                    nowUtc);
            }
            catch (Exception exception) when (
                exception is MicrosoftGraphException or
                HttpRequestException or
                TaskCanceledException)
            {
                failures.Add(
                    $"Calendar: {SanitizeException(exception)}");
            }
        }
        else
        {
            failures.Add("Calendar: Calendars.Read is not granted.");
        }

        account.LastSuggestionCount = suggestions;
        bool succeeded = failures.Count == 0;
        bool partial = failures.Count > 0 &&
            (mailCandidates > 0 || calendarCandidates > 0);

        account.ConnectionState = succeeded
            ? IntegrationConnectionState.Connected
            : partial
                ? IntegrationConnectionState.NeedsAttention
                : ClassifyFailure(failures);

        if (succeeded)
        {
            account.LastErrorUtc = null;
            account.LastErrorCode = string.Empty;
            account.LastErrorMessage = string.Empty;
        }
        else
        {
            account.LastErrorUtc = nowUtc;
            account.LastErrorCode = partial
                ? "partial-provider-availability"
                : "provider-sync-failed";
            account.LastErrorMessage =
                string.Join(" ", failures);
        }

        AppendAudit(
            account.Id,
            succeeded
                ? MicrosoftProviderAuditAction.SyncCompleted
                : partial
                    ? MicrosoftProviderAuditAction.SyncPartiallyCompleted
                    : MicrosoftProviderAuditAction.SyncFailed,
            succeeded,
            succeeded
                ? $"Microsoft read-only sync completed: {mailCandidates} mail, {calendarCandidates} calendar and {suggestions} suggestions."
                : string.Join(" ", failures),
            nowUtc);

        return new MicrosoftProviderSyncResult(
            succeeded,
            partial,
            mailCandidates,
            calendarCandidates,
            suggestions,
            succeeded
                ? "Read-only Mail and Calendar sync completed."
                : string.Join(" ", failures));
    }

    public void MarkDisconnected(
        string accountId,
        DateTimeOffset nowUtc)
    {
        MicrosoftProviderAccount account =
            GetRequiredAccount(accountId);

        account.ConnectionState =
            IntegrationConnectionState.Disconnected;
        account.TokenExpiresUtc = null;
        account.LastErrorUtc = null;
        account.LastErrorCode = string.Empty;
        account.LastErrorMessage = string.Empty;

        AppendAudit(
            account.Id,
            MicrosoftProviderAuditAction.Disconnected,
            true,
            "Local Microsoft token material was removed. Accepted LifeOS records were retained.",
            nowUtc);
    }

    public void MarkTokenRefreshed(
        string accountId,
        DateTimeOffset expiresUtc,
        DateTimeOffset nowUtc)
    {
        MicrosoftProviderAccount account =
            GetRequiredAccount(accountId);
        account.TokenExpiresUtc = expiresUtc;
        account.ConnectionState =
            IntegrationConnectionState.Connected;

        AppendAudit(
            account.Id,
            MicrosoftProviderAuditAction.TokenRefreshed,
            true,
            "Microsoft access token was refreshed inside the secure local boundary.",
            nowUtc);
    }

    public MicrosoftProviderAccount GetRequiredAccount(
        string accountId) =>
        State.Accounts.SingleOrDefault(account =>
            string.Equals(
                account.Id,
                accountId,
                StringComparison.Ordinal))
        ?? throw new KeyNotFoundException(
            $"Microsoft account '{accountId}' was not found.");

    public void AppendAudit(
        string? accountId,
        MicrosoftProviderAuditAction action,
        bool succeeded,
        string summary,
        DateTimeOffset nowUtc)
    {
        State.AuditEntries.Add(new MicrosoftProviderAuditEntry
        {
            Sequence = State.NextAuditSequence++,
            TimestampUtc = nowUtc,
            AccountId = accountId,
            Action = action,
            Succeeded = succeeded,
            Summary = summary.Trim()
        });
    }

    private static void MarkSourceRemovedIfPresent(
        IntegrationInboxV9Service inbox,
        string accountId,
        string externalId,
        DateTimeOffset nowUtc)
    {
        IntegrationCandidate? existing =
            inbox.State.Candidates.FirstOrDefault(candidate =>
                string.Equals(
                    candidate.Provenance.ProviderId,
                    "microsoft",
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    candidate.Provenance.AccountId,
                    accountId,
                    StringComparison.Ordinal) &&
                string.Equals(
                    candidate.Provenance.ExternalId,
                    externalId,
                    StringComparison.Ordinal));

        if (existing is not null &&
            existing.Status !=
                IntegrationCandidateStatus.SourceRemoved)
        {
            inbox.MarkSourceRemoved(existing.Id, nowUtc);
        }
    }

    private static IntegrationConnectionState ClassifyFailure(
        IReadOnlyList<string> failures)
    {
        string combined = string.Join(" ", failures);

        if (combined.Contains(
                "InvalidAuthenticationToken",
                StringComparison.OrdinalIgnoreCase) ||
            combined.Contains(
                "401",
                StringComparison.OrdinalIgnoreCase))
        {
            return IntegrationConnectionState.Expired;
        }

        if (combined.Contains(
                "Authorization_RequestDenied",
                StringComparison.OrdinalIgnoreCase) ||
            combined.Contains(
                "403",
                StringComparison.OrdinalIgnoreCase))
        {
            return IntegrationConnectionState.Revoked;
        }

        if (combined.Contains(
                "throttle",
                StringComparison.OrdinalIgnoreCase) ||
            combined.Contains(
                "429",
                StringComparison.OrdinalIgnoreCase))
        {
            return IntegrationConnectionState.NeedsAttention;
        }

        return IntegrationConnectionState.Offline;
    }

    private static string SanitizeException(Exception exception)
    {
        if (exception is MicrosoftGraphException graph)
        {
            return $"{graph.Code}: {graph.Message}";
        }

        return exception switch
        {
            TaskCanceledException =>
                "request-timeout",
            HttpRequestException =>
                "network-unavailable",
            _ =>
                "provider-read-failed"
        };
    }

    private static string RedactIdentity(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity))
        {
            return "Microsoft identity verified";
        }

        int at = identity.IndexOf('@');
        if (at <= 1)
        {
            return identity.Length <= 2
                ? "**"
                : identity[..1] + "***";
        }

        string local = identity[..at];
        string domain = identity[(at + 1)..];
        string redactedLocal = local.Length <= 2
            ? local[..1] + "*"
            : local[..2] + new string('*', Math.Min(6, local.Length - 2));

        return $"{redactedLocal}@{domain}";
    }
}
