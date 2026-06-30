namespace LifeOS.Core.WorkPipeline;

public sealed class WorkPipelineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string ClientOrCompany { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string OpportunityType { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string WaitingOn { get; set; } = string.Empty;

    public WorkPipelineStage Stage { get; set; } = WorkPipelineStage.LeadIdea;

    public WorkPipelineStatus Status { get; set; } = WorkPipelineStatus.Active;

    public WorkPipelinePriority Priority { get; set; } = WorkPipelinePriority.Normal;

    public string NextAction { get; set; } = string.Empty;

    public DateOnly? FollowUpDate { get; set; }

    public DateOnly? LastContactDate { get; set; }

    public DateOnly? KeepWarmDate { get; set; }

    public decimal? ExpectedValue { get; set; }

    public string ExpectedValueNote { get; set; } = string.Empty;

    public int LikelihoodPercent { get; set; }

    public bool IsBillable { get; set; }

    public bool NeedsTimesheet { get; set; }

    public bool NeedsInvoice { get; set; }

    public bool PaymentExpected { get; set; }

    public string LinkedProofNotes { get; set; } = string.Empty;

    public string LinkedSessionNotes { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public bool HasFollowUpDate => FollowUpDate.HasValue;

    public WorkPipelineFollowUpState GetFollowUpState(DateOnly today)
    {
        if (IsArchived || Status == WorkPipelineStatus.Archived || Status == WorkPipelineStatus.Completed)
        {
            return WorkPipelineFollowUpState.None;
        }

        if (FollowUpDate.HasValue)
        {
            if (FollowUpDate.Value < today) return WorkPipelineFollowUpState.Overdue;
            if (FollowUpDate.Value == today) return WorkPipelineFollowUpState.DueToday;
            if (FollowUpDate.Value <= today.AddDays(7)) return WorkPipelineFollowUpState.DueSoon;
            return WorkPipelineFollowUpState.Scheduled;
        }

        if (KeepWarmDate.HasValue)
        {
            if (KeepWarmDate.Value <= today.AddDays(14)) return WorkPipelineFollowUpState.KeepWarm;
            return WorkPipelineFollowUpState.Scheduled;
        }

        return WorkPipelineFollowUpState.None;
    }

    public bool IsMoneyRelated =>
        IsBillable ||
        NeedsTimesheet ||
        NeedsInvoice ||
        PaymentExpected ||
        ExpectedValue.HasValue;

    public bool IsBlocked => Status == WorkPipelineStatus.Blocked;

    public bool IsWaiting =>
        Status == WorkPipelineStatus.Waiting ||
        Stage is WorkPipelineStage.WaitingOnReply or WorkPipelineStage.PaymentExpected;

    public bool IsOpen => Status is not WorkPipelineStatus.Completed and not WorkPipelineStatus.Archived && !IsArchived;

    public void Touch()
    {
        UpdatedAt = DateTime.Now;
    }

    public void Archive()
    {
        IsArchived = true;
        Status = WorkPipelineStatus.Archived;
        Stage = WorkPipelineStage.ClosedArchived;
        Touch();
    }
}
