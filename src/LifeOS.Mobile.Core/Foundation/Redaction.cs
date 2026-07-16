using System.Text.RegularExpressions;

namespace LifeOS.Mobile.Core.Foundation;

public static partial class Redaction
{
    [GeneratedRegex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?i)(token|secret|authorization|password)\s*[:=]\s*[^\s,;]+")]
    private static partial Regex SecretRegex();

    public static string Safe(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var redacted = EmailRegex().Replace(value, "[redacted-email]");
        return SecretRegex().Replace(redacted, "$1=[redacted]");
    }
}
