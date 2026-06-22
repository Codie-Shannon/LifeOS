namespace LifeOS.Shared.Shell;

public sealed class LifeOSModuleDefinition
{
    public required LifeOSModuleKind Kind { get; init; }

    public required string Title { get; init; }

    public required string ShortDescription { get; init; }

    public required string DetailDescription { get; init; }

    public required string Badge { get; init; }

    public string PlatformRole { get; init; } = "Shared LifeOS module";

    public string NextBuildFocus { get; init; } = "Not scheduled yet.";

    public bool IsDesktopOnly { get; init; }

    public bool IsSharedCoreModule { get; init; }

    public bool IsBuilt { get; init; }
}