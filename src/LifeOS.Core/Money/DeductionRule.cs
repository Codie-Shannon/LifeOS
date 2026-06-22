namespace LifeOS.Core.Money;

public enum DeductionType
{
    FixedAmount,
    Percentage
}

public sealed class DeductionRule
{
    public required string Name { get; init; }

    public required DeductionType Type { get; init; }

    public required decimal Value { get; init; }

    public string Frequency { get; init; } = "Weekly";

    public bool IsActive { get; init; } = true;

    public string Notes { get; init; } = string.Empty;
}