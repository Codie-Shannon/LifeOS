namespace LifeOS.Core.WorkPipeline;

public enum WorkPipelineStatus
{
    Active = 0,
    Waiting = 10,
    Blocked = 20,
    Warm = 30,
    Parked = 40,
    Completed = 50,
    Archived = 999
}
