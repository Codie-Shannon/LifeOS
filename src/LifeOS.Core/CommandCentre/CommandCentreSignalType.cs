namespace LifeOS.Core.CommandCentre;

public enum CommandCentreSignalType
{
    PaidWorkAvailable = 0,
    FollowUpDue = 10,
    BlockedWork = 20,
    WaitingOnReply = 30,
    TimesheetNeeded = 40,
    InvoiceNeeded = 50,
    PaymentExpected = 60,
    MoneyPressure = 70,
    BillDue = 80,
    ProofNeeded = 90,
    MissingNextAction = 100,
    PassiveWaiting = 110,
    ScheduledCommunication = 120,
    EvidenceReview = 130,
    General = 999
}
