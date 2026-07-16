using System.Security.Cryptography;

namespace LifeOS.Mobile.Security;

public sealed class MobileKeyStore
{
    private const string KeyName = "lifeos_full_mobile_local_key_v1";
    public async Task<byte[]> GetOrCreateKeyAsync()
    {
        var stored = await SecureStorage.Default.GetAsync(KeyName);
        if (!string.IsNullOrWhiteSpace(stored)) return Convert.FromBase64String(stored);
        var key = RandomNumberGenerator.GetBytes(32);
        await SecureStorage.Default.SetAsync(KeyName, Convert.ToBase64String(key));
        return key;
    }
}
