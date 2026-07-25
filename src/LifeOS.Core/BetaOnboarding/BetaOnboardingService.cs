namespace LifeOS.Core.BetaOnboarding;

public enum SetupChoice
{
    SetupNow,
    Later,
    Declined
}

public sealed record SetupModule(
    string Id,
    string Name,
    bool IsCore,
    bool UsesSensitiveData,
    SetupChoice Choice,
    string ChangePath);

public sealed record BetaReadinessCheck(
    string Id,
    string Label,
    bool Passed,
    string Evidence);

public sealed record BetaReadiness(
    IReadOnlyList<SetupModule> Modules,
    IReadOnlyList<BetaReadinessCheck> Checks,
    bool Ready,
    IReadOnlyList<string> Blockers);

public sealed class BetaOnboardingService
{
    public SetupModule Choose(SetupModule module, SetupChoice choice) =>
        module with { Choice = choice };

    public BetaReadiness Evaluate(
        IEnumerable<SetupModule> modules,
        IEnumerable<BetaReadinessCheck> checks)
    {
        SetupModule[] moduleList = modules.ToArray();
        BetaReadinessCheck[] checkList = checks.ToArray();
        List<string> blockers = checkList
            .Where(check => !check.Passed)
            .Select(check => check.Label)
            .ToList();

        blockers.AddRange(moduleList
            .Where(module => module.IsCore && module.Choice == SetupChoice.Declined)
            .Select(module => $"{module.Name} core setup was declined."));

        return new BetaReadiness(
            moduleList,
            checkList,
            blockers.Count == 0,
            blockers);
    }
}
