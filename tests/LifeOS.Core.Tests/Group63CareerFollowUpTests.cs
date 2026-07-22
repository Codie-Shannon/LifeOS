using LifeOS.Core.CareerStudio;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Group63CareerFollowUpTests
{
    private static readonly DateTimeOffset Now = new(2026,7,22,14,0,0,TimeSpan.FromHours(12));

    [Fact] public void Proof_contains_due_overdue_waiting_and_completed_followups()
    {
        var proof = CareerClosureProofData.Build(Now);
        Assert.Contains(proof.FollowUps, x => x.Status == CareerFollowUpStatus.Due);
        Assert.Contains(proof.FollowUps, x => x.Status == CareerFollowUpStatus.Overdue);
        Assert.Contains(proof.FollowUps, x => x.Status == CareerFollowUpStatus.Waiting);
        Assert.Contains(proof.FollowUps, x => x.Status == CareerFollowUpStatus.Completed);
    }

    [Fact] public void Status_calculation_is_date_bounded()
    {
        var service = new CareerClosureService();
        var plan = CareerClosureProofData.Build(Now).FollowUps[0] with { Status = CareerFollowUpStatus.Planned, DueUtc = Now.AddMinutes(-1) };
        Assert.Equal(CareerFollowUpStatus.Overdue, service.CalculateStatus(plan, Now));
    }

    [Fact] public void Transition_preserves_audit_history()
    {
        var service = new CareerClosureService();
        var plan = CareerClosureProofData.Build(Now).FollowUps[0];
        var updated = service.Transition(plan, CareerFollowUpStatus.Deferred, Now, "Deferred explicitly.");
        Assert.Equal(CareerFollowUpStatus.Deferred, updated.Status);
        Assert.Contains(updated.History, x => x.Action == "Deferred");
    }

    [Fact] public void Reference_requires_explicit_permission_and_readiness()
    {
        var proof = CareerClosureProofData.Build(Now);
        var service = new CareerClosureService();
        Assert.True(service.CanIncludeReference(proof.References[0]));
        Assert.False(service.CanIncludeReference(proof.References[1]));
    }

    [Fact] public void Questions_are_ordered_and_linked_to_interview()
    {
        var plan = CareerClosureProofData.Build(Now).QuestionsToAsk;
        Assert.Equal(new[]{1,2,3}, plan.Questions.Select(x => x.Order));
        Assert.All(plan.Questions, x => Assert.Equal(plan.InterviewId, x.InterviewId));
    }

    [Fact] public void No_contract_exposes_send_or_external_write_capability()
    {
        var names = typeof(CareerFollowUpPlan).Assembly.GetTypes().Where(x => x.Namespace == typeof(CareerFollowUpPlan).Namespace).SelectMany(x => x.GetMethods()).Select(x => x.Name);
        Assert.DoesNotContain(names, x => x.Contains("SendRecruiter", StringComparison.OrdinalIgnoreCase) || x.Contains("SubmitApplication", StringComparison.OrdinalIgnoreCase));
    }
}
