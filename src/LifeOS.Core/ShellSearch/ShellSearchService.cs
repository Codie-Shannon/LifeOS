using System.Text;

namespace LifeOS.Core.ShellSearch;

public enum ShellSearchTargetKind
{
    Workspace,
    Module,
    Preference
}

public sealed record ShellSearchCandidate(
    string Id,
    string Label,
    string Description,
    ShellSearchTargetKind Kind,
    string? Workspace = null,
    string? RouteId = null,
    string? CommandText = null,
    IReadOnlyList<string>? Keywords = null);

public sealed record ShellSearchResult(
    ShellSearchCandidate Candidate,
    int Score,
    string MatchReason);

public static class ShellSearchService
{
    public static IReadOnlyList<ShellSearchResult> Search(
        string? query,
        IEnumerable<ShellSearchCandidate> candidates,
        int maximumResults = 8)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (maximumResults < 1)
            throw new ArgumentOutOfRangeException(nameof(maximumResults));

        ShellSearchCandidate[] available = candidates
            .Where(candidate =>
                !string.IsNullOrWhiteSpace(candidate.Id) &&
                !string.IsNullOrWhiteSpace(candidate.Label))
            .GroupBy(candidate => candidate.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        string normalizedQuery = Normalize(query);

        if (normalizedQuery.Length == 0)
        {
            return available
                .OrderBy(candidate => candidate.Kind)
                .ThenBy(candidate => candidate.Label, StringComparer.OrdinalIgnoreCase)
                .Take(maximumResults)
                .Select(candidate => new ShellSearchResult(candidate, 0, "Browse"))
                .ToArray();
        }

        string[] queryTokens = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return available
            .Select(candidate => Score(candidate, normalizedQuery, queryTokens))
            .Where(result => result is not null)
            .Cast<ShellSearchResult>()
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Candidate.Kind)
            .ThenBy(result => result.Candidate.Label, StringComparer.OrdinalIgnoreCase)
            .Take(maximumResults)
            .ToArray();
    }

    private static ShellSearchResult? Score(
        ShellSearchCandidate candidate,
        string query,
        IReadOnlyList<string> queryTokens)
    {
        string label = Normalize(candidate.Label);
        string id = Normalize(candidate.Id);
        string description = Normalize(candidate.Description);
        string command = Normalize(candidate.CommandText);
        string keywords = Normalize(string.Join(' ', candidate.Keywords ?? []));
        string haystack = string.Join(' ', new[] { label, id, description, command, keywords });

        if (queryTokens.Any(token => !haystack.Contains(token, StringComparison.Ordinal)))
            return null;

        int score = 100;
        string reason = "Keywords";
        if (label == query)
        {
            score += 1000;
            reason = "Exact title";
        }
        else if (id == query || command == query)
        {
            score += 900;
            reason = "Exact command";
        }
        else if (label.StartsWith(query, StringComparison.Ordinal))
        {
            score += 700;
            reason = "Title starts with query";
        }
        else if (label.Contains(query, StringComparison.Ordinal))
        {
            score += 550;
            reason = "Title contains query";
        }
        else if (keywords.Contains(query, StringComparison.Ordinal))
        {
            score += 350;
            reason = "Keyword match";
        }
        else if (description.Contains(query, StringComparison.Ordinal))
        {
            score += 200;
            reason = "Description match";
        }

        string[] labelTokens = label.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        score += queryTokens.Count(queryToken =>
            labelTokens.Any(labelToken => labelToken.StartsWith(queryToken, StringComparison.Ordinal))) * 40;

        return new ShellSearchResult(candidate, score, reason);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        StringBuilder builder = new(value.Length);
        bool previousSpace = true;
        foreach (char character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousSpace = false;
            }
            else if (!previousSpace)
            {
                builder.Append(' ');
                previousSpace = true;
            }
        }
        return builder.ToString().Trim();
    }
}
