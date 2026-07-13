using LifeOS.Companion.Core.Models;
using LifeOS.Companion.Core.Storage;

namespace LifeOS.Companion.Core.Services;

public sealed class CaptureService
{
    private readonly ICompanionStore _store;
    public CaptureService(ICompanionStore store) => _store = store;

    public async Task<QuickCapture> SavePendingAsync(
        string? captureId, string? title, string? body, string? category,
        DeviceProfile device, DateTimeOffset? createdAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        var validation = CaptureValidator.Validate(title, body, category);
        if (!validation.IsValid) throw new CaptureValidationException(validation.Errors);

        var now = DateTimeOffset.UtcNow;
        var existing = string.IsNullOrWhiteSpace(captureId)
            ? null
            : (await _store.GetCapturesAsync(cancellationToken)).FirstOrDefault(x => x.CaptureId == captureId);

        var capture = new QuickCapture
        {
            CaptureId = existing?.CaptureId ?? Guid.NewGuid().ToString("N"),
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            Body = body?.Trim() ?? string.Empty,
            Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
            CreatedAtUtc = existing?.CreatedAtUtc ?? createdAtUtc ?? now,
            UpdatedAtUtc = now,
            DeliveryState = DeliveryState.Pending,
            SchemaVersion = SQLiteCompanionStore.CurrentSchemaVersion,
            DeviceId = device.DeviceId,
            ContentHash = ContentHasher.Compute(title, body ?? string.Empty, category)
        };
        await _store.SavePendingCaptureAsync(capture, cancellationToken);
        return capture;
    }
}

public sealed class CaptureValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }
    public CaptureValidationException(IReadOnlyList<string> errors) : base(string.Join(Environment.NewLine, errors)) => Errors = errors;
}
