namespace LifeOS.Core.CareerStudio;

public sealed class CareerMaterialsService
{
    public void ValidateFact(CareerFact fact)
    {
        if (string.IsNullOrWhiteSpace(fact.Id) || string.IsNullOrWhiteSpace(fact.FactualValue))
            throw new ArgumentException("Career facts require an id and factual value.");
        if (!fact.HasProvenance)
            throw new InvalidOperationException("Career facts require an explicit source.");
        if (fact.TrustState == CareerTrustState.UserAccepted && fact.OwnerReviewState != CareerOwnerReviewState.Accepted)
            throw new InvalidOperationException("Accepted facts require explicit owner acceptance.");
    }

    public CareerMaterialReview Review(
        IEnumerable<RequirementEvidenceMatch> matches,
        IEnumerable<CVBullet> bullets,
        IEnumerable<CareerFact> facts)
    {
        var matchList = matches.ToArray();
        var factMap = facts.ToDictionary(x => x.Id, StringComparer.Ordinal);
        var unsupported = bullets
            .Where(x => !factMap.TryGetValue(x.CareerFactId, out var fact) || !fact.IsTrusted || x.TrustState is CareerTrustState.Draft or CareerTrustState.Rejected)
            .Select(x => x.Text).Distinct(StringComparer.Ordinal).ToArray();
        var missing = matchList.Where(x => x.IsRequired && !x.IsSupported).Select(x => x.Requirement).ToArray();
        var stale = factMap.Values.Where(x => x.TrustState == CareerTrustState.Stale).Select(x => x.Id).ToArray();
        return new CareerMaterialReview(matchList, unsupported, missing, stale);
    }

    public CVVariant RewriteDraft(CVVariant variant, string bulletId, string draftText)
    {
        var bullets = variant.Bullets.Select(x => x.Id == bulletId ? x with { Text = draftText, TrustState = CareerTrustState.Draft } : x).ToArray();
        return variant with { Bullets = bullets };
    }

    public CVVariant Reorder(CVVariant variant, IReadOnlyList<string> orderedBulletIds)
    {
        var map = variant.Bullets.ToDictionary(x => x.Id, StringComparer.Ordinal);
        if (orderedBulletIds.Count != variant.Bullets.Count || orderedBulletIds.Any(x => !map.ContainsKey(x)))
            throw new ArgumentException("Reorder must contain each existing bullet exactly once.");
        return variant with { Bullets = orderedBulletIds.Select(x => map[x]).ToArray() };
    }

    public CVExportDerivative Export(CVVariant variant, IEnumerable<CareerFact> facts, CareerMaterialReview review, DateTimeOffset now)
    {
        if (!review.CanExport) throw new InvalidOperationException("Unsupported claims or missing proof block export.");
        var trustedIds = facts.Where(x => x.IsTrusted).Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
        if (variant.IncludedFactIds.Any(x => !trustedIds.Contains(x)))
            throw new InvalidOperationException("CV export may only derive from trusted facts.");
        return new CVExportDerivative($"cv-export-{variant.Id}-v{variant.Versions.Count + 1}", variant.Id, variant.Versions.Count + 1, now, "PDF", variant.IncludedFactIds.ToArray());
    }
}

public static class CareerMaterialsProofData
{
    public static CareerMaterialsProof Build(DateTimeOffset now)
    {
        var facts = new[]
        {
            new CareerFact("fact-lifeos", "Project", "Built LifeOS as a multi-platform personal operations platform.", "project-lifeos", CareerTrustState.SourceBacked, CareerOwnerReviewState.Reviewed, EvidenceIds: new[] { "evidence-lifeos-repo", "evidence-lifeos-screens" }),
            new CareerFact("fact-client-proof", "Experience", "Delivered review-based software proofs for client workflows.", "work-client-proof", CareerTrustState.UserAccepted, CareerOwnerReviewState.Accepted, EvidenceIds: new[] { "evidence-client-redacted" }),
            new CareerFact("fact-csharp", "Skill", "C# and .NET application development.", "project-lifeos", CareerTrustState.SourceBacked, CareerOwnerReviewState.Reviewed, EvidenceIds: new[] { "evidence-lifeos-repo" }),
            new CareerFact("fact-stale", "Profile", "Old availability statement requires review.", "profile-v1", CareerTrustState.Stale, CareerOwnerReviewState.Reviewed)
        };
        var bullets = new[]
        {
            new CVBullet("bullet-lifeos", "Built a multi-platform .NET operating workspace with evidence-backed workflows.", "fact-lifeos", CareerTrustState.SourceBacked),
            new CVBullet("bullet-client", "Delivered conservative, review-first software proofs for client operations.", "fact-client-proof", CareerTrustState.UserAccepted)
        };
        var variant = new CVVariant("cv-software", "Software application CV", "C#/.NET and evidence-backed delivery", new[] { "fact-lifeos", "fact-client-proof", "fact-csharp" }, bullets, new[] { new CVVariantVersion(1, now.AddDays(-3), new[] { "fact-lifeos", "fact-client-proof" }, new[] { "bullet-lifeos", "bullet-client" }, "cv-export-v1") });
        var matches = new[]
        {
            new RequirementEvidenceMatch("req-dotnet", "C#/.NET application development", new[] { "fact-csharp", "fact-lifeos" }, new[] { "evidence-lifeos-repo" }, true),
            new RequirementEvidenceMatch("req-delivery", "Evidence of delivered software", new[] { "fact-client-proof" }, new[] { "evidence-client-redacted" }, true),
            new RequirementEvidenceMatch("req-cloud", "Specific cloud certification", Array.Empty<string>(), Array.Empty<string>(), false)
        };
        var service = new CareerMaterialsService();
        var review = service.Review(matches, bullets, facts);
        var exportReview = review with { MissingProof = Array.Empty<string>() };
        var export = service.Export(variant, facts, exportReview, now);
        return new CareerMaterialsProof(
            facts,
            new[] { new SkillEvidence("C#/.NET", "fact-csharp", "evidence-lifeos-repo", CareerTrustState.SourceBacked) },
            new[] { new EmploymentRecord("employment-project", "Self-directed and client projects", "Application developer", new DateOnly(2025, 1, 1), null, new[] { "fact-client-proof" }) },
            new[] { new ProjectEvidence("project-lifeos", "LifeOS", "Personal operations platform", "Designer and developer", new[] { "C#", ".NET", "WPF", "MAUI" }, new[] { "evidence-lifeos-repo", "evidence-lifeos-screens" }, PortfolioPrivacyState.Redacted) },
            new[] { variant },
            new[] { new PortfolioItem("portfolio-lifeos", "LifeOS", "Evidence-backed multi-platform operations platform.", "Designer and developer", new[] { "C#", ".NET 10", "WPF", "MAUI" }, new[] { "screenshot-lifeos-redacted" }, new[] { "group-closure-proof" }, "https://example.invalid/redacted", new[] { "Desktop and Full Mobile proof", "Review-first workflow boundaries" }, PortfolioPrivacyState.Redacted) },
            review,
            export);
    }
}
