namespace LifeOS.Core.ScheduledCommunications;

public enum CommunicationChannel
{
    Sms,
    Gmail,
    Outlook,
    Social
}

public enum ScheduledCommunicationState
{
    Draft,
    Approved,
    Cancelled,
    Sent,
    Failed
}

public sealed record QuietHours(TimeOnly StartsAt, TimeOnly EndsAt)
{
    public bool Contains(TimeOnly time) =>
        StartsAt <= EndsAt
            ? time >= StartsAt && time < EndsAt
            : time >= StartsAt || time < EndsAt;
}

public sealed record ScheduledCommunication(
    string Id,
    CommunicationChannel Channel,
    string Recipient,
    string Subject,
    string Body,
    DateTimeOffset ScheduledFor,
    string Source,
    ScheduledCommunicationState State,
    DateTimeOffset? ApprovedAt,
    string ApprovedBy);

public sealed record CommunicationAudit(
    DateTimeOffset At,
    string CommunicationId,
    string Action,
    string Detail);

public sealed class ScheduledCommunicationService
{
    public bool EmergencyStopEnabled { get; private set; }

    public ScheduledCommunication Draft(
        CommunicationChannel channel,
        string recipient,
        string subject,
        string body,
        DateTimeOffset scheduledFor,
        string source)
    {
        if (string.IsNullOrWhiteSpace(recipient) || string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("A recipient and message body are required.");
        }

        return new ScheduledCommunication(
            Guid.NewGuid().ToString("N"),
            channel,
            recipient.Trim(),
            subject.Trim(),
            body.Trim(),
            scheduledFor,
            source.Trim(),
            ScheduledCommunicationState.Draft,
            null,
            "");
    }

    public ScheduledCommunication Approve(
        ScheduledCommunication communication,
        string approvedBy,
        DateTimeOffset at)
    {
        if (communication.State != ScheduledCommunicationState.Draft)
        {
            throw new InvalidOperationException("Only a draft can be approved.");
        }
        if (string.IsNullOrWhiteSpace(approvedBy))
        {
            throw new ArgumentException("The approving user is required.", nameof(approvedBy));
        }

        return communication with
        {
            State = ScheduledCommunicationState.Approved,
            ApprovedAt = at,
            ApprovedBy = approvedBy.Trim()
        };
    }

    public ScheduledCommunication Edit(
        ScheduledCommunication communication,
        string body)
    {
        if (communication.State is ScheduledCommunicationState.Sent or ScheduledCommunicationState.Cancelled)
        {
            throw new InvalidOperationException("Sent or cancelled communication cannot be edited.");
        }

        return communication with
        {
            Body = body.Trim(),
            State = ScheduledCommunicationState.Draft,
            ApprovedAt = null,
            ApprovedBy = ""
        };
    }

    public ScheduledCommunication Reschedule(
        ScheduledCommunication communication,
        DateTimeOffset scheduledFor) =>
        communication.State is ScheduledCommunicationState.Sent or ScheduledCommunicationState.Cancelled
            ? throw new InvalidOperationException("Sent or cancelled communication cannot be rescheduled.")
            : communication with
            {
                ScheduledFor = scheduledFor,
                State = ScheduledCommunicationState.Draft,
                ApprovedAt = null,
                ApprovedBy = ""
            };

    public ScheduledCommunication Cancel(ScheduledCommunication communication) =>
        communication.State == ScheduledCommunicationState.Sent
            ? throw new InvalidOperationException("Sent communication cannot be cancelled.")
            : communication with { State = ScheduledCommunicationState.Cancelled };

    public bool CanSend(
        ScheduledCommunication communication,
        QuietHours quietHours,
        DateTimeOffset now)
    {
        return !EmergencyStopEnabled &&
               communication.State == ScheduledCommunicationState.Approved &&
               communication.ApprovedAt is not null &&
               communication.ScheduledFor <= now &&
               !quietHours.Contains(TimeOnly.FromDateTime(now.LocalDateTime));
    }

    public ScheduledCommunication MarkSent(
        ScheduledCommunication communication,
        QuietHours quietHours,
        DateTimeOffset now) =>
        CanSend(communication, quietHours, now)
            ? communication with { State = ScheduledCommunicationState.Sent }
            : throw new InvalidOperationException("The communication is not permitted to send.");

    public void SetEmergencyStop(bool enabled) => EmergencyStopEnabled = enabled;

    public CommunicationAudit Audit(
        ScheduledCommunication communication,
        string action,
        string detail,
        DateTimeOffset at) =>
        new(at, communication.Id, action, detail);
}
