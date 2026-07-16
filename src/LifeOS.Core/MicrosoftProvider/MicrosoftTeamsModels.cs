using System.Text.Json.Serialization;

namespace LifeOS.Core.MicrosoftProvider;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MicrosoftTeamsSourceState
{
    Current,
    Edited,
    Deleted,
    SourceRemoved,
    AccessLost
}

public sealed record MicrosoftTeamDescriptor(
    string Id,
    string DisplayName,
    string Description,
    bool IsArchived);

public sealed record MicrosoftChannelDescriptor(
    string Id,
    string TeamId,
    string DisplayName,
    string Description,
    string MembershipType);

public sealed record MicrosoftTeamsMessageDescriptor(
    string Id,
    string TeamId,
    string ChannelId,
    string ThreadId,
    string? ReplyToId,
    string AuthorDisplayName,
    string Subject,
    string BodyPreview,
    DateTimeOffset CreatedUtc,
    DateTimeOffset? ModifiedUtc,
    string WebUrl,
    MicrosoftTeamsSourceState SourceState);

public sealed record MicrosoftTeamsMeetingDescriptor(
    string Id,
    string Subject,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    string Organizer,
    IReadOnlyList<string> Attendees,
    string JoinUrl,
    MicrosoftTeamsSourceState SourceState);

public sealed class MicrosoftTeamsSelection
{
    public List<string> SelectedTeamIds { get; set; } = [];
    public List<string> SelectedChannelIds { get; set; } = [];
    public int LookbackDays { get; set; } = 30;
    public int MaximumMessagesPerChannel { get; set; } = 100;

    public MicrosoftTeamsSelection Normalize()
    {
        SelectedTeamIds = NormalizeIds(SelectedTeamIds);
        SelectedChannelIds = NormalizeIds(SelectedChannelIds);
        LookbackDays = Math.Clamp(LookbackDays, 1, 180);
        MaximumMessagesPerChannel = Math.Clamp(MaximumMessagesPerChannel, 1, 500);
        return this;
    }

    private static List<string> NormalizeIds(IEnumerable<string>? values) =>
        (values ?? [])
            .Select(value => value.Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}

public sealed record MicrosoftTeamsSyncResult(
    int MessageCandidates,
    int MeetingCandidates,
    int Suggestions,
    int SourceRemoved,
    int AccessLost,
    IReadOnlyList<string> Errors,
    DateTimeOffset CompletedUtc)
{
    public bool Succeeded => Errors.Count == 0;
    public bool PartiallySucceeded =>
        MessageCandidates + MeetingCandidates + Suggestions > 0 &&
        Errors.Count > 0;
}
