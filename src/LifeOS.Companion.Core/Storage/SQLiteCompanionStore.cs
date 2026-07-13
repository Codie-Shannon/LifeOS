using System.Globalization;
using LifeOS.Companion.Core.Models;
using System.IO;
using LifeOS.Companion.Core.Security;
using SQLite;

namespace LifeOS.Companion.Core.Storage;

public sealed class SQLiteCompanionStore : ICompanionStore
{
    private static readonly object SQLiteInitializationLock = new();
    private static bool _sqliteInitialized;

    private static void EnsureSQLiteInitialized()
    {
        if (_sqliteInitialized)
        {
            return;
        }

        lock (SQLiteInitializationLock)
        {
            if (_sqliteInitialized)
            {
                return;
            }

            SQLitePCL.Batteries.Init();
            _sqliteInitialized = true;
        }
    }
    public const int CurrentSchemaVersion = 3;
    private readonly SQLiteAsyncConnection _database;
    private readonly IFieldProtector _protector;
    private bool _initialized;

    public SQLiteCompanionStore(string databasePath, IFieldProtector protector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        _protector = protector ?? throw new ArgumentNullException(nameof(protector));
        _database = new SQLiteAsyncConnection(databasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        EnsureSQLiteInitialized();
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await _database.CreateTableAsync<SchemaRevisionEntity>();
            await _database.CreateTableAsync<AppStateEntity>();
            await _database.CreateTableAsync<CaptureEntity>();
            await _database.CreateTableAsync<OutboxEntity>();
            await _database.CreateTableAsync<GlanceCacheEntity>();
            await _database.CreateTableAsync<ConflictEntity>();
            await _database.CreateTableAsync<ConflictAuditEntity>();
            var revision = await _database.Table<SchemaRevisionEntity>().OrderByDescending(x => x.Revision).FirstOrDefaultAsync();
            if (revision is null)
            {
                await _database.InsertAsync(new SchemaRevisionEntity
                {
                    Revision = CurrentSchemaVersion,
                    AppliedAtUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture)
                });
            }
            else if (revision.Revision > CurrentSchemaVersion)
            {
                throw new InvalidOperationException($"Local data schema {revision.Revision} is newer than supported schema {CurrentSchemaVersion}.");
            }
            _initialized = true;
        }
        catch (Exception ex) when (ex is SQLiteException or InvalidOperationException)
        {
            throw new CompanionStoreException("LifeOS Companion could not safely open local data. No data was erased. Reinstall only after preserving diagnostic details.", ex);
        }
    }

    public async Task<DeviceProfile> GetOrCreateDeviceAsync(string defaultLabel, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        cancellationToken.ThrowIfCancellationRequested();
        var id = await GetStateAsync("device_id");
        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString("N");
            await SetStateAsync("device_id", id);
            await SetStateAsync("device_label", defaultLabel);
            await SetStateAsync("first_run_complete", "true");
            await SetStateAsync("connection_state", ((int)ConnectionState.NotPaired).ToString(CultureInfo.InvariantCulture));
        }
        var label = await GetStateAsync("device_label") ?? defaultLabel;
        var firstRun = bool.TryParse(await GetStateAsync("first_run_complete"), out var value) && value;
        var connectionValue = int.TryParse(await GetStateAsync("connection_state"), out var parsed) ? parsed : 0;
        return new DeviceProfile(id, label, (ConnectionState)connectionValue, firstRun, CurrentSchemaVersion);
    }

    public async Task UpdateDeviceLabelAsync(string deviceLabel, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(deviceLabel) || deviceLabel.Length > 80)
            throw new ArgumentException("Device label must be 1-80 characters.", nameof(deviceLabel));
        await SetStateAsync("device_label", deviceLabel.Trim());
    }

    public async Task<IReadOnlyList<QuickCapture>> GetCapturesAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        cancellationToken.ThrowIfCancellationRequested();
        var rows = await _database.Table<CaptureEntity>().OrderByDescending(x => x.UpdatedAtUtc).ToListAsync();
        return rows.Select(ToModel).ToList();
    }

    public async Task<IReadOnlyList<QuickCapture>> GetOutboxAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        cancellationToken.ThrowIfCancellationRequested();
        var queue = await _database.Table<OutboxEntity>().OrderBy(x => x.QueueOrdinal).ToListAsync();
        var result = new List<QuickCapture>();
        foreach (var item in queue)
        {
            var capture = await _database.FindAsync<CaptureEntity>(item.CaptureId);
            if (capture is not null) result.Add(ToModel(capture));
        }
        return result;
    }

    public async Task SavePendingCaptureAsync(QuickCapture capture, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        cancellationToken.ThrowIfCancellationRequested();
        if (capture.DeliveryState != DeliveryState.Pending)
            throw new InvalidOperationException("Group 32 saves desktop-bound captures only as Pending.");

        var entity = ToEntity(capture);
        await _database.RunInTransactionAsync(connection =>
        {
            connection.InsertOrReplace(entity);
            var existing = connection.Find<OutboxEntity>(capture.CaptureId);
            if (existing is null)
            {
                var next = connection.ExecuteScalar<long>("SELECT COALESCE(MAX(QueueOrdinal), 0) + 1 FROM outbox");
                connection.Insert(new OutboxEntity
                {
                    CaptureId = capture.CaptureId,
                    QueueOrdinal = next,
                    DeliveryState = (int)DeliveryState.Pending,
                    EnqueuedAtUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture)
                });
            }
            else
            {
                existing.DeliveryState = (int)DeliveryState.Pending;
                connection.Update(existing);
            }
        });
    }


    public async Task UpdateDeliveryStateAsync(string captureId, DeliveryState state, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        cancellationToken.ThrowIfCancellationRequested();
        var capture = await _database.FindAsync<CaptureEntity>(captureId) ?? throw new InvalidOperationException("Capture not found.");
        capture.DeliveryState = (int)state;
        capture.UpdatedAtUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        await _database.RunInTransactionAsync(connection =>
        {
            connection.Update(capture);
            var outbox = connection.Find<OutboxEntity>(captureId);
            if (outbox is not null)
            {
                outbox.DeliveryState = (int)state;
                connection.Update(outbox);
            }
        });
    }

    public async Task NormalizeStaleSendingAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        cancellationToken.ThrowIfCancellationRequested();
        var rows = await _database.Table<CaptureEntity>().Where(x => x.DeliveryState == (int)DeliveryState.Sending).ToListAsync();
        foreach (var row in rows) await UpdateDeliveryStateAsync(row.CaptureId, DeliveryState.Pending, cancellationToken);
    }

    public async Task DeleteDraftAsync(string captureId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        cancellationToken.ThrowIfCancellationRequested();
        var capture = await _database.FindAsync<CaptureEntity>(captureId);
        if (capture is null) return;
        if ((DeliveryState)capture.DeliveryState != DeliveryState.Draft)
            throw new InvalidOperationException("Pending captures cannot be silently deleted. Cancel behaviour belongs to a later approved workflow.");
        await _database.DeleteAsync(capture);
    }


    public async Task SaveGlanceAsync(GlanceSnapshot snapshot,CancellationToken cancellationToken=default)
    { EnsureInitialized(); cancellationToken.ThrowIfCancellationRequested(); var json=System.Text.Json.JsonSerializer.Serialize(snapshot); await _database.InsertOrReplaceAsync(new GlanceCacheEntity{Kind=(int)snapshot.Kind,PayloadProtected=_protector.Protect(json),UpdatedAtUtc=snapshot.UpdatedAtUtc.ToString("O",CultureInfo.InvariantCulture)}); }
    public async Task<IReadOnlyList<GlanceSnapshot>> GetGlanceAsync(CancellationToken cancellationToken=default)
    { EnsureInitialized(); cancellationToken.ThrowIfCancellationRequested(); var rows=await _database.Table<GlanceCacheEntity>().ToListAsync(); var list=new List<GlanceSnapshot>(); foreach(var row in rows){try{var v=System.Text.Json.JsonSerializer.Deserialize<GlanceSnapshot>(_protector.Unprotect(row.PayloadProtected));if(v is not null)list.Add(v);}catch{throw new CompanionStoreException("Cached glance data is malformed. Captures were not deleted. Clear glance cache and refresh explicitly.",new InvalidDataException("Malformed protected glance payload."));}} return list; }
    public async Task ClearGlanceCacheAsync(CancellationToken cancellationToken=default){EnsureInitialized();cancellationToken.ThrowIfCancellationRequested();await _database.DeleteAllAsync<GlanceCacheEntity>();}
    public async Task SaveConflictAsync(CaptureConflict c,CancellationToken cancellationToken=default){EnsureInitialized();cancellationToken.ThrowIfCancellationRequested();await _database.InsertOrReplaceAsync(new ConflictEntity{ConflictId=c.ConflictId,CaptureId=c.CaptureId,PhoneContentHash=c.PhoneContentHash,DesktopContentHash=c.DesktopContentHash,DetectedAtUtc=c.DetectedAtUtc.ToString("O"),Resolved=c.Resolved,Resolution=c.Resolution is null?null:(int)c.Resolution,ResolvedAtUtc=c.ResolvedAtUtc?.ToString("O")});}
    public async Task<IReadOnlyList<CaptureConflict>> GetConflictsAsync(CancellationToken cancellationToken=default){EnsureInitialized();var rows=await _database.Table<ConflictEntity>().OrderByDescending(x=>x.DetectedAtUtc).ToListAsync();return rows.Select(x=>new CaptureConflict(x.ConflictId,x.CaptureId,x.PhoneContentHash,x.DesktopContentHash,DateTimeOffset.Parse(x.DetectedAtUtc),x.Resolved,x.Resolution is null?null:(ConflictResolutionChoice)x.Resolution, string.IsNullOrWhiteSpace(x.ResolvedAtUtc)?null:DateTimeOffset.Parse(x.ResolvedAtUtc))).ToList();}
    public async Task ResolveConflictAsync(string conflictId,ConflictResolutionChoice choice,CancellationToken cancellationToken=default){EnsureInitialized();var row=await _database.FindAsync<ConflictEntity>(conflictId)??throw new InvalidOperationException("Conflict not found.");if(choice==ConflictResolutionChoice.Cancel)return;row.Resolved=true;row.Resolution=(int)choice;row.ResolvedAtUtc=DateTimeOffset.UtcNow.ToString("O");await _database.RunInTransactionAsync(c=>{c.Update(row);c.Insert(new ConflictAuditEntity{AuditId=Guid.NewGuid().ToString("N"),ConflictId=conflictId,Choice=(int)choice,AtUtc=DateTimeOffset.UtcNow.ToString("O"),Detail="Explicit phone-side conflict resolution."});});}
    public async Task<LocalDataSummary> GetLocalDataSummaryAsync(bool paired,CancellationToken cancellationToken=default){var captures=await GetCapturesAsync(cancellationToken);var glances=await GetGlanceAsync(cancellationToken);var conflicts=await GetConflictsAsync(cancellationToken);return new(captures.Count,captures.Count(x=>x.DeliveryState==DeliveryState.Pending),captures.Count(x=>x.DeliveryState==DeliveryState.Failed),captures.Count(x=>x.DeliveryState==DeliveryState.Delivered),paired,glances.Count,conflicts.Count(x=>!x.Resolved));}
    public async Task SetStateValueAsync(string key,string value,CancellationToken cancellationToken=default){EnsureInitialized();cancellationToken.ThrowIfCancellationRequested();await SetStateAsync(key,value);}
    public async Task<string?> GetStateValueAsync(string key,CancellationToken cancellationToken=default){EnsureInitialized();cancellationToken.ThrowIfCancellationRequested();return await GetStateAsync(key);}

    private CaptureEntity ToEntity(QuickCapture capture) => new()
    {
        CaptureId = capture.CaptureId,
        TitleProtected = _protector.Protect(capture.Title ?? string.Empty),
        BodyProtected = _protector.Protect(capture.Body),
        CategoryProtected = _protector.Protect(capture.Category ?? string.Empty),
        CreatedAtUtc = capture.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
        UpdatedAtUtc = capture.UpdatedAtUtc.ToString("O", CultureInfo.InvariantCulture),
        DeliveryState = (int)capture.DeliveryState,
        SchemaVersion = capture.SchemaVersion,
        DeviceId = capture.DeviceId,
        ContentHash = capture.ContentHash
    };

    private QuickCapture ToModel(CaptureEntity entity) => new()
    {
        CaptureId = entity.CaptureId,
        Title = EmptyToNull(_protector.Unprotect(entity.TitleProtected)),
        Body = _protector.Unprotect(entity.BodyProtected),
        Category = EmptyToNull(_protector.Unprotect(entity.CategoryProtected)),
        CreatedAtUtc = DateTimeOffset.Parse(entity.CreatedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        UpdatedAtUtc = DateTimeOffset.Parse(entity.UpdatedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        DeliveryState = (DeliveryState)entity.DeliveryState,
        SchemaVersion = entity.SchemaVersion,
        DeviceId = entity.DeviceId,
        ContentHash = entity.ContentHash
    };

    private static string? EmptyToNull(string value) => string.IsNullOrEmpty(value) ? null : value;
    private async Task<string?> GetStateAsync(string key) => (await _database.FindAsync<AppStateEntity>(key))?.Value;
    private Task<int> SetStateAsync(string key, string value) => _database.InsertOrReplaceAsync(new AppStateEntity { Key = key, Value = value });
    public async Task CloseAsync()
    {
        await _database.CloseAsync();
        _initialized = false;
    }

    private void EnsureInitialized()
    {
        if (!_initialized) throw new InvalidOperationException("InitializeAsync must complete before local data is used.");
    }
}

public sealed class CompanionStoreException : Exception
{
    public CompanionStoreException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class GlanceCacheEntity { [PrimaryKey] public int Kind{get;set;} public byte[] PayloadProtected{get;set;}=Array.Empty<byte>(); public string UpdatedAtUtc{get;set;}=""; }
public sealed class ConflictEntity { [PrimaryKey] public string ConflictId{get;set;}=""; public string CaptureId{get;set;}=""; public string PhoneContentHash{get;set;}=""; public string DesktopContentHash{get;set;}=""; public string DetectedAtUtc{get;set;}=""; public bool Resolved{get;set;} public int? Resolution{get;set;} public string? ResolvedAtUtc{get;set;} }
public sealed class ConflictAuditEntity { [PrimaryKey] public string AuditId{get;set;}=""; public string ConflictId{get;set;}=""; public int Choice{get;set;} public string AtUtc{get;set;}=""; public string Detail{get;set;}=""; }
