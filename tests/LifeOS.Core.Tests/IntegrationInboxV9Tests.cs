using System.Text.Json;
using LifeOS.Core.IntegrationInbox;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class IntegrationInboxV9Tests
{
    private static readonly DateTimeOffset Now =
        new(2026, 7, 17, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FictionalSeed_CoversEveryNormalizedCandidateType()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);

        IntegrationCandidateType[] expected =
            Enum.GetValues<IntegrationCandidateType>();

        Assert.All(
            expected,
            type => Assert.Contains(
                state.Candidates,
                candidate => candidate.Type == type));
    }

    [Fact]
    public void FictionalSeed_CoversRequiredReviewStates()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);

        IntegrationCandidateStatus[] expected =
        [
            IntegrationCandidateStatus.New,
            IntegrationCandidateStatus.NeedsReview,
            IntegrationCandidateStatus.DuplicateSuspected,
            IntegrationCandidateStatus.Conflict,
            IntegrationCandidateStatus.Accepted,
            IntegrationCandidateStatus.SourceRemoved
        ];

        Assert.All(
            expected,
            status => Assert.Contains(
                state.Candidates,
                candidate => candidate.Status == status));
    }

    [Fact]
    public void Normalizer_RequiresCompleteProvenance()
    {
        IntegrationCandidate candidate =
            IntegrationCandidateNormalizer.Normalize(
                new GenericProviderRecordCandidateDraft
                {
                    ProviderId = "",
                    AccountId = "account",
                    ExternalId = "external",
                    CapabilityId = "generic",
                    SourceTimestampUtc = Now.AddMinutes(-1),
                    RecordType = "Example",
                    DisplayValue = "Example"
                },
                Now);

        Assert.True(candidate.IsQuarantined);
        Assert.Equal(
            IntegrationCandidateStatus.NeedsReview,
            candidate.Status);
        Assert.Contains(
            "Provider ID",
            candidate.QuarantineReason,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Normalizer_PreservesRequiredProvenanceFields()
    {
        IntegrationCandidate candidate =
            IntegrationCandidateNormalizer.Normalize(
                CreateMessageDraft("mail-1", "Subject"),
                Now);

        Assert.Equal("fictional-microsoft", candidate.Provenance.ProviderId);
        Assert.Equal("northstar-work", candidate.Provenance.AccountId);
        Assert.Equal("mail-1", candidate.Provenance.ExternalId);
        Assert.Equal("mail", candidate.Provenance.CapabilityId);
        Assert.Equal(Now, candidate.Provenance.ImportedTimestampUtc);
        Assert.False(string.IsNullOrWhiteSpace(candidate.Provenance.RawReference));
    }

    [Fact]
    public void Fingerprint_IsDeterministicAcrossWhitespaceAndCase()
    {
        string first = IntegrationCandidateFingerprint.Build(
            IntegrationCandidateType.ContactPerson,
            [
                new("email", " Person@Example.Invalid "),
                new("name", "Michele   Carter")
            ]);
        string second = IntegrationCandidateFingerprint.Build(
            IntegrationCandidateType.ContactPerson,
            [
                new("name", "michele carter"),
                new("email", "person@example.invalid")
            ]);

        Assert.Equal(first, second);
    }

    [Fact]
    public void IdempotentReimport_ReturnsExistingCandidate()
    {
        IntegrationInboxV9State state = new();
        IntegrationInboxV9Service service = new(state);
        MessageCandidateDraft draft =
            CreateMessageDraft("mail-1", "Same subject");

        IntegrationCandidate first = service.Ingest(draft, Now);
        IntegrationCandidate second =
            service.Ingest(draft, Now.AddMinutes(1));

        Assert.Same(first, second);
        Assert.Single(state.Candidates);
        Assert.Equal(
            IntegrationReviewAuditAction.ReimportIgnored,
            state.AuditEntries[^1].Action);
    }

    [Fact]
    public void ChangedExternalRecord_CreatesConflictCandidate()
    {
        IntegrationInboxV9State state = new();
        IntegrationInboxV9Service service = new(state);

        service.Ingest(
            CreateMessageDraft("mail-1", "Original subject"),
            Now);

        IntegrationCandidate changed = service.Ingest(
            CreateMessageDraft("mail-1", "Changed subject"),
            Now.AddMinutes(2));

        Assert.Equal(
            IntegrationCandidateStatus.Conflict,
            changed.Status);
        Assert.NotNull(changed.ConflictWithRecordId);
        Assert.Contains(
            changed.Fields,
            field =>
                field.Key == "subject" &&
                field.IsConflict &&
                field.CurrentValue == "Original subject");
    }

    [Fact]
    public void DifferentExternalIds_WithSameFingerprint_CreateDuplicateReview()
    {
        IntegrationInboxV9State state = new();
        IntegrationInboxV9Service service = new(state);

        ContactPersonCandidateDraft firstDraft =
            CreateContactDraft("contact-1", "person@example.invalid");
        ContactPersonCandidateDraft secondDraft =
            CreateContactDraft("contact-2", "person@example.invalid");

        IntegrationCandidate first =
            service.Ingest(firstDraft, Now);
        IntegrationCandidate second =
            service.Ingest(secondDraft, Now.AddMinutes(1));

        Assert.Equal(
            IntegrationCandidateStatus.DuplicateSuspected,
            first.Status);
        Assert.Equal(
            IntegrationCandidateStatus.DuplicateSuspected,
            second.Status);
        Assert.Equal(first.Id, second.DuplicateOfCandidateId);
    }

    [Fact]
    public void DifferentFingerprint_DoesNotCreateFalsePositiveDuplicate()
    {
        IntegrationInboxV9State state = new();
        IntegrationInboxV9Service service = new(state);

        service.Ingest(
            CreateContactDraft("contact-1", "one@example.invalid"),
            Now);
        IntegrationCandidate second = service.Ingest(
            CreateContactDraft("contact-2", "two@example.invalid"),
            Now.AddMinutes(1));

        Assert.Equal(
            IntegrationCandidateStatus.New,
            second.Status);
        Assert.Null(second.DuplicateOfCandidateId);
    }

    [Fact]
    public void DuplicateLinkExisting_UsesAuthoritativeLink()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);
        IntegrationInboxV9Service service = new(state);

        service.ResolveDuplicate(
            "contact-duplicate",
            IntegrationDuplicateResolutionChoice.LinkExisting,
            Now.AddMinutes(1));

        IntegrationCandidate candidate =
            service.GetRequiredCandidate("contact-duplicate");
        Assert.Equal(
            IntegrationCandidateStatus.Accepted,
            candidate.Status);
        Assert.Equal(
            "people-118",
            candidate.AuthoritativeLink?.RecordId);
    }

    [Fact]
    public void ConflictReview_RequiresEveryFieldChoice()
    {
        IntegrationInboxV9State state = new();
        IntegrationInboxV9Service service = new(state);

        service.Ingest(
            CreateMessageDraft("mail-1", "Original"),
            Now);
        IntegrationCandidate conflict = service.Ingest(
            CreateMessageDraft("mail-1", "Changed"),
            Now.AddMinutes(1));

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => service.ApplyConflictReview(
                conflict.Id,
                new Dictionary<string, IntegrationConflictFieldChoice>(),
                Now.AddMinutes(2)));

        Assert.Contains(
            "requires an explicit choice",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ConflictReview_AppliesFieldLevelChoices()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);
        IntegrationInboxV9Service service = new(state);
        IntegrationCandidate conflict =
            service.GetRequiredCandidate("calendar-conflict");

        Dictionary<string, IntegrationConflictFieldChoice> choices =
            conflict.Fields
                .Where(field => field.IsConflict)
                .ToDictionary(
                    field => field.Key,
                    field => field.ConflictChoice,
                    StringComparer.Ordinal);

        service.ApplyConflictReview(
            conflict.Id,
            choices,
            Now.AddMinutes(1));

        Assert.Equal(
            IntegrationCandidateStatus.Accepted,
            conflict.Status);
        Assert.All(
            conflict.Fields.Where(field => field.IsConflict),
            field => Assert.NotEqual(
                IntegrationConflictFieldChoice.Unreviewed,
                field.ConflictChoice));
    }

    [Fact]
    public void AcceptedCandidate_LinksWithoutCreatingAnotherCandidate()
    {
        IntegrationInboxV9State state = new();
        IntegrationInboxV9Service service = new(state);
        IntegrationCandidate candidate =
            service.Ingest(
                CreateMessageDraft("mail-1", "Subject"),
                Now);

        service.Accept(
            candidate.Id,
            "Reviewed",
            Now.AddMinutes(1));
        service.LinkAccepted(
            candidate.Id,
            new LifeOsAuthoritativeLink
            {
                Module = "Follow-Ups",
                RecordId = "follow-up-9",
                DisplayName = "Supplier reply"
            },
            Now.AddMinutes(2));

        Assert.Single(state.Candidates);
        Assert.Equal(
            "follow-up-9",
            candidate.AuthoritativeLink?.RecordId);
        Assert.Equal(
            IntegrationCandidateStatus.Accepted,
            candidate.Status);
    }

    [Fact]
    public void ReviewTransitions_CoverRejectIgnoreAndSupersede()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);
        IntegrationInboxV9Service service = new(state);

        service.Reject(
            "message-follow-up",
            "Rejected",
            Now.AddMinutes(1));
        service.Ignore(
            "financial-review",
            "Ignored",
            Now.AddMinutes(2));
        service.Supersede(
            "file-review",
            "task-accepted",
            Now.AddMinutes(3));

        Assert.Equal(
            IntegrationCandidateStatus.Rejected,
            service.GetRequiredCandidate("message-follow-up").Status);
        Assert.Equal(
            IntegrationCandidateStatus.Ignored,
            service.GetRequiredCandidate("financial-review").Status);
        Assert.Equal(
            IntegrationCandidateStatus.Superseded,
            service.GetRequiredCandidate("file-review").Status);
    }

    [Fact]
    public void BatchPreview_DoesNotMutateCandidates()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);
        IntegrationInboxV9Service service = new(state);

        IntegrationBatchReviewPreview preview =
            service.PreviewBatch(
                ["batch-calendar-1", "batch-calendar-2"],
                IntegrationBatchDecision.Accept,
                Now.AddMinutes(1));

        Assert.True(preview.CanApply);
        Assert.Equal(2, preview.CandidateIds.Count);
        Assert.All(
            preview.CandidateIds,
            id => Assert.Equal(
                IntegrationCandidateStatus.New,
                service.GetRequiredCandidate(id).Status));
    }

    [Fact]
    public void BatchApply_AppliesOnlyPreviewedLowRiskCandidates()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);
        IntegrationInboxV9Service service = new(state);
        IntegrationBatchReviewPreview preview =
            service.PreviewBatch(
                ["batch-calendar-1", "batch-calendar-2"],
                IntegrationBatchDecision.Accept,
                Now.AddMinutes(1));

        service.ApplyBatch(
            preview,
            "Batch accepted",
            Now.AddMinutes(2));

        Assert.All(
            preview.CandidateIds,
            id => Assert.Equal(
                IntegrationCandidateStatus.Accepted,
                service.GetRequiredCandidate(id).Status));
        Assert.Equal(
            IntegrationCandidateStatus.New,
            service.GetRequiredCandidate("message-follow-up").Status);
    }

    [Fact]
    public void BatchPreview_BlocksMixedOrHighRiskCandidates()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);
        IntegrationInboxV9Service service = new(state);

        IntegrationBatchReviewPreview preview =
            service.PreviewBatch(
                ["batch-calendar-1", "message-follow-up"],
                IntegrationBatchDecision.Accept,
                Now.AddMinutes(1));

        Assert.False(preview.CanApply);
        Assert.NotEmpty(preview.Warnings);
    }

    [Fact]
    public void SourceRemoval_CreatesTombstoneAndKeepsNormalizedFields()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);
        IntegrationInboxV9Service service = new(state);
        IntegrationCandidate candidate =
            service.GetRequiredCandidate("message-follow-up");
        int fieldCount = candidate.Fields.Count;

        service.MarkSourceRemoved(
            candidate.Id,
            Now.AddMinutes(1));

        Assert.Equal(
            IntegrationCandidateStatus.SourceRemoved,
            candidate.Status);
        Assert.True(candidate.Provenance.IsSourceRemoved);
        Assert.Equal(
            IntegrationCandidateFreshness.SourceRemoved,
            candidate.Provenance.Freshness);
        Assert.Equal(fieldCount, candidate.Fields.Count);
    }

    [Theory]
    [InlineData(-1, IntegrationCandidateFreshness.Fresh)]
    [InlineData(-8, IntegrationCandidateFreshness.Ageing)]
    [InlineData(-48, IntegrationCandidateFreshness.Stale)]
    public void FreshnessCalculator_UsesSourceAndImportTimestamps(
        int sourceHours,
        IntegrationCandidateFreshness expected)
    {
        Assert.Equal(
            expected,
            IntegrationCandidateNormalizer.CalculateFreshness(
                Now.AddHours(sourceHours),
                Now,
                sourceRemoved: false));
    }

    [Fact]
    public void Store_RoundTripsCandidatesProvenanceAndAudit()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"lifeos-integration-inbox-v9-{Guid.NewGuid():N}.json");

        try
        {
            IntegrationInboxV9State expected =
                IntegrationInboxV9Seed.CreateFictional(Now);
            IntegrationInboxV9Store.Save(expected, path);

            IntegrationInboxV9State actual =
                IntegrationInboxV9Store.LoadOrCreate(Now, path);

            Assert.Equal(
                expected.Candidates.Count,
                actual.Candidates.Count);
            Assert.Equal(
                expected.AuditEntries.Count,
                actual.AuditEntries.Count);
            Assert.Equal(
                expected.Candidates[0].Provenance.ExternalId,
                actual.Candidates[0].Provenance.ExternalId);
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    [Fact]
    public void EveryReviewDecision_AppendsOrderedAudit()
    {
        IntegrationInboxV9State state =
            IntegrationInboxV9Seed.CreateFictional(Now);
        IntegrationInboxV9Service service = new(state);
        long startingSequence = state.NextAuditSequence;

        service.Reject(
            "message-follow-up",
            "Rejected",
            Now.AddMinutes(1));
        service.Ignore(
            "financial-review",
            "Ignored",
            Now.AddMinutes(2));

        Assert.Equal(startingSequence + 2, state.NextAuditSequence);
        Assert.True(
            state.AuditEntries[^2].Sequence <
            state.AuditEntries[^1].Sequence);
        Assert.Equal(
            IntegrationReviewAuditAction.Rejected,
            state.AuditEntries[^2].Action);
        Assert.Equal(
            IntegrationReviewAuditAction.Ignored,
            state.AuditEntries[^1].Action);
    }

    [Fact]
    public void SerializedState_ContainsNoCredentialOrRawPayloadFields()
    {
        string json = JsonSerializer
            .Serialize(
                IntegrationInboxV9Seed.CreateFictional(Now))
            .ToLowerInvariant();

        Assert.DoesNotContain("clientsecret", json);
        Assert.DoesNotContain("access_token", json);
        Assert.DoesNotContain("refresh_token", json);
        Assert.DoesNotContain("authorizationcode", json);
        Assert.DoesNotContain("rawpayload", json);
        Assert.DoesNotContain("password", json);
    }

    private static MessageCandidateDraft CreateMessageDraft(
        string externalId,
        string subject) => new()
        {
            ProviderId = "fictional-microsoft",
            ProviderDisplayName = "Microsoft 365 (fictional)",
            AccountId = "northstar-work",
            AccountDisplayName = "Northstar Operations",
            ExternalId = externalId,
            CapabilityId = "mail",
            RawReference = $"fictional://mail/{externalId}",
            SourceTimestampUtc = Now.AddMinutes(-10),
            Subject = subject,
            Sender = "sender@example.invalid",
            Recipients = "work-account@example.invalid",
            ConversationId = "thread-1",
            Importance = "Normal",
            Summary = "Test message candidate."
        };

    private static ContactPersonCandidateDraft CreateContactDraft(
        string externalId,
        string email) => new()
        {
            ProviderId = "fictional-provider",
            ProviderDisplayName = "Fictional provider",
            AccountId = "northstar-work",
            AccountDisplayName = "Northstar Operations",
            ExternalId = externalId,
            CapabilityId = "contacts",
            RawReference = $"fictional://contacts/{externalId}",
            SourceTimestampUtc = Now.AddMinutes(-5),
            DisplayName = "Michele Carter",
            PrimaryEmail = email,
            Phone = "+64 20 555 0141",
            Organization = "AIE Example",
            Summary = "Test contact candidate."
        };

    private static void DeleteIfPresent(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        string temporaryPath = path + ".tmp";
        if (File.Exists(temporaryPath))
        {
            File.Delete(temporaryPath);
        }
    }
}
