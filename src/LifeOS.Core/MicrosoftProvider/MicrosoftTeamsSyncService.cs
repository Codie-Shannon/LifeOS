using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.MicrosoftProvider;

public sealed class MicrosoftTeamsSyncService
{
    private readonly IMicrosoftTeamsReader _reader;

    public MicrosoftTeamsSyncService(IMicrosoftTeamsReader reader) =>
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    public async Task<MicrosoftTeamsSyncResult> SyncAsync(
        MicrosoftProviderAccount account,
        MicrosoftTeamsSelection selection,
        IntegrationInboxV9Service inbox,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(inbox);

        selection.Normalize();
        DateTimeOffset sinceUtc = nowUtc.AddDays(-selection.LookbackDays);
        int messages = 0, meetings = 0, suggestions = 0, removed = 0, accessLost = 0;
        List<string> errors = [];

        foreach (string teamId in selection.SelectedTeamIds)
        {
            foreach (string channelId in selection.SelectedChannelIds)
            {
                try
                {
                    IReadOnlyList<MicrosoftTeamsMessageDescriptor> items =
                        await _reader.GetMessagesAsync(
                            teamId,
                            channelId,
                            sinceUtc,
                            selection.MaximumMessagesPerChannel,
                            cancellationToken);

                    foreach (MicrosoftTeamsMessageDescriptor item in items)
                    {
                        IntegrationCandidate candidate = inbox.Ingest(
                            MicrosoftTeamsCandidateFactory.CreateMessageDraft(account, item),
                            nowUtc);
                        messages++;

                        if (item.SourceState is MicrosoftTeamsSourceState.Deleted or
                            MicrosoftTeamsSourceState.SourceRemoved)
                        {
                            inbox.MarkSourceRemoved(candidate.Id, nowUtc);
                            removed++;
                        }
                        else if (item.SourceState == MicrosoftTeamsSourceState.AccessLost)
                        {
                            accessLost++;
                        }

                        if (LooksActionable(item.BodyPreview))
                        {
                            inbox.Ingest(
                                MicrosoftTeamsCandidateFactory.CreateActionSuggestion(
                                    account,
                                    item,
                                    item.BodyPreview.Contains("document", StringComparison.OrdinalIgnoreCase)
                                        ? "Requested document"
                                        : "Follow-up"),
                                nowUtc);
                            suggestions++;
                        }
                    }
                }
                catch (Exception exception) when (
                    exception is not OperationCanceledException)
                {
                    errors.Add(
                        $"Selected Teams channel failed: {exception.GetType().Name}");
                }
            }
        }

        try
        {
            IReadOnlyList<MicrosoftTeamsMeetingDescriptor> meetingItems =
                await _reader.GetMeetingsAsync(
                    sinceUtc,
                    nowUtc.AddDays(90),
                    cancellationToken);

            foreach (MicrosoftTeamsMeetingDescriptor meeting in meetingItems)
            {
                inbox.Ingest(
                    MicrosoftTeamsCandidateFactory.CreateMeetingDraft(account, meeting),
                    nowUtc);
                meetings++;
            }
        }
        catch (Exception exception) when (
            exception is not OperationCanceledException)
        {
            errors.Add($"Teams meeting context failed: {exception.GetType().Name}");
        }

        account.LastErrorUtc = errors.Count == 0 ? null : nowUtc;
        account.LastErrorCode = errors.Count == 0
            ? string.Empty
            : "microsoft-teams-partial-failure";
        account.LastErrorMessage = errors.Count == 0
            ? string.Empty
            : string.Join("; ", errors);

        return new(
            messages,
            meetings,
            suggestions,
            removed,
            accessLost,
            errors,
            nowUtc);
    }

    private static bool LooksActionable(string text) =>
        text.Contains("follow up", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("please send", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("document", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("waiting on", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("decision", StringComparison.OrdinalIgnoreCase) ||
        text.Contains("approval", StringComparison.OrdinalIgnoreCase);
}
