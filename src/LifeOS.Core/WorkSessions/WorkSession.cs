namespace LifeOS.Core.WorkSessions;

public sealed class WorkSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ClientOrProject { get; set; } = string.Empty;

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public decimal Hours { get; set; }

    public decimal HourlyRate { get; set; }

    public bool IsBillable { get; set; } = true;

    public WorkSessionStatus Status { get; set; } = WorkSessionStatus.Completed;

    public string Description { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public decimal BillableValue => IsBillable ? Math.Round(Hours * HourlyRate, 2) : 0m;
}
