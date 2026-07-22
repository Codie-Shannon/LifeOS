namespace LifeOS.Core.CareerStudio;

public enum DraftSectionState { Generated, Manual, Accepted, Rejected, Stale }
public enum PreparationCheckState { Open, Complete, Blocked, Stale }
public enum ApplicationPackItemType { CV, CoverLetter, Portfolio, ReferencesPlaceholder, SupportingFile }

public sealed record CoverLetterSection(string Id, string Heading, string Text, DraftSectionState State, IReadOnlyList<string> SourceFactIds);
public sealed record CoverLetterDraft(string Id, string OpportunityId, string CVVariantId, IReadOnlyList<CoverLetterSection> Sections, int Version);
public sealed record ApplicationPackItem(string Id, ApplicationPackItemType Type, string Label, string? MaterialId, bool Required, MaterialFreshnessState Freshness);
public sealed record ApplicationPack(string Id, string OpportunityId, IReadOnlyList<ApplicationPackItem> Items, int Version)
{
    public bool IsReady => Items.Where(x => x.Required).All(x => x.MaterialId is not null && x.Freshness == MaterialFreshnessState.Current);
}
public sealed record InterviewQuestion(string Id, string Prompt, string Source, bool UserAuthored);
public sealed record InterviewAnswerDraft(string Id, string QuestionId, string Text, DraftSectionState State, IReadOnlyList<string> SourceFactIds);
public sealed record STARExample(string Id, string Title, string Situation, string Task, string Action, string Result, IReadOnlyList<string> EvidenceIds);
public sealed record PreparationCheck(string Id, string Label, PreparationCheckState State);
public sealed record InterviewPrepPlan(string Id, string OpportunityId, string InterviewId, DateTimeOffset StartsUtc, string Format, string Logistics, IReadOnlyList<InterviewQuestion> Questions, IReadOnlyList<InterviewAnswerDraft> Answers, IReadOnlyList<STARExample> StarExamples, IReadOnlyList<string> QuestionsToAsk, IReadOnlyList<PreparationCheck> Checks, bool CalendarReadOnly);
public sealed record MobilePreparationCard(string Title, string Summary, string Badge, bool IsPrivateSafe);
public sealed record CareerPreparationProof(CoverLetterDraft CoverLetter, ApplicationPack Pack, InterviewPrepPlan Interview, IReadOnlyList<MobilePreparationCard> MobileCards);
