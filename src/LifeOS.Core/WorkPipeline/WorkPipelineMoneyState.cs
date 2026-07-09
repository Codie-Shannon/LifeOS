namespace LifeOS.Core.WorkPipeline;

public enum WorkPipelineMoneyState
{
    None,
    EstimateOnly,
    TimesheetNeeded,
    InvoiceNeeded,
    PaymentExpected,
    PaidOrClosed
}
