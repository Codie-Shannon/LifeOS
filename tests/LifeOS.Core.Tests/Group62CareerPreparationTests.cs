using Xunit;
using LifeOS.Core.CareerStudio;

namespace LifeOS.Core.Tests;

public sealed class Group62CareerPreparationTests
{
    private static (CareerMaterialsProof materials, CareerStudioProof career, DateTimeOffset now) Data() { var now = DateTimeOffset.UtcNow; return (CareerMaterialsProofData.Build(now), CareerProofData.Build(now), now); }
    [Fact] public void Cover_letter_uses_trusted_matched_evidence_only() { var d=Data(); var draft=new CareerPreparationService().CreateDraft("d", d.career.Opportunities[0].Id, d.materials.Variants[0], d.materials.Facts, d.materials.Review.Matches); Assert.All(draft.Sections.SelectMany(x=>x.SourceFactIds), id=>Assert.Contains(d.materials.Facts, f=>f.Id==id && f.IsTrusted)); }
    [Fact] public void Section_states_are_explicit_and_versioned() { var d=Data(); var proof=CareerPreparationProofData.Build(d.materials,d.career,d.now); Assert.Contains(proof.CoverLetter.Sections,x=>x.State==DraftSectionState.Generated); Assert.Contains(proof.CoverLetter.Sections,x=>x.State==DraftSectionState.Accepted); Assert.True(proof.CoverLetter.Version>1); }
    [Fact] public void Required_missing_or_stale_material_blocks_pack() { var d=Data(); var bad=new ApplicationPack("p",d.career.Opportunities[0].Id,new[]{new ApplicationPackItem("cv",ApplicationPackItemType.CV,"CV",null,true,MaterialFreshnessState.Missing)},1); Assert.False(bad.IsReady); Assert.Throws<InvalidOperationException>(()=>new CareerPreparationService().ValidatePack(bad,d.career.Opportunities[0].Id)); }
    [Fact] public void Pack_ownership_is_enforced() { var d=Data(); var proof=CareerPreparationProofData.Build(d.materials,d.career,d.now); Assert.Throws<InvalidOperationException>(()=>new CareerPreparationService().ValidatePack(proof.Pack,"other")); }
    [Fact] public void Interview_requires_evidenced_star_and_read_only_calendar() { var d=Data(); var proof=CareerPreparationProofData.Build(d.materials,d.career,d.now); new CareerPreparationService().ValidateInterview(proof.Interview); Assert.True(proof.Interview.CalendarReadOnly); Assert.All(proof.Interview.StarExamples,x=>Assert.NotEmpty(x.EvidenceIds)); }
    [Fact] public void Mobile_cards_hide_private_material() { var d=Data(); var proof=CareerPreparationProofData.Build(d.materials,d.career,d.now); Assert.All(proof.MobileCards,x=>Assert.True(x.IsPrivateSafe)); Assert.DoesNotContain(proof.MobileCards,x=>x.Summary.Contains("meeting link",StringComparison.OrdinalIgnoreCase)); }
}

