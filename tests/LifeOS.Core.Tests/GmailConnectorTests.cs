using Xunit; using LifeOS.Core.EmailRadar; using LifeOS.Core.IntegrationConnectors.Gmail;
namespace LifeOS.Core.Tests;
public sealed class GmailConnectorTests
{
 static EmailRadarProfile Profile()=>new(){Name="Example",RelatedLabel="Example",EmailAddresses=["person@example.invalid"],SubjectPhrases=["Project update"],Keywords=["proof"],ExcludeTerms=["newsletter"]};
 [Fact] public void Version_IsAlpha5()=>Assert.Equal("5.0.0-alpha.5",LifeOS.Core.ProductVersion.Semantic);
 [Fact] public void ReadOnly_Scope_IsExact()=>Assert.Equal("https://www.googleapis.com/auth/gmail.readonly",LifeOS.Shared.IntegrationConnectors.Gmail.GmailOAuthPkceClient.ReadOnlyScope);
 [Fact] public void Query_IsBoundedAndVisible(){var p=Profile();var plan=GmailSearchPlanner.Build(p,new(p.Id,DateTimeOffset.UtcNow.AddDays(-7),DateTimeOffset.UtcNow,25));Assert.Contains("after:",plan.GeneratedQuery);Assert.Contains("before:",plan.GeneratedQuery);Assert.Contains("-category:promotions",plan.GeneratedQuery);}
 [Fact] public void Profile_IsRequired(){var p=Profile();Assert.Throws<ArgumentException>(()=>GmailSearchPlanner.Build(p,new(Guid.NewGuid(),DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow,25)));}
 [Fact] public void Range_MustBeBounded(){var p=Profile();Assert.Throws<ArgumentOutOfRangeException>(()=>GmailSearchPlanner.Build(p,new(p.Id,DateTimeOffset.UtcNow.AddDays(-32),DateTimeOffset.UtcNow,25)));}
 [Theory][InlineData(0)][InlineData(101)] public void Cap_IsEnforced(int cap){var p=Profile();Assert.Throws<ArgumentOutOfRangeException>(()=>GmailSearchPlanner.Build(p,new(p.Id,DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow,cap)));}
 [Fact] public void Exclusions_AreVisible(){var p=Profile();var plan=GmailSearchPlanner.Build(p,new(p.Id,DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow,25));Assert.Contains("-category:social",plan.VisibleExclusions);Assert.Contains("-from:(no-reply OR noreply)",plan.VisibleExclusions);}
 [Fact] public void Normalization_IsProviderNeutral(){var m=new GmailProviderMessage("m1","t1",DateTimeOffset.UtcNow,"a@example.invalid",["b@example.invalid"],[],"Update","<b>Hello</b>",false,[]);var r=GmailMessageNormalizer.Normalize(m,"hidden","abc",DateTimeOffset.UtcNow);Assert.Equal("gmail-readonly",r.SourceKind);Assert.Equal("m1",r.ExternalReference);Assert.DoesNotContain("<",r.Text);Assert.Equal(CommunicationReviewState.NeedsReview,r.ReviewState);}
 [Fact] public void DuplicateKey_IsStable(){var m=new GmailProviderMessage("m1","t1",DateTimeOffset.UtcNow,"a",["b"],[],"s","x",false,[]);var a=GmailMessageNormalizer.Normalize(m,"hidden","a",DateTimeOffset.UtcNow);var b=GmailMessageNormalizer.Normalize(m,"hidden","b",DateTimeOffset.UtcNow.AddMinutes(2));Assert.Equal(a.DuplicateKey,b.DuplicateKey);}
 [Fact] public void Snippet_IsCapped(){Assert.Equal(500,GmailMessageNormalizer.Sanitize(new string('x',600),500).Length);}
}
