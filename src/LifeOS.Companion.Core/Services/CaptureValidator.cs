namespace LifeOS.Companion.Core.Services;

public static class CaptureValidator
{
    public const int MaxTitleLength = 120;
    public const int MaxBodyLength = 10_000;
    public const int MaxCategoryLength = 40;

    public static CaptureValidationResult Validate(string? title, string? body, string? category)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(body))
            errors.Add("Enter a title or note body.");
        if (title?.Length > MaxTitleLength)
            errors.Add($"Title must be {MaxTitleLength} characters or fewer.");
        if (body?.Length > MaxBodyLength)
            errors.Add($"Body must be {MaxBodyLength} characters or fewer.");
        if (category?.Length > MaxCategoryLength)
            errors.Add($"Category must be {MaxCategoryLength} characters or fewer.");
        return errors.Count == 0 ? CaptureValidationResult.Success : new(false, errors);
    }
}
