# LifeOS current status

LifeOS Desktop is at `v9.0.0-alpha.3`. Groups 46, 47 and 48 are complete and closed. Desktop v8 remains complete and closed at `v8.0.0-beta.1`; Website remains complete and closed at `v0.3.0-beta.1`; Companion remains a separate application, complete and closed at `v0.1.0-beta.1`.

Group 48 closes the single Microsoft provider registration boundary and the first live Microsoft integrations: read-only Outlook Mail and Microsoft Calendar. Microsoft Provider Setup is embedded under Settings and supports explicit Personal/Work account classification, browser PKCE consent, DPAPI-protected per-user token storage, safe identity verification, bounded foreground synchronization, attachment metadata without automatic body download, Integration Inbox normalization, reviewable suggestions, partial-permission recovery and safe disconnect.

Live proof verified two separately classified Microsoft identities, successful Outlook Mail reads, a successful personal-calendar read, attachment metadata, source provenance, reviewable follow-up generation and recovery from a deliberately invalid calendar selection. The final clean sync returned `50 mail`, `1 calendar` and `44 suggestions` with the provider connected and no current error.

The complete planned Microsoft capability catalogue is recorded once. Only identity, `Mail.Read` and `Calendars.Read` are enabled for Group 48. Contacts/People, OneDrive, SharePoint, Teams, Power BI and Power Automate remain unimplemented and Not Requested. Background synchronization and all external writes remain disabled.

Exactly eight approved Group 48 screenshots are committed. Full email addresses, opaque provider message IDs, thread IDs and raw provider references are redacted where shown. No Microsoft token, authorization code, client secret, message body, attachment body or private configuration file is committed.

Group 49 has not started.
