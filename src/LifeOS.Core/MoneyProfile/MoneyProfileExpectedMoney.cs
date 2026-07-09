using LifeOS.Core.ItemState;

namespace LifeOS.Core.MoneyProfile;

public sealed class MoneyProfileExpectedMoney
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public LifeOsItemState State { get; set; } = LifeOsItemState.PaymentExpected;

    public decimal Amount { get; set; }

    public DateTime? ExpectedDate { get; set; }

    public string SourceSummary { get; set; } = string.Empty;

    public string EvidenceSummary { get; set; } = string.Empty;

    public string ReviewGate { get; set; } = string.Empty;

    public string SafeMoneyRule { get; set; } = string.Empty;

    public bool Trusted { get; set; }

    public bool CountsAsSafeMoney { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
