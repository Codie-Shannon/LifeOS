using System.Text.Json;
using LifeOS.Core.EmailRadar;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.EmailRadar;

public static class EmailRadarStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public static string FolderPath => Path.Combine(LocalAppDataPath.GetLifeOSFolder(), "email-radar");
    public static string ProfilesPath => Path.Combine(FolderPath, "profiles.json");
    public static string RecordsPath => Path.Combine(FolderPath, "communication-records.json");
    public static string AuditPath => Path.Combine(FolderPath, "audit.json");
    public static List<EmailRadarProfile> LoadProfiles() => Load<List<EmailRadarProfile>>(ProfilesPath) ?? DemoProfiles();
    public static List<ImportedCommunicationRecord> LoadRecords() => Load<List<ImportedCommunicationRecord>>(RecordsPath) ?? DemoRecords();
    public static List<EmailRadarAuditEntry> LoadAudit() => Load<List<EmailRadarAuditEntry>>(AuditPath) ?? [];
    public static void SaveProfiles(IEnumerable<EmailRadarProfile> value) => Save(ProfilesPath, value);
    public static void SaveRecords(IEnumerable<ImportedCommunicationRecord> value) => Save(RecordsPath, value);
    public static void AppendAudit(EmailRadarAuditEntry entry) { var list = LoadAudit(); list.Add(entry); Save(AuditPath, list); }
    public static void UpsertProfile(EmailRadarProfile profile) { profile.Validate(); var list=LoadProfiles(); var i=list.FindIndex(x=>x.Id==profile.Id); profile.UpdatedAt=DateTimeOffset.Now; if(i<0) list.Add(profile); else list[i]=profile; SaveProfiles(list); }
    public static void ArchiveProfile(Guid id) { var list=LoadProfiles(); var item=list.Single(x=>x.Id==id); item.Status=EmailRadarProfileStatus.Archived; item.UpdatedAt=DateTimeOffset.Now; SaveProfiles(list); }
    public static void ConfirmImport(CommunicationImportPreview preview) { if(!preview.RequiresConfirmation) throw new InvalidOperationException("Nothing is available to confirm."); var records=LoadRecords(); CommunicationImportService.MarkDuplicates(records, preview.Records); records.AddRange(preview.Records); SaveRecords(records); AppendAudit(new(DateTimeOffset.Now,"import-confirmed",$"Saved {preview.Records.Count} inert local communication records; skipped {preview.Errors.Count} malformed rows.")); }
    private static T? Load<T>(string path) { try { return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options) : default; } catch { return default; } }
    private static void Save<T>(string path,T value) { Directory.CreateDirectory(FolderPath); File.WriteAllText(path,JsonSerializer.Serialize(value,Options)); }
    private static List<EmailRadarProfile> DemoProfiles() => [new(){Name="Harbour Workshop",RelatedLabel="Harbour Workshop",People=["Morgan Reed"],EmailAddresses=["morgan@harbour-workshop.example.invalid"],SubjectPhrases=["Workshop update"],Keywords=["proof","invoice","access","update"],ExcludeTerms=["newsletter"],FollowUpDays=7,Notes="Safe fictional Group 24 profile."}];
    private static List<ImportedCommunicationRecord> DemoRecords() { var p=DemoProfiles()[0]; var r=new ImportedCommunicationRecord{SourceKind="fictional-demo",SourceLabel="Group 24 safe sample",SourceFile="docs/integrations/lifeos-v5-email-radar-sample.json",ExternalReference="demo-001",ThreadReference="thread-demo-01",SentAt=DateTimeOffset.Now.AddDays(-8),Sender="owner@example.invalid",Recipients=["morgan@harbour-workshop.example.invalid"],Subject="Workshop update and proof",Text="The requested proof is attached in the fictional sample. Please review the update.",Provenance="fictional-demo:sample#1",ReviewState=CommunicationReviewState.PossibleMatch}; r.DuplicateKey=CommunicationImportService.BuildDuplicateKey(r); return [r]; }
}
