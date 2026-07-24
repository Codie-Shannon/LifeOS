using System.IO;
using System.Windows;
using System.Windows.Controls;
using LifeOS.Core.IntegrationControlCentre;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Core.MicrosoftProvider;

namespace LifeOS.Desktop;

public partial class IntegrationInboxView : UserControl
{
    private const string DefaultSelectedCandidateId = "message-follow-up";
    private const string DuplicateCandidateId = "contact-duplicate";
    private const string ConflictCandidateId = "calendar-conflict";
    private const string AcceptedCandidateId = "task-accepted";
    private static readonly string[] BatchCandidateIds =
    [
        "batch-calendar-1",
        "batch-calendar-2"
    ];

    private readonly string _storePath;
    private readonly IntegrationInboxV9Service _service;
    private readonly HashSet<string> _connectedAccountIds;
    private string? _selectedCandidateId;
    private IntegrationBatchReviewPreview? _batchPreview;

    public IntegrationInboxView(bool compactDensity)
    {
        InitializeComponent();

        _storePath = IntegrationInboxV9Store.DefaultPath;
        IntegrationInboxV9State state =
            Group51IntegrationInboxMigration.Apply(
                Group50IntegrationInboxMigration.Apply(
                    Group49IntegrationInboxMigration.LoadOrCreateProofState(DateTimeOffset.UtcNow),
                    DateTimeOffset.UtcNow),
                DateTimeOffset.UtcNow);
        _service = new IntegrationInboxV9Service(state);
        _connectedAccountIds = MicrosoftProviderStore.LoadState()
            .Accounts
            .Where(account =>
                account.ConnectionState ==
                IntegrationConnectionState.Connected)
            .Select(account => account.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);

        ScopeComboBox.ItemsSource = ReviewScopeOption.Options;
        ScopeComboBox.SelectedItem = ReviewScopeOption.CurrentItems;

        ApplyDensity(compactDensity);
        RefreshAll();
    }

    public event EventHandler? BackRequested;

    public event Action<int>? ReviewCountChanged;

    public void ApplyDensity(bool compactDensity)
    {
        InboxTabs.Height = compactDensity ? 590 : 640;
    }

    public int CurrentReviewCount => _service.State.Candidates.Count(
        candidate =>
            IsCandidateVisible(candidate, ReviewScopeKind.CurrentItems) &&
            IsReviewable(candidate));

    private void RefreshAll(string? selectedCandidateId = null)
    {
        ReviewScopeOption scope =
            ScopeComboBox.SelectedItem as ReviewScopeOption ??
            ReviewScopeOption.CurrentItems;

        IntegrationCandidate[] visibleCandidates = _service.State.Candidates
            .Where(candidate => IsCandidateVisible(candidate, scope.Kind))
            .ToArray();

        TotalText.Text = visibleCandidates.Length.ToString();
        ReviewCountText.Text = visibleCandidates.Count(IsReviewable).ToString();
        DuplicateCountText.Text = visibleCandidates.Count(candidate =>
            candidate.Status ==
            IntegrationCandidateStatus.DuplicateSuspected).ToString();
        ConflictCountText.Text = visibleCandidates.Count(candidate =>
            candidate.Status ==
            IntegrationCandidateStatus.Conflict).ToString();

        ScopeBannerText.Text = _connectedAccountIds.Count > 0
            ? "CURRENT ACCOUNT · READ-ONLY"
            : "LOCAL REVIEW · NO CONNECTED ACCOUNT";
        QueueHeadingText.Text = scope.Heading;
        QueueDescriptionText.Text = scope.Description;

        CandidateListView[] candidateViews = visibleCandidates
            .OrderBy(candidate => StatusSort(candidate.Status))
            .ThenByDescending(candidate => candidate.Provenance.ImportedTimestampUtc)
            .Select(candidate => new CandidateListView(
                candidate.Id,
                candidate.Title,
                $"{FormatCandidateType(candidate.Type)} · {FormatCandidateStatus(candidate.Status)}",
                $"{candidate.Provenance.ProviderDisplayName} · {candidate.Provenance.AccountDisplayName}"))
            .ToArray();

        CandidateListBox.ItemsSource = candidateViews;

        string desiredId = selectedCandidateId
            ?? _selectedCandidateId
            ?? candidateViews.FirstOrDefault()?.Id
            ?? DefaultSelectedCandidateId;

        CandidateListView? selectedView = candidateViews.FirstOrDefault(
            view => string.Equals(
                view.Id,
                desiredId,
                StringComparison.Ordinal));

        CandidateListBox.SelectedItem =
            selectedView ?? candidateViews.FirstOrDefault();

        if (CandidateListBox.SelectedItem is null)
        {
            _selectedCandidateId = null;
            PopulateEmptyState(scope);
        }

        PopulateDuplicateReview();
        PopulateConflictReview();
        PopulateBatchAndAccepted();
        PopulateSourceAndAudit();

        ReviewCountChanged?.Invoke(visibleCandidates.Count(IsReviewable));
    }

    private void ScopeComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        _selectedCandidateId = null;
        RefreshAll();
    }

    private bool IsCandidateVisible(
        IntegrationCandidate candidate,
        ReviewScopeKind scope)
    {
        bool isCurrentAccount =
            _connectedAccountIds.Contains(candidate.Provenance.AccountId);
        bool isSuggestion =
            candidate.Provenance.CapabilityId.EndsWith(
                "-suggestion",
                StringComparison.OrdinalIgnoreCase);
        bool isActive = IsReviewable(candidate);

        return scope switch
        {
            ReviewScopeKind.CurrentItems =>
                isCurrentAccount && !isSuggestion && isActive,
            ReviewScopeKind.Suggestions =>
                isCurrentAccount && isSuggestion && isActive,
            ReviewScopeKind.HistoryAndDemo =>
                !isCurrentAccount || !isActive,
            ReviewScopeKind.All => true,
            _ => false
        };
    }

    private static bool IsReviewable(IntegrationCandidate candidate) =>
        candidate.Status is
            IntegrationCandidateStatus.New or
            IntegrationCandidateStatus.NeedsReview or
            IntegrationCandidateStatus.DuplicateSuspected or
            IntegrationCandidateStatus.Conflict;

    private void CandidateListBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (CandidateListBox.SelectedItem is not CandidateListView selected)
        {
            _selectedCandidateId = null;
            return;
        }

        _selectedCandidateId = selected.Id;
        CandidateActionPanel.Visibility = Visibility.Visible;
        TechnicalDetailsExpander.Visibility = Visibility.Visible;
        PopulateCandidateDetail(
            _service.GetRequiredCandidate(selected.Id));
    }

    private void PopulateEmptyState(ReviewScopeOption scope)
    {
        DetailTitleText.Text = "All caught up";
        DetailTypeText.Text = scope.Label;
        DetailStatusText.Text = "Complete";
        DetailSummaryText.Text =
            "There are no remaining items in this review queue.";
        ProvenanceText.Text = string.Empty;
        RawReferenceText.Text = string.Empty;
        DetailFieldItems.ItemsSource = Array.Empty<FieldView>();
        CandidateActionPanel.Visibility = Visibility.Collapsed;
        TechnicalDetailsExpander.IsExpanded = false;
        TechnicalDetailsExpander.Visibility = Visibility.Collapsed;
        ActionStatusText.Text = string.Empty;
    }

    private void PopulateCandidateDetail(IntegrationCandidate candidate)
    {
        DetailTitleText.Text = candidate.Title;
        DetailTypeText.Text =
            $"{FormatCandidateType(candidate.Type)} · {candidate.Provenance.ProviderDisplayName}";
        DetailStatusText.Text = FormatCandidateStatus(candidate.Status);
        DetailSummaryText.Text = candidate.Summary;

        IntegrationCandidateProvenance provenance = candidate.Provenance;
        ProvenanceText.Text =
            $"Provider: {provenance.ProviderDisplayName}\n" +
            $"Account: {provenance.AccountDisplayName} ({provenance.AccountId})\n" +
            $"External ID: {provenance.ExternalId}\n" +
            $"Capability: {provenance.CapabilityId}\n" +
            $"Source timestamp: {FormatTimestamp(provenance.SourceTimestampUtc)}\n" +
            $"Imported timestamp: {FormatTimestamp(provenance.ImportedTimestampUtc)}\n" +
            $"Freshness: {FormatEnum(provenance.Freshness)}";

        RawReferenceText.Text =
            $"Raw provider reference retained without unnecessary raw payload: {provenance.RawReference}";

        DetailFieldItems.ItemsSource = candidate.Fields
            .Select(field => new FieldView(
                field.DisplayName,
                field.Value))
            .ToArray();

        ActionStatusText.Text = candidate.IsQuarantined
            ? $"Quarantined: {candidate.QuarantineReason}"
            : candidate.AuthoritativeLink is null
                ? candidate.ReviewNote
                : $"Linked to {candidate.AuthoritativeLink.Module} · {candidate.AuthoritativeLink.RecordId}";
    }

    private void PopulateDuplicateReview()
    {
        IntegrationCandidate incoming =
            _service.GetRequiredCandidate(DuplicateCandidateId);
        IntegrationCandidate existing =
            _service.GetRequiredCandidate(
                incoming.DuplicateOfCandidateId ?? "contact-accepted");

        DuplicateIncomingTitleText.Text = incoming.Title;
        DuplicateIncomingSourceText.Text =
            $"{incoming.Provenance.ProviderDisplayName} · {incoming.Provenance.AccountDisplayName}";
        DuplicateIncomingExternalText.Text =
            $"External ID: {incoming.Provenance.ExternalId}";

        DuplicateExistingTitleText.Text = existing.Title;
        DuplicateExistingSourceText.Text =
            $"{existing.Provenance.ProviderDisplayName} · {existing.Provenance.AccountDisplayName}";
        DuplicateExistingLinkText.Text =
            existing.AuthoritativeLink is null
                ? "No authoritative link"
                : $"{existing.AuthoritativeLink.Module} · {existing.AuthoritativeLink.RecordId}";

        DuplicateFingerprintText.Text =
            $"Matching fingerprint: {ShortHash(incoming.Fingerprint)} · external IDs remain distinct.";

        DuplicateFieldItems.ItemsSource = incoming.Fields
            .Select(field =>
            {
                IntegrationCandidateField? comparison =
                    existing.Fields.FirstOrDefault(existingField =>
                        string.Equals(
                            existingField.Key,
                            field.Key,
                            StringComparison.Ordinal));

                return new DuplicateFieldView(
                    field.DisplayName,
                    field.Value,
                    comparison?.Value ?? "Not present");
            })
            .ToArray();

        DuplicateStatusText.Text =
            $"Current state: {FormatEnum(incoming.Status)}. Review choices remain explicit and auditable.";
    }

    private void PopulateConflictReview()
    {
        IntegrationCandidate conflict =
            _service.GetRequiredCandidate(ConflictCandidateId);

        ConflictHeaderText.Text =
            $"{conflict.Title} conflicts with authoritative record {conflict.ConflictWithRecordId}.";

        ConflictFieldItems.ItemsSource = conflict.Fields
            .Where(field => field.IsConflict)
            .Select(field => new ConflictFieldView(
                field.DisplayName,
                field.CurrentValue ?? "Not present",
                field.Value,
                FormatEnum(field.ConflictChoice)))
            .ToArray();

        ConflictStatusText.Text =
            $"Current state: {FormatEnum(conflict.Status)}. Applying choices accepts only the explicitly reviewed field result.";
    }

    private void PopulateBatchAndAccepted()
    {
        _batchPreview ??= _service.PreviewBatch(
            BatchCandidateIds,
            IntegrationBatchDecision.Accept,
            DateTimeOffset.UtcNow);

        BatchSummaryText.Text = _batchPreview.Summary;
        BatchCandidateItems.ItemsSource = BatchCandidateIds
            .Select(candidateId =>
            {
                IntegrationCandidate candidate =
                    _service.GetRequiredCandidate(candidateId);

                return new BatchCandidateView(
                    FormatEnum(candidate.Type),
                    candidate.Title,
                    "Accept");
            })
            .ToArray();

        if (_batchPreview.Warnings.Count > 0)
        {
            BatchStatusText.Text =
                string.Join(" ", _batchPreview.Warnings);
        }
        else if (string.IsNullOrWhiteSpace(BatchStatusText.Text))
        {
            BatchStatusText.Text =
                "Preview created. Cancel leaves every candidate unchanged.";
        }

        IntegrationCandidate accepted =
            _service.GetRequiredCandidate(AcceptedCandidateId);

        AcceptedTitleText.Text = accepted.Title;
        AcceptedSourceText.Text =
            $"{accepted.Provenance.ProviderDisplayName} · {accepted.Provenance.ExternalId}";
        AcceptedStatusText.Text =
            $"{FormatEnum(accepted.Status)} · {accepted.ReviewNote}";
        AcceptedLinkText.Text =
            accepted.AuthoritativeLink is null
                ? "No authoritative link"
                : $"{accepted.AuthoritativeLink.Module}\n" +
                  $"{accepted.AuthoritativeLink.RecordId} · {accepted.AuthoritativeLink.DisplayName}";
    }

    private void PopulateSourceAndAudit()
    {
        string[] ids =
        [
            "generic-stale",
            "message-source-removed"
        ];

        SourceCandidateItems.ItemsSource = ids
            .Select(candidateId =>
            {
                IntegrationCandidate candidate =
                    _service.GetRequiredCandidate(candidateId);

                return new SourceCandidateView(
                    candidate.Title,
                    candidate.Provenance.ProviderDisplayName,
                    FormatEnum(candidate.Status),
                    FormatEnum(candidate.Provenance.Freshness),
                    candidate.Provenance.IsSourceRemoved
                        ? "Provider tombstone retained with normalized fields and provenance."
                        : "Stale source timestamp remains visible and is never presented as current.");
            })
            .ToArray();

        AuditItems.ItemsSource = _service.State.AuditEntries
            .OrderByDescending(entry => entry.Sequence)
            .Take(12)
            .Select(entry => new AuditView(
                FormatTimestamp(entry.TimestampUtc),
                FormatEnum(entry.Action),
                entry.Summary))
            .ToArray();
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCandidateId is null)
        {
            return;
        }

        try
        {
            _service.Accept(
                _selectedCandidateId,
                "Accepted through the Group 47 review surface.",
                DateTimeOffset.UtcNow);
            SaveAndRefresh(
                "Candidate accepted. A separate authoritative link is still required.",
                _selectedCandidateId);
        }
        catch (InvalidOperationException exception)
        {
            ActionStatusText.Text = exception.Message;
        }
    }

    private void RejectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCandidateId is null)
        {
            return;
        }

        _service.Reject(
            _selectedCandidateId,
            "Rejected through the Group 47 review surface.",
            DateTimeOffset.UtcNow);
        SaveAndRefresh("Candidate rejected.", _selectedCandidateId);
    }

    private void IgnoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCandidateId is null)
        {
            return;
        }

        _service.Ignore(
            _selectedCandidateId,
            "Ignored through the Group 47 review surface.",
            DateTimeOffset.UtcNow);
        SaveAndRefresh("Candidate ignored.", _selectedCandidateId);
    }

    private void DuplicateLinkButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ResolveDuplicate(IntegrationDuplicateResolutionChoice.LinkExisting);

    private void DuplicateSeparateButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ResolveDuplicate(IntegrationDuplicateResolutionChoice.KeepSeparate);

    private void DuplicateReplaceButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ResolveDuplicate(IntegrationDuplicateResolutionChoice.ReplaceCandidate);

    private void DuplicateIgnoreButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ResolveDuplicate(IntegrationDuplicateResolutionChoice.Ignore);

    private void DuplicateRejectButton_Click(
        object sender,
        RoutedEventArgs e) =>
        ResolveDuplicate(IntegrationDuplicateResolutionChoice.Reject);

    private void ResolveDuplicate(
        IntegrationDuplicateResolutionChoice choice)
    {
        try
        {
            _service.ResolveDuplicate(
                DuplicateCandidateId,
                choice,
                DateTimeOffset.UtcNow);
            SaveAndRefresh(
                $"Duplicate review completed with choice {FormatEnum(choice)}.",
                DuplicateCandidateId);
        }
        catch (InvalidOperationException exception)
        {
            DuplicateStatusText.Text = exception.Message;
        }
    }

    private void ApplyConflictButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        IntegrationCandidate conflict =
            _service.GetRequiredCandidate(ConflictCandidateId);

        Dictionary<string, IntegrationConflictFieldChoice> choices =
            conflict.Fields
                .Where(field => field.IsConflict)
                .ToDictionary(
                    field => field.Key,
                    field => field.ConflictChoice,
                    StringComparer.Ordinal);

        try
        {
            _service.ApplyConflictReview(
                ConflictCandidateId,
                choices,
                DateTimeOffset.UtcNow);
            SaveAndRefresh(
                "Field-level conflict choices applied.",
                ConflictCandidateId);
        }
        catch (InvalidOperationException exception)
        {
            ConflictStatusText.Text = exception.Message;
        }
    }

    private void ApplyBatchButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_batchPreview is null)
        {
            return;
        }

        try
        {
            _service.ApplyBatch(
                _batchPreview,
                "Accepted through explicit low-risk batch preview.",
                DateTimeOffset.UtcNow);
            BatchStatusText.Text =
                "Batch acceptance applied. Authoritative links still require separate explicit actions.";
            SaveAndRefresh(
                BatchStatusText.Text,
                BatchCandidateIds[0]);
        }
        catch (InvalidOperationException exception)
        {
            BatchStatusText.Text = exception.Message;
        }
    }

    private void CancelBatchButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _batchPreview = null;
        BatchStatusText.Text =
            "Batch preview cancelled. No candidate state changed.";
    }

    private void SaveAndRefresh(
        string status,
        string? selectedCandidateId)
    {
        try
        {
            IntegrationInboxV9Store.Save(
                _service.State,
                _storePath);
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException)
        {
            status +=
                " State changed in memory, but the local Integration Inbox file could not be saved.";
        }

        RefreshAll(selectedCandidateId);
        ActionStatusText.Text = status;
    }

    private void BackButton_Click(
        object sender,
        RoutedEventArgs e) =>
        BackRequested?.Invoke(this, EventArgs.Empty);

    private static int StatusSort(
        IntegrationCandidateStatus status) =>
        status switch
        {
            IntegrationCandidateStatus.Conflict => 0,
            IntegrationCandidateStatus.DuplicateSuspected => 1,
            IntegrationCandidateStatus.NeedsReview => 2,
            IntegrationCandidateStatus.New => 3,
            IntegrationCandidateStatus.Accepted => 4,
            IntegrationCandidateStatus.SourceRemoved => 5,
            IntegrationCandidateStatus.Ignored => 6,
            IntegrationCandidateStatus.Rejected => 7,
            IntegrationCandidateStatus.Superseded => 8,
            _ => 9
        };

    private static string FormatTimestamp(
        DateTimeOffset timestampUtc) =>
        timestampUtc
            .ToLocalTime()
            .ToString("yyyy-MM-dd HH:mm");

    private static string FormatCandidateType(
        IntegrationCandidateType type) =>
        type switch
        {
            IntegrationCandidateType.Message => "Email",
            IntegrationCandidateType.CalendarEvent => "Calendar event",
            IntegrationCandidateType.GenericProviderRecord => "Suggestion",
            _ => FormatEnum(type)
        };

    private static string FormatCandidateStatus(
        IntegrationCandidateStatus status) =>
        status switch
        {
            IntegrationCandidateStatus.New => "Needs review",
            IntegrationCandidateStatus.NeedsReview => "Needs review",
            IntegrationCandidateStatus.DuplicateSuspected => "Possible duplicate",
            _ => FormatEnum(status)
        };

    private static string FormatEnum<T>(T value)
        where T : struct, Enum =>
        string.Concat(
            value
                .ToString()
                .Select((character, index) =>
                    index > 0 &&
                    char.IsUpper(character) &&
                    !char.IsUpper(
                        value.ToString()[index - 1])
                        ? $" {character}"
                        : character.ToString()));

    private static string ShortHash(string hash) =>
        hash.Length <= 16
            ? hash
            : $"{hash[..8]}…{hash[^8..]}";

    private enum ReviewScopeKind
    {
        CurrentItems,
        Suggestions,
        HistoryAndDemo,
        All
    }

    private sealed record ReviewScopeOption(
        ReviewScopeKind Kind,
        string Label,
        string Heading,
        string Description)
    {
        public override string ToString() => Label;

        public static ReviewScopeOption CurrentItems { get; } =
            new(
                ReviewScopeKind.CurrentItems,
                "Current account · original items",
                "Current mail and calendar",
                "Original items from the connected account that still need a decision.");

        public static IReadOnlyList<ReviewScopeOption> Options { get; } =
        [
            CurrentItems,
            new(
                ReviewScopeKind.Suggestions,
                "Current account · suggestions",
                "Suggested follow-ups",
                "Derived suggestions are separated from their source messages and events."),
            new(
                ReviewScopeKind.HistoryAndDemo,
                "History and demo evidence",
                "Historical and demo evidence",
                "Retained proof records and resolved current-account items."),
            new(
                ReviewScopeKind.All,
                "Everything",
                "All integration candidates",
                "Full engineering view across current, historical and demo records.")
        ];
    }

    private sealed record CandidateListView(
        string Id,
        string Title,
        string TypeAndStatus,
        string Source);

    private sealed record FieldView(
        string Name,
        string Value);

    private sealed record DuplicateFieldView(
        string Name,
        string Incoming,
        string Existing);

    private sealed record ConflictFieldView(
        string Name,
        string Current,
        string Candidate,
        string Decision);

    private sealed record BatchCandidateView(
        string Type,
        string Title,
        string Decision);

    private sealed record SourceCandidateView(
        string Title,
        string Source,
        string Status,
        string Freshness,
        string Detail);

    private sealed record AuditView(
        string Timestamp,
        string Action,
        string Summary);
}
