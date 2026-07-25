namespace LifeOS.Core.EvidenceAutomation;

public sealed record ScreenshotCandidate(
    string SourcePath,
    DateTimeOffset CapturedAt,
    string SuggestedGroup,
    string SuggestedName,
    bool IsOld);

public sealed record CompletionGateInput(
    bool ReadmeUpdated,
    int ScreenshotCount,
    bool TestsPassed,
    bool DesktopBuildPassed,
    bool MobileBuildPassed,
    bool ReleaseNotesUpdated,
    bool GitDiffClean,
    bool GitHubChecked);

public sealed record CompletionGateResult(
    bool Passed,
    IReadOnlyList<string> Missing);

public sealed class EvidenceAutomationService
{
    public IReadOnlyList<ScreenshotCandidate> Classify(
        IEnumerable<(string Path, DateTimeOffset CapturedAt)> files,
        DateTimeOffset cutoff,
        string group)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentException.ThrowIfNullOrWhiteSpace(group);

        int sequence = 0;
        return files
            .Where(file =>
                file.CapturedAt >= cutoff &&
                !file.Path.Contains(
                    $"{Path.DirectorySeparatorChar}old{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase))
            .OrderBy(file => file.CapturedAt)
            .Select(file =>
            {
                sequence++;
                return new ScreenshotCandidate(
                    file.Path,
                    file.CapturedAt,
                    group,
                    $"{sequence:00}_{group}_{Slug(Path.GetFileNameWithoutExtension(file.Path))}.png",
                    false);
            })
            .ToArray();
    }

    public CompletionGateResult Evaluate(CompletionGateInput input)
    {
        List<string> missing = new();
        if (!input.ReadmeUpdated) missing.Add("README");
        if (input.ScreenshotCount <= 0) missing.Add("screenshots");
        if (!input.TestsPassed) missing.Add("tests");
        if (!input.DesktopBuildPassed) missing.Add("desktop Release build");
        if (!input.MobileBuildPassed) missing.Add("mobile Release build");
        if (!input.ReleaseNotesUpdated) missing.Add("release notes");
        if (!input.GitDiffClean) missing.Add("git diff --check");
        if (!input.GitHubChecked) missing.Add("GitHub status");
        return new CompletionGateResult(missing.Count == 0, missing);
    }

    private static string Slug(string value)
    {
        char[] normalized = value
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();
        return string.Join('-', new string(normalized)
            .Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
