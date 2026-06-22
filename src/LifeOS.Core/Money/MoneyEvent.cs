namespace LifeOS.Core.Money;

public sealed class MoneyEvent
{
    public required string Name { get; init; }

    public required decimal Amount { get; init; }

    public required DateOnly DueDate { get; init; }

    public string Category { get; init; } = "General";

    public bool IsEssential { get; init; } = true;

    public bool IsPaid { get; init; }

    public bool IsFlexible { get; init; }

    public string Notes { get; init; } = string.Empty;
}