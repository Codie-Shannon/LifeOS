using System.Security.Cryptography;

namespace LifeOS.Companion.Security;

public sealed class SecureStorageKeyStore
{
    private const string KeyName = "lifeos_companion_field_key_v1";

    public async Task<byte[]> GetOrCreateKeyAsync()
    {
        var encoded = await SecureStorage.Default.GetAsync(KeyName);
        if (!string.IsNullOrWhiteSpace(encoded)) return Convert.FromBase64String(encoded);
        var key = RandomNumberGenerator.GetBytes(32);
        await SecureStorage.Default.SetAsync(KeyName, Convert.ToBase64String(key));
        return key;
    }
}
