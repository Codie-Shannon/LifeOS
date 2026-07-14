using LifeOS.Core.Assistant;

namespace LifeOS.Shared.Assistant;

public static class LocalAssistantEvidenceSources
{
    public static IReadOnlyList<IAssistantEvidenceSource> Create() =>
        Enum.GetValues<AssistantSourceType>().Select(source => new FictionalGroup36Source(source)).Cast<IAssistantEvidenceSource>().ToArray();

    private sealed class FictionalGroup36Source(AssistantSourceType sourceType) : IAssistantEvidenceSource
    {
        public AssistantSourceType SourceType => sourceType;
        public IReadOnlyList<AssistantEvidenceRecord> Retrieve(string question, int maximumRecords)
        {
            var q = question.ToLowerInvariant();
            if (q.Contains("zephyr quill") && q.Contains("logo")) return [];
            return Records().Where(r => Matches(q, r)).Take(maximumRecords).ToArray();
        }

        private IEnumerable<AssistantEvidenceRecord> Records()
        {
            var nz = TimeSpan.FromHours(12);
            return SourceType switch
            {
                AssistantSourceType.WaitingOn =>
                [new("g36-wait-northstar", SourceType, "Northstar sample-field review", "Waiting on a fictional reviewer to confirm invoice number, date and total fields.", new(2026,7,14,16,30,0,nz), "LifeOS fictional Group 36 Waiting On", AssistantTrustLevel.Direct, "northstar-ocr", "Waiting", IsFictional:true)],
                AssistantSourceType.FollowUps =>
                [new("g36-follow-northstar", SourceType, "Northstar Systems follow-up", "Follow up on 2026-07-18 if sample-field confirmation has not arrived.", new(2026,7,14,16,20,0,nz), "LifeOS fictional Group 36 Follow-Ups", AssistantTrustLevel.Direct, "northstar-ocr", "Waiting", IsFictional:true)],
                AssistantSourceType.WorkPipeline =>
                [new("g36-pipeline-northstar", SourceType, "Northstar invoice OCR proof", "Status Waiting; next action is manual sample-field review; bounded proof scope only.", new(2026,7,14,16,29,0,nz), "LifeOS fictional Group 36 Work Pipeline", AssistantTrustLevel.Direct, "northstar-ocr", "Waiting", IsFictional:true),
                 new("g36-pipeline-zephyr-old", SourceType, "Project Zephyr Quill", "Status Blocked; waiting on fictional source-file access.", new(2026,5,1,9,0,0,nz), "LifeOS fictional Group 36 Work Pipeline", AssistantTrustLevel.Direct, "zephyr-quill", "Blocked", IsFictional:true)],
                AssistantSourceType.Projects =>
                [new("g36-project-zephyr", SourceType, "Project Zephyr Quill", "Current project summary reports Active with a documentation task in progress.", new(2026,7,14,14,0,0,nz), "LifeOS fictional Group 36 Projects", AssistantTrustLevel.Direct, "zephyr-quill", "Active", IsFictional:true),
                 new("g36-project-northstar", SourceType, "Northstar proof project", "Status Waiting; scope is invoice OCR review only.", new(2026,7,14,16,10,0,nz), "LifeOS fictional Group 36 Projects", AssistantTrustLevel.Direct, "northstar-ocr", "Waiting", IsFictional:true)],
                AssistantSourceType.MoneyPressure =>
                [new("g36-money-northstar", SourceType, "Northstar payment attention", "No confirmed payment date or amount; treat expected money as unsafe.", new(2026,7,14,15,45,0,nz), "LifeOS fictional Group 36 Money Pressure", AssistantTrustLevel.Derived, "northstar-ocr", "Attention", Amount:850m, IsFictional:true)],
                AssistantSourceType.Receipts =>
                [new("g36-receipt-northstar", SourceType, "Northstar draft invoice evidence", "Draft evidence records NZD 900, but it is not a confirmed payable amount.", new(2026,7,13,12,0,0,nz), "LifeOS fictional Group 36 Receipts", AssistantTrustLevel.Direct, "northstar-ocr", "Draft", Amount:900m, IsFictional:true)],
                AssistantSourceType.Evidence =>
                [new("g36-evidence-northstar", SourceType, "Northstar evidence summary", "Evidence confirms the review request but contains no approval date.", new(2026,7,14,16,27,0,nz), "LifeOS fictional Group 36 Evidence", AssistantTrustLevel.Direct, "northstar-ocr", "Waiting", IsFictional:true)],
                AssistantSourceType.WorkSessions =>
                [new("g36-session-1", SourceType, "Northstar OCR review session", "Recorded 2.0 hours on 2026-07-13 reviewing fictional extraction fields.", new(2026,7,13,17,0,0,nz), "LifeOS fictional Group 36 Work Sessions", AssistantTrustLevel.Direct, "northstar-ocr", "Recorded", new(2026,7,13,17,0,0,nz), IsFictional:true),
                 new("g36-session-2", SourceType, "Zephyr documentation session", "Recorded 1.5 hours on 2026-07-14 documenting the fictional project state.", new(2026,7,14,15,0,0,nz), "LifeOS fictional Group 36 Work Sessions", AssistantTrustLevel.Direct, "zephyr-quill", "Recorded", new(2026,7,14,15,0,0,nz), IsFictional:true)],
                AssistantSourceType.Timesheets =>
                [new("g36-timesheet-northstar", SourceType, "Northstar timesheet — week of 2026-07-13", "Timesheet records 2.0 hours for fictional Northstar OCR review work during the week of 2026-07-13.", new(2026,7,14,15,5,0,nz), "LifeOS fictional Group 36 Timesheets", AssistantTrustLevel.Direct, "northstar-ocr", "Draft", new(2026,7,13,17,0,0,nz), IsFictional:true)],
                AssistantSourceType.Relationships =>
                [new("g36-relationship-northstar", SourceType, "Northstar reviewer context", "Preferred contact path is asynchronous review; no response time is guaranteed.", new(2026,7,12,11,0,0,nz), "LifeOS fictional Group 36 Relationships", AssistantTrustLevel.Direct, "northstar-ocr", "Waiting", IsFictional:true)],
                AssistantSourceType.Agenda =>
                [new("g36-agenda-today", SourceType, "Today: review proof evidence", "Agenda includes a manual Northstar evidence review; no action is automated.", new(2026,7,14,8,0,0,nz), "LifeOS fictional Group 36 Agenda", AssistantTrustLevel.Direct, "northstar-ocr", "Planned", IsFictional:true)],
                AssistantSourceType.DailyState =>
                [new("g36-daily-change", SourceType, "Daily state change", "Zephyr documentation moved from Not Started to In Progress today.", new(2026,7,14,14,5,0,nz), "LifeOS fictional Group 36 Daily State", AssistantTrustLevel.Direct, "zephyr-quill", "In Progress", IsFictional:true)],
                AssistantSourceType.Timeline =>
                [new("g36-timeline-northstar", SourceType, "Northstar timeline", "Review requested on 2026-07-14; no later response is recorded.", new(2026,7,14,16,28,0,nz), "LifeOS fictional Group 36 Timeline", AssistantTrustLevel.Derived, "northstar-ocr", "Waiting", IsFictional:true)],
                AssistantSourceType.CommandCentre =>
                [new("g36-command", SourceType, "Command Centre summary", "Northstar is passive waiting; Zephyr has an active documentation task.", new(2026,7,14,16,26,0,nz), "LifeOS fictional Group 36 Command Centre", AssistantTrustLevel.Summary, IsFictional:true)],
                _ => []
            };
        }

        private static bool Matches(string question, AssistantEvidenceRecord record)
        {
            if (question.Contains("northstar")) return record.Title.Contains("Northstar", StringComparison.OrdinalIgnoreCase) || record.EntityKey == "northstar-ocr";
            if (question.Contains("zephyr")) return record.Title.Contains("Zephyr", StringComparison.OrdinalIgnoreCase) || record.EntityKey == "zephyr-quill";
            if (question.Contains("this week") || question.Contains("work did") || question.Contains("timesheet") || question.Contains("hours"))
                return record.Source is AssistantSourceType.WorkSessions or AssistantSourceType.Timesheets;
            if (question.Contains("invoice") || question.Contains("payment") || question.Contains("money"))
                return record.Source is AssistantSourceType.MoneyPressure or AssistantSourceType.Receipts or AssistantSourceType.WorkPipeline or AssistantSourceType.Evidence;
            if (question.Contains("changed") || question.Contains("today"))
                return record.Source is AssistantSourceType.DailyState or AssistantSourceType.Agenda or AssistantSourceType.Timeline or AssistantSourceType.CommandCentre;
            return false;
        }
    }
}
