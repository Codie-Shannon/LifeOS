namespace LifeOS.Companion.Core.Services;

public sealed record CaptureValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static CaptureValidationResult Success { get; } = new(true, Array.Empty<string>());
}
