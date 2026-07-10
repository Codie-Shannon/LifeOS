namespace LifeOS.Core.WeeklyCloseOut;

public static class WeeklyCloseOutOperatingCalculator
{
    public static WeeklyCloseOutOperatingSummary Calculate(IEnumerable<WeeklyCloseOutReviewItem> items)
    {
        var list = items.ToList();

        var readyToClose = list
            .Where(item => item.Status == WeeklyCloseOutReviewStatus.ReadyToClose)
            .ToList();

        var rollForward = list
            .Where(item => item.RollIntoNextWeek && item.Status != WeeklyCloseOutReviewStatus.Closed)
            .ToList();

        var waitingOrBlocked = list
            .Where(item => item.Status is WeeklyCloseOutReviewStatus.Waiting or WeeklyCloseOutReviewStatus.Blocked)
            .ToList();

        var moneyChecks = list.Where(item => item.IsMoneyReview).ToList();
        var proofChecks = list.Where(item => item.IsProofReview).ToList();
        var receiptChecks = list.Where(item => item.IsReceiptReview).ToList();
        var workChecks = list.Where(item => item.IsWorkReview).ToList();

        var waiting = list.Count(item => item.Status == WeeklyCloseOutReviewStatus.Waiting);
        var blocked = list.Count(item => item.Status == WeeklyCloseOutReviewStatus.Blocked);
        var untrusted = list.Count(item => !item.IsTrusted);
        var critical = list.Count(item => item.Pressure == WeeklyCloseOutPressureLevel.Critical);
        var high = list.Count(item => item.Pressure == WeeklyCloseOutPressureLevel.High);
        var moneyUnderReview = moneyChecks
            .Where(item => item.Status != WeeklyCloseOutReviewStatus.Closed)
            .Sum(item => item.Amount);

        var pressureLabel = critical > 0 || blocked > 1
            ? "Critical"
            : high > 0 || waiting > 0 || untrusted > 0
                ? "High"
                : readyToClose.Count > 0
                    ? "Normal"
                    : "Low";

        var reasons = new List<string>
        {
            $"{readyToClose.Count} item(s) can be closed this week.",
            $"{rollForward.Count} item(s) should roll into next week.",
            $"{waiting} item(s) are waiting and {blocked} item(s) are blocked.",
            $"{moneyChecks.Count} money/payment check(s) remain visible.",
            $"{proofChecks.Count} proof/evidence check(s) remain visible.",
            $"{receiptChecks.Count} receipt evidence check(s) remain visible.",
            $"{workChecks.Count} work pipeline check(s) remain visible.",
            $"{untrusted} item(s) remain untrusted or review-only.",
            $"${moneyUnderReview:0.00} remains under review and is not automatically safe money."
        };

        return new WeeklyCloseOutOperatingSummary
        {
            TotalItems = list.Count,
            ReadyToCloseItems = readyToClose.Count,
            RollForwardItems = rollForward.Count,
            WaitingItems = waiting,
            BlockedItems = blocked,
            MoneyReviewItems = moneyChecks.Count,
            ProofReviewItems = proofChecks.Count,
            ReceiptReviewItems = receiptChecks.Count,
            WorkReviewItems = workChecks.Count,
            UntrustedItems = untrusted,
            MoneyStillUnderReview = moneyUnderReview,
            PressureLabel = pressureLabel,
            Reasons = reasons,
            CloseNow = readyToClose,
            RollForward = rollForward,
            WaitingOrBlocked = waitingOrBlocked,
            MoneyChecks = moneyChecks,
            ProofChecks = proofChecks,
            ReceiptChecks = receiptChecks,
            WorkChecks = workChecks
        };
    }
}
