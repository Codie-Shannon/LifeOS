using System.Text.Json;
using LifeOS.Core.WorkPipeline;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.WorkPipeline;

public static class WorkPipelineStorage
{
    private const string FileName = "work-pipeline.json";
    private const string BackupFileName = "work-pipeline.backup.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static string BackupFilePath => LocalAppDataPath.GetFilePath(BackupFileName);

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
            if (File.Exists(BackupFilePath))
            {
                try
                {
                    var backupJson = File.ReadAllText(BackupFilePath);
                    return JsonSerializer.Deserialize<List<WorkPipelineItem>>(backupJson, JsonOptions)
                        ?? CreateDefaultItems();
                }
                catch
                {
                    return CreateDefaultItems();
                }
            }

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

        if (File.Exists(FilePath))
        {
            File.Copy(FilePath, BackupFilePath, overwrite: true);
        }

        var json = JsonSerializer.Serialize(cleanItems, JsonOptions);
        var tempPath = FilePath + ".tmp";
        File.WriteAllText(tempPath, json);

        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }

        File.Move(tempPath, FilePath);
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
        item.LastOutcome = item.LastOutcome.Trim();
        item.RiskNote = item.RiskNote.Trim();
        item.NextAction = item.NextAction.Trim();
        item.ExpectedValueNote = item.ExpectedValueNote.Trim();
        item.LinkedProofNotes = item.LinkedProofNotes.Trim();
        item.LinkedSessionNotes = item.LinkedSessionNotes.Trim();
        item.Notes = item.Notes.Trim();

        if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
        if (item.CreatedAt == default) item.CreatedAt = DateTime.Now;
        if (item.UpdatedAt == default) item.UpdatedAt = DateTime.Now;

        item.LikelihoodPercent = Math.Clamp(item.LikelihoodPercent, 0, 100);

        if (item.Status == WorkPipelineStatus.Parked && item.OpportunityTemperature == WorkPipelineOpportunityTemperature.Active)
        {
            item.OpportunityTemperature = WorkPipelineOpportunityTemperature.Warm;
        }

        if (item.PaymentExpected && !item.ExpectedValue.HasValue && string.IsNullOrWhiteSpace(item.ExpectedValueNote))
        {
            item.ExpectedValueNote = "Payment expected, but amount has not been entered. Expected money is not safe money until paid.";
        }

        return item;
    }

    private static List<WorkPipelineItem> CreateDefaultItems()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return
        [
            new WorkPipelineItem
            {
                Title = "Workshop Proof Project",
                ContactName = "Project Owner",
                ClientOrCompany = "Fictional Workshop Client",
                Category = "Paid Work / Proof",
                OpportunityType = "Active paid proof",
                Source = "Demo relationship",
                WaitingOn = "Proof review and invoice-ready summary",
                Stage = WorkPipelineStage.ProofInProgress,
                Status = WorkPipelineStatus.Active,
                Priority = WorkPipelinePriority.High,
                NextAction = "Review proof, prepare the client-safe summary, then decide whether to invoice.",
                FollowUpDate = today.AddDays(2),
                ExpectedValue = 180m,
                ExpectedValueNote = "Demo expected value. Expected money is not safe money until paid.",
                LikelihoodPercent = 85,
                OpportunityTemperature = WorkPipelineOpportunityTemperature.Active,
                LastOutcome = "Proof workflow is moving.",
                RiskNote = "Must keep proof attached before treating the work as ready to bill.",
                IsBillable = true,
                NeedsTimesheet = true,
                NeedsInvoice = true,
                PaymentExpected = false,
                LinkedProofNotes = "Local screenshot and documentation proof ready for review.",
                Notes = "Fictional demo data."
            },
            new WorkPipelineItem
            {
                Title = "Client Portal Cleanup",
                ContactName = "Operations Contact",
                ClientOrCompany = "Fictional Portal Client",
                Category = "Client Work",
                OpportunityType = "Blocked paid work",
                Source = "Demo referral",
                WaitingOn = "Access confirmation",
                Stage = WorkPipelineStage.WaitingOnReply,
                Status = WorkPipelineStatus.Blocked,
                Priority = WorkPipelinePriority.High,
                NextAction = "Wait for access confirmation. Do not sink unpaid time while blocked.",
                FollowUpDate = today.AddDays(3),
                LikelihoodPercent = 60,
                OpportunityTemperature = WorkPipelineOpportunityTemperature.Warm,
                LastOutcome = "Access path not confirmed.",
                RiskNote = "Blocked work must not consume the active paid-work block.",
                Notes = "Fictional demo data."
            },
            new WorkPipelineItem
            {
                Title = "Door Invoice OCR Proof",
                ContactName = "Accounts Contact",
                ClientOrCompany = "Fictional Door Company",
                Category = "OCR Proof / Warm Lead",
                OpportunityType = "Warm client proof",
                Source = "Demo relationship",
                WaitingOn = "Payment confirmation or review reply",
                Stage = WorkPipelineStage.PaymentExpected,
                Status = WorkPipelineStatus.Waiting,
                Priority = WorkPipelinePriority.Normal,
                NextAction = "Wait until the follow-up date, then check whether payment/review has landed.",
                FollowUpDate = today.AddDays(5),
                ExpectedValue = 120m,
                ExpectedValueNote = "Demo payment expected. Not safe money until paid.",
                LikelihoodPercent = 70,
                OpportunityTemperature = WorkPipelineOpportunityTemperature.Warm,
                LastOutcome = "Staged OCR proof approach accepted in demo data.",
                RiskNote = "Payment/reply still pending; not safe money.",
                PaymentExpected = true,
                LinkedProofNotes = "Invoice extraction screenshot and review notes.",
                Notes = "Fictional demo data."
            },
            new WorkPipelineItem
            {
                Title = "Cloud Admin Follow-up",
                ContactName = "Cloud Contact",
                ClientOrCompany = "Fictional Cloud Team",
                Category = "Admin Proof / Opportunity",
                OpportunityType = "Warm active opportunity",
                Source = "Demo referral",
                WaitingOn = "Proof package preparation",
                Stage = WorkPipelineStage.MaterialsReceived,
                Status = WorkPipelineStatus.Active,
                Priority = WorkPipelinePriority.High,
                NextAction = "Prepare a small proof package and keep expected money separate from safe money.",
                FollowUpDate = today.AddDays(4),
                ExpectedValue = 90m,
                ExpectedValueNote = "Potential demo value only.",
                LikelihoodPercent = 65,
                LinkedProofNotes = "Admin cleanup proof package.",
                Notes = "Fictional demo data."
            },
            new WorkPipelineItem
            {
                Title = "Portfolio Review Lead",
                ContactName = "Portfolio Contact",
                Category = "Warm Lead",
                OpportunityType = "Keep warm",
                Stage = WorkPipelineStage.KeepWarm,
                Status = WorkPipelineStatus.Parked,
                Priority = WorkPipelinePriority.Low,
                NextAction = "Park until the review window opens.",
                KeepWarmDate = today.AddDays(14),
                LikelihoodPercent = 35,
                Notes = "Fictional demo data."
            },
            new WorkPipelineItem
            {
                Title = "Family Archive Project",
                Category = "Serious Portfolio Project",
                OpportunityType = "Future portfolio project",
                Stage = WorkPipelineStage.KeepWarm,
                Status = WorkPipelineStatus.Parked,
                Priority = WorkPipelinePriority.Low,
                NextAction = "Not active today.",
                LikelihoodPercent = 100,
                Notes = "Fictional demo data. Originals are sacred."
            },
            new WorkPipelineItem
            {
                Title = "LifeOS v1.7",
                Category = "Flagship Build",
                OpportunityType = "Current build",
                Stage = WorkPipelineStage.ProofInProgress,
                Status = WorkPipelineStatus.Active,
                Priority = WorkPipelinePriority.Critical,
                NextAction = "Finish Paid Work / Money / Proof integration and capture Group 03 screenshots.",
                FollowUpDate = today,
                LikelihoodPercent = 100,
                LinkedProofNotes = "LifeOS is the flagship platform.",
                Notes = "Current build focus."
            }
        ];
    }
}
