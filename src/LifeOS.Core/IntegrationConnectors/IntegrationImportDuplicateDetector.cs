using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors;

public static class IntegrationImportDuplicateDetector
{
    public static int MarkDuplicateSuspicions(
        IEnumerable<IntegrationPreviewItem> existingItems,
        IEnumerable<IntegrationPreviewItem> incomingItems)
    {
        ArgumentNullException.ThrowIfNull(existingItems);
        ArgumentNullException.ThrowIfNull(incomingItems);

        var existingKeys = existingItems
            .Select(item => item.DuplicateKey)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var seenIncomingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var duplicateCount = 0;

        foreach (var item in incomingItems)
        {
            if (string.IsNullOrWhiteSpace(item.DuplicateKey))
            {
                continue;
            }

            var duplicateKey = item.DuplicateKey.Trim();
            var duplicatesExisting = existingKeys.Contains(duplicateKey);
            var duplicatesIncoming = !seenIncomingKeys.Add(duplicateKey);

            if (!duplicatesExisting && !duplicatesIncoming)
            {
                continue;
            }

            item.Status = IntegrationPreviewStatus.DuplicateSuspected;
            item.ReviewNote = duplicatesExisting
                ? "Duplicate suspected during import: matching duplicate key already exists in Integration Inbox."
                : "Duplicate suspected during import: matching duplicate key appears more than once in this import batch.";
            item.UpdatedAt = DateTime.Now;
            duplicateCount++;
        }

        return duplicateCount;
    }
}
