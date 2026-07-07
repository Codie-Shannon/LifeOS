namespace LifeOS.Core.TimesheetEvidence;

public static class TimesheetEvidenceRules
{
    public static decimal SuggestHours(TimesheetEvidenceType type)
    {
        return type switch
        {
            TimesheetEvidenceType.LightAdmin or TimesheetEvidenceType.ClientUpdate => 0.25m,
            TimesheetEvidenceType.Investigation or TimesheetEvidenceType.Testing => 0.5m,
            TimesheetEvidenceType.Implementation or TimesheetEvidenceType.ProofBuild or TimesheetEvidenceType.Documentation => 1.0m,
            _ => 0.5m
        };
    }

    public static string BuildClientSafeDescription(string whatHappened, string outcome)
    {
        var work = string.IsNullOrWhiteSpace(whatHappened) ? "Completed work review/investigation" : whatHappened.Trim();
        var result = string.IsNullOrWhiteSpace(outcome) ? "prepared next action and evidence notes" : outcome.Trim();

        return $"{work}. {result}.";
    }
}
