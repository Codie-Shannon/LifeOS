using System.Security.Cryptography;
using System.Text;

namespace LifeOS.Companion.Core.Pairing;

public static class CompanionAuthenticator
{
    public static string CreateSignature(string secretBase64, string timestamp, string nonce, string body)
    {
        var key = Convert.FromBase64String(secretBase64);
        var bodyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        var canonical = $"{timestamp}\n{nonce}\n{bodyHash}";
        return Convert.ToBase64String(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(canonical)));
    }

    public static bool Verify(string secretBase64, string timestamp, string nonce, string body, string supplied)
    {
        try
        {
            var expected = Convert.FromBase64String(CreateSignature(secretBase64, timestamp, nonce, body));
            var actual = Convert.FromBase64String(supplied);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch { return false; }
    }
}
