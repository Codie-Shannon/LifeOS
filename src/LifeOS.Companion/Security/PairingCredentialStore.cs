using System.Text.Json;
using LifeOS.Companion.Core.Pairing;

namespace LifeOS.Companion.Security;

public sealed class PairingCredentialStore
{
    private const string StorageKey = "lifeos_companion_pairing_v1";

    // Static process cache guarantees that Pair and Outbox see the same
    // confirmed credential even if MAUI recreates either page or service.
    private static PairingCredential? _processCredential;
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public async Task<PairingCredential?> LoadAsync()
    {
        if (_processCredential is not null)
        {
            return _processCredential;
        }

        await Gate.WaitAsync();

        try
        {
            if (_processCredential is not null)
            {
                return _processCredential;
            }

            var stored = await SecureStorage.Default.GetAsync(StorageKey);

            if (string.IsNullOrWhiteSpace(stored))
            {
                return null;
            }

            var credential =
                JsonSerializer.Deserialize<PairingCredential>(stored);

            if (credential is null ||
                string.IsNullOrWhiteSpace(credential.PairingId) ||
                string.IsNullOrWhiteSpace(credential.Endpoint) ||
                string.IsNullOrWhiteSpace(credential.SecretBase64))
            {
                SecureStorage.Default.Remove(StorageKey);
                return null;
            }

            _processCredential = credential;
            return credential;
        }
        finally
        {
            Gate.Release();
        }
    }

    public async Task SaveAsync(PairingCredential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        if (string.IsNullOrWhiteSpace(credential.PairingId) ||
            string.IsNullOrWhiteSpace(credential.Endpoint) ||
            string.IsNullOrWhiteSpace(credential.SecretBase64))
        {
            throw new InvalidOperationException(
                "The confirmed pairing credential is incomplete.");
        }

        await Gate.WaitAsync();

        try
        {
            var json = JsonSerializer.Serialize(credential);

            await SecureStorage.Default.SetAsync(StorageKey, json);

            // Publish only after protected persistence succeeds.
            _processCredential = credential;
        }
        finally
        {
            Gate.Release();
        }
    }

    public void Revoke()
    {
        _processCredential = null;
        SecureStorage.Default.Remove(StorageKey);
    }
}
