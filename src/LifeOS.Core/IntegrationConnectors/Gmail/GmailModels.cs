using LifeOS.Core.EmailRadar;

namespace LifeOS.Core.IntegrationConnectors.Gmail;

public sealed record GmailSearchRequest(Guid ProfileId, DateTimeOffset From, DateTimeOffset To, int MaxResults, bool ExcludePromotions=true, bool ExcludeSocial=true, bool ExcludeNoReply=true, bool ExcludeYouTube=true, string ManualTerms="");
public sealed record GmailSearchPlan(GmailSearchRequest Request, string GeneratedQuery, IReadOnlyList<string> VisibleExclusions);
public sealed record GmailProviderMessage(string MessageId,string ThreadId,DateTimeOffset SentAt,string Sender,IReadOnlyList<string> Recipients,IReadOnlyList<string> Cc,string Subject,string Snippet,bool HasAttachments,IReadOnlyList<string> Labels);
public sealed record GmailFetchResult(IReadOnlyList<GmailProviderMessage> Messages,int Skipped,bool Partial,string SanitizedError="");
public sealed record GmailImportResult(int Received,int Imported,int Duplicates,int Skipped,int Candidates,bool Partial,string GeneratedQuery);
public interface IReadOnlyGmailProvider { Task<GmailFetchResult> SearchAsync(GmailSearchPlan plan,CancellationToken cancellationToken=default); }

public static class GmailSearchPlanner
{
    public const int DefaultCap=25;
    public const int MaximumCap=100;
    public const int MaximumRangeDays=31;
    public static GmailSearchPlan Build(EmailRadarProfile profile,GmailSearchRequest request)
    {
        ArgumentNullException.ThrowIfNull(profile); profile.Validate();
        if(profile.Status!=EmailRadarProfileStatus.Active) throw new ArgumentException("An active Email Radar profile is required.");
        if(request.ProfileId!=profile.Id) throw new ArgumentException("The selected Email Radar profile does not match the search request.");
        if(request.To<=request.From) throw new ArgumentException("A bounded Gmail date range is required.");
        if((request.To-request.From).TotalDays>MaximumRangeDays) throw new ArgumentOutOfRangeException(nameof(request),"Gmail searches are limited to 31 days.");
        if(request.MaxResults<1 || request.MaxResults>MaximumCap) throw new ArgumentOutOfRangeException(nameof(request),"Gmail result cap must be between 1 and 100.");
        var positive=new List<string>();
        if(profile.EmailAddresses.Count>0) positive.Add("("+string.Join(" OR ",profile.EmailAddresses.Select(x=>$"from:{Quote(x)} OR to:{Quote(x)}"))+")");
        positive.AddRange(profile.SubjectPhrases.Where(NotBlank).Select(x=>$"subject:{Quote(x)}"));
        positive.AddRange(profile.Keywords.Where(NotBlank).Select(Quote));
        if(NotBlank(request.ManualTerms)) positive.Add(request.ManualTerms.Trim());
        if(positive.Count==0) throw new ArgumentException("The selected profile must provide an address, subject phrase, keyword, or bounded manual term.");
        var exclusions=new List<string>();
        exclusions.AddRange(profile.ExcludeTerms.Where(NotBlank).Select(x=>"-"+Quote(x)));
        if(request.ExcludePromotions) exclusions.Add("-category:promotions");
        if(request.ExcludeSocial) exclusions.Add("-category:social");
        if(request.ExcludeNoReply) exclusions.Add("-from:(no-reply OR noreply)");
        if(request.ExcludeYouTube) exclusions.Add("-from:(youtube.com OR googlevideo.com)");
        var after=request.From.UtcDateTime.ToString("yyyy/MM/dd"); var before=request.To.UtcDateTime.AddDays(1).ToString("yyyy/MM/dd");
        var query=$"({string.Join(" OR ",positive)}) after:{after} before:{before} {string.Join(" ",exclusions)}".Trim();
        return new GmailSearchPlan(request,query,exclusions);
    }
    private static bool NotBlank(string? value)=>!string.IsNullOrWhiteSpace(value);
    private static string Quote(string value)=>$"\"{value.Trim().Replace("\"",string.Empty)}\"";
}

public static class GmailMessageNormalizer
{
    public static ImportedCommunicationRecord Normalize(GmailProviderMessage message,string connectorIdentity,string queryReference,DateTimeOffset fetchedAt)
    {
        var record=new ImportedCommunicationRecord{SourceKind="gmail-readonly",SourceLabel="Gmail read-only manual search",SourceFile="",ExternalReference=message.MessageId,ThreadReference=message.ThreadId,SentAt=message.SentAt,Sender=message.Sender,Recipients=message.Recipients.ToList(),Subject=message.Subject,Text=Sanitize(message.Snippet,500),HasAttachments=message.HasAttachments,ImportedAt=fetchedAt,Provenance=$"gmail:{connectorIdentity}:message:{message.MessageId}:query:{queryReference}",ReviewState=CommunicationReviewState.NeedsReview};
        record.DuplicateKey=$"gmail|{connectorIdentity.Trim().ToLowerInvariant()}|{message.MessageId.Trim()}";
        return record;
    }
    public static string Sanitize(string? value,int max){var text=(value??string.Empty).Replace("<"," ").Replace(">"," "); text=new string(text.Where(c=>!char.IsControl(c)||c is '\r' or '\n' or '\t').ToArray()).Trim(); return text.Length<=max?text:text[..max];}
}
