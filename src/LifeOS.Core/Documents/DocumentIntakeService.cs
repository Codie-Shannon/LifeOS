namespace LifeOS.Core.Documents;
public sealed class DocumentIntakeService
{
 public DocumentRecord CreateDraft(OriginalDocument original,DocumentType type,IReadOnlyList<MetadataCandidate>? metadata=null,IReadOnlyList<ClassificationSuggestion>? classifications=null)
 {
  DocumentIntegrity.Verify(original);
  return new($"doc-{original.Id}",original,type,DocumentIntakeState.Draft,[],metadata??[],classifications??[],[],[new("Draft created","Original preserved; extraction and classification remain suggestions.",original.ImportedUtc)],"Awaiting explicit review");
 }
 public DocumentRecord MoveToReview(DocumentRecord item)=>Require(item,DocumentIntakeState.Draft) with { State=DocumentIntakeState.ReviewRequired, Audit=Append(item,"Review requested","Suggestions remain untrusted.") };
 public DocumentRecord Accept(DocumentRecord item,DocumentType correctedType,IEnumerable<MetadataCandidate> reviewed,IEnumerable<DocumentLink> links)
 {
  if(item.State is not (DocumentIntakeState.Draft or DocumentIntakeState.ReviewRequired or DocumentIntakeState.Deferred)) throw new InvalidOperationException("Only reviewable documents can be accepted.");
  DocumentIntegrity.Verify(item.Original);
  return item with { Type=correctedType,State=DocumentIntakeState.Accepted,Metadata=reviewed.Select(x=>x with { IsAccepted=true }).ToArray(),Links=links.ToArray(),Classifications=item.Classifications.Select(x=>x with { IsAccepted=x.Type==correctedType }).ToArray(),Audit=Append(item,"Accepted after review","Original retained; reviewed metadata and links accepted."),ReviewNote="Accepted explicitly"};
 }
 public DocumentRecord Defer(DocumentRecord item)=>item with {State=DocumentIntakeState.Deferred,Audit=Append(item,"Deferred","No metadata, classification or financial posting accepted.")};
 public DocumentRecord Reject(DocumentRecord item)=>item with {State=DocumentIntakeState.Rejected,Audit=Append(item,"Rejected","Original retained; no destructive deletion performed.")};
 public IReadOnlyList<DuplicateDocumentCandidate> FindExactDuplicates(DocumentRecord incoming,IEnumerable<DocumentRecord> existing)=>existing.Where(x=>x.Id!=incoming.Id&&x.Original.Sha256==incoming.Original.Sha256).Select(x=>new DuplicateDocumentCandidate(incoming.Id,x.Id,incoming.Original.Sha256,1m)).ToArray();
 public DocumentIntakeOverview Summarize(IEnumerable<DocumentRecord> records,IEnumerable<DuplicateDocumentCandidate> dupes)
 { var a=records.ToArray(); return new(a.Count(x=>x.State==DocumentIntakeState.Draft),a.Count(x=>x.State==DocumentIntakeState.ReviewRequired),a.Count(x=>x.State==DocumentIntakeState.Accepted),dupes.Count(),a.Sum(x=>x.Metadata.Count(m=>!m.IsAccepted)),a.Sum(x=>x.Links.Count)); }
 public string Redact(string value)=>string.IsNullOrWhiteSpace(value)?"[redacted]":System.Text.RegularExpressions.Regex.Replace(value,@"[\w.+-]+@[\w.-]+\.[A-Za-z]{2,}","[email-redacted]");
 private static DocumentRecord Require(DocumentRecord item,DocumentIntakeState state)=>item.State==state?item:throw new InvalidOperationException($"Expected {state} state.");
 private static IReadOnlyList<DocumentAuditEntry> Append(DocumentRecord item,string action,string summary)=>item.Audit.Concat([new(action,summary,DateTimeOffset.UtcNow)]).ToArray();
}
