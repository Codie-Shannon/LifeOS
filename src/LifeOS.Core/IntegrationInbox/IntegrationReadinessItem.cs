namespace LifeOS.Core.IntegrationInbox;

public sealed class IntegrationReadinessItem
{
    public string Connector { get; init; } = "";
    public string Capability { get; init; } = "";
    public string CurrentState { get; init; } = "";
    public string RequiredBeforeV5 { get; init; } = "";
    public bool ContractReady { get; init; }
    public bool LiveConnectionActive { get; init; }
}
