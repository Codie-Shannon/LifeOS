namespace LifeOS.Core.IntegrationConnectors.GoogleCalendar;

public sealed record CalendarRefreshRange(DateTimeOffset From, DateTimeOffset To)
{
    public const int MaximumDays = 31;

    public void Validate(DateTimeOffset now)
    {
        if (To <= From)
        {
            throw new ArgumentException("Calendar refresh end must be after its start.");
        }

        if (To - From > TimeSpan.FromDays(MaximumDays))
        {
            throw new ArgumentException($"Calendar refresh is limited to {MaximumDays} days.");
        }

        if (From < now.AddYears(-1) || To > now.AddYears(1))
        {
            throw new ArgumentException("Calendar refresh must stay within one year of today.");
        }
    }
}
