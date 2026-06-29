using System.Text.Json;
using LifeOS.Core.WorkPipeline;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.WorkPipeline;

public static class WorkPipelineStorage
{
    private const string FileName = "work-pipeline.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static List<WorkPipelineItem> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return CreateDefaultItems();
            }

            var json = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return CreateDefaultItems();
            }

            return JsonSerializer.Deserialize<List<WorkPipelineItem>>(json, JsonOptions)
                ?? CreateDefaultItems();
        }
        catch
        {
            return CreateDefaultItems();
        }
    }

    public static void Save(IEnumerable<WorkPipelineItem> items)
    {
        var cleanItems = items
            .Select(Normalise)
            .OrderBy(item => item.IsArchived)
            .ThenByDescending(item => item.Priority)
            .ThenBy(item => item.Stage)
            .ThenBy(item => item.Title)
            .ToList();

        var json = JsonSerializer.Serialize(cleanItems, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }

    private static WorkPipelineItem Normalise(WorkPipelineItem item)
    {
        item.Title = item.Title.Trim();
        item.ContactName = item.ContactName.Trim();
        item.ClientOrCompany = item.ClientOrCompany.Trim();
        item.Category = item.Category.Trim();
        item.OpportunityType = item.OpportunityType.Trim();
        item.Source = item.Source.Trim();
        item.WaitingOn = item.WaitingOn.Trim();
        item.NextAction = item.NextAction.Trim();
        item.ExpectedValueNote = item.ExpectedValueNote.Trim();
        item.LinkedProofNotes = item.LinkedProofNotes.Trim();
        item.LinkedSessionNotes = item.LinkedSessionNotes.Trim();
        item.Notes = item.Notes.Trim();

        if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
        if (item.CreatedAt == default) item.CreatedAt = DateTime.Now;
        if (item.UpdatedAt == default) item.UpdatedAt = DateTime.Now;

        item.LikelihoodPercent = Math.Clamp(item.LikelihoodPercent, 0, 100);

        return item;
    }

    private static List<WorkPipelineItem> CreateDefaultItems()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new WorkPipelineItem
            {
                Title = "AIE Welding Proof",
                ContactName = "AIE",
                ClientOrCompany = "AIE",
                Category = "Paid Work / Proof",
                OpportunityType = "Active paid proof",
                Source = "Existing client",
                WaitingOn = "Source registers/rules and build time",
                Stage = WorkPipelineStage.ProofInProgress,
                Status = WorkPipelineStatus.Active,
                Priority = WorkPipelinePriority.High,
                NextAction = "Build v0.2 material classification lookup layer.",
                FollowUpDate = today.AddDays(2),
                ExpectedValue = 140m,
                ExpectedValueNote = "4.0h / $140 once completed properly. Expected money is not safe money until paid.",
                LikelihoodPercent = 90,
                IsBillable = true,
                NeedsTimesheet = true,
                PaymentExpected = false,
                LinkedProofNotes = "Welding proof work.",
                Notes = "Active paid work. Keep wording conservative: candidate, possible match, requires review."
            },
            new WorkPipelineItem
            {
                Title = "JV Systems",
                ContactName = "Jeff",
                ClientOrCompany = "JV Systems",
                Category = "Client Work",
                OpportunityType = "Blocked paid work",
                Source = "Existing contact",
                WaitingOn = "Jeff / DevExpress licence or trial path",
                Stage = WorkPipelineStage.WaitingOnReply,
                Status = WorkPipelineStatus.Blocked,
                Priority = WorkPipelinePriority.High,
                NextAction = "Wait for Jeff re DevExpress licence/trial.",
                FollowUpDate = today.AddDays(3),
                LikelihoodPercent = 60,
                Notes = "Blocked until licence/access path is clear. Do not prioritise above paying/clearer work while unpaid."
            },
            new WorkPipelineItem
            {
                Title = "Total Door / Lester",
                ContactName = "Lester",
                ClientOrCompany = "Total Door Systems",
                Category = "OCR Proof / Warm Lead",
                OpportunityType = "Warm client proof",
                Source = "Existing relationship",
                WaitingOn = "Lester / payment or reply",
                Stage = WorkPipelineStage.PaymentExpected,
                Status = WorkPipelineStatus.Waiting,
                Priority = WorkPipelinePriority.Normal,
                NextAction = "Wait/follow up later.",
                FollowUpDate = today.AddDays(5),
                LikelihoodPercent = 70,
                PaymentExpected = true,
                Notes = "Warm/waiting. Keep separate from safe money until paid."
            },
            new WorkPipelineItem
            {
                Title = "Thundercloud / Nick OCR Proof",
                ContactName = "Nick",
                ClientOrCompany = "Thundercloud",
                Category = "OCR Proof / Opportunity",
                OpportunityType = "Warm active opportunity",
                Source = "Referred by Lester",
                WaitingOn = "File inspection / proof preparation",
                Stage = WorkPipelineStage.MaterialsReceived,
                Status = WorkPipelineStatus.Active,
                Priority = WorkPipelinePriority.High,
                NextAction = "Prepare OCR proof context/files.",
                FollowUpDate = today.AddDays(4),
                LikelihoodPercent = 65,
                LinkedProofNotes = "OCR/lab report extraction proof.",
                Notes = "Warm active opportunity. Keep separate from Total Door/Lester."
            },
            new WorkPipelineItem
            {
                Title = "Peter",
                ContactName = "Peter",
                Category = "Warm Lead",
                OpportunityType = "Keep warm",
                Stage = WorkPipelineStage.KeepWarm,
                Status = WorkPipelineStatus.Parked,
                Priority = WorkPipelinePriority.Low,
                NextAction = "Get back mid/end July.",
                KeepWarmDate = new DateOnly(2026, 7, 20),
                LikelihoodPercent = 30,
                Notes = "Parked/warm."
            },
            new WorkPipelineItem
            {
                Title = "Vanessa / OperationsFlow",
                ContactName = "Vanessa",
                Category = "Portfolio Follow-up",
                OpportunityType = "Keep warm",
                Stage = WorkPipelineStage.KeepWarm,
                Status = WorkPipelineStatus.Parked,
                Priority = WorkPipelinePriority.Low,
                NextAction = "Get back mid/end July.",
                KeepWarmDate = new DateOnly(2026, 7, 20),
                LikelihoodPercent = 35,
                Notes = "Parked/warm. OperationsFlow is separate from LifeOS."
            },
            new WorkPipelineItem
            {
                Title = "Family Archive",
                Category = "Serious Portfolio Project",
                OpportunityType = "Future portfolio project",
                Stage = WorkPipelineStage.KeepWarm,
                Status = WorkPipelineStatus.Parked,
                Priority = WorkPipelinePriority.Low,
                NextAction = "Not active today.",
                LikelihoodPercent = 100,
                Notes = "Preserved future project. Originals are sacred."
            },
            new WorkPipelineItem
            {
                Title = "LifeOS v0.6",
                Category = "Flagship Build",
                OpportunityType = "Current build",
                Stage = WorkPipelineStage.ProofInProgress,
                Status = WorkPipelineStatus.Active,
                Priority = WorkPipelinePriority.Critical,
                NextAction = "Finish Work Pipeline foundation models, storage, and desktop view.",
                FollowUpDate = today,
                LikelihoodPercent = 100,
                LinkedProofNotes = "LifeOS is the flagship platform.",
                Notes = "Current build focus."
            }
        ];
    }
}
