namespace LifeOS.Core.WorkSessions;

public static class WorkSessionCalculator
{
    public static WorkSessionSummary Calculate(IEnumerable<WorkSession> sessions)
    {
        var sessionList = sessions.ToList();
        var billableSessions = sessionList.Where(session => session.IsBillable && session.Status is not WorkSessionStatus.Cancelled and not WorkSessionStatus.NonBillable).ToList();
        var paidSessions = billableSessions.Where(session => session.Status == WorkSessionStatus.Paid).ToList();
        var unpaidSessions = billableSessions.Where(session => session.Status is WorkSessionStatus.Completed or WorkSessionStatus.Invoiced or WorkSessionStatus.InProgress or WorkSessionStatus.Planned).ToList();

        var reasons = new List<string>();
        var unpaidValue = unpaidSessions.Sum(session => session.BillableValue);

        if (unpaidValue > 0) reasons.Add($"{unpaidValue:C} of billable work is not marked paid yet.");
        if (billableSessions.Count > 0) reasons.Add($"{billableSessions.Sum(session => session.Hours):0.##} billable hour(s) are tracked.");
        if (sessionList.Count == 0) reasons.Add("No work sessions have been tracked yet.");
        if (reasons.Count == 0) reasons.Add("No active work-session pressure detected.");

        return new WorkSessionSummary
        {
            TotalSessions = sessionList.Count,
            TotalHours = sessionList.Sum(session => session.Hours),
            BillableHours = billableSessions.Sum(session => session.Hours),
            BillableValue = billableSessions.Sum(session => session.BillableValue),
            PaidValue = paidSessions.Sum(session => session.BillableValue),
            UnpaidBillableValue = unpaidValue,
            ActiveClientOrProjectCount = sessionList.Select(session => session.ClientOrProject).Where(text => !string.IsNullOrWhiteSpace(text)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            Reasons = reasons
        };
    }
}
