using LifeOS.Core.Intelligence;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups83To86IntelligenceTests
{
    private readonly IntelligenceService _service = new();

    [Fact]
    public void Native_ranking_never_requires_external_ai()
    {
        IntelligenceSuggestion suggestion = _service.Rank(new[]
        {
            new IntelligenceSignal("bill", "money", "Power bill", 90, 85, new[] { "Due tomorrow", "Confirmed source" }, "local")
        }).Single();

        Assert.False(suggestion.UsedExternalAi);
        Assert.Equal(SuggestionState.Proposed, suggestion.State);
        Assert.Contains("Due tomorrow", suggestion.Reasons);
    }

    [Theory]
    [InlineData(ExternalAiMode.Off, true, false)]
    [InlineData(ExternalAiMode.AskEveryTime, false, false)]
    [InlineData(ExternalAiMode.AskEveryTime, true, true)]
    public void External_ai_respects_off_and_ask_modes(
        ExternalAiMode mode,
        bool confirmed,
        bool expected)
    {
        IntelligenceSettings settings = Settings(mode);

        Assert.Equal(expected, _service.CanUseExternalAi(settings, "money", 0.10m, confirmed));
    }

    [Fact]
    public void Cost_cap_and_category_permissions_are_hard_boundaries()
    {
        IntelligenceSettings settings = Settings(ExternalAiMode.Capped) with
        {
            MonthlyCap = 5m,
            UsedThisMonth = 4.95m
        };

        Assert.False(_service.CanUseExternalAi(settings, "money", 0.10m, true));
        Assert.False(_service.CanUseExternalAi(settings, "health", 0.01m, true));
    }

    [Fact]
    public void Suggestions_require_a_separate_review_decision()
    {
        IntelligenceSuggestion proposed = _service.Rank(new[]
        {
            new IntelligenceSignal("follow-up", "work", "Supplier follow-up", 40, 80, new[] { "Waiting three days" }, "local")
        }).Single();

        Assert.Equal(SuggestionState.Accepted, _service.Accept(proposed).State);
        Assert.Equal(SuggestionState.Rejected, _service.Reject(proposed).State);
    }

    private static IntelligenceSettings Settings(ExternalAiMode mode) =>
        new(
            mode,
            IntelligenceSetupChoice.SetupNow,
            5m,
            0m,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "money", "work" },
            "OpenAI");
}
