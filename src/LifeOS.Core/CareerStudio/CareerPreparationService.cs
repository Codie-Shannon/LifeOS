namespace LifeOS.Core.CareerStudio;

public sealed class CareerPreparationService
{
    public CoverLetterDraft CreateDraft(string id, string opportunityId, CVVariant variant, IEnumerable<CareerFact> facts, IEnumerable<RequirementEvidenceMatch> matches)
    {
        var trusted = facts.Where(x => x.IsTrusted).ToDictionary(x => x.Id, StringComparer.Ordinal);
        var supported = matches.Where(x => x.IsSupported).SelectMany(x => x.MatchedFactIds).Distinct(StringComparer.Ordinal).Where(trusted.ContainsKey).ToArray();
        if (supported.Length == 0) throw new InvalidOperationException("Cover-letter drafts require trusted matched evidence.");
        return new CoverLetterDraft(id, opportunityId, variant.Id, new[]
        {
            new CoverLetterSection("opening", "Opening", "Draft opening for the selected opportunity; review required.", DraftSectionState.Generated, supported),
            new CoverLetterSection("evidence", "Relevant evidence", string.Join(" ", supported.Select(x => trusted[x].FactualValue)), DraftSectionState.Generated, supported),
            new CoverLetterSection("closing", "Closing", "User-authored closing and availability statement.", DraftSectionState.Manual, Array.Empty<string>())
        }, 1);
    }

    public CoverLetterDraft SetSectionState(CoverLetterDraft draft, string sectionId, DraftSectionState state)
    {
        if (state == DraftSectionState.Accepted && draft.Sections.First(x => x.Id == sectionId).State == DraftSectionState.Stale)
            throw new InvalidOperationException("Stale sections must be refreshed before acceptance.");
        return draft with { Sections = draft.Sections.Select(x => x.Id == sectionId ? x with { State = state } : x).ToArray(), Version = draft.Version + 1 };
    }

    public void ValidatePack(ApplicationPack pack, string opportunityId)
    {
        if (!string.Equals(pack.OpportunityId, opportunityId, StringComparison.Ordinal)) throw new InvalidOperationException("Application pack belongs to another opportunity.");
        if (!pack.IsReady) throw new InvalidOperationException("Required application materials are missing or stale.");
    }

    public void ValidateInterview(InterviewPrepPlan plan)
    {
        if (string.IsNullOrWhiteSpace(plan.InterviewId) || !plan.CalendarReadOnly) throw new InvalidOperationException("Interview preparation requires a read-only linked interview context.");
        if (plan.StarExamples.Any(x => x.EvidenceIds.Count == 0)) throw new InvalidOperationException("STAR examples require evidence.");
    }

    public IReadOnlyList<MobilePreparationCard> BuildMobileCards(ApplicationPack pack, InterviewPrepPlan plan, DateTimeOffset now) => new[]
    {
        new MobilePreparationCard("Application pack", pack.IsReady ? "Required materials are current." : "Missing or stale materials need review.", pack.IsReady ? "READY FOR REVIEW" : "ACTION REQUIRED", true),
        new MobilePreparationCard("Interview countdown", $"{Math.Max(0, (int)Math.Ceiling((plan.StartsUtc-now).TotalDays))} days • {plan.Format}", "READ-ONLY CALENDAR", true),
        new MobilePreparationCard("Preparation checks", $"{plan.Checks.Count(x => x.State == PreparationCheckState.Complete)}/{plan.Checks.Count} complete", "OFFLINE-SAFE ACTIONS", true)
    };
}

public static class CareerPreparationProofData
{
    public static CareerPreparationProof Build(CareerMaterialsProof materials, CareerStudioProof career, DateTimeOffset now)
    {
        var opportunity = career.Opportunities[0];
        var service = new CareerPreparationService();
        var letter = service.CreateDraft("cover-letter-01", opportunity.Id, materials.Variants[0], materials.Facts, materials.Review.Matches);
        letter = service.SetSectionState(letter, "evidence", DraftSectionState.Accepted);
        var pack = new ApplicationPack("pack-01", opportunity.Id, new[]
        {
            new ApplicationPackItem("pack-cv", ApplicationPackItemType.CV, "Role-specific CV", materials.Variants[0].Id, true, MaterialFreshnessState.Current),
            new ApplicationPackItem("pack-letter", ApplicationPackItemType.CoverLetter, "Reviewed cover letter", letter.Id, true, MaterialFreshnessState.Current),
            new ApplicationPackItem("pack-portfolio", ApplicationPackItemType.Portfolio, "Selected portfolio proof", materials.Portfolio[0].Id, false, MaterialFreshnessState.Current),
            new ApplicationPackItem("pack-references", ApplicationPackItemType.ReferencesPlaceholder, "References placeholder", null, false, MaterialFreshnessState.Missing),
            new ApplicationPackItem("pack-support", ApplicationPackItemType.SupportingFile, "Older supporting file", "support-old", false, MaterialFreshnessState.Stale)
        }, 1);
        var interview = new InterviewPrepPlan("prep-01", opportunity.Id, "interview-redacted-01", now.AddDays(3), "Video interview", "Private meeting link hidden • test audio 20 minutes early", new[]
        {
            new InterviewQuestion("q1", "Describe a project where evidence and safety boundaries mattered.", "User-curated likely question", true),
            new InterviewQuestion("q2", "How do you handle unsupported requirements?", "Opportunity requirement mapping", true)
        }, new[] { new InterviewAnswerDraft("a1", "q1", "User-authored answer draft linked to LifeOS proof.", DraftSectionState.Manual, new[] { "fact-lifeos" }) },
        new[] { new STARExample("star-lifeos", "Review-first delivery", "A workflow required trustworthy automation.", "Build a useful proof without making final decisions.", "Separated source facts from generated wording and added explicit review states.", "Produced an auditable proof while preserving human approval.", new[] { "evidence-lifeos-repo" }) },
        new[] { "What would success look like in the first 90 days?", "Which systems and review boundaries matter most?" },
        new[] { new PreparationCheck("check-role", "Review role requirements", PreparationCheckState.Complete), new PreparationCheck("check-audio", "Test audio and camera", PreparationCheckState.Open), new PreparationCheck("check-examples", "Review STAR examples", PreparationCheckState.Complete) }, true);
        service.ValidateInterview(interview);
        return new CareerPreparationProof(letter, pack, interview, service.BuildMobileCards(pack, interview, now));
    }
}
