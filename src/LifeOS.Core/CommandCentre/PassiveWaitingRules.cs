using LifeOS.Core.WorkPipeline;

namespace LifeOS.Core.CommandCentre;

public static class PassiveWaitingRules
{
    public static PassiveWaitingDecision Decide(WorkPipelineItem item, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Status == WorkPipelineStatus.Parked || item.Stage == WorkPipelineStage.KeepWarm)
        {
            var due = item.KeepWarmDate.HasValue && item.KeepWarmDate.Value <= today;
            return new PassiveWaitingDecision
            {
                ShouldWait = !due,
                ShowInToday = due,
                Reason = due ? "Warm/parked item is due for review." : "Warm/parked item is not due yet.",
                SuggestedAction = due ? Safe(item.NextAction, "Review whether this should move, wait, or stay parked.") : "Wait. Keep it out of Today until the follow-up/keep-warm date."
            };
        }

        if (item.FollowUpDate.HasValue && item.FollowUpDate.Value > today)
        {
            return new PassiveWaitingDecision
            {
                ShouldWait = true,
                ShowInToday = false,
                Reason = $"Follow-up is scheduled for {item.FollowUpDate:yyyy-MM-dd}.",
                SuggestedAction = "Wait. Do not chase yet unless the subject matter changes."
            };
        }

        if (item.IsWaiting && string.IsNullOrWhiteSpace(item.NextAction))
        {
            return new PassiveWaitingDecision
            {
                ShouldWait = false,
                ShowInToday = true,
                Reason = "Waiting item has no next action.",
                SuggestedAction = "Set who/what is being waited on and add a follow-up date."
            };
        }

        return new PassiveWaitingDecision
        {
            ShouldWait = false,
            ShowInToday = true,
            Reason = "Item can be reviewed normally.",
            SuggestedAction = Safe(item.NextAction, "Review and decide the next safe action.")
        };
    }

    private static string Safe(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
