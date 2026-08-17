using System.Text.Json;

namespace LifeOS.Core.Forms;

public enum FormIssueSeverity
{
    Information,
    Warning,
    Error
}

public sealed record FormFieldIssue(
    string FieldId,
    string Code,
    string Message,
    FormIssueSeverity Severity = FormIssueSeverity.Error);

public sealed record FormValidationResult(IReadOnlyList<FormFieldIssue> Issues)
{
    public bool IsValid => Issues.All(issue => issue.Severity != FormIssueSeverity.Error);

    public IReadOnlyList<FormFieldIssue> ForField(string fieldId) => Issues
        .Where(issue => string.Equals(issue.FieldId, fieldId, StringComparison.OrdinalIgnoreCase))
        .ToArray();
}

public static class FormValidation
{
    public static FormFieldIssue? Required(string fieldId, string? value, string label) =>
        string.IsNullOrWhiteSpace(value)
            ? new FormFieldIssue(fieldId, "required", $"{label} is required.")
            : null;

    public static FormFieldIssue? MaximumLength(
        string fieldId,
        string? value,
        string label,
        int maximumLength)
    {
        if (maximumLength < 1)
            throw new ArgumentOutOfRangeException(nameof(maximumLength));
        return (value ?? string.Empty).Trim().Length > maximumLength
            ? new FormFieldIssue(
                fieldId,
                "maximum-length",
                $"{label} must be {maximumLength} characters or fewer.")
            : null;
    }

    public static FormFieldIssue? SingleLine(string fieldId, string? value, string label) =>
        (value ?? string.Empty).Any(character => character is '\r' or '\n' || char.IsControl(character))
            ? new FormFieldIssue(fieldId, "single-line", $"{label} must use one line of display text.")
            : null;

    public static FormValidationResult Combine(params FormFieldIssue?[] issues) =>
        new(issues.Where(issue => issue is not null).Cast<FormFieldIssue>().ToArray());
}

public sealed record UserFacingProblem(
    string Code,
    string Title,
    string Detail,
    string RecoveryAction,
    bool CanRetry,
    FormIssueSeverity Severity = FormIssueSeverity.Error);

public static class UserFacingProblemFactory
{
    public static UserFacingProblem FromException(
        Exception exception,
        string operation,
        string? recoveryAction = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        string safeOperation = string.IsNullOrWhiteSpace(operation)
            ? "complete that action"
            : operation.Trim();

        return exception switch
        {
            UnauthorizedAccessException => new UserFacingProblem(
                "access-denied",
                "Permission is required",
                $"LifeOS could not {safeOperation} because the local location is not writable.",
                recoveryAction ?? "Check the folder permissions, then try again.",
                true),
            IOException => new UserFacingProblem(
                "local-storage-unavailable",
                "Local storage is unavailable",
                $"LifeOS could not {safeOperation}. Existing data was left unchanged.",
                recoveryAction ?? "Close any app using the file, confirm free space, then try again.",
                true),
            JsonException => new UserFacingProblem(
                "local-data-unreadable",
                "Local data needs attention",
                $"LifeOS could not {safeOperation} because the local data format was unreadable.",
                recoveryAction ?? "Open Local Data & Recovery and inspect the preserved source and backup.",
                false),
            _ => new UserFacingProblem(
                "unexpected-local-error",
                "The action was not completed",
                $"LifeOS could not {safeOperation}. No success was recorded.",
                recoveryAction ?? "Review the current values and try again. If it repeats, inspect diagnostics.",
                true)
        };
    }
}
