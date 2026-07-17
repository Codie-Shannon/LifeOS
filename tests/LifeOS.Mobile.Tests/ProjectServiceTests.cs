using LifeOS.Mobile.Core.Projects;
using Xunit;

namespace LifeOS.Mobile.Tests;

public sealed class ProjectServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 17, 10, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void SnapshotContainsAllRequiredStates()
    {
        var snapshot = new ProjectService().BuildSnapshot(Now);

        Assert.Contains(
            snapshot.Projects,
            x => x.Status == MobileProjectStatus.Active);

        Assert.Contains(
            snapshot.Projects,
            x => x.Status == MobileProjectStatus.Waiting);

        Assert.Contains(
            snapshot.Projects,
            x => x.Status == MobileProjectStatus.Blocked);

        Assert.Contains(
            snapshot.Projects,
            x => x.Status == MobileProjectStatus.Parked);
    }

    [Fact]
    public void ProjectDetailsContainNextAction()
    {
        var snapshot = new ProjectService().BuildSnapshot(Now);

        Assert.All(
            snapshot.Projects,
            x => Assert.False(
                string.IsNullOrWhiteSpace(x.NextAction)));
    }

    [Fact]
    public void FilterIsDeterministic()
    {
        var service = new ProjectService();
        var snapshot = service.BuildSnapshot(Now);

        var first = service.Filter(
            snapshot,
            MobileProjectStatus.Active,
            "LifeOS");

        var second = service.Filter(
            snapshot,
            MobileProjectStatus.Active,
            "LifeOS");

        Assert.Equal(
            first.Select(x => x.Id),
            second.Select(x => x.Id));
    }

    [Fact]
    public void ParkActionIsIdempotent()
    {
        var service = new ProjectService();

        var first = service.Park("project", Now);
        var second = service.Park(
            "project",
            Now.AddMinutes(5));

        Assert.Same(first, second);
    }

    [Fact]
    public void ResumeActionIsIdempotent()
    {
        var service = new ProjectService();

        var first = service.Resume("project", Now);
        var second = service.Resume(
            "project",
            Now.AddMinutes(5));

        Assert.Same(first, second);
    }

    [Fact]
    public void ProjectEvidenceNeverClaimsAutomaticGeneration()
    {
        var snapshot = new ProjectService().BuildSnapshot(Now);

        Assert.DoesNotContain(
            snapshot.Projects.SelectMany(x => x.Evidence),
            x => x.Source.Contains(
                "automatic",
                StringComparison.OrdinalIgnoreCase));
    }
}
