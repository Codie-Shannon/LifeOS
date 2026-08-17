using System.Collections.Concurrent;
using System.Text.Json;

namespace LifeOS.Shared.Storage;

public enum LocalStoreLoadState
{
    Empty,
    Current,
    MigratedLegacy,
    MigratedSchema,
    RecoveredBackup,
    UnsupportedNewerSchema,
    Unreadable
}

public enum LocalStoreHealthState
{
    Missing,
    Healthy,
    LegacyFormat,
    OlderSchema,
    NewerSchema,
    Unreadable
}

public sealed record LocalStoreLoadResult<T>(
    T Value,
    LocalStoreLoadState State,
    int SchemaVersion,
    string Detail,
    string? PreservedPath = null);

public sealed record LocalStoreHealth(
    string StoreId,
    string FilePath,
    LocalStoreHealthState State,
    int? SchemaVersion,
    bool BackupAvailable,
    long Bytes,
    DateTimeOffset? LastWriteUtc,
    string Detail);

public sealed record LocalStoreTrashEntry(
    string Id,
    string StoreId,
    string OriginalPath,
    string TrashPath,
    string? BackupTrashPath,
    DateTimeOffset DeletedUtc,
    DateTimeOffset PurgeAfterUtc,
    long Bytes);

public sealed class VersionedJsonLocalStore<T> where T : notnull
{
    private static readonly ConcurrentDictionary<string, object> PathLocks =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly string _filePath;
    private readonly string _backupPath;
    private readonly string _storeId;
    private readonly int _currentSchema;
    private readonly Func<T> _emptyFactory;
    private readonly Func<T, T> _normalize;
    private readonly Func<int, JsonElement, T>? _migrate;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly JsonSerializerOptions _jsonOptions;

    public VersionedJsonLocalStore(
        string filePath,
        string storeId,
        int currentSchema,
        Func<T> emptyFactory,
        Func<T, T>? normalize = null,
        Func<int, JsonElement, T>? migrate = null,
        Func<DateTimeOffset>? utcNow = null,
        string? backupPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(storeId);
        if (currentSchema < 1)
            throw new ArgumentOutOfRangeException(nameof(currentSchema));

        _filePath = Path.GetFullPath(filePath);
        _backupPath = Path.GetFullPath(backupPath ?? (_filePath + ".backup"));
        _storeId = storeId.Trim();
        _currentSchema = currentSchema;
        _emptyFactory = emptyFactory ?? throw new ArgumentNullException(nameof(emptyFactory));
        _normalize = normalize ?? (value => value);
        _migrate = migrate;
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public string FilePath => _filePath;

    public string BackupPath => _backupPath;

    public LocalStoreLoadResult<T> Load()
    {
        lock (Gate())
        {
            if (!File.Exists(_filePath))
            {
                return new LocalStoreLoadResult<T>(
                    _normalize(_emptyFactory()),
                    LocalStoreLoadState.Empty,
                    _currentSchema,
                    "No local file exists; the ordinary empty state is active.");
            }

            try
            {
                return ReadPath(_filePath, persistMigration: true);
            }
            catch (Exception exception) when (IsReadableFailure(exception))
            {
                string? preserved = PreserveUnreadable(_filePath);
                LocalStoreLoadResult<T>? recovered = TryReadBackup();
                if (recovered is not null)
                {
                    WriteEnvelopeAtomic(recovered.Value, preserveCurrentAsBackup: false);
                    return recovered with
                    {
                        State = LocalStoreLoadState.RecoveredBackup,
                        SchemaVersion = _currentSchema,
                        Detail = "The primary file was unreadable; a validated backup was restored.",
                        PreservedPath = preserved
                    };
                }

                return new LocalStoreLoadResult<T>(
                    _normalize(_emptyFactory()),
                    LocalStoreLoadState.Unreadable,
                    _currentSchema,
                    "The local file was unreadable and no valid backup was available.",
                    preserved);
            }
        }
    }

    public void Save(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        lock (Gate())
        {
            WriteEnvelopeAtomic(_normalize(value), preserveCurrentAsBackup: true);
        }
    }

    public LocalStoreHealth Inspect()
    {
        lock (Gate())
        {
            FileInfo file = new(_filePath);
            if (!file.Exists)
            {
                return Health(
                    LocalStoreHealthState.Missing,
                    null,
                    0,
                    null,
                    "No local file exists yet.");
            }

            try
            {
                using JsonDocument document = ReadDocument(_filePath);
                if (!TryReadEnvelope(document.RootElement, out int schema, out _, out string? storeId))
                {
                    return Health(
                        LocalStoreHealthState.LegacyFormat,
                        0,
                        file.Length,
                        file.LastWriteTimeUtc,
                        "A supported legacy payload will migrate on the next load.");
                }

                if (!string.IsNullOrWhiteSpace(storeId) &&
                    !string.Equals(storeId, _storeId, StringComparison.Ordinal))
                {
                    return Health(
                        LocalStoreHealthState.Unreadable,
                        schema,
                        file.Length,
                        file.LastWriteTimeUtc,
                        "The file belongs to a different registered store.");
                }

                LocalStoreHealthState state = schema switch
                {
                    _ when schema == _currentSchema => LocalStoreHealthState.Healthy,
                    _ when schema < _currentSchema => LocalStoreHealthState.OlderSchema,
                    _ => LocalStoreHealthState.NewerSchema
                };
                string detail = state switch
                {
                    LocalStoreHealthState.Healthy => "The versioned local file is current.",
                    LocalStoreHealthState.OlderSchema => "The file will migrate without discarding its previous version.",
                    _ => "The file was created by a newer LifeOS schema and will not be overwritten."
                };
                return Health(state, schema, file.Length, file.LastWriteTimeUtc, detail);
            }
            catch (Exception exception) when (IsReadableFailure(exception))
            {
                return Health(
                    LocalStoreHealthState.Unreadable,
                    null,
                    file.Length,
                    file.LastWriteTimeUtc,
                    "The file cannot be read; loading will preserve it and attempt backup recovery.");
            }
        }
    }

    public LocalStoreTrashEntry MoveToTrash(TimeSpan? retention = null)
    {
        lock (Gate())
        {
            if (!File.Exists(_filePath))
                throw new InvalidOperationException("There is no local store file to move to Trash.");

            DateTimeOffset now = _utcNow();
            string id = $"{now:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";
            string trashDirectory = TrashDirectory();
            Directory.CreateDirectory(trashDirectory);
            string trashPath = Path.Combine(trashDirectory, id + ".data.json");
            string? backupTrashPath = null;
            long bytes = new FileInfo(_filePath).Length;

            File.Move(_filePath, trashPath);
            if (File.Exists(BackupPath))
            {
                backupTrashPath = Path.Combine(trashDirectory, id + ".backup.json");
                File.Move(BackupPath, backupTrashPath);
            }

            LocalStoreTrashEntry entry = new(
                id,
                _storeId,
                _filePath,
                trashPath,
                backupTrashPath,
                now,
                now.Add(retention ?? TimeSpan.FromDays(30)),
                bytes);
            WriteJsonAtomic(
                Path.Combine(trashDirectory, id + ".trash.json"),
                JsonSerializer.SerializeToUtf8Bytes(entry, _jsonOptions));
            return entry;
        }
    }

    public IReadOnlyList<LocalStoreTrashEntry> ListTrash()
    {
        lock (Gate())
        {
            string directory = TrashDirectory();
            if (!Directory.Exists(directory))
                return [];

            List<LocalStoreTrashEntry> entries = [];
            foreach (string manifest in Directory.GetFiles(directory, "*.trash.json"))
            {
                try
                {
                    LocalStoreTrashEntry? entry = JsonSerializer.Deserialize<LocalStoreTrashEntry>(
                        File.ReadAllBytes(manifest),
                        _jsonOptions);
                    if (entry is not null && File.Exists(entry.TrashPath))
                        entries.Add(entry);
                }
                catch (Exception exception) when (IsReadableFailure(exception))
                {
                }
            }
            return entries.OrderByDescending(entry => entry.DeletedUtc).ToArray();
        }
    }

    public void RestoreTrash(string entryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);
        lock (Gate())
        {
            if (File.Exists(_filePath))
                throw new InvalidOperationException("Restore would overwrite a current local file.");

            string manifest = Path.Combine(TrashDirectory(), entryId + ".trash.json");
            if (!File.Exists(manifest))
                throw new ArgumentException("The Trash entry was not found.", nameof(entryId));
            LocalStoreTrashEntry entry = JsonSerializer.Deserialize<LocalStoreTrashEntry>(
                File.ReadAllBytes(manifest),
                _jsonOptions) ?? throw new InvalidDataException("The Trash manifest is unreadable.");
            string trashRoot = Path.GetFullPath(TrashDirectory()) + Path.DirectorySeparatorChar;
            string payloadPath = Path.GetFullPath(entry.TrashPath);
            string? backupPayloadPath = string.IsNullOrWhiteSpace(entry.BackupTrashPath)
                ? null
                : Path.GetFullPath(entry.BackupTrashPath);
            if (!string.Equals(entry.StoreId, _storeId, StringComparison.Ordinal) ||
                !string.Equals(Path.GetFullPath(entry.OriginalPath), _filePath, StringComparison.OrdinalIgnoreCase) ||
                !payloadPath.StartsWith(trashRoot, StringComparison.OrdinalIgnoreCase) ||
                (backupPayloadPath is not null &&
                 !backupPayloadPath.StartsWith(trashRoot, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidDataException("The Trash entry does not belong to this store.");
            if (!File.Exists(payloadPath))
                throw new InvalidDataException("The Trash payload is missing.");

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.Move(payloadPath, _filePath);
            if (backupPayloadPath is not null && File.Exists(backupPayloadPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(BackupPath)!);
                File.Move(backupPayloadPath, BackupPath);
            }
            File.Delete(manifest);
        }
    }

    private LocalStoreLoadResult<T> ReadPath(string path, bool persistMigration)
    {
        using JsonDocument document = ReadDocument(path);
        if (!TryReadEnvelope(document.RootElement, out int schema, out JsonElement payload, out string? storeId))
        {
            T legacy = Deserialize(document.RootElement);
            T normalized = _normalize(legacy);
            if (persistMigration)
                WriteEnvelopeAtomic(normalized, preserveCurrentAsBackup: true);
            return new LocalStoreLoadResult<T>(
                normalized,
                LocalStoreLoadState.MigratedLegacy,
                _currentSchema,
                "A legacy payload was migrated and preserved as the backup.",
                persistMigration ? BackupPath : null);
        }

        if (!string.IsNullOrWhiteSpace(storeId) &&
            !string.Equals(storeId, _storeId, StringComparison.Ordinal))
            throw new InvalidDataException("The local file belongs to a different store.");
        if (schema > _currentSchema)
        {
            return new LocalStoreLoadResult<T>(
                _normalize(_emptyFactory()),
                LocalStoreLoadState.UnsupportedNewerSchema,
                schema,
                "A newer schema was detected. The file was left unchanged.");
        }

        T value = schema < _currentSchema && _migrate is not null
            ? _migrate(schema, payload)
            : Deserialize(payload);
        value = _normalize(value);
        if (schema < _currentSchema && persistMigration)
            WriteEnvelopeAtomic(value, preserveCurrentAsBackup: true);
        return new LocalStoreLoadResult<T>(
            value,
            schema < _currentSchema ? LocalStoreLoadState.MigratedSchema : LocalStoreLoadState.Current,
            _currentSchema,
            schema < _currentSchema
                ? "The older schema was migrated and preserved as the backup."
                : "The current versioned local file loaded successfully.",
            schema < _currentSchema && persistMigration ? BackupPath : null);
    }

    private LocalStoreLoadResult<T>? TryReadBackup()
    {
        if (!File.Exists(BackupPath))
            return null;
        try
        {
            LocalStoreLoadResult<T> result = ReadPath(BackupPath, persistMigration: false);
            return result.State == LocalStoreLoadState.UnsupportedNewerSchema
                ? null
                : result;
        }
        catch (Exception exception) when (IsReadableFailure(exception))
        {
            return null;
        }
    }

    private void WriteEnvelopeAtomic(T value, bool preserveCurrentAsBackup)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        if (preserveCurrentAsBackup && File.Exists(_filePath))
        {
            if (CanPreserveAsBackup(_filePath))
                File.Copy(_filePath, BackupPath, overwrite: true);
            else
                PreserveUnreadable(_filePath);
        }

        LocalStoreEnvelope<T> envelope = new(
            _storeId,
            _currentSchema,
            _utcNow(),
            value);
        WriteJsonAtomic(
            _filePath,
            JsonSerializer.SerializeToUtf8Bytes(envelope, _jsonOptions));
    }

    private void WriteJsonAtomic(string path, byte[] bytes)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        string temporary = path + $".{Guid.NewGuid():N}.tmp";
        try
        {
            using (FileStream stream = new(
                       temporary,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       4096,
                       FileOptions.WriteThrough))
            {
                stream.Write(bytes);
                stream.Write(Environment.NewLine.Select(character => (byte)character).ToArray());
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    private LocalStoreHealth Health(
        LocalStoreHealthState state,
        int? schema,
        long bytes,
        DateTime? lastWriteUtc,
        string detail) => new(
        _storeId,
        _filePath,
        state,
        schema,
        File.Exists(BackupPath),
        bytes,
        lastWriteUtc is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(lastWriteUtc.Value, DateTimeKind.Utc)),
        detail);

    private T Deserialize(JsonElement element) =>
        element.Deserialize<T>(_jsonOptions)
        ?? throw new InvalidDataException("The local payload was empty.");

    private static JsonDocument ReadDocument(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        if (bytes.Length == 0)
            throw new InvalidDataException("The local file was empty.");
        return JsonDocument.Parse(bytes);
    }

    private static bool TryReadEnvelope(
        JsonElement root,
        out int schema,
        out JsonElement payload,
        out string? storeId)
    {
        schema = 0;
        payload = default;
        storeId = null;
        if (root.ValueKind != JsonValueKind.Object ||
            !TryGet(root, "schemaVersion", out JsonElement schemaElement) ||
            !TryGet(root, "payload", out payload) ||
            schemaElement.ValueKind != JsonValueKind.Number ||
            !schemaElement.TryGetInt32(out schema))
            return false;
        if (TryGet(root, "storeId", out JsonElement storeElement) &&
            storeElement.ValueKind == JsonValueKind.String)
            storeId = storeElement.GetString();
        return true;
    }

    private static bool TryGet(JsonElement root, string name, out JsonElement value)
    {
        if (root.TryGetProperty(name, out value))
            return true;
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private string? PreserveUnreadable(string path)
    {
        if (!File.Exists(path))
            return null;
        string preserved = path + $".unreadable-{_utcNow():yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";
        File.Copy(path, preserved, overwrite: false);
        return preserved;
    }

    private bool CanPreserveAsBackup(string path)
    {
        try
        {
            using JsonDocument document = ReadDocument(path);
            if (TryReadEnvelope(document.RootElement, out int schema, out _, out string? storeId))
            {
                if (schema > _currentSchema)
                    throw new InvalidOperationException(
                        "A newer local schema cannot be overwritten by this LifeOS version.");
                if (!string.IsNullOrWhiteSpace(storeId) &&
                    !string.Equals(storeId, _storeId, StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "A local file registered to a different store cannot be overwritten.");
            }
            return true;
        }
        catch (Exception exception) when (IsReadableFailure(exception))
        {
            return false;
        }
    }

    private string TrashDirectory()
    {
        string safeStoreId = string.Concat(_storeId.Select(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_'
                ? character
                : '-'));
        return Path.Combine(Path.GetDirectoryName(_filePath)!, "trash", safeStoreId);
    }

    private object Gate() => PathLocks.GetOrAdd(_filePath, _ => new object());

    private static bool IsReadableFailure(Exception exception) =>
        exception is JsonException or IOException or UnauthorizedAccessException or InvalidDataException;

    private sealed record LocalStoreEnvelope<TPayload>(
        string StoreId,
        int SchemaVersion,
        DateTimeOffset UpdatedUtc,
        TPayload Payload);
}
