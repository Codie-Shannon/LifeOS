using Xunit;
using LifeOS.Core.CareerStudio;

namespace LifeOS.Core.Tests;

public sealed class Group62CareerMaterialsTests
{
    [Fact] public void CareerFact_requires_provenance() => Assert.Throws<InvalidOperationException>(() => new CareerMaterialsService().ValidateFact(new CareerFact("x", "Skill", "C#", "", CareerTrustState.SourceBacked, CareerOwnerReviewState.Reviewed)));
    [Fact] public void Accepted_fact_requires_owner_acceptance() => Assert.Throws<InvalidOperationException>(() => new CareerMaterialsService().ValidateFact(new CareerFact("x", "Skill", "C#", "source", CareerTrustState.UserAccepted, CareerOwnerReviewState.Reviewed)));
    [Fact] public void Proof_data_links_portfolio_to_evidence() { var proof = CareerMaterialsProofData.Build(DateTimeOffset.UtcNow); Assert.NotEmpty(proof.Portfolio[0].DocumentIds); Assert.NotEmpty(proof.Projects[0].EvidenceIds); }
    [Fact] public void Unsupported_draft_claim_blocks_export() { var proof = CareerMaterialsProofData.Build(DateTimeOffset.UtcNow); var bad = proof.Variants[0] with { Bullets = proof.Variants[0].Bullets.Append(new CVBullet("bad", "Invented claim", "missing", CareerTrustState.Draft)).ToArray() }; var review = new CareerMaterialsService().Review(proof.Review.Matches, bad.Bullets, proof.Facts); Assert.NotEmpty(review.UnsupportedClaims); Assert.Throws<InvalidOperationException>(() => new CareerMaterialsService().Export(bad, proof.Facts, review, DateTimeOffset.UtcNow)); }
    [Fact] public void Rewrite_remains_draft_and_does_not_mutate_source() { var proof = CareerMaterialsProofData.Build(DateTimeOffset.UtcNow); var original = proof.Variants[0]; var rewritten = new CareerMaterialsService().RewriteDraft(original, "bullet-lifeos", "Draft wording"); Assert.Equal(CareerTrustState.Draft, rewritten.Bullets[0].TrustState); Assert.NotEqual(rewritten.Bullets[0].Text, original.Bullets[0].Text); }
    [Fact] public void Variants_preserve_version_history() { var proof = CareerMaterialsProofData.Build(DateTimeOffset.UtcNow); Assert.Single(proof.Variants[0].Versions); Assert.Equal("PDF", proof.Export.Format); Assert.All(proof.Export.SourceFactIds, id => Assert.Contains(proof.Facts, f => f.Id == id && f.IsTrusted)); }
    [Fact] public void Privacy_state_is_explicit() { var proof = CareerMaterialsProofData.Build(DateTimeOffset.UtcNow); Assert.Equal(PortfolioPrivacyState.Redacted, proof.Portfolio[0].PrivacyState); }
}

