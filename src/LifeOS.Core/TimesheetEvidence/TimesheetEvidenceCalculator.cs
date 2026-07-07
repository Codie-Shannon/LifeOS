namespace LifeOS.Core.TimesheetEvidence;

public static class TimesheetEvidenceCalculator
{
    public static TimesheetEvidenceSummary Calculate(IEnumerable<TimesheetEvidenceEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var entryList = entries.ToList();
        var ready = entryList
            .Where(entry => entry.Status == TimesheetEvidenceStatus.ReadyForTimesheet)
            .OrderByDescending(entry => entry.Date)
            .ThenBy(entry => entry.ClientOrProject)
            .ToList();

        var reasons = new List<string>();
        if (ready.Count > 0) reasons.Add($"{ready.Count} timesheet evidence item(s) are ready for entry.");
        if (entryList.Count(entry => entry.Status == TimesheetEvidenceStatus.Draft) > 0) reasons.Add("Draft timesheet evidence exists and may need cleanup.");
        if (reasons.Count == 0) reasons.Add("No timesheet evidence pressure detected.");

        return new TimesheetEvidenceSummary
        {
            TotalEntries = entryList.Count,
            ReadyForTimesheetCount = ready.Count,
            DraftCount = entryList.Count(entry => entry.Status == TimesheetEvidenceStatus.Draft),
            SuggestedHoursTotal = ready.Sum(entry => entry.SuggestedHours),
            ReadyEntries = ready,
            Reasons = reasons
        };
    }
}
