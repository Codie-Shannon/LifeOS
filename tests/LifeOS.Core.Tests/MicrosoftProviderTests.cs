using System.Net;
using System.Text;
using LifeOS.Core.IntegrationControlCentre;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Core.MicrosoftProvider;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class MicrosoftProviderTests
{
    [Fact]
    public void Group48ScopesAreReadOnlyAndIncremental()
    {
        IReadOnlyList<string> scopes =
            MicrosoftProviderConfiguration.Group48RequestedScopes;

        Assert.Contains("User.Read", scopes);
        Assert.Contains("Mail.Read", scopes);
        Assert.Contains("Calendars.Read", scopes);
        Assert.DoesNotContain("Mail.Send", scopes);
        Assert.DoesNotContain("Mail.ReadWrite", scopes);
        Assert.DoesNotContain("Calendars.ReadWrite", scopes);
        Assert.DoesNotContain("Files.Read", scopes);
        Assert.DoesNotContain("Sites.Read.All", scopes);
    }

    [Fact]
    public void AuthorizationUriUsesPkceAndLocalhostRedirect()
    {
        MicrosoftProviderConfiguration configuration = new()
        {
            ClientId = "11111111-1111-1111-1111-111111111111",
            Tenant = "common",
            RedirectPort = 53682
        };

        MicrosoftPkcePair pkce = new(
            "verifier-value",
            "challenge-value");

        Uri uri = MicrosoftOAuthProtocol.BuildAuthorizationUri(
            configuration,
            pkce,
            "state-value");

        string text = uri.ToString();

        Assert.StartsWith(
            "https://login.microsoftonline.com/common/",
            text);
        Assert.Contains("response_type=code", text);
        Assert.Contains("code_challenge=challenge-value", text);
        Assert.Contains("code_challenge_method=S256", text);
        Assert.Contains("state=state-value", text);
        Assert.Contains("Mail.Read", Uri.UnescapeDataString(text));
        Assert.Contains("Calendars.Read", Uri.UnescapeDataString(text));
        Assert.Contains(
            Uri.EscapeDataString(configuration.RedirectUri),
            text);
    }

    [Fact]
    public void MessageCandidateIncludesAttachmentMetadata()
    {
        MicrosoftProviderAccount account = CreateAccount();

        MicrosoftGraphMessage message = new(
            "message-1",
            "Supplier quote",
            "Supplier <supplier@example.invalid>",
            "Codie <codie@example.invalid>",
            DateTimeOffset.Parse("2026-07-17T00:00:00Z"),
            DateTimeOffset.Parse("2026-07-16T23:59:00Z"),
            "high",
            false,
            "conversation-1",
            true,
            DateTimeOffset.Parse("2026-07-17T00:01:00Z"),
            [
                new MicrosoftAttachmentMetadata(
                    "attachment-1",
                    "quote.pdf",
                    "application/pdf",
                    2048,
                    false,
                    "#microsoft.graph.fileAttachment")
            ],
            false);

        IntegrationCandidate candidate =
            IntegrationCandidateNormalizer.Normalize(
                MicrosoftCandidateFactory.CreateMessageDraft(
                    account,
                    message),
                DateTimeOffset.Parse("2026-07-17T00:02:00Z"));

        Assert.Equal(
            IntegrationCandidateType.Message,
            candidate.Type);
        Assert.Contains(
            candidate.Fields,
            field =>
                field.Key == "attachments" &&
                field.Value.Contains("quote.pdf"));
        Assert.Contains(
            candidate.Fields,
            field =>
                field.Key == "last-modified" &&
                field.Value.Contains("2026-07-17"));
    }

    [Fact]
    public void CalendarCandidateIncludesTimezoneAttendeesAndOnlineMeeting()
    {
        MicrosoftProviderAccount account = CreateAccount();

        MicrosoftGraphCalendarEvent calendarEvent = new(
            "event-1",
            "AIE review",
            DateTimeOffset.Parse("2026-07-18T10:00:00Z"),
            DateTimeOffset.Parse("2026-07-18T11:00:00Z"),
            "Pacific/Auckland",
            "Workshop 2",
            "Alice <alice@example.invalid>",
            "Codie <codie@example.invalid>",
            "accepted",
            "{\"pattern\":{\"type\":\"weekly\"}}",
            "https://teams.example.invalid/join",
            DateTimeOffset.Parse("2026-07-17T00:00:00Z"),
            false,
            false);

        IntegrationCandidate candidate =
            IntegrationCandidateNormalizer.Normalize(
                MicrosoftCandidateFactory.CreateCalendarDraft(
                    account,
                    calendarEvent),
                DateTimeOffset.Parse("2026-07-17T00:05:00Z"));

        Assert.Equal(
            IntegrationCandidateType.CalendarEvent,
            candidate.Type);
        Assert.Contains(
            candidate.Fields,
            field =>
                field.Key == "timezone" &&
                field.Value == "Pacific/Auckland");
        Assert.Contains(
            candidate.Fields,
            field =>
                field.Key == "response" &&
                field.Value == "accepted");
        Assert.Contains(
            candidate.Fields,
            field =>
                field.Key == "online-meeting" &&
                field.Value.Contains("teams"));
    }

    [Fact]
    public async Task ReadOnlySyncRoutesMailCalendarAndSuggestionsToInbox()
    {
        string meJson =
            """
            {
              "id":"user-1",
              "displayName":"Alex Morgan",
              "userPrincipalName":"alex@example.invalid",
              "mail":"alex@example.invalid"
            }
            """;

        string messagesJson =
            """
            {
              "value":[
                {
                  "id":"mail-1",
                  "subject":"Client invoice waiting",
                  "from":{"emailAddress":{"name":"Client","address":"client@example.invalid"}},
                  "toRecipients":[{"emailAddress":{"name":"Alex","address":"alex@example.invalid"}}],
                  "receivedDateTime":"2026-07-17T00:10:00Z",
                  "sentDateTime":"2026-07-17T00:09:00Z",
                  "importance":"high",
                  "isRead":false,
                  "conversationId":"thread-1",
                  "hasAttachments":false,
                  "lastModifiedDateTime":"2026-07-17T00:10:30Z"
                }
              ]
            }
            """;

        string calendarJson =
            """
            {
              "value":[
                {
                  "id":"event-1",
                  "subject":"Project review",
                  "start":{"dateTime":"2026-07-18T10:00:00Z","timeZone":"UTC"},
                  "end":{"dateTime":"2026-07-18T11:00:00Z","timeZone":"UTC"},
                  "location":{"displayName":"Workshop"},
                  "organizer":{"emailAddress":{"name":"Alice","address":"alice@example.invalid"}},
                  "attendees":[{"emailAddress":{"name":"Alex","address":"alex@example.invalid"}}],
                  "responseStatus":{"response":"accepted"},
                  "recurrence":null,
                  "onlineMeeting":{"joinUrl":"https://teams.example.invalid/join"},
                  "lastModifiedDateTime":"2026-07-17T00:11:00Z",
                  "isCancelled":false
                }
              ]
            }
            """;

        HttpClient httpClient = new(
            new RouteHandler(request =>
            {
                string path = request.RequestUri?.AbsolutePath ?? string.Empty;

                if (path.EndsWith("/me", StringComparison.Ordinal))
                {
                    return Json(meJson);
                }

                if (path.Contains("/messages", StringComparison.Ordinal))
                {
                    return Json(messagesJson);
                }

                if (path.Contains("/calendarView", StringComparison.Ordinal))
                {
                    return Json(calendarJson);
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("{}")
                };
            }));

        MicrosoftProviderState providerState = new();
        MicrosoftProviderService provider =
            new(providerState);

        MicrosoftProviderAccount account =
            provider.UpsertConnectedAccount(
                new MicrosoftGraphIdentity(
                    "user-1",
                    "Alex Morgan",
                    "alex@example.invalid",
                    "alex@example.invalid",
                    string.Empty),
                IntegrationAccountClassification.Personal,
                ["User.Read", "Mail.Read", "Calendars.Read"],
                DateTimeOffset.Parse("2026-07-17T01:00:00Z"),
                DateTimeOffset.Parse("2026-07-17T00:00:00Z"));

        IntegrationInboxV9State inbox = new();
        MicrosoftProviderConfiguration configuration =
            new MicrosoftProviderConfiguration
            {
                ClientId = "11111111-1111-1111-1111-111111111111",
                SelectedMailFolderIds = ["inbox"],
                SelectedCalendarIds = ["default"],
                MaximumMailItemsPerFolder = 10,
                MaximumCalendarItems = 10
            }.Normalize();

        MicrosoftProviderSyncResult result =
            await provider.SyncAsync(
                account.Id,
                configuration,
                new MicrosoftGraphClient(
                    httpClient,
                    "test-access-token"),
                inbox,
                DateTimeOffset.Parse("2026-07-17T00:20:00Z"));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.MailCandidates);
        Assert.Equal(1, result.CalendarCandidates);
        Assert.True(result.Suggestions >= 3);
        Assert.Contains(
            inbox.Candidates,
            candidate =>
                candidate.Type == IntegrationCandidateType.Message &&
                candidate.Provenance.ProviderId == "microsoft");
        Assert.Contains(
            inbox.Candidates,
            candidate =>
                candidate.Type == IntegrationCandidateType.CalendarEvent);
        Assert.Contains(
            inbox.Candidates,
            candidate =>
                candidate.Type ==
                    IntegrationCandidateType.GenericProviderRecord &&
                candidate.Provenance.CapabilityId.Contains("suggestion"));
    }

    [Fact]
    public void BridgePublishesRealMicrosoftProviderToControlCentre()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"lifeos-g48-control-{Guid.NewGuid():N}.json");

        try
        {
            MicrosoftProviderService service =
                new(new MicrosoftProviderState());

            service.UpsertConnectedAccount(
                new MicrosoftGraphIdentity(
                    "user-bridge",
                    "Northstar Operations",
                    "work@example.invalid",
                    "work@example.invalid",
                    "tenant-1"),
                IntegrationAccountClassification.Work,
                ["User.Read", "Mail.Read", "Calendars.Read"],
                DateTimeOffset.UtcNow.AddHours(1),
                DateTimeOffset.UtcNow);

            IntegrationControlCentreState control =
                MicrosoftControlCentreBridge.Synchronize(
                    service.State,
                    DateTimeOffset.UtcNow,
                    path);

            IntegrationProviderProfile provider =
                Assert.Single(control.Providers, candidate =>
                    candidate.Id == "microsoft");
            Assert.False(provider.IsFictional);
            Assert.Contains(
                provider.Capabilities,
                capability => capability.Id == "onedrive");

            ConnectedIntegrationAccount account =
                Assert.Single(control.Accounts, candidate =>
                    candidate.ProviderId == "microsoft");
            Assert.Equal(
                IntegrationAccountClassification.Work,
                account.Classification);
            Assert.Contains(
                account.Permissions,
                permission =>
                    permission.CapabilityId == "outlook-mail" &&
                    permission.State ==
                        IntegrationPermissionState.Granted);
            Assert.Contains(
                account.Permissions,
                permission =>
                    permission.CapabilityId == "onedrive" &&
                    permission.State ==
                        IntegrationPermissionState.NotRequested);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void LaterCapabilitiesRemainNotRequested()
    {
        MicrosoftProviderService service =
            new(new MicrosoftProviderState());

        MicrosoftProviderAccount account =
            service.UpsertConnectedAccount(
                new MicrosoftGraphIdentity(
                    "user-2",
                    "Northstar Operations",
                    "work@example.invalid",
                    "work@example.invalid",
                    "tenant-1"),
                IntegrationAccountClassification.Work,
                ["User.Read", "Mail.Read", "Calendars.Read"],
                DateTimeOffset.UtcNow.AddHours(1),
                DateTimeOffset.UtcNow);

        Assert.Equal(
            MicrosoftProviderPermissionState.Granted,
            account.Permissions[
                MicrosoftProviderCapability.OutlookMail]);
        Assert.Equal(
            MicrosoftProviderPermissionState.Granted,
            account.Permissions[
                MicrosoftProviderCapability.Calendar]);
        Assert.Equal(
            MicrosoftProviderPermissionState.NotRequested,
            account.Permissions[
                MicrosoftProviderCapability.OneDrive]);
        Assert.Equal(
            MicrosoftProviderPermissionState.NotRequested,
            account.Permissions[
                MicrosoftProviderCapability.Teams]);
    }

    private static MicrosoftProviderAccount CreateAccount() =>
        new()
        {
            Id = "microsoft-user-1",
            MicrosoftUserId = "user-1",
            DisplayName = "Alex Morgan",
            RedactedIdentity = "al****@example.invalid",
            Classification =
                IntegrationAccountClassification.Personal,
            ConnectionState =
                IntegrationConnectionState.Connected
        };

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json")
        };

    private sealed class RouteHandler :
        HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage>
            _route;

        public RouteHandler(
            Func<HttpRequestMessage, HttpResponseMessage> route)
        {
            _route = route;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(_route(request));
    }
}
