namespace LifeOS.Core.CareerStudio;

public sealed record CareerStudioProof(
    IReadOnlyList<CareerOpportunity> Opportunities,
    IReadOnlyList<ImportedOpportunityCandidate> ImportedCandidates,
    IReadOnlyList<DuplicateOpportunityCandidate> DuplicateCandidates);

public static class CareerProofData
{
    public static CareerStudioProof Build(DateTimeOffset now)
    {
        var employerA = new Employer("emp-a", "Fictional Engineering Ltd", "https://example.invalid", "Tauranga");
        var employerB = new Employer("emp-b", "Sample Digital Studio", null, "Remote");
        var recruiter = new RecruiterContact("person-r", "Alex Example", "Sample Recruitment", "redacted@example.invalid", null);

        CareerOpportunity Opportunity(
            string id, string title, Employer employer, OpportunityStage stage,
            OpportunitySourceType sourceType, string sourceRef, DateTimeOffset? closing,
            PriorityLevel priority, string salary, WorkMode mode, params string[] requirements)
        {
            var source = new OpportunitySource(
                $"src-{id}", sourceType, sourceType.ToString(), sourceRef,
                now.AddDays(-5), now.AddHours(-3));
            var reqs = requirements.Select((x, i) =>
                new RoleRequirement($"{id}-req-{i+1}", i == 0 ? RequirementType.Skill : RequirementType.Experience, x, true, i == 0 ? "portfolio-proof-01" : null)).ToArray();
            var fit = new OpportunityFit(
                ["Evidence-backed application development", "Client delivery proof"],
                ["Domain-specific terminology to review"],
                [],
                ["portfolio-proof-01", "project-lifeos"],
                now.AddHours(-1),
                "CS",
                "User-reviewed fit only; no certainty or invented experience.");
            return new CareerOpportunity(
                id, title, employer, recruiter, source, stage,
                "Fictional role used for Group 61 proof.",
                employer.Location ?? "Flexible", mode, EmploymentType.FullTime,
                salary, now.AddDays(-5), closing, now.AddHours(-3), priority,
                reqs, fit,
                ["person-r"], ["work-proof-01"], ["project-lifeos"],
                ["doc-cv-redacted"], ["portfolio-proof-01"],
                [new CareerNextAction($"{id}-next", "Review role requirements", now.AddDays(1), false, id)],
                [new OpportunityHistory(now.AddDays(-5), "Captured", null, stage, "Fictional opportunity captured locally.")]);
        }

        var opportunities = new[]
        {
            Opportunity("opp-61-a", "Software Application Developer", employerA, OpportunityStage.Reviewing,
                OpportunitySourceType.DirectEmployer, "https://example.invalid/jobs/61-a", now.AddDays(3),
                PriorityLevel.High, "NZD 80k–95k context only", WorkMode.Hybrid,
                "C# and .NET", "Evidence of delivered applications"),
            Opportunity("opp-61-b", "Junior Automation Developer", employerB, OpportunityStage.Interested,
                OpportunitySourceType.JobBoard, "board-61-b", now.AddDays(9),
                PriorityLevel.Normal, "Rate not supplied", WorkMode.Remote,
                "Automation workflows", "Clear communication"),
            Opportunity("opp-61-c", "Software Application Developer", employerA, OpportunityStage.Discovered,
                OpportunitySourceType.Email, "https://example.invalid/jobs/61-a", now.AddDays(3),
                PriorityLevel.High, "NZD 80k–95k context only", WorkMode.Hybrid,
                "C# and .NET", "Evidence of delivered applications")
        };

        var service = new CareerOpportunityService();
        var imported = new[]
        {
            service.LinkImportedCandidate("inbox-fictional-61", "Example Fabrication", "Systems Support Developer", "email:fictional", now.AddHours(-6))
        };

        return new CareerStudioProof(opportunities, imported, service.FindDuplicateCandidates(opportunities));
    }
}
