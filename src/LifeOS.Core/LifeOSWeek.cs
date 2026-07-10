namespace LifeOS.Core;

public static class LifeOSWeek
{
    public static DateOnly GetMondayStart(DateOnly date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
