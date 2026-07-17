using LifeOS.Mobile.Core.Services;

namespace LifeOS.Mobile.Core.Home;

public sealed class HomeDailyService
{
    private readonly Dictionary<string, HomeActionResult> _actions =
        new(StringComparer.Ordinal);

    public HomeDailyOverview BuildOverview(
        DateTimeOffset now,
        string timeZoneId = "Pacific/Auckland")
    {
        var priorities = new[]
        {
            new HomePriority(
                "work-proof",
                "Work",
                "Review client proof package",
                "Due today and waiting for an explicit review decision.",
                1),
            new HomePriority(
                "money-invoice",
                "Money",
                "Check pending invoice evidence",
                "Payment status requires a calm follow-up, not an automatic action.",
                2),
            new HomePriority(
                "project-mobile",
                "Projects",
                "Close the Full Mobile Home proof",
                "Current release work with the nearest verified milestone.",
                3),
            new HomePriority(
                "life-admin",
                "Life",
                "Confirm tomorrow's personal admin",
                "Keeps tomorrow clear without exposing sensitive detail.",
                4)
        };

        var upcoming = new[]
        {
            new HomeUpcomingItem(
                "calendar-focus",
                "Focused build block",
                "Calendar",
                now.AddHours(1),
                timeZoneId),
            new HomeUpcomingItem(
                "follow-up-window",
                "Client follow-up window",
                "Scheduled",
                now.AddHours(3),
                timeZoneId)
        };

        return new HomeDailyOverview(
            DateOnly.FromDateTime(now.Date),
            "Finish the Group 53 mobile Home proof without hidden decisions.",
            priorities,
            upcoming,
            new HomeWaitingSummary(
                WaitingOn: 2,
                Blocked: 1,
                Overdue: 1,
                NeedsReview: 3),
            new HomeReviewSummary(
                IntegrationInboxCount: 3,
                FreshnessLabel: "Fresh",
                LastSuccessfulSyncUtc: now.AddMinutes(-4)));
    }

    public MobileCaptureDraft CreateDraft(
        MobileCaptureKind kind,
        string text,
        DateTimeOffset now)
    {
        var normalized = NormalizeText(text);

        return new MobileCaptureDraft(
            MobileFoundationService.DeterministicId(
                $"{kind}:{normalized}:{now:yyyyMMddHHmm}"),
            kind,
            normalized,
            now);
    }

    public HomeActionResult Complete(
        string itemId,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);

        if (_actions.TryGetValue(itemId, out var existing) &&
            existing.State == "Completed")
        {
            return existing;
        }

        var result = new HomeActionResult(
            itemId,
            "Completed",
            now);

        _actions[itemId] = result;
        return result;
    }

    public HomeActionResult Defer(
        string itemId,
        DateTimeOffset untilUtc,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);

        if (untilUtc <= now)
        {
            throw new ArgumentOutOfRangeException(
                nameof(untilUtc),
                "Deferred time must be in the future.");
        }

        var state = $"Deferred until {untilUtc:O}";

        if (_actions.TryGetValue(itemId, out var existing) &&
            existing.State == state)
        {
            return existing;
        }

        var result = new HomeActionResult(itemId, state, now);
        _actions[itemId] = result;
        return result;
    }

    public static string FormatLocalTime(
        HomeUpcomingItem item,
        TimeZoneInfo timeZone)
    {
        var local = TimeZoneInfo.ConvertTime(item.StartsUtc, timeZone);
        return local.ToString("ddd d MMM, h:mm tt");
    }

    private static string NormalizeText(string text)
    {
        var normalized = string.Join(
            " ",
            (text ?? string.Empty)
                .Split(
                    [' ', '\t', '\r', '\n'],
                    StringSplitOptions.RemoveEmptyEntries));

        if (normalized.Length == 0)
        {
            throw new ArgumentException(
                "Capture text is required.",
                nameof(text));
        }

        if (normalized.Length > 240)
        {
            throw new ArgumentException(
                "Capture text cannot exceed 240 characters.",
                nameof(text));
        }

        return normalized;
    }
}
