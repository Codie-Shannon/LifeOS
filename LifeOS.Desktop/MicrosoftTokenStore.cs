using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Text.Json;

namespace LifeOS.Desktop;

public sealed class MicrosoftTokenRecord
{
    public string AccountId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public string Scope { get; set; } = string.Empty;
    public DateTimeOffset ExpiresUtc { get; set; }
}

public sealed class MicrosoftTokenStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "LifeOS",
        "microsoft-provider-tokens.dat");

    private readonly string _path;

    public MicrosoftTokenStore(string? path = null)
    {
        _path = path ?? DefaultPath;
    }

    public IReadOnlyList<MicrosoftTokenRecord> LoadAll()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return [];
            }

            byte[] protectedBytes = File.ReadAllBytes(_path);
            byte[] clearBytes = ProtectedData.Unprotect(
                protectedBytes,
                optionalEntropy: null,
                DataProtectionScope.CurrentUser);

            string json = Encoding.UTF8.GetString(clearBytes);
            return JsonSerializer.Deserialize<List<MicrosoftTokenRecord>>(
                    json,
                    SerializerOptions) ??
                [];
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            CryptographicException or
            JsonException)
        {
            return [];
        }
    }

    public MicrosoftTokenRecord? Load(string accountId) =>
        LoadAll().SingleOrDefault(record =>
            string.Equals(
                record.AccountId,
                accountId,
                StringComparison.Ordinal));

    public void Save(MicrosoftTokenRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        List<MicrosoftTokenRecord> records = LoadAll().ToList();
        records.RemoveAll(existing =>
            string.Equals(
                existing.AccountId,
                record.AccountId,
                StringComparison.Ordinal));
        records.Add(record);

        string? directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(
            records,
            SerializerOptions);
        byte[] clearBytes = Encoding.UTF8.GetBytes(json);
        byte[] protectedBytes = ProtectedData.Protect(
            clearBytes,
            optionalEntropy: null,
            DataProtectionScope.CurrentUser);

        string temporaryPath = _path + ".tmp";
        File.WriteAllBytes(temporaryPath, protectedBytes);
        File.Move(temporaryPath, _path, overwrite: true);
    }

    public void Delete(string accountId)
    {
        List<MicrosoftTokenRecord> records = LoadAll()
            .Where(record => !string.Equals(
                record.AccountId,
                accountId,
                StringComparison.Ordinal))
            .ToList();

        if (records.Count == 0)
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }

            return;
        }

        string json = JsonSerializer.Serialize(
            records,
            SerializerOptions);
        byte[] protectedBytes = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(json),
            optionalEntropy: null,
            DataProtectionScope.CurrentUser);

        File.WriteAllBytes(_path, protectedBytes);
    }
}
