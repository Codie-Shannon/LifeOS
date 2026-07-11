namespace LifeOS.Core.EmailRadar;

public static class EmailRadarService
{
    public static IReadOnlyList<EmailRadarMatchCandidate> FindCandidates(EmailRadarProfile profile, IEnumerable<ImportedCommunicationRecord> records)
    {
        profile.Validate();
        var candidates = new List<EmailRadarMatchCandidate>();
        foreach (var record in records)
        {
            var reasons = new List<string>(); var score = 0;
            var haystack = $"{record.Sender} {string.Join(' ', record.Recipients)} {record.Subject} {record.Text}";
            foreach (var address in profile.EmailAddresses.Where(x => x.Length > 0)) if (record.Sender.Equals(address, StringComparison.OrdinalIgnoreCase) || record.Recipients.Any(x => x.Equals(address, StringComparison.OrdinalIgnoreCase))) { reasons.Add($"Matched configured email address: {address}"); score += 50; }
            foreach (var phrase in profile.SubjectPhrases.Where(x => x.Length > 0)) if (record.Subject.Contains(phrase, StringComparison.OrdinalIgnoreCase)) { reasons.Add($"Matched subject phrase: {phrase}"); score += 25; }
            foreach (var keyword in profile.Keywords.Where(x => x.Length > 0)) if (haystack.Contains(keyword, StringComparison.OrdinalIgnoreCase)) { reasons.Add($"Matched keyword: {keyword}"); score += 10; }
            if (!string.IsNullOrWhiteSpace(profile.RelatedLabel) && haystack.Contains(profile.RelatedLabel, StringComparison.OrdinalIgnoreCase)) { reasons.Add($"Matched related label: {profile.RelatedLabel}"); score += 15; }
            var excluded = profile.ExcludeTerms.Any(x => x.Length > 0 && haystack.Contains(x, StringComparison.OrdinalIgnoreCase));
            if (excluded) reasons.Add("Excluded term detected");
            var outside = (profile.DateFrom.HasValue && record.SentAt < profile.DateFrom) || (profile.DateTo.HasValue && record.SentAt > profile.DateTo);
            if (outside) reasons.Add("Outside preferred date range");
            if (score > 0 || excluded) candidates.Add(new(record, score, reasons, excluded, outside));
        }
        return candidates.OrderBy(x => x.Excluded).ThenBy(x => x.OutsideDateRange).ThenByDescending(x => x.Score).ThenByDescending(x => x.Record.SentAt).ToList();
    }

    public static void Confirm(EmailRadarProfile profile, ImportedCommunicationRecord record, string note = "")
    { if (record.ReviewState == CommunicationReviewState.DuplicateSuspected) throw new InvalidOperationException("Resolve duplicate suspicion before confirmation."); record.ReviewState = CommunicationReviewState.ConfirmedMatch; record.ConfirmedProfileId = profile.Id; record.ReviewNote = note; }
    public static void Reject(ImportedCommunicationRecord record, string note = "") { record.ReviewState = CommunicationReviewState.RejectedMatch; record.ConfirmedProfileId = null; record.ReviewNote = note; }

    public static IReadOnlyList<CommunicationTimelineItem> BuildTimeline(EmailRadarProfile profile, IEnumerable<ImportedCommunicationRecord> records, IEnumerable<string>? ownerAddresses = null)
    {
        var owners = (ownerAddresses ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return records.Where(x => x.ReviewState == CommunicationReviewState.ConfirmedMatch && x.ConfirmedProfileId == profile.Id)
            .OrderBy(x => x.SentAt).Select(x => new CommunicationTimelineItem(x.SentAt, Direction(x, owners), $"{x.Sender} → {string.Join(", ", x.Recipients)}", x.Subject, x.Text.Length > 180 ? x.Text[..180] + "…" : x.Text, x.Provenance, x.ReviewState, KeywordFlags(profile, x))).ToList();
    }

    public static CommunicationSuggestion Suggest(EmailRadarProfile profile, IEnumerable<ImportedCommunicationRecord> records, IEnumerable<string>? ownerAddresses = null, DateTimeOffset? now = null)
    {
        var timeline = BuildTimeline(profile, records, ownerAddresses); if (timeline.Count == 0) return new(CommunicationSuggestionKind.InsufficientEvidence, "Insufficient evidence", "No confirmed imported communication evidence is available.");
        var last = timeline[^1]; var age = ((now ?? DateTimeOffset.Now) - last.SentAt).TotalDays;
        if (last.Direction == CommunicationDirection.Outgoing && age >= profile.FollowUpDays) return new(CommunicationSuggestionKind.PossibleWaitingOnThem, "Possible waiting on them", "Based on confirmed imported evidence; requires review.");
        if (last.Direction == CommunicationDirection.Incoming) return new(CommunicationSuggestionKind.PossibleWaitingOnMe, "Possible waiting on me", "Based on confirmed imported evidence; requires review.");
        if (age >= profile.FollowUpDays) return new(CommunicationSuggestionKind.PossibleFollowUpDue, "Possible follow-up due", "Direction is unknown and the confirmed evidence is older than the profile interval; requires review.");
        return new(CommunicationSuggestionKind.RecentActivityNoFollowUp, "Recent activity; no follow-up suggested", "Based on confirmed imported evidence; requires review.");
    }

    private static CommunicationDirection Direction(ImportedCommunicationRecord record, HashSet<string> owners) => owners.Contains(record.Sender) ? CommunicationDirection.Outgoing : record.Recipients.Any(owners.Contains) ? CommunicationDirection.Incoming : CommunicationDirection.Unknown;
    private static IReadOnlyList<string> KeywordFlags(EmailRadarProfile p, ImportedCommunicationRecord r) => p.Keywords.Where(k => $"{r.Subject} {r.Text}".Contains(k, StringComparison.OrdinalIgnoreCase)).ToList();
}
