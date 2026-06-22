namespace LifeOS.Core.Models;

public sealed class ContactProfile
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string DisplayName { get; set; } = string.Empty;
    public ContactType Type { get; set; } = ContactType.Client;

    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public decimal DefaultHourlyRate { get; set; } = 35m;
    public decimal DefaultTaxSetAsidePercent { get; set; } = 20m;
    public string DefaultWorkType { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset? UpdatedAt { get; set; }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(DisplayName)
            ? "Unnamed contact"
            : DisplayName;
    }
}