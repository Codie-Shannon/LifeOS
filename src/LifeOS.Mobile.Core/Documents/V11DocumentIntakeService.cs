using LifeOS.Core.Documents;
namespace LifeOS.Mobile.Core.Documents;
public sealed class V11DocumentIntakeService
{
 readonly DocumentIntakeService inner=new();
 public (IReadOnlyList<DocumentRecord> Records,IReadOnlyList<DuplicateDocumentCandidate> Duplicates) BuildProofData(DateTimeOffset now)=>DocumentProofData.Build(now);
 public DocumentRecord Accept(DocumentRecord record)=>inner.Accept(record,record.Classifications.FirstOrDefault()?.Type??record.Type,record.Metadata, [new(DocumentLinkArea.Money,"inv-58-a","Money invoice"),new(DocumentLinkArea.Work,"work-a","Work record")]);
}
