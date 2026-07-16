using System.Text.Json.Serialization;

namespace LifeOS.Core.IntegrationConnectors.GoogleWorkspace;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GoogleWorkspaceCapability
{
    Gmail,
    Calendar,
    Drive,
    Contacts,
    Tasks
}

public sealed record GoogleWorkspaceApiDefinition(
    GoogleWorkspaceCapability Capability,
    string ApiName,
    string ReadOnlyScope,
    string CandidateType,
    bool EnabledInProject);

public sealed class GoogleWorkspaceProviderConfiguration
{
    public const string ProviderId = "google";
    public const string ProjectBoundary =
        "One complete LifeOS Google Cloud project";
    public const string DesktopClientBoundary =
        "Desktop OAuth loopback client";
    public const string FutureClientBoundary =
        "Companion, Full Mobile and Portal clients are documented separately and reuse the same project";

    public string ProjectDisplayName { get; init; } =
        "LifeOS Google Workspace";

    public string AccountClassification { get; init; } = "Personal";

    public IReadOnlyList<GoogleWorkspaceApiDefinition> Catalogue { get; init; } =
    [
        new(
            GoogleWorkspaceCapability.Gmail,
            "Gmail API",
            "https://www.googleapis.com/auth/gmail.readonly",
            "Message / thread",
            true),
        new(
            GoogleWorkspaceCapability.Calendar,
            "Google Calendar API",
            "https://www.googleapis.com/auth/calendar.readonly",
            "Calendar event",
            true),
        new(
            GoogleWorkspaceCapability.Drive,
            "Google Drive API",
            "https://www.googleapis.com/auth/drive.readonly",
            "File document",
            true),
        new(
            GoogleWorkspaceCapability.Contacts,
            "People API",
            "https://www.googleapis.com/auth/contacts.readonly",
            "Contact person",
            true),
        new(
            GoogleWorkspaceCapability.Tasks,
            "Google Tasks API",
            "https://www.googleapis.com/auth/tasks.readonly",
            "Task",
            true)
    ];

    public IReadOnlyList<string> RequestedReadOnlyScopes =>
        Catalogue
            .Where(item => item.EnabledInProject)
            .Select(item => item.ReadOnlyScope)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    public void Validate()
    {
        if (Catalogue.Count != 5)
        {
            throw new InvalidOperationException(
                "The complete Google Workspace catalogue must contain exactly five capabilities.");
        }

        if (Catalogue.Any(item =>
            !item.EnabledInProject ||
            !item.ReadOnlyScope.EndsWith(".readonly", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                "Every enabled Google Workspace capability must use a read-only scope.");
        }

        if (RequestedReadOnlyScopes.Any(scope =>
            scope.Contains("modify", StringComparison.OrdinalIgnoreCase) ||
            scope.Contains("compose", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "Google Workspace write scopes are forbidden.");
        }
    }
}
