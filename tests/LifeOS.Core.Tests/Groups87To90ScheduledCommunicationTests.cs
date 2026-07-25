using LifeOS.Core.ScheduledCommunications;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups87To90ScheduledCommunicationTests
{
    private readonly ScheduledCommunicationService _service = new();
    private readonly DateTimeOffset _sendAt = new(2026, 7, 27, 14, 0, 0, TimeSpan.FromHours(12));
    private readonly QuietHours _quietHours = new(new TimeOnly(21, 0), new TimeOnly(7, 0));

    [Fact]
    public void Sms_and_email_begin_as_unapproved_drafts()
    {
        ScheduledCommunication sms = Draft(CommunicationChannel.Sms);
        ScheduledCommunication email = Draft(CommunicationChannel.Gmail);

        Assert.Equal(ScheduledCommunicationState.Draft, sms.State);
        Assert.Equal(ScheduledCommunicationState.Draft, email.State);
        Assert.Null(sms.ApprovedAt);
    }

    [Fact]
    public void Editing_or_rescheduling_revokes_prior_approval()
    {
        ScheduledCommunication approved = _service.Approve(Draft(CommunicationChannel.Outlook), "user", _sendAt.AddHours(-1));

        ScheduledCommunication edited = _service.Edit(approved, "Updated body");
        ScheduledCommunication rescheduled = _service.Reschedule(approved, _sendAt.AddDays(1));

        Assert.Equal(ScheduledCommunicationState.Draft, edited.State);
        Assert.Null(edited.ApprovedAt);
        Assert.Equal(ScheduledCommunicationState.Draft, rescheduled.State);
    }

    [Fact]
    public void Quiet_hours_block_an_approved_message()
    {
        ScheduledCommunication message = _service.Approve(
            Draft(CommunicationChannel.Sms) with { ScheduledFor = _sendAt.Date.AddHours(22) },
            "user",
            _sendAt);

        Assert.False(_service.CanSend(message, _quietHours, _sendAt.Date.AddHours(22)));
    }

    [Fact]
    public void Emergency_stop_blocks_all_channels()
    {
        ScheduledCommunication message = _service.Approve(Draft(CommunicationChannel.Gmail), "user", _sendAt.AddHours(-1));
        _service.SetEmergencyStop(true);

        Assert.False(_service.CanSend(message, _quietHours, _sendAt));
    }

    [Fact]
    public void Approved_due_message_can_be_marked_sent_with_an_audit_record()
    {
        ScheduledCommunication message = _service.Approve(Draft(CommunicationChannel.Outlook), "user", _sendAt.AddHours(-1));

        ScheduledCommunication sent = _service.MarkSent(message, _quietHours, _sendAt);
        CommunicationAudit audit = _service.Audit(sent, "sent", "Provider acknowledged the message.", _sendAt);

        Assert.Equal(ScheduledCommunicationState.Sent, sent.State);
        Assert.Equal(message.Id, audit.CommunicationId);
    }

    private ScheduledCommunication Draft(CommunicationChannel channel) =>
        _service.Draft(
            channel,
            channel == CommunicationChannel.Sms ? "+64 21 555 0199" : "fictional@example.invalid",
            "Fictional follow-up",
            "This is fictional private-beta content.",
            _sendAt,
            "fictional://work/follow-up-1");
}
