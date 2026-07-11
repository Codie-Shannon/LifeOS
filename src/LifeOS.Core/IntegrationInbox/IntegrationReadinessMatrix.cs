namespace LifeOS.Core.IntegrationInbox;

public static class IntegrationReadinessMatrix
{
    public static IReadOnlyList<IntegrationReadinessItem> Create() =>
    [
        new() { Connector = "Manual CSV / JSON intake", Capability = "Read-only preview intake", CurrentState = "Active - local import only", RequiredBeforeV5 = "Explicit preview and confirmation", ContractReady = true, LiveConnectionActive = false },
        new() { Connector = "Local ICS calendar intake", Capability = "Read-only calendar previews", CurrentState = "Active - local import only", RequiredBeforeV5 = "Bounded local file import and duplicate rules", ContractReady = true, LiveConnectionActive = false },
        new() { Connector = "Google Calendar", Capability = "Authenticated event previews", CurrentState = "Active - manual bounded refresh", RequiredBeforeV5 = "calendar.readonly, explicit connection, review gate", ContractReady = true, LiveConnectionActive = true },
        new() { Connector = "Local Email Radar import", Capability = "Provider-neutral communication evidence", CurrentState = "Active - JSON / CSV import", RequiredBeforeV5 = "Confirmation, duplicate detection, candidate review", ContractReady = true, LiveConnectionActive = false },
        new() { Connector = "Gmail", Capability = "Authenticated communication evidence", CurrentState = "Active - manual profile-bound bounded search", RequiredBeforeV5 = "gmail.readonly, explicit retrieval confirmation, candidate review", ContractReady = true, LiveConnectionActive = true },
        new() { Connector = "Outlook / Microsoft Graph", Capability = "Email and calendar", CurrentState = "Not active", RequiredBeforeV5 = "Separate future provider decision", ContractReady = false, LiveConnectionActive = false }
    ];
}
