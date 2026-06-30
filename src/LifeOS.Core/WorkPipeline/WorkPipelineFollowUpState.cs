namespace LifeOS.Core.WorkPipeline;

public enum WorkPipelineFollowUpState
{
    None = 0,
    Overdue = 10,
    DueToday = 20,
    DueSoon = 30,
    Scheduled = 40,
    KeepWarm = 50
}
