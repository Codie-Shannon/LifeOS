using System.Text; using LifeOS.Core.Documents; using Xunit;
namespace LifeOS.Core.Tests;
public sealed class Group59DocumentIntakeTests
{
 static readonly DateTimeOffset Now=new(2026,7,20,12,0,0,TimeSpan.FromHours(12));
 [Fact] public void OriginalBytesAndHashArePreserved(){var x=DocumentIntegrity.PreserveOriginal("a","a.pdf","application/pdf",Encoding.UTF8.GetBytes("proof"),Now,"Desktop","test");Assert.Equal("proof",Encoding.UTF8.GetString(x.Bytes));DocumentIntegrity.Verify(x);}
 [Fact] public void DraftReviewAcceptIsExplicit(){var s=new DocumentIntakeService();var o=DocumentIntegrity.PreserveOriginal("a","a.pdf","application/pdf",[1,2,3],Now,"Desktop","test");var d=s.CreateDraft(o,DocumentType.GeneralEvidence,[new("total","12.00",.8m,"candidate")]);Assert.Equal(DocumentIntakeState.Draft,d.State);var r=s.MoveToReview(d);var a=s.Accept(r,DocumentType.Invoice,r.Metadata,[new(DocumentLinkArea.Money,"inv","Invoice")]);Assert.Equal(DocumentIntakeState.Accepted,a.State);Assert.True(a.Metadata[0].IsAccepted);Assert.True(a.HasTrustedOriginal);}
 [Fact] public void ExactHashCreatesCandidateOnly(){var (records,dupes)=DocumentProofData.Build(Now);Assert.Single(dupes);Assert.Equal(1m,dupes[0].Confidence);Assert.Equal("pending_review",dupes[0].State);Assert.Equal(4,records.Count);}
 [Fact] public void SuggestionsRemainUntrustedUntilAccepted(){var (records,_)=DocumentProofData.Build(Now);var r=records.Single(x=>x.Id=="doc-receipt-59");Assert.All(r.Metadata,x=>Assert.False(x.IsAccepted));Assert.Equal(DocumentIntakeState.ReviewRequired,r.State);}
 [Fact] public void RejectionAndDeferralRetainOriginal(){var s=new DocumentIntakeService();var d=DocumentProofData.Build(Now).Records[2];Assert.True(s.Reject(d).HasTrustedOriginal);Assert.True(s.Defer(d).HasTrustedOriginal);}
 [Fact] public void AcceptedDocumentLinksMoneyAndWork(){var a=DocumentProofData.Build(Now).Records.Single(x=>x.State==DocumentIntakeState.Accepted);Assert.Contains(a.Links,x=>x.Area==DocumentLinkArea.Money);Assert.Contains(a.Links,x=>x.Area==DocumentLinkArea.Work);}
 [Fact] public void ProofOverviewIsDeterministic(){var (r,d)=DocumentProofData.Build(Now);var x=new DocumentIntakeService().Summarize(r,d);Assert.Equal(2,x.Drafts);Assert.Equal(1,x.AwaitingReview);Assert.Equal(1,x.Accepted);Assert.Equal(1,x.DuplicateCandidates);}
 [Fact] public void SensitiveContentIsRedacted(){Assert.DoesNotContain("test@example.com",new DocumentIntakeService().Redact("test@example.com"));}
}
