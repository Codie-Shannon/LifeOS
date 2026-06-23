namespace LifeOS.Core.ProofTracker;

public static class ProofCalculator
{
    public static ProofSummary Calculate(IEnumerable<ProofItem> items, DateOnly today)
    {
        var itemList = items.ToList();
        var recentCutoff = today.AddDays(-14);

        var readyCount = itemList.Count(item => item.Status == ProofStatus.Ready);
        var sharedCount = itemList.Count(item => item.Status == ProofStatus.Shared);
        var acceptedCount = itemList.Count(item => item.Status == ProofStatus.Accepted);
        var clientProofCount = itemList.Count(item => item.Type is ProofType.ClientReply or ProofType.PaidInvoice or ProofType.CaseStudy);
        var recentCount = itemList.Count(item => item.Date >= recentCutoff);

        var reasons = new List<string>();

        if (readyCount > 0) reasons.Add($"{readyCount} proof item(s) are ready to share.");
        if (sharedCount > 0) reasons.Add($"{sharedCount} proof item(s) have already been shared.");
        if (acceptedCount > 0) reasons.Add($"{acceptedCount} proof item(s) are accepted/confirmed.");
        if (clientProofCount > 0) reasons.Add($"{clientProofCount} proof item(s) are client/payment/case-study relevant.");
        if (itemList.Count == 0) reasons.Add("No proof items have been tracked yet.");
        if (reasons.Count == 0) reasons.Add("Proof tracker is active, but no urgent proof pressure is detected.");

        return new ProofSummary
        {
            TotalProofItems = itemList.Count,
            ReadyCount = readyCount,
            SharedCount = sharedCount,
            AcceptedCount = acceptedCount,
            ClientProofCount = clientProofCount,
            RecentCount = recentCount,
            Reasons = reasons
        };
    }
}
