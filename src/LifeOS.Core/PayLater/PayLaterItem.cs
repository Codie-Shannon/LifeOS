namespace LifeOS.Core.PayLater;

public sealed class PayLaterItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Payee { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateOnly? DueDate { get; set; }

    public PayLaterStatus Status { get; set; } = PayLaterStatus.Planned;

    public PayLaterPressureLevel PressureLevel { get; set; } = PayLaterPressureLevel.Normal;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
