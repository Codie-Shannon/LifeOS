using LifeOS.Core.ShellSearch;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups137To140ShellSearchTests
{
    private static readonly ShellSearchCandidate[] Candidates =
    [
        new("workspace-work", "Work", "Client delivery", ShellSearchTargetKind.Workspace, Workspace: "Work"),
        new("local-data-recovery", "Local Data & Recovery", "Versioned stores and Trash", ShellSearchTargetKind.Module, "Settings", "local-data-recovery", Keywords: ["backup", "storage"]),
        new("work-pipeline", "Work Pipeline", "Opportunities and delivery", ShellSearchTargetKind.Module, "Work", "work-pipeline", Keywords: ["clients"]),
        new("theme-light", "Theme light", "Apply the light appearance", ShellSearchTargetKind.Preference, CommandText: "Theme light", Keywords: ["appearance"])
    ];

    [Fact]
    public void Exact_title_outranks_keyword_and_description_matches()
    {
        IReadOnlyList<ShellSearchResult> results = ShellSearchService.Search("Work", Candidates);

        Assert.Equal("workspace-work", results[0].Candidate.Id);
        Assert.Equal("Exact title", results[0].MatchReason);
        Assert.Contains(results, result => result.Candidate.Id == "work-pipeline");
    }

    [Fact]
    public void Multiple_query_tokens_must_all_match_the_candidate()
    {
        ShellSearchResult result = Assert.Single(
            ShellSearchService.Search("local backup", Candidates));

        Assert.Equal("local-data-recovery", result.Candidate.Id);
    }

    [Fact]
    public void Punctuation_and_case_do_not_change_search_identity()
    {
        ShellSearchResult result = Assert.Single(
            ShellSearchService.Search("LOCAL-DATA recovery", Candidates));

        Assert.Equal("local-data-recovery", result.Candidate.Id);
    }

    [Fact]
    public void Unknown_query_returns_an_honest_empty_result()
    {
        Assert.Empty(ShellSearchService.Search("definitely unavailable", Candidates));
    }

    [Fact]
    public void Empty_query_browses_deterministically_with_a_result_limit()
    {
        IReadOnlyList<ShellSearchResult> results = ShellSearchService.Search("", Candidates, 2);

        Assert.Equal(2, results.Count);
        Assert.All(results, result => Assert.Equal("Browse", result.MatchReason));
    }

    [Fact]
    public void Duplicate_candidate_ids_are_not_executed_twice()
    {
        ShellSearchCandidate duplicate = Candidates[0] with { Label = "Work duplicate" };

        Assert.Single(ShellSearchService.Search("workspace work", [.. Candidates, duplicate]));
    }

    [Fact]
    public void Invalid_result_limit_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ShellSearchService.Search("work", Candidates, 0));
    }
}
