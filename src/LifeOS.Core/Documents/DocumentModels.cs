namespace LifeOS.Core.Documents;
public sealed record OriginalDocument(string Id,string FileName,string MediaType,long SizeBytes,string Sha256,byte[] Bytes,DateTimeOffset ImportedUtc,string Source,string Provenance);
public sealed record DocumentVersion(string Id,string DocumentId,string Kind,string MediaType,string Sha256,DateTimeOffset CreatedUtc);
public sealed record MetadataCandidate(string Field,string? Value,decimal Confidence,string Explanation,bool IsAccepted=false);
public sealed record ClassificationSuggestion(DocumentType Type,decimal Confidence,string Explanation,bool IsAccepted=false);
public sealed record DocumentLink(DocumentLinkArea Area,string RecordId,string Label);
public sealed record DocumentAuditEntry(string Action,string SafeSummary,DateTimeOffset OccurredUtc);
public sealed record DocumentRecord(string Id,OriginalDocument Original,DocumentType Type,DocumentIntakeState State,IReadOnlyList<DocumentVersion> Derivatives,IReadOnlyList<MetadataCandidate> Metadata,IReadOnlyList<ClassificationSuggestion> Classifications,IReadOnlyList<DocumentLink> Links,IReadOnlyList<DocumentAuditEntry> Audit,string ReviewNote)
{
 public bool HasTrustedOriginal => Original.Bytes.Length == Original.SizeBytes && DocumentIntegrity.Sha256(Original.Bytes)==Original.Sha256;
}
public sealed record DuplicateDocumentCandidate(string IncomingDocumentId,string ExistingDocumentId,string Sha256,decimal Confidence,string State="pending_review");
public sealed record DocumentIntakeOverview(int Drafts,int AwaitingReview,int Accepted,int DuplicateCandidates,int ExtractionSuggestions,int EvidenceLinks);
