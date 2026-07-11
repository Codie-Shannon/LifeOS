using LifeOS.Core.EmailRadar; using LifeOS.Core.IntegrationConnectors.Gmail; using LifeOS.Shared.EmailRadar;
namespace LifeOS.Shared.IntegrationConnectors.Gmail;
public sealed class GmailImportCoordinator(IReadOnlyGmailProvider provider)
{
 public async Task<GmailImportResult> ExecuteAsync(EmailRadarProfile profile,GmailSearchPlan plan,CancellationToken ct=default){var fetched=await provider.SearchAsync(plan,ct);var existing=EmailRadarStorage.LoadRecords();var normalized=fetched.Messages.Select(x=>GmailMessageNormalizer.Normalize(x,"local-account-hidden",ShortHash(plan.GeneratedQuery),DateTimeOffset.Now)).ToList();var duplicates=CommunicationImportService.MarkDuplicates(existing,normalized);existing.AddRange(normalized);EmailRadarStorage.SaveRecords(existing);var candidates=EmailRadarService.FindCandidates(profile,normalized).Count();EmailRadarStorage.AppendAudit(new(DateTimeOffset.Now,"gmail-search-imported",$"Received {fetched.Messages.Count}; duplicates {duplicates}; skipped {fetched.Skipped}; candidates {candidates}.",profile.Id));return new(fetched.Messages.Count,normalized.Count,duplicates,fetched.Skipped,candidates,fetched.Partial,plan.GeneratedQuery);}
 static string ShortHash(string value)=>Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)))[..12].ToLowerInvariant();
}
