using System.Text.RegularExpressions;

namespace LifeOS.Core.AssistantMemory;

public sealed class AssistantMemoryService
{
    private static readonly string[] SecretMarkers =
    [
        "api_key", "apikey", "access_token", "refresh_token", "client_secret",
        "password", "private key", "bearer ", "oauth token", "credential"
    ];

    private readonly IAssistantMemoryStore _store;
    private readonly Dictionary<Guid, ProposedAssistantMemory> _proposals = [];

    public AssistantMemoryService(IAssistantMemoryStore store) => _store = store;

    public IReadOnlyList<AssistantMemoryRecord> List() => NormalizeExpired(_store.Load(), DateTimeOffset.UtcNow);

    public ProposedAssistantMemory Propose(
        string title,
        string statement,
        AssistantMemoryType type,
        AssistantMemoryScope scope,
        AssistantMemorySensitivity sensitivity,
        AssistantMemoryOrigin origin,
        DateTimeOffset? expiresAt = null,
        DateTimeOffset? reviewAt = null,
        DateTimeOffset? now = null)
    {
        var at = now ?? DateTimeOffset.UtcNow;
        ValidateText(title, statement, sensitivity);
        ValidateScope(scope);
        if (expiresAt is not null && expiresAt <= at) throw new ArgumentOutOfRangeException(nameof(expiresAt), "Expiry must be in the future.");

        var active = NormalizeExpired(_store.Load(), at)
            .Where(m => m.Status == AssistantMemoryStatus.Active)
            .ToArray();
        var normalized = Normalize(statement);
        var duplicates = active.Where(m => Normalize(m.Statement) == normalized).Select(m => m.MemoryId).ToArray();
        var conflicts = active.Where(m =>
                m.Type == type &&
                SameScope(m.Scope, scope) &&
                Normalize(m.Statement) != normalized &&
                TokenOverlap(m.Statement, statement) >= 0.45)
            .Select(m => m.MemoryId)
            .ToArray();

        var proposal = new ProposedAssistantMemory(
            Guid.NewGuid(), at, title.Trim(), statement.Trim(), type, scope, sensitivity,
            origin, expiresAt, reviewAt, duplicates, conflicts);
        _proposals[proposal.ProposalId] = proposal;
        return proposal;
    }

    public AssistantMemoryRecord Confirm(Guid proposalId, string confirmedBy, bool sensitivityAcknowledged, DateTimeOffset? now = null)
    {
        if (!_proposals.Remove(proposalId, out var proposal)) throw new InvalidOperationException("Memory proposal is missing, cancelled or already handled.");
        if (string.IsNullOrWhiteSpace(confirmedBy)) throw new ArgumentException("A confirming user is required.", nameof(confirmedBy));
        if (proposal.Sensitivity != AssistantMemorySensitivity.Standard && !sensitivityAcknowledged)
            throw new InvalidOperationException("Sensitive memory requires explicit acknowledgement.");
        if (proposal.DuplicateCandidates.Count > 0 || proposal.ConflictingCandidates.Count > 0)
            throw new InvalidOperationException("Duplicate or conflicting candidates require explicit resolution before saving.");

        var at = now ?? DateTimeOffset.UtcNow;
        var records = NormalizeExpired(_store.Load(), at).ToList();
        var record = new AssistantMemoryRecord(
            Guid.NewGuid(), at, at, proposal.Title, proposal.Statement, proposal.Type, proposal.Scope,
            proposal.Sensitivity, AssistantMemoryStatus.Active, proposal.Origin, confirmedBy.Trim(), at,
            proposal.ExpiresAt, proposal.ReviewAt, null, 0, []);
        records.Add(record);
        _store.Save(records);
        return record;
    }

    public AssistantMemoryRecord ConfirmResolved(Guid proposalId, string confirmedBy, bool sensitivityAcknowledged, bool saveDespiteCandidates, DateTimeOffset? now = null)
    {
        if (!_proposals.TryGetValue(proposalId, out var proposal)) throw new InvalidOperationException("Memory proposal is missing, cancelled or already handled.");
        if ((proposal.DuplicateCandidates.Count > 0 || proposal.ConflictingCandidates.Count > 0) && !saveDespiteCandidates)
            throw new InvalidOperationException("Explicit duplicate/conflict resolution is required.");
        _proposals[proposalId] = proposal with { DuplicateCandidates = [], ConflictingCandidates = [] };
        return Confirm(proposalId, confirmedBy, sensitivityAcknowledged, now);
    }

    public void Cancel(Guid proposalId) => _proposals.Remove(proposalId);

    public AssistantMemoryRecord Edit(Guid memoryId, string title, string statement, string revisedBy, string reason, DateTimeOffset? now = null)
    {
        var at = now ?? DateTimeOffset.UtcNow;
        var records = NormalizeExpired(_store.Load(), at).ToList();
        var index = records.FindIndex(m => m.MemoryId == memoryId);
        if (index < 0) throw new KeyNotFoundException("Memory record was not found.");
        var current = records[index];
        if (current.Status is AssistantMemoryStatus.Deleted or AssistantMemoryStatus.Revoked)
            throw new InvalidOperationException("Revoked or deleted memory cannot be edited.");
        ValidateText(title, statement, current.Sensitivity);
        var revision = new AssistantMemoryRevision(Guid.NewGuid(), at, current.Title, current.Statement, revisedBy, reason);
        var updated = current with
        {
            UpdatedAt = at,
            Title = title.Trim(),
            Statement = statement.Trim(),
            Revisions = current.Revisions.Append(revision).ToArray()
        };
        records[index] = updated;
        _store.Save(records);
        return updated;
    }

    public AssistantMemoryRecord Revoke(Guid memoryId, string revisedBy, DateTimeOffset? now = null) => ChangeStatus(memoryId, AssistantMemoryStatus.Revoked, revisedBy, "Revoked by user", now);
    public AssistantMemoryRecord Delete(Guid memoryId, string revisedBy, DateTimeOffset? now = null) => ChangeStatus(memoryId, AssistantMemoryStatus.Deleted, revisedBy, "Deleted by user; audit tombstone retained", now);

    public IReadOnlyList<AssistantMemoryUse> Retrieve(AssistantMemoryQuery query, DateTimeOffset? now = null)
    {
        var at = now ?? DateTimeOffset.UtcNow;
        var records = NormalizeExpired(_store.Load(), at).ToList();
        var uses = records
            .Where(m => m.IsRetrievable(at) && ScopeAllows(m.Scope, query))
            .Select(m => new { Memory = m, Score = Score(m, query.Text, at) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Memory.UpdatedAt)
            .Take(Math.Clamp(query.Limit, 1, 20))
            .Select(x => new AssistantMemoryUse(
                x.Memory,
                "Relevant permitted memory matched the current question and scope.",
                x.Score,
                $"Memory used: {x.Memory.Title} • {x.Memory.Scope.Display} • confirmed {x.Memory.ConfirmedAt:yyyy-MM-dd} • source {x.Memory.Origin.SourceTitle}"))
            .ToArray();

        if (query.RecordUsage && uses.Length > 0)
        {
            foreach (var use in uses)
            {
                var index = records.FindIndex(m => m.MemoryId == use.Memory.MemoryId);
                records[index] = records[index] with { LastUsedAt = at, UsageCount = records[index].UsageCount + 1 };
            }
            _store.Save(records);
        }
        return uses;
    }

    public AssistantMemoryConflictResolution ResolveAgainstCurrentSource(AssistantMemoryRecord memory, string currentTrustedStatement)
        => new(currentTrustedStatement, memory, currentTrustedStatement,
            "Current trusted LifeOS source state outranks conflicting remembered context; memory remains visible for review only.");

    private AssistantMemoryRecord ChangeStatus(Guid id, AssistantMemoryStatus status, string revisedBy, string reason, DateTimeOffset? now)
    {
        var at = now ?? DateTimeOffset.UtcNow;
        var records = NormalizeExpired(_store.Load(), at).ToList();
        var index = records.FindIndex(m => m.MemoryId == id);
        if (index < 0) throw new KeyNotFoundException("Memory record was not found.");
        var current = records[index];
        var revision = new AssistantMemoryRevision(Guid.NewGuid(), at, current.Title, current.Statement, revisedBy, reason);
        var updated = current with { UpdatedAt = at, Status = status, Revisions = current.Revisions.Append(revision).ToArray() };
        records[index] = updated;
        _store.Save(records);
        return updated;
    }

    private IReadOnlyList<AssistantMemoryRecord> NormalizeExpired(IReadOnlyList<AssistantMemoryRecord> input, DateTimeOffset now)
    {
        var changed = false;
        var records = input.Select(m =>
        {
            if (m.Status == AssistantMemoryStatus.Active && m.ExpiresAt is not null && m.ExpiresAt <= now)
            {
                changed = true;
                return m with { Status = AssistantMemoryStatus.Expired, UpdatedAt = now };
            }
            return m;
        }).ToArray();
        if (changed) _store.Save(records);
        return records;
    }

    private static void ValidateText(string title, string statement, AssistantMemorySensitivity sensitivity)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Memory title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(statement)) throw new ArgumentException("Remembered statement is required.", nameof(statement));
        if (statement.Length > 2000) throw new ArgumentOutOfRangeException(nameof(statement), "Remembered statement exceeds 2,000 characters.");
        var lower = statement.ToLowerInvariant();
        if (sensitivity == AssistantMemorySensitivity.SecretProhibited || SecretMarkers.Any(lower.Contains) || Regex.IsMatch(statement, @"\b(?:sk|pk)-[A-Za-z0-9_-]{16,}\b"))
            throw new InvalidOperationException("Credentials, secrets and tokens cannot be stored as Assistant memory.");
    }

    private static void ValidateScope(AssistantMemoryScope scope)
    {
        if (scope.Type != AssistantMemoryScopeType.Global && string.IsNullOrWhiteSpace(scope.Name))
            throw new ArgumentException("Named memory scopes require a scope name.", nameof(scope));
    }

    private static bool SameScope(AssistantMemoryScope left, AssistantMemoryScope right) =>
        left.Type == right.Type && string.Equals(left.Name?.Trim(), right.Name?.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool ScopeAllows(AssistantMemoryScope scope, AssistantMemoryQuery query) => scope.Type switch
    {
        AssistantMemoryScopeType.Global => true,
        AssistantMemoryScopeType.Workspace => string.Equals(scope.Name, query.Workspace, StringComparison.OrdinalIgnoreCase),
        AssistantMemoryScopeType.Project => string.Equals(scope.Name, query.Project, StringComparison.OrdinalIgnoreCase),
        AssistantMemoryScopeType.Relationship => string.Equals(scope.Name, query.Relationship, StringComparison.OrdinalIgnoreCase),
        AssistantMemoryScopeType.SessionLimited => false,
        _ => false
    };

    private static double Score(AssistantMemoryRecord memory, string text, DateTimeOffset now)
    {
        var overlap = TokenOverlap(memory.Title + " " + memory.Statement, text);
        if (overlap <= 0) return 0;
        var ageDays = Math.Max(0, (now - memory.UpdatedAt).TotalDays);
        var freshness = 1d / (1d + ageDays / 90d);
        var scope = memory.Scope.Type == AssistantMemoryScopeType.Global ? 0.85 : 1.0;
        return overlap * freshness * scope;
    }

    private static double TokenOverlap(string left, string right)
    {
        var a = Tokens(left); var b = Tokens(right);
        if (a.Count == 0 || b.Count == 0) return 0;
        return (double)a.Intersect(b).Count() / Math.Min(a.Count, b.Count);
    }

    private static HashSet<string> Tokens(string value) => Regex.Matches(value.ToLowerInvariant(), @"[a-z0-9]{3,}")
        .Select(m => m.Value)
        .Where(x => x is not "the" and not "and" and not "for" and not "with")
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static string Normalize(string value) => string.Join(' ', Tokens(value).OrderBy(x => x));
}
