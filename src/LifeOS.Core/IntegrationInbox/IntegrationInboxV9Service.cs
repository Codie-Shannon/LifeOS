namespace LifeOS.Core.IntegrationInbox;

public sealed class IntegrationInboxV9Service
{
    public IntegrationInboxV9Service(IntegrationInboxV9State state)
    {
        ArgumentNullException.ThrowIfNull(state);
        State = state.Normalize();
    }

    public IntegrationInboxV9State State { get; }

    public IntegrationInboxV9Summary GetSummary()
    {
        int reviewCount = State.Candidates.Count(IsReviewCandidate);

        return new IntegrationInboxV9Summary(
            State.Candidates.Count,
            reviewCount,
            State.Candidates.Count(candidate =>
                candidate.Status == IntegrationCandidateStatus.DuplicateSuspected),
            State.Candidates.Count(candidate =>
                candidate.Status == IntegrationCandidateStatus.Conflict),
            State.Candidates.Count(candidate =>
                candidate.Status == IntegrationCandidateStatus.Accepted),
            State.Candidates.Count(candidate =>
                candidate.Status == IntegrationCandidateStatus.SourceRemoved),
            State.Candidates.Count(candidate =>
                candidate.IsQuarantined));
    }

    public int GetReviewCount() => GetSummary().ReviewCount;

    public IntegrationCandidate GetRequiredCandidate(string candidateId)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
        {
            throw new ArgumentException(
                "Candidate ID is required.",
                nameof(candidateId));
        }

        return State.Candidates.SingleOrDefault(candidate =>
            string.Equals(
                candidate.Id,
                candidateId,
                StringComparison.Ordinal))
            ?? throw new KeyNotFoundException(
                $"Integration candidate '{candidateId}' was not found.");
    }

    public IntegrationCandidate Ingest(
        IntegrationCandidateDraftBase draft,
        DateTimeOffset importedUtc)
    {
        IntegrationCandidate incoming =
            IntegrationCandidateNormalizer.Normalize(draft, importedUtc);

        if (incoming.IsQuarantined)
        {
            State.Candidates.Add(incoming);
            AddAudit(
                incoming.Id,
                IntegrationReviewAuditAction.CandidateQuarantined,
                incoming.QuarantineReason,
                importedUtc);
            return incoming;
        }

        string externalKey =
            IntegrationCandidateFingerprint.BuildExternalKey(incoming);

        IntegrationCandidate? sameExternal = State.Candidates
            .FirstOrDefault(candidate =>
                string.Equals(
                    IntegrationCandidateFingerprint.BuildExternalKey(candidate),
                    externalKey,
                    StringComparison.Ordinal));

        if (sameExternal is not null)
        {
            sameExternal.Provenance.LastSeenUtc = importedUtc;

            if (string.Equals(
                sameExternal.ContentHash,
                incoming.ContentHash,
                StringComparison.Ordinal))
            {
                AddAudit(
                    sameExternal.Id,
                    IntegrationReviewAuditAction.ReimportIgnored,
                    "Idempotent re-import matched provider, account, external ID and normalized content.",
                    importedUtc);
                return sameExternal;
            }

            incoming.Status = IntegrationCandidateStatus.Conflict;
            incoming.ConflictWithRecordId =
                sameExternal.AuthoritativeLink?.RecordId ?? sameExternal.Id;
            MergeConflictFields(sameExternal, incoming);
            State.Candidates.Add(incoming);
            AddAudit(
                incoming.Id,
                IntegrationReviewAuditAction.ConflictCreated,
                $"Changed source record conflicts with {incoming.ConflictWithRecordId}.",
                importedUtc);
            return incoming;
        }

        IntegrationCandidate? duplicate = State.Candidates
            .FirstOrDefault(candidate =>
                !string.Equals(
                    candidate.Provenance.ExternalId,
                    incoming.Provenance.ExternalId,
                    StringComparison.Ordinal) &&
                string.Equals(
                    candidate.Fingerprint,
                    incoming.Fingerprint,
                    StringComparison.Ordinal) &&
                candidate.Status is not
                    IntegrationCandidateStatus.Rejected and not
                    IntegrationCandidateStatus.Ignored and not
                    IntegrationCandidateStatus.Superseded and not
                    IntegrationCandidateStatus.SourceRemoved);

        if (duplicate is not null)
        {
            incoming.Status = IntegrationCandidateStatus.DuplicateSuspected;
            incoming.DuplicateOfCandidateId = duplicate.Id;

            if (duplicate.Status is
                IntegrationCandidateStatus.New or
                IntegrationCandidateStatus.NeedsReview)
            {
                duplicate.Status =
                    IntegrationCandidateStatus.DuplicateSuspected;
                duplicate.DuplicateOfCandidateId = incoming.Id;
                duplicate.UpdatedUtc = importedUtc;
            }

            State.Candidates.Add(incoming);
            AddAudit(
                incoming.Id,
                IntegrationReviewAuditAction.DuplicateDetected,
                $"Fingerprint matches candidate {duplicate.Id}; no silent merge occurred.",
                importedUtc);
            return incoming;
        }

        State.Candidates.Add(incoming);
        AddAudit(
            incoming.Id,
            IntegrationReviewAuditAction.CandidateImported,
            $"Normalized {incoming.Type} candidate imported for review.",
            importedUtc);
        return incoming;
    }

    public void Accept(
        string candidateId,
        string reviewNote,
        DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = GetRequiredCandidate(candidateId);

        if (candidate.IsQuarantined)
        {
            throw new InvalidOperationException(
                "Quarantined candidates cannot be accepted.");
        }

        if (candidate.Status ==
            IntegrationCandidateStatus.DuplicateSuspected)
        {
            throw new InvalidOperationException(
                "Resolve duplicate suspicion before acceptance.");
        }

        if (candidate.Status ==
            IntegrationCandidateStatus.Conflict)
        {
            throw new InvalidOperationException(
                "Resolve field-level conflict choices before acceptance.");
        }

        candidate.Status = IntegrationCandidateStatus.Accepted;
        candidate.ReviewNote = reviewNote.Trim();
        candidate.UpdatedUtc = nowUtc;

        AddAudit(
            candidate.Id,
            IntegrationReviewAuditAction.Accepted,
            "Candidate accepted for an explicit authoritative LifeOS link.",
            nowUtc);
    }

    public void LinkAccepted(
        string candidateId,
        LifeOsAuthoritativeLink link,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(link);
        IntegrationCandidate candidate = GetRequiredCandidate(candidateId);

        if (candidate.Status != IntegrationCandidateStatus.Accepted)
        {
            throw new InvalidOperationException(
                "Only accepted candidates can link to authoritative LifeOS records.");
        }

        if (string.IsNullOrWhiteSpace(link.Module) ||
            string.IsNullOrWhiteSpace(link.RecordId))
        {
            throw new ArgumentException(
                "Authoritative module and record ID are required.",
                nameof(link));
        }

        link.LinkedUtc = nowUtc;
        candidate.AuthoritativeLink = link;
        candidate.UpdatedUtc = nowUtc;

        AddAudit(
            candidate.Id,
            IntegrationReviewAuditAction.Linked,
            $"Linked to {link.Module} record {link.RecordId} without creating a second editor.",
            nowUtc);
    }

    public void Reject(
        string candidateId,
        string reviewNote,
        DateTimeOffset nowUtc)
    {
        Transition(
            candidateId,
            IntegrationCandidateStatus.Rejected,
            IntegrationReviewAuditAction.Rejected,
            reviewNote,
            nowUtc);
    }

    public void Ignore(
        string candidateId,
        string reviewNote,
        DateTimeOffset nowUtc)
    {
        Transition(
            candidateId,
            IntegrationCandidateStatus.Ignored,
            IntegrationReviewAuditAction.Ignored,
            reviewNote,
            nowUtc);
    }

    public void Supersede(
        string candidateId,
        string replacementCandidateId,
        DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = GetRequiredCandidate(candidateId);
        IntegrationCandidate replacement =
            GetRequiredCandidate(replacementCandidateId);

        if (ReferenceEquals(candidate, replacement))
        {
            throw new InvalidOperationException(
                "A candidate cannot supersede itself.");
        }

        candidate.Status = IntegrationCandidateStatus.Superseded;
        candidate.ReviewNote =
            $"Superseded by candidate {replacement.Id}.";
        candidate.UpdatedUtc = nowUtc;

        AddAudit(
            candidate.Id,
            IntegrationReviewAuditAction.Superseded,
            candidate.ReviewNote,
            nowUtc);
    }

    public void MarkSourceRemoved(
        string candidateId,
        DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = GetRequiredCandidate(candidateId);
        candidate.Status = IntegrationCandidateStatus.SourceRemoved;
        candidate.Provenance.IsSourceRemoved = true;
        candidate.Provenance.Freshness =
            IntegrationCandidateFreshness.SourceRemoved;
        candidate.Provenance.LastSeenUtc = nowUtc;
        candidate.UpdatedUtc = nowUtc;

        AddAudit(
            candidate.Id,
            IntegrationReviewAuditAction.SourceRemoved,
            "Provider tombstone recorded; normalized fields and provenance were retained.",
            nowUtc);
    }

    public void ResolveDuplicate(
        string candidateId,
        IntegrationDuplicateResolutionChoice choice,
        DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = GetRequiredCandidate(candidateId);

        if (candidate.Status !=
            IntegrationCandidateStatus.DuplicateSuspected)
        {
            throw new InvalidOperationException(
                "Candidate is not awaiting duplicate review.");
        }

        IntegrationCandidate existing = GetRequiredCandidate(
            candidate.DuplicateOfCandidateId ??
            throw new InvalidOperationException(
                "Duplicate candidate has no comparison target."));

        switch (choice)
        {
            case IntegrationDuplicateResolutionChoice.LinkExisting:
                if (existing.AuthoritativeLink is null)
                {
                    throw new InvalidOperationException(
                        "The existing candidate has no authoritative link.");
                }

                candidate.Status = IntegrationCandidateStatus.Accepted;
                candidate.AuthoritativeLink = new LifeOsAuthoritativeLink
                {
                    Module = existing.AuthoritativeLink.Module,
                    RecordId = existing.AuthoritativeLink.RecordId,
                    DisplayName = existing.AuthoritativeLink.DisplayName,
                    LinkedUtc = nowUtc
                };
                candidate.ReviewNote =
                    $"Linked to existing authoritative record {existing.AuthoritativeLink.RecordId}.";
                break;

            case IntegrationDuplicateResolutionChoice.KeepSeparate:
                candidate.Status = IntegrationCandidateStatus.NeedsReview;
                candidate.DuplicateOfCandidateId = null;
                candidate.ReviewNote =
                    "Duplicate suspicion reviewed; keep as a separate candidate.";
                break;

            case IntegrationDuplicateResolutionChoice.ReplaceCandidate:
                existing.Status = IntegrationCandidateStatus.Superseded;
                existing.ReviewNote =
                    $"Replaced by duplicate candidate {candidate.Id}.";
                existing.UpdatedUtc = nowUtc;
                candidate.Status = IntegrationCandidateStatus.Accepted;
                candidate.AuthoritativeLink =
                    existing.AuthoritativeLink is null
                        ? null
                        : new LifeOsAuthoritativeLink
                        {
                            Module = existing.AuthoritativeLink.Module,
                            RecordId = existing.AuthoritativeLink.RecordId,
                            DisplayName = existing.AuthoritativeLink.DisplayName,
                            LinkedUtc = nowUtc
                        };
                candidate.ReviewNote =
                    $"Replaced candidate {existing.Id}.";
                break;

            case IntegrationDuplicateResolutionChoice.Ignore:
                candidate.Status = IntegrationCandidateStatus.Ignored;
                candidate.ReviewNote =
                    "Duplicate candidate ignored after explicit review.";
                break;

            case IntegrationDuplicateResolutionChoice.Reject:
                candidate.Status = IntegrationCandidateStatus.Rejected;
                candidate.ReviewNote =
                    "Duplicate candidate rejected after explicit review.";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(choice));
        }

        candidate.UpdatedUtc = nowUtc;

        AddAudit(
            candidate.Id,
            IntegrationReviewAuditAction.DuplicateResolved,
            $"Duplicate review completed with choice {choice}.",
            nowUtc);
    }

    public void ApplyConflictReview(
        string candidateId,
        IReadOnlyDictionary<string, IntegrationConflictFieldChoice> choices,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(choices);
        IntegrationCandidate candidate = GetRequiredCandidate(candidateId);

        if (candidate.Status != IntegrationCandidateStatus.Conflict)
        {
            throw new InvalidOperationException(
                "Candidate is not awaiting conflict review.");
        }

        List<IntegrationCandidateField> conflicts = candidate.Fields
            .Where(field => field.IsConflict)
            .ToList();

        if (conflicts.Count == 0)
        {
            throw new InvalidOperationException(
                "Conflict candidate has no field comparisons.");
        }

        foreach (IntegrationCandidateField field in conflicts)
        {
            if (!choices.TryGetValue(
                    field.Key,
                    out IntegrationConflictFieldChoice choice) ||
                choice == IntegrationConflictFieldChoice.Unreviewed)
            {
                throw new InvalidOperationException(
                    $"Conflict field '{field.DisplayName}' requires an explicit choice.");
            }

            field.ConflictChoice = choice;

            if (choice == IntegrationConflictFieldChoice.KeepCurrent &&
                field.CurrentValue is not null)
            {
                field.Value = field.CurrentValue;
            }
        }

        candidate.Status = IntegrationCandidateStatus.Accepted;
        candidate.ReviewNote =
            "Field-level conflict review completed.";
        candidate.UpdatedUtc = nowUtc;

        AddAudit(
            candidate.Id,
            IntegrationReviewAuditAction.ConflictResolved,
            $"Resolved {conflicts.Count} conflict fields with explicit per-field choices.",
            nowUtc);
    }

    public IntegrationBatchReviewPreview PreviewBatch(
        IEnumerable<string> candidateIds,
        IntegrationBatchDecision decision,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(candidateIds);

        List<string> ids = candidateIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        List<IntegrationCandidate> candidates = ids
            .Select(GetRequiredCandidate)
            .ToList();
        List<string> warnings = [];

        if (candidates.Count == 0)
        {
            warnings.Add("Select at least one candidate.");
        }

        if (candidates.Any(candidate =>
            !candidate.LowRiskBatchEligible))
        {
            warnings.Add(
                "Batch review is limited to explicitly low-risk candidates.");
        }

        if (candidates.Any(candidate =>
            candidate.Status is not
                IntegrationCandidateStatus.New and not
                IntegrationCandidateStatus.NeedsReview))
        {
            warnings.Add(
                "Every batch candidate must be New or Needs Review.");
        }

        if (candidates.Select(candidate => candidate.Type).Distinct().Count() > 1)
        {
            warnings.Add(
                "Batch preview requires candidates of the same normalized type.");
        }

        bool canApply = candidates.Count > 0 && warnings.Count == 0;
        string summary = canApply
            ? $"{decision} {candidates.Count} low-risk {candidates[0].Type} candidates after explicit preview."
            : "Batch preview is blocked until every warning is resolved.";

        AddAudit(
            candidateId: null,
            IntegrationReviewAuditAction.BatchPreviewed,
            summary,
            nowUtc);

        return new IntegrationBatchReviewPreview(
            decision,
            ids,
            candidates.Select(candidate => candidate.Title).ToArray(),
            canApply,
            summary,
            warnings);
    }

    public void ApplyBatch(
        IntegrationBatchReviewPreview preview,
        string reviewNote,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(preview);

        if (!preview.CanApply)
        {
            throw new InvalidOperationException(
                "Blocked batch preview cannot be applied.");
        }

        foreach (string candidateId in preview.CandidateIds)
        {
            switch (preview.Decision)
            {
                case IntegrationBatchDecision.Accept:
                    Accept(candidateId, reviewNote, nowUtc);
                    break;

                case IntegrationBatchDecision.Reject:
                    Reject(candidateId, reviewNote, nowUtc);
                    break;

                case IntegrationBatchDecision.Ignore:
                    Ignore(candidateId, reviewNote, nowUtc);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(preview));
            }
        }

        AddAudit(
            candidateId: null,
            IntegrationReviewAuditAction.BatchApplied,
            $"Applied {preview.Decision} to {preview.CandidateIds.Count} explicitly previewed candidates.",
            nowUtc);
    }

    private void Transition(
        string candidateId,
        IntegrationCandidateStatus status,
        IntegrationReviewAuditAction action,
        string reviewNote,
        DateTimeOffset nowUtc)
    {
        IntegrationCandidate candidate = GetRequiredCandidate(candidateId);
        candidate.Status = status;
        candidate.ReviewNote = reviewNote.Trim();
        candidate.UpdatedUtc = nowUtc;

        AddAudit(
            candidate.Id,
            action,
            string.IsNullOrWhiteSpace(candidate.ReviewNote)
                ? $"{status} after explicit review."
                : candidate.ReviewNote,
            nowUtc);
    }

    private static bool IsReviewCandidate(
        IntegrationCandidate candidate) =>
        candidate.Status is
            IntegrationCandidateStatus.New or
            IntegrationCandidateStatus.NeedsReview or
            IntegrationCandidateStatus.DuplicateSuspected or
            IntegrationCandidateStatus.Conflict;

    private static void MergeConflictFields(
        IntegrationCandidate current,
        IntegrationCandidate incoming)
    {
        foreach (IntegrationCandidateField field in incoming.Fields)
        {
            IntegrationCandidateField? currentField =
                current.Fields.FirstOrDefault(existing =>
                    string.Equals(
                        existing.Key,
                        field.Key,
                        StringComparison.Ordinal));

            if (currentField is null ||
                string.Equals(
                    currentField.Value,
                    field.Value,
                    StringComparison.Ordinal))
            {
                continue;
            }

            field.CurrentValue = currentField.Value;
            field.IsConflict = true;
            field.ConflictChoice =
                IntegrationConflictFieldChoice.Unreviewed;
        }
    }

    private void AddAudit(
        string? candidateId,
        IntegrationReviewAuditAction action,
        string summary,
        DateTimeOffset timestampUtc)
    {
        State.AuditEntries.Add(new IntegrationReviewAuditEntry
        {
            Sequence = State.NextAuditSequence++,
            TimestampUtc = timestampUtc,
            CandidateId = candidateId,
            Action = action,
            Summary = summary.Trim()
        });
    }
}
