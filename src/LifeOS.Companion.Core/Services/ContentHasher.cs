using System.Security.Cryptography;
using System.Text;

namespace LifeOS.Companion.Core.Services;

public static class ContentHasher
{
    public static string Compute(string? title, string body, string? category)
    {
        var canonical = $"{title?.Trim()}\n{body.Trim()}\n{category?.Trim()}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }
}
