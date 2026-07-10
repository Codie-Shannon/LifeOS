namespace LifeOS.Core.IntegrationInbox;

public static class IntegrationReadinessMatrix
{
    public static IReadOnlyList<IntegrationReadinessItem> Create() =>
    [
        new() { Connector = "Gmail / Outlook", Capability = "Email thread previews", CurrentState = "Local contract ready", RequiredBeforeV5 = "OAuth, scoped read, profile rules, user confirmation", ContractReady = true },
        new() { Connector = "Google / Outlook Calendar", Capability = "Event previews", CurrentState = "Local contract ready", RequiredBeforeV5 = "OAuth, date mapping, duplicate rules", ContractReady = true },
        new() { Connector = "Xero / accounting", Capability = "Invoice and bill previews", CurrentState = "Local contract ready", RequiredBeforeV5 = "OAuth, tenant selection, money trust rules", ContractReady = true },
        new() { Connector = "SharePoint / Drive", Capability = "File metadata previews", CurrentState = "Local contract ready", RequiredBeforeV5 = "OAuth, source links, file permission checks", ContractReady = true },
        new() { Connector = "Receipt OCR", Capability = "Receipt field candidates", CurrentState = "Review contract ready", RequiredBeforeV5 = "OCR provider, source-image retention, confidence mapping", ContractReady = true },
        new() { Connector = "Banking", Capability = "Transaction previews", CurrentState = "Deferred", RequiredBeforeV5 = "Provider decision, legal/security review, strict read-only design", ContractReady = false },
    ];
}
