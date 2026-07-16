using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LifeOS.Mobile.Core.Foundation;

namespace LifeOS.Mobile.Core.Storage;

public sealed class JsonMobileLocalStore : IMobileLocalStore
{
    private const int CurrentSchema = 1;
    private readonly string _path;
    private readonly byte[] _key;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private StoreDocument _document = StoreDocument.CreateDefault();

    public JsonMobileLocalStore(string path, byte[] key)
    {
        _path = path;
        _key = key.Length == 32 ? key.ToArray() : throw new ArgumentException("A 256-bit key is required.", nameof(key));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            if (!File.Exists(_path)) { await PersistAsync(cancellationToken); return; }
            var encrypted = await File.ReadAllBytesAsync(_path, cancellationToken);
            var json = Decrypt(encrypted);
            _document = JsonSerializer.Deserialize<StoreDocument>(json) ?? throw new InvalidDataException("Local store is unreadable.");
            if (_document.SchemaVersion > CurrentSchema) throw new InvalidDataException("Local store schema is newer than this app.");
            if (_document.SchemaVersion < CurrentSchema) { _document = Migrate(_document); await PersistAsync(cancellationToken); }
        }
        finally { _gate.Release(); }
    }

    public Task<MobilePreferences> LoadPreferencesAsync(CancellationToken cancellationToken = default) => Task.FromResult(_document.Preferences);
    public Task<MobileSessionState> LoadSessionAsync(CancellationToken cancellationToken = default) => Task.FromResult(_document.Session);
    public Task<MobileSyncSnapshot> LoadSyncAsync(CancellationToken cancellationToken = default) => Task.FromResult(_document.Sync);
    public Task<IReadOnlyList<MobileOutboxItem>> LoadOutboxAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<MobileOutboxItem>>(_document.Outbox.ToArray());

    public Task SavePreferencesAsync(MobilePreferences preferences, CancellationToken cancellationToken = default) => MutateAsync(d => d.Preferences = preferences, cancellationToken);
    public Task SaveSessionAsync(MobileSessionState state, CancellationToken cancellationToken = default) => MutateAsync(d => d.Session = state, cancellationToken);
    public Task SaveSyncAsync(MobileSyncSnapshot state, CancellationToken cancellationToken = default) => MutateAsync(d => d.Sync = state, cancellationToken);

    public Task QueueAsync(MobileOutboxItem item, CancellationToken cancellationToken = default) => MutateAsync(d =>
    {
        if (d.Outbox.Any(x => x.CommandId == item.CommandId)) return;
        d.Outbox.Add(item);
        d.Sync = d.Sync with { State = MobileSyncState.Pending, PendingCount = d.Outbox.Count };
    }, cancellationToken);

    public async Task ClearDeviceDataAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try { _document = StoreDocument.CreateDefault(); if (File.Exists(_path)) File.Delete(_path); }
        finally { _gate.Release(); }
    }

    private async Task MutateAsync(Action<StoreDocument> mutation, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try { mutation(_document); await PersistAsync(cancellationToken); }
        finally { _gate.Release(); }
    }

    private async Task PersistAsync(CancellationToken cancellationToken)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(_document, new JsonSerializerOptions { WriteIndented = true });
        var encrypted = Encrypt(json);
        var temp = _path + ".tmp";
        await File.WriteAllBytesAsync(temp, encrypted, cancellationToken);
        File.Move(temp, _path, true);
    }

    private byte[] Encrypt(byte[] plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var ciphertext = new byte[plaintext.Length];
        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        return [.. nonce, .. tag, .. ciphertext];
    }

    private byte[] Decrypt(byte[] input)
    {
        if (input.Length < 29) throw new InvalidDataException("Local store is truncated.");
        var nonce = input[..12]; var tag = input[12..28]; var ciphertext = input[28..]; var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(_key, 16);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }

    private static StoreDocument Migrate(StoreDocument value) => value with { SchemaVersion = CurrentSchema };

    private sealed record StoreDocument
    {
        public int SchemaVersion { get; set; } = CurrentSchema;
        public MobilePreferences Preferences { get; set; } = default!;
        public MobileSessionState Session { get; set; } = default!;
        public MobileSyncSnapshot Sync { get; set; } = default!;
        public List<MobileOutboxItem> Outbox { get; set; } = [];

        public static StoreDocument CreateDefault() => new()
        {
            Preferences = new(MobileThemeMode.System, "Violet", MobileDensity.Comfortable, true, true),
            Session = new(true, false, "Full Mobile demo device"),
            Sync = new(MobileSyncState.Idle, 0, DateTimeOffset.UtcNow.AddMinutes(-4), "Fresh")
        };
    }
}
