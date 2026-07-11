using Xunit;
using LifeOS.Core;
using LifeOS.Core.EmailRadar;
using LifeOS.Shared.EmailRadar;

namespace LifeOS.Core.Tests;

public sealed class EmailRadarTests
{
    private static EmailRadarProfile Profile() => new() { Name="Harbour Workshop", RelatedLabel="Harbour Workshop", EmailAddresses=["morgan@harbour-workshop.example.invalid"], SubjectPhrases=["Workshop update"], Keywords=["proof","invoice"], ExcludeTerms=["newsletter"], FollowUpDays=7 };
    private static ImportedCommunicationRecord Record(string sender="morgan@harbour-workshop.example.invalid", string subject="Workshop update and proof", int days=-2) { var r=new ImportedCommunicationRecord{SourceKind="test",SentAt=DateTimeOffset.Now.AddDays(days),Sender=sender,Recipients=["owner@example.invalid"],Subject=subject,Text="Proof and invoice update",Provenance="test#1"}; r.DuplicateKey=CommunicationImportService.BuildDuplicateKey(r); return r; }

    [Fact] public void ProductVersion_IsFormalGroup24Version() => Assert.Equal("v5.0.0-alpha.4", ProductVersion.Display);
    [Fact] public void Profile_Validates() => Profile().Validate();
    [Fact] public void Profile_RequiresName() { var p=Profile(); p.Name=""; Assert.Throws<ArgumentException>(p.Validate); }
    [Fact] public void Profile_RejectsInvalidDateRange() { var p=Profile(); p.DateFrom=DateTimeOffset.Now; p.DateTo=DateTimeOffset.Now.AddDays(-1); Assert.Throws<ArgumentException>(p.Validate); }
    [Fact] public void JsonImport_ParsesValidRecord() { var x=CommunicationImportService.PreviewJson("[{\"sentAt\":\"2026-07-01T10:00:00+12:00\",\"sender\":\"demo@example.invalid\",\"recipients\":\"owner@example.invalid\",\"subject\":\"Update\",\"text\":\"Hello\"}]","sample.json"); Assert.Single(x.Records); Assert.True(x.RequiresConfirmation); }
    [Fact] public void CsvImport_ParsesValidRecord() { var x=CommunicationImportService.PreviewCsv("sentAt,sender,recipients,subject,text\n2026-07-01T10:00:00+12:00,demo@example.invalid,owner@example.invalid,Update,Hello","sample.csv"); Assert.Single(x.Records); }
    [Fact] public void CsvImport_SkipsMalformedRow() { var x=CommunicationImportService.PreviewCsv("sentAt,sender,subject\nbad,,","sample.csv"); Assert.Empty(x.Records); Assert.Single(x.Errors); }
    [Fact] public void ImportedHtml_BecomesInertText() { var x=CommunicationImportService.SanitizeText("<script>alert(1)</script><b>Hello</b> javascript:"); Assert.DoesNotContain("<",x); Assert.Contains("[removed]",x); }
    [Fact] public void Provenance_IsRetained() { var x=CommunicationImportService.PreviewJson("[{\"sentAt\":\"2026-07-01\",\"sender\":\"demo@example.invalid\",\"subject\":\"Update\"}]","sample.json"); Assert.Contains("sample.json#1",x.Records[0].Provenance); }
    [Fact] public void Duplicate_IsSuspected() { var a=Record(); var b=Record(); b.DuplicateKey=a.DuplicateKey; var count=CommunicationImportService.MarkDuplicates([a],[b]); Assert.Equal(1,count); Assert.Equal(CommunicationReviewState.DuplicateSuspected,b.ReviewState); }
    [Fact] public void Match_ExplainsExactAddress() { var c=EmailRadarService.FindCandidates(Profile(),[Record()]).Single(); Assert.Contains(c.Reasons,x=>x.StartsWith("Matched configured email address")); }
    [Fact] public void Match_ExplainsSubjectPhrase() { var c=EmailRadarService.FindCandidates(Profile(),[Record()]).Single(); Assert.Contains(c.Reasons,x=>x.StartsWith("Matched subject phrase")); }
    [Fact] public void Match_ExplainsKeyword() { var c=EmailRadarService.FindCandidates(Profile(),[Record()]).Single(); Assert.Contains(c.Reasons,x=>x.StartsWith("Matched keyword")); }
    [Fact] public void Match_ReportsExcludeTerm() { var c=EmailRadarService.FindCandidates(Profile(),[Record(subject:"Workshop newsletter")]).Single(); Assert.True(c.Excluded); }
    [Fact] public void Match_ReportsOutsideDateRange() { var p=Profile(); p.DateFrom=DateTimeOffset.Now.AddDays(-1); var c=EmailRadarService.FindCandidates(p,[Record(days:-5)]).Single(); Assert.True(c.OutsideDateRange); }
    [Fact] public void PossibleMatch_RemainsUntrusted() { var r=Record(); EmailRadarService.FindCandidates(Profile(),[r]); Assert.Equal(CommunicationReviewState.NeedsReview,r.ReviewState); }
    [Fact] public void Confirm_SetsConfirmedState() { var p=Profile(); var r=Record(); EmailRadarService.Confirm(p,r); Assert.Equal(CommunicationReviewState.ConfirmedMatch,r.ReviewState); Assert.Equal(p.Id,r.ConfirmedProfileId); }
    [Fact] public void Reject_SetsRejectedState() { var r=Record(); EmailRadarService.Reject(r); Assert.Equal(CommunicationReviewState.RejectedMatch,r.ReviewState); }
    [Fact] public void Duplicate_CannotBeConfirmed() { var r=Record(); r.ReviewState=CommunicationReviewState.DuplicateSuspected; Assert.Throws<InvalidOperationException>(()=>EmailRadarService.Confirm(Profile(),r)); }
    [Fact] public void Timeline_ContainsOnlyConfirmedMatches() { var p=Profile(); var a=Record(); var b=Record(); EmailRadarService.Confirm(p,a); Assert.Single(EmailRadarService.BuildTimeline(p,[a,b],["owner@example.invalid"])); }
    [Fact] public void Direction_IncomingDerived() { var p=Profile(); var r=Record(); EmailRadarService.Confirm(p,r); Assert.Equal(CommunicationDirection.Incoming,EmailRadarService.BuildTimeline(p,[r],["owner@example.invalid"])[0].Direction); }
    [Fact] public void Direction_OutgoingDerived() { var p=Profile(); var r=Record("owner@example.invalid"); r.Recipients=["morgan@harbour-workshop.example.invalid"]; EmailRadarService.Confirm(p,r); Assert.Equal(CommunicationDirection.Outgoing,EmailRadarService.BuildTimeline(p,[r],["owner@example.invalid"])[0].Direction); }
    [Fact] public void SuggestsWaitingOnThem() { var p=Profile(); var r=Record("owner@example.invalid",days:-10); r.Recipients=["morgan@harbour-workshop.example.invalid"]; EmailRadarService.Confirm(p,r); Assert.Equal(CommunicationSuggestionKind.PossibleWaitingOnThem,EmailRadarService.Suggest(p,[r],["owner@example.invalid"]).Kind); }
    [Fact] public void SuggestsWaitingOnMe() { var p=Profile(); var r=Record(); EmailRadarService.Confirm(p,r); Assert.Equal(CommunicationSuggestionKind.PossibleWaitingOnMe,EmailRadarService.Suggest(p,[r],["owner@example.invalid"]).Kind); }
    [Fact] public void SuggestionIsReviewFirst() { var s=EmailRadarService.Suggest(Profile(),[]); Assert.True(s.RequiresReview); }
}
