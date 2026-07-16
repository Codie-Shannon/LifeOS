namespace LifeOS.Core.MicrosoftProvider;

public interface IMicrosoftTeamsReader
{
    Task<IReadOnlyList<MicrosoftTeamDescriptor>> GetTeamsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MicrosoftChannelDescriptor>> GetChannelsAsync(
        string teamId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MicrosoftTeamsMessageDescriptor>> GetMessagesAsync(
        string teamId,
        string channelId,
        DateTimeOffset sinceUtc,
        int maximumItems,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MicrosoftTeamsMeetingDescriptor>> GetMeetingsAsync(
        DateTimeOffset sinceUtc,
        DateTimeOffset untilUtc,
        CancellationToken cancellationToken = default);
}
