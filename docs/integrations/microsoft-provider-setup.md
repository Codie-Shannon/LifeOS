# LifeOS Microsoft provider setup — Group 48

This is the single complete Microsoft provider registration for the planned LifeOS Microsoft family. Later OneDrive, SharePoint and Teams groups must reuse this registration rather than create new projects.

## Registration boundary

Create one Microsoft Entra application registration named clearly, for example:

```text
LifeOS Microsoft Provider
```

Recommended supported account type for Personal and Work account proof:

```text
Accounts in any organizational directory and personal Microsoft accounts
```

LifeOS Desktop is a public client. It uses authorization-code flow with PKCE and a loopback redirect. It does not require or store a client secret.

## Desktop redirect URI

Add the Desktop redirect URI shown inside LifeOS Microsoft Provider Setup. The default is:

```text
http://localhost:53682/
```

Use the **Mobile and desktop applications** platform boundary where supported by the Entra registration experience. Keep the exact URI and trailing slash aligned with LifeOS local configuration.

Future Companion, Full Mobile and Portal redirect boundaries must be documented separately and must not be enabled until those clients are implemented.

## Group 48 delegated scopes

The application requests only these Group 48 scopes when a user explicitly connects:

```text
openid
profile
offline_access
User.Read
Mail.Read
Calendars.Read
```

Do not add:

```text
Mail.Send
Mail.ReadWrite
Calendars.ReadWrite
Files.Read
Sites.Read.All
Team.ReadBasic.All
Channel.ReadBasic.All
```

Those later capability permissions remain **Not Requested** until the related feature is implemented and explicitly enabled.

## Complete planned capability catalogue

Record the following capability family once in the same registration/design record:

- Outlook Mail
- Microsoft Calendar
- Contacts / People
- OneDrive
- SharePoint
- Teams
- Power BI
- Power Automate

Recording a capability does not grant its permission. Incremental consent remains mandatory.

## Local configuration and token boundary

LifeOS stores public configuration outside Git at:

```text
%LOCALAPPDATA%\LifeOS\microsoft-provider-config.json
```

Provider state is stored at:

```text
%LOCALAPPDATA%\LifeOS\microsoft-provider-state.json
```

Access and refresh tokens are DPAPI-protected for the current Windows user at:

```text
%LOCALAPPDATA%\LifeOS\microsoft-provider-tokens.dat
```

Never copy token files into the repository, build packs, screenshots or support messages.

## Consent and verification

1. Save the application client ID, tenant boundary and redirect port in LifeOS.
2. Choose Personal, Work, Business, Family / Household or Other before connecting.
3. Select **Connect Microsoft account**.
4. Review Microsoft consent. Confirm that only identity, Mail read and Calendar read access are requested.
5. Return to LifeOS after the browser callback.
6. Verify that identity and classification are visible separately.
7. Select **Verify identity** to perform the safe `/me` read.
8. Select **Sync Mail + Calendar** to run one bounded foreground read.
9. Review all imported records in Integration Inbox. They remain untrusted candidates.

## Bounded read behavior

- Mail defaults to the Inbox folder and a 14-day lookback.
- Calendar defaults to the primary calendar with a 30-day lookback and 90-day lookahead.
- Attachment bodies are not downloaded automatically.
- Only attachment metadata is normalized.
- Sync is foreground-only and bounded.
- Retry and recovery states remain visible.
- No external write action is implemented.

## Disconnect and deletion

Disconnecting:

- deletes the selected account's local DPAPI token;
- stops future reads until reconnect;
- keeps accepted LifeOS records;
- keeps review history and source provenance;
- does not silently delete authoritative LifeOS data.

To remove all Microsoft provider local configuration, close LifeOS and delete only the three local files listed above. Do not delete Integration Inbox or authoritative module stores unless separately reviewed.

## Tenant, publisher and production considerations

Before production distribution:

- deliberately choose supported account types;
- document tenant restrictions and admin-consent requirements;
- review publisher verification requirements;
- use environment-specific app registrations only when a real security/release boundary requires it;
- document production redirect URIs;
- publish privacy, account removal and token deletion procedures;
- keep diagnostics redacted;
- verify that no secret or token enters Git, CI logs or screenshot evidence.
