using System.Globalization;
using LifeOS.Core.Forms;

namespace LifeOS.Core.RelationshipRadar;

public sealed record RelationshipRadarDraft(string? Name, string? RoleOrContext, RelationshipWaitingOn WaitingOn, string? LastContactDate, string? NextFollowUpDate, string? LinkedWork, string? NextAction, string? Notes, bool DoNotChase);

public static class RelationshipRadarService
{
    public static FormValidationResult Validate(RelationshipRadarDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Field(issues, "relationship-name", draft.Name, "Name", 100, true); Field(issues, "relationship-context", draft.RoleOrContext, "Role or context", 100, true);
        Field(issues, "relationship-linked-work", draft.LinkedWork, "Linked work", 150, false); Field(issues, "relationship-next-action", draft.NextAction, "Next action", 200, true);
        Add(issues, FormValidation.MaximumLength("relationship-notes", draft.Notes, "Notes", 1000)); Date(issues, "relationship-last-contact", draft.LastContactDate, "Last contact"); Date(issues, "relationship-next-follow-up", draft.NextFollowUpDate, "Next follow-up");
        return new FormValidationResult(issues);
    }
    public static RelationshipRadarProfile Create(RelationshipRadarDraft draft, DateTimeOffset now)
    {
        if (!Validate(draft).IsValid) throw new ArgumentException("The relationship profile is invalid.", nameof(draft));
        return Normalize(new RelationshipRadarProfile { Id = Guid.NewGuid(), Name = draft.Name!.Trim(), RoleOrContext = draft.RoleOrContext!.Trim(), Status = draft.DoNotChase ? RelationshipRadarStatus.DoNotChaseYet : StatusFor(draft.WaitingOn), WaitingOn = draft.WaitingOn, LastContactDate = Parse(draft.LastContactDate), NextFollowUpDate = Parse(draft.NextFollowUpDate), LinkedWork = Empty(draft.LinkedWork) ?? string.Empty, NextAction = draft.NextAction!.Trim(), Notes = Empty(draft.Notes) ?? string.Empty, DoNotChase = draft.DoNotChase, CreatedAt = now.LocalDateTime, UpdatedAt = now.LocalDateTime });
    }
    public static RelationshipRadarProfile Transition(RelationshipRadarProfile profile, RelationshipRadarStatus next, DateTimeOffset now)
    {
        bool valid = profile.Status switch { RelationshipRadarStatus.Active or RelationshipRadarStatus.WaitingOnThem or RelationshipRadarStatus.WaitingOnMe or RelationshipRadarStatus.FollowUpDue => next is RelationshipRadarStatus.Active or RelationshipRadarStatus.WaitingOnThem or RelationshipRadarStatus.WaitingOnMe or RelationshipRadarStatus.DoNotChaseYet or RelationshipRadarStatus.Parked or RelationshipRadarStatus.Closed, RelationshipRadarStatus.DoNotChaseYet or RelationshipRadarStatus.Parked => next is RelationshipRadarStatus.Active or RelationshipRadarStatus.Closed, RelationshipRadarStatus.Closed => next == RelationshipRadarStatus.Active, _ => false };
        if (!valid || next == profile.Status) throw new InvalidOperationException($"Invalid relationship transition: {profile.Status} -> {next}.");
        RelationshipRadarProfile changed = Clone(profile); changed.Status = next; changed.DoNotChase = next == RelationshipRadarStatus.DoNotChaseYet; changed.UpdatedAt = now.LocalDateTime; return changed;
    }
    public static RelationshipRadarProfile Normalize(RelationshipRadarProfile profile)
    {
        if (profile.Id == Guid.Empty) throw new InvalidDataException("The relationship id is invalid."); RelationshipRadarProfile value = Clone(profile);
        value.Name = Required(profile.Name, "name", 100); value.RoleOrContext = Required(profile.RoleOrContext, "context", 100); value.LinkedWork = OptionalLine(profile.LinkedWork, "linked work", 150) ?? string.Empty; value.NextAction = Required(profile.NextAction, "next action", 200); value.Notes = OptionalText(profile.Notes, "notes", 1000) ?? string.Empty; return value;
    }
    private static RelationshipRadarStatus StatusFor(RelationshipWaitingOn waiting) => waiting switch { RelationshipWaitingOn.Me => RelationshipRadarStatus.WaitingOnMe, RelationshipWaitingOn.Them => RelationshipRadarStatus.WaitingOnThem, _ => RelationshipRadarStatus.Active };
    private static RelationshipRadarProfile Clone(RelationshipRadarProfile profile) => new() { Id = profile.Id, Name = profile.Name, RoleOrContext = profile.RoleOrContext, Status = profile.Status, WaitingOn = profile.WaitingOn, LastContactDate = profile.LastContactDate, NextFollowUpDate = profile.NextFollowUpDate, LinkedWork = profile.LinkedWork, NextAction = profile.NextAction, Notes = profile.Notes, DoNotChase = profile.DoNotChase, CreatedAt = profile.CreatedAt, UpdatedAt = profile.UpdatedAt };
    private static void Field(ICollection<FormFieldIssue> issues, string id, string? value, string label, int max, bool required) { if (required) Add(issues, FormValidation.Required(id, value, label)); Add(issues, FormValidation.MaximumLength(id, value, label, max)); Add(issues, FormValidation.SingleLine(id, value, label)); }
    private static void Date(ICollection<FormFieldIssue> issues, string id, string? value, string label) { if (!string.IsNullOrWhiteSpace(value) && !DateOnly.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) issues.Add(new FormFieldIssue(id, "date-format", $"{label} must use YYYY-MM-DD.")); }
    private static DateOnly? Parse(string? value) => string.IsNullOrWhiteSpace(value) ? null : DateOnly.ParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
    private static string Required(string? value, string label, int max) => OptionalLine(value, label, max) ?? throw new InvalidDataException($"The relationship {label} is required.");
    private static string? OptionalLine(string? value, string label, int max) { string? normalized = Empty(value); if (normalized is not null && (normalized.Length > max || normalized.Any(char.IsControl))) throw new InvalidDataException($"The relationship {label} is invalid."); return normalized; }
    private static string? OptionalText(string? value, string label, int max) { string? normalized = Empty(value); if (normalized is not null && (normalized.Length > max || normalized.Any(character => char.IsControl(character) && character is not '\r' and not '\n' and not '\t'))) throw new InvalidDataException($"The relationship {label} is invalid."); return normalized; }
    private static string? Empty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim(); private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue) { if (issue is not null) issues.Add(issue); }
}
