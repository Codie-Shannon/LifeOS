using LifeOS.Core.FollowUps;
using LifeOS.Core.WorkPipeline;

namespace LifeOS.Shared.WorkPipeline;

public static class WorkPipelineFollowUpBridge
{
    public static FollowUpItem CreateFollowUp(WorkPipelineItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new FollowUpItem
        {
            PersonOrOrganisation = GetPersonOrOrganisation(item),
            Context = $"Work Pipeline: {item.Title}",
            NextAction = string.IsNullOrWhiteSpace(item.NextAction) ? "Review pipeline item and choose next action." : item.NextAction,
            FollowUpDate = item.FollowUpDate ?? item.KeepWarmDate,
            Status = MapStatus(item),
            Priority = MapPriority(item.Priority),
            IsMoneyLinked = item.IsMoneyRelated,
            Notes = BuildNotes(item),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
    }

    public static IReadOnlyList<FollowUpItem> CreateFollowUps(IEnumerable<WorkPipelineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .Where(item => item.IsOpen)
            .Where(item => item.FollowUpDate.HasValue || item.KeepWarmDate.HasValue || item.IsWaiting)
            .Select(CreateFollowUp)
            .ToList();
    }

    private static string GetPersonOrOrganisation(WorkPipelineItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.ContactName)) return item.ContactName;
        if (!string.IsNullOrWhiteSpace(item.ClientOrCompany)) return item.ClientOrCompany;
        return item.Title;
    }

    private static FollowUpStatus MapStatus(WorkPipelineItem item)
    {
        if (item.Status == WorkPipelineStatus.Parked) return FollowUpStatus.Parked;
        if (item.IsBlocked || item.IsWaiting) return FollowUpStatus.Waiting;
        return FollowUpStatus.NeedsAction;
    }

    private static FollowUpPriority MapPriority(WorkPipelinePriority priority)
    {
        return priority switch
        {
            WorkPipelinePriority.Low => FollowUpPriority.Low,
            WorkPipelinePriority.High => FollowUpPriority.High,
            WorkPipelinePriority.Critical => FollowUpPriority.Critical,
            _ => FollowUpPriority.Normal
        };
    }

    private static string BuildNotes(WorkPipelineItem item)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(item.ClientOrCompany)) parts.Add($"Client/company: {item.ClientOrCompany}");
        if (!string.IsNullOrWhiteSpace(item.WaitingOn)) parts.Add($"Waiting on: {item.WaitingOn}");
        if (!string.IsNullOrWhiteSpace(item.ExpectedValueNote)) parts.Add(item.ExpectedValueNote);
        if (!string.IsNullOrWhiteSpace(item.Notes)) parts.Add(item.Notes);

        return string.Join(Environment.NewLine, parts);
    }
}
