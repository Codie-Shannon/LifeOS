namespace LifeOS.Core.IntegrationInbox;

public sealed class IntegrationInboxSummary
{
    public int Total { get; init; }
    public int NewItems { get; init; }
    public int NeedsReview { get; init; }
    public int DuplicateSuspected { get; init; }
    public int Deferred { get; init; }
    public int Accepted { get; init; }
    public int Rejected { get; init; }
    public int Linked { get; init; }
    public int Untrusted { get; init; }
    public int SourceBacked { get; init; }
    public int ReadyForHandoff { get; init; }
    public decimal PreviewMoney { get; init; }
    public int EmailPreviews { get; init; }
    public int CalendarPreviews { get; init; }
    public int AccountingPreviews { get; init; }
    public int FilePreviews { get; init; }
    public int OcrPreviews { get; init; }
    public int BankingPreviews { get; init; }
    public string PressureLabel { get; init; } = "Normal";
}
