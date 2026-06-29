namespace LifeOS.Core.WorkPipeline;

public enum WorkPipelineStage
{
    LeadIdea = 0,
    Contacted = 10,
    WaitingOnReply = 20,
    MeetingBooked = 30,
    MaterialsReceived = 40,
    ProofInProgress = 50,
    SentForReview = 60,
    ApprovedHappy = 70,
    PaidWorkActive = 80,
    TimesheetNeeded = 90,
    InvoiceNeeded = 100,
    PaymentExpected = 110,
    KeepWarm = 120,
    ClosedArchived = 999
}
