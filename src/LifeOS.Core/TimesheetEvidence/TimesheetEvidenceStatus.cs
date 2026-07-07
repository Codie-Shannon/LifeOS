namespace LifeOS.Core.TimesheetEvidence;

public enum TimesheetEvidenceStatus
{
    Draft = 0,
    ReadyForTimesheet = 10,
    AddedToTimesheet = 20,
    Invoiced = 30,
    Paid = 40,
    Archived = 999
}
