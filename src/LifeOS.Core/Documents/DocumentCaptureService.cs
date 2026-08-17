using LifeOS.Core.Forms;

namespace LifeOS.Core.Documents;

public sealed record DocumentCaptureDraft(
    string? FileName,
    string? MediaType,
    byte[]? Bytes,
    DocumentType Type);

public static class DocumentCaptureService
{
    public const long MaximumBytes = 25L * 1024L * 1024L;

    public static FormValidationResult Validate(DocumentCaptureDraft draft)
    {
        List<FormFieldIssue> issues = [];
        Add(issues, FormValidation.Required("document-file", draft.FileName, "File"));
        Add(issues, FormValidation.MaximumLength("document-file", draft.FileName, "File name", 260));
        Add(issues, FormValidation.SingleLine("document-file", draft.FileName, "File name"));
        Add(issues, FormValidation.Required("document-media-type", draft.MediaType, "Media type"));
        if (draft.Bytes is null || draft.Bytes.Length == 0)
            issues.Add(new FormFieldIssue("document-file", "empty-file", "The selected file is empty."));
        else if (draft.Bytes.LongLength > MaximumBytes)
            issues.Add(new FormFieldIssue("document-file", "file-too-large", "The selected file must be 25 MB or smaller."));
        return new FormValidationResult(issues);
    }

    public static DocumentRecord Create(DocumentCaptureDraft draft, DateTimeOffset importedUtc)
    {
        FormValidationResult validation = Validate(draft);
        if (!validation.IsValid)
            throw new ArgumentException("The document capture is invalid.", nameof(draft));

        string id = Guid.NewGuid().ToString("N");
        OriginalDocument original = DocumentIntegrity.PreserveOriginal(
            id,
            Path.GetFileName(draft.FileName!.Trim()),
            draft.MediaType!.Trim(),
            draft.Bytes!,
            importedUtc,
            "Desktop file picker",
            "User-selected local original");
        return new DocumentIntakeService().CreateDraft(original, draft.Type);
    }

    private static void Add(ICollection<FormFieldIssue> issues, FormFieldIssue? issue)
    {
        if (issue is not null) issues.Add(issue);
    }
}
