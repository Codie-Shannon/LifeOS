namespace LifeOS.Core.DocumentationHub;

public enum DocumentationAudience
{
    Public,
    BetaTester,
    Internal
}

public sealed record DocumentationEntry(
    string Id,
    string Title,
    string Summary,
    string Route,
    DocumentationAudience Audience,
    string Release,
    IReadOnlyList<string> Tags,
    bool KeepConciseInApp);

public sealed class DocumentationHubService
{
    public IReadOnlyList<DocumentationEntry> Search(
        IEnumerable<DocumentationEntry> entries,
        string query,
        DocumentationAudience maximumAudience)
    {
        string term = query.Trim();
        return entries
            .Where(entry => entry.Audience <= maximumAudience)
            .Where(entry =>
                term.Length == 0 ||
                entry.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                entry.Summary.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                entry.Tags.Any(tag => tag.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(entry => entry.Title)
            .ToArray();
    }

    public IReadOnlyList<DocumentationEntry> AppHelp(
        IEnumerable<DocumentationEntry> entries) =>
        entries
            .Where(entry => entry.KeepConciseInApp)
            .OrderBy(entry => entry.Title)
            .ToArray();

    public void ValidatePublicBoundary(DocumentationEntry entry)
    {
        if (entry.Audience == DocumentationAudience.Public &&
            (entry.Summary.Contains("private Codex", StringComparison.OrdinalIgnoreCase) ||
             entry.Summary.Contains(@"C:\", StringComparison.OrdinalIgnoreCase) ||
             entry.Summary.Contains("@", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Public documentation contains private or machine-specific context.");
        }
    }
}
