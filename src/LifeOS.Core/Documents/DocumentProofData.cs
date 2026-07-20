using System.Text;
namespace LifeOS.Core.Documents;
public static class DocumentProofData
{
 public static (IReadOnlyList<DocumentRecord> Records,IReadOnlyList<DuplicateDocumentCandidate> Duplicates) Build(DateTimeOffset now)
 {
  var service=new DocumentIntakeService();
  OriginalDocument O(string id,string name,string type,string text)=>DocumentIntegrity.PreserveOriginal(id,name,type,Encoding.UTF8.GetBytes(text),now,id.Contains("mobile")?"Full Mobile capture":"Desktop upload","fictional-proof");
  var receipt=service.MoveToReview(service.CreateDraft(O("receipt-59","fictional-receipt.pdf","application/pdf","fictional receipt bytes"),DocumentType.Receipt,[new("merchant","Example Office Supply",.94m,"Detected merchant candidate"),new("date","2026-07-18",.91m,"Detected date candidate"),new("currency","NZD",.99m,"ISO-style currency candidate"),new("total","42.50",.96m,"Detected total candidate")],[new(DocumentType.Receipt,.93m,"Merchant and total layout resemble a receipt")]));
  var accepted=service.Accept(service.MoveToReview(service.CreateDraft(O("invoice-59","fictional-invoice.pdf","application/pdf","fictional invoice bytes"),DocumentType.GeneralEvidence,[new("supplier","Fictional Engineering",.88m,"Supplier candidate"),new("invoice_number","INV-DEMO-059",.97m,"Invoice number candidate"),new("total","480.00",.95m,"Total candidate")],[new(DocumentType.Invoice,.92m,"Invoice number and due-date layout")])),DocumentType.Invoice,[new("supplier","Fictional Engineering",1m,"Corrected and reviewed"),new("invoice_number","INV-DEMO-059",1m,"Reviewed"),new("total","480.00",1m,"Reviewed")],[new(DocumentLinkArea.Money,"inv-58-a","Money invoice INV-DEMO-058"),new(DocumentLinkArea.Work,"work-a","Welding proof review")]);
  var mobile=service.CreateDraft(O("mobile-timesheet","mobile-timesheet.jpg","image/jpeg","fictional mobile capture"),DocumentType.Timesheet,[new("date","2026-07-20",.72m,"Capture date suggestion")],[new(DocumentType.Timesheet,.81m,"Tabular hours layout")]);
  var duplicate=service.CreateDraft(DocumentIntegrity.PreserveOriginal("receipt-copy","receipt-copy.pdf","application/pdf",receipt.Original.Bytes,now,"Desktop upload","fictional-proof"),DocumentType.Receipt);
  var records=new[]{receipt,accepted,mobile,duplicate};
  return (records,service.FindExactDuplicates(duplicate,records));
 }
}
