namespace LifeOS.Core.Intelligence;

public enum ExternalAiMode
{
    Off,
    AskEveryTime,
    Capped
}

public enum IntelligenceSetupChoice
{
    SetupNow,
    Later,
    Declined
}

public enum SuggestionState
{
    Proposed,
    Accepted,
    Rejected
}

public sealed record IntelligenceSettings(
    ExternalAiMode Mode,
    IntelligenceSetupChoice SetupChoice,
    decimal MonthlyCap,
    decimal UsedThisMonth,
    IReadOnlySet<string> AllowedCategories,
    string Provider);

public sealed record IntelligenceSignal(
    string Id,
    string Category,
    string Title,
    int Urgency,
    int Confidence,
    IReadOnlyList<string> Reasons,
    string Source);

public sealed record IntelligenceSuggestion(
    string Id,
    string Title,
    string ProposedAction,
    int Score,
    int Confidence,
    IReadOnlyList<string> Reasons,
    string Source,
    bool UsedExternalAi,
    SuggestionState State);

public sealed class IntelligenceService
{
    public IReadOnlyList<IntelligenceSuggestion> Rank(
        IEnumerable<IntelligenceSignal> signals)
    {
        return signals
            .Select(signal => new IntelligenceSuggestion(
                signal.Id,
                signal.Title,
                $"Review next action for {signal.Title}",
                Math.Clamp(signal.Urgency * 2 + signal.Confidence, 0, 300),
                signal.Confidence,
                signal.Reasons,
                signal.Source,
                false,
                SuggestionState.Proposed))
            .OrderByDescending(suggestion => suggestion.Score)
            .ThenBy(suggestion => suggestion.Title)
            .ToArray();
    }

    public bool CanUseExternalAi(
        IntelligenceSettings settings,
        string category,
        decimal estimatedCost,
        bool userConfirmed)
    {
        if (settings.Mode == ExternalAiMode.Off ||
            settings.SetupChoice != IntelligenceSetupChoice.SetupNow ||
            !settings.AllowedCategories.Contains(category))
        {
            return false;
        }

        if (settings.Mode == ExternalAiMode.AskEveryTime && !userConfirmed)
        {
            return false;
        }

        return settings.Mode != ExternalAiMode.Capped ||
               settings.UsedThisMonth + estimatedCost <= settings.MonthlyCap;
    }

    public IntelligenceSuggestion MarkExternalEnrichment(
        IntelligenceSuggestion suggestion,
        IntelligenceSettings settings,
        string category,
        decimal estimatedCost,
        bool userConfirmed)
    {
        if (!CanUseExternalAi(settings, category, estimatedCost, userConfirmed))
        {
            throw new InvalidOperationException("External AI use is not permitted for this request.");
        }

        return suggestion with
        {
            UsedExternalAi = true,
            Reasons = suggestion.Reasons
                .Append($"Optional {settings.Provider} enrichment was explicitly permitted.")
                .ToArray()
        };
    }

    public IntelligenceSuggestion Accept(IntelligenceSuggestion suggestion) =>
        suggestion.State == SuggestionState.Proposed
            ? suggestion with { State = SuggestionState.Accepted }
            : throw new InvalidOperationException("Only proposed suggestions can be accepted.");

    public IntelligenceSuggestion Reject(IntelligenceSuggestion suggestion) =>
        suggestion.State == SuggestionState.Proposed
            ? suggestion with { State = SuggestionState.Rejected }
            : throw new InvalidOperationException("Only proposed suggestions can be rejected.");
}
