using System.Text;
using System.Text.Json;
using LifeOS.Core.Documents;
using LifeOS.Shared.Documents;
using LifeOS.Shared.Storage;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class Groups157To160DocumentIntakeTests
{
    [Fact]
    public void Capture_requires_non_empty_named_file()
    {
        var result = DocumentCaptureService.Validate(new DocumentCaptureDraft(
            null, null, [], DocumentType.GeneralEvidence));

        Assert.False(result.IsValid);
        Assert.Contains(result.ForField("document-file"), issue => issue.Code == "required");
        Assert.Contains(result.ForField("document-file"), issue => issue.Code == "empty-file");
    }

    [Fact]
    public void Capture_rejects_files_over_25_megabytes()
    {
        byte[] bytes = new byte[DocumentCaptureService.MaximumBytes + 1];

        var result = DocumentCaptureService.Validate(new DocumentCaptureDraft(
            "large.bin", "application/octet-stream", bytes, DocumentType.GeneralEvidence));

        Assert.Equal("file-too-large", Assert.Single(result.ForField("document-file")).Code);
    }

    [Fact]
    public void Create_preserves_bytes_hash_and_safe_source_without_full_path()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("fictional document evidence");
        DateTimeOffset imported = new(2026, 8, 18, 1, 2, 3, TimeSpan.Zero);

        DocumentRecord record = DocumentCaptureService.Create(new DocumentCaptureDraft(
            @"C:\private\proof.txt", "text/plain", bytes, DocumentType.GeneralEvidence), imported);

        Assert.Equal("proof.txt", record.Original.FileName);
        Assert.Equal(DocumentIntegrity.Sha256(bytes), record.Original.Sha256);
        Assert.Equal("Desktop file picker", record.Original.Source);
        Assert.True(record.HasTrustedOriginal);
        Assert.DoesNotContain("private", record.Original.Source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Review_transitions_retain_original_and_require_explicit_acceptance()
    {
        DocumentIntakeService service = new();
        DocumentRecord draft = Record("alpha.txt", "same");
        DocumentRecord review = service.MoveToReview(draft);
        DocumentRecord accepted = service.Accept(review, DocumentType.Contract, [], []);

        Assert.Equal(DocumentIntakeState.Draft, draft.State);
        Assert.Equal(DocumentIntakeState.ReviewRequired, review.State);
        Assert.Equal(DocumentIntakeState.Accepted, accepted.State);
        Assert.Equal(DocumentType.Contract, accepted.Type);
        Assert.Equal(draft.Original.Bytes, accepted.Original.Bytes);
        Assert.True(accepted.HasTrustedOriginal);
    }

    [Fact]
    public void Exact_duplicate_is_candidate_only_and_does_not_merge_records()
    {
        DocumentIntakeService service = new();
        DocumentRecord first = Record("first.txt", "same");
        DocumentRecord second = Record("second.txt", "same");

        DuplicateDocumentCandidate duplicate = Assert.Single(service.FindExactDuplicates(second, [first]));

        Assert.Equal(1m, duplicate.Confidence);
        Assert.Equal("pending_review", duplicate.State);
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Missing_repository_returns_honest_empty_state_without_writing()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "document-intake.json");
        DocumentIntakeRepository repository = new(path);

        LocalStoreLoadResult<List<DocumentRecord>> result = repository.LoadResult();

        Assert.Equal(LocalStoreLoadState.Empty, result.State);
        Assert.Empty(result.Value);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void Repository_round_trips_versioned_integrity_checked_original()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "document-intake.json");
        DocumentIntakeRepository repository = new(path);
        repository.Save([Record("alpha.txt", "preserved")]);

        DocumentRecord loaded = Assert.Single(repository.Load());
        using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));

        Assert.True(loaded.HasTrustedOriginal);
        Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("document-intake", json.RootElement.GetProperty("storeId").GetString());
        Assert.Equal(LocalStoreHealthState.Healthy, repository.Inspect().State);
    }

    [Fact]
    public void Repository_rejects_corrupt_original_and_trash_restore_is_recoverable()
    {
        using TemporaryDirectory temporary = new();
        string path = Path.Combine(temporary.Path, "document-intake.json");
        DocumentIntakeRepository repository = new(path);
        DocumentRecord valid = Record("alpha.txt", "preserved");
        DocumentRecord corrupt = valid with
        {
            Original = valid.Original with { Bytes = Encoding.UTF8.GetBytes("changed") }
        };

        Assert.Throws<InvalidDataException>(() => repository.Save([corrupt]));
        repository.Save([valid]);
        LocalStoreTrashEntry trash = repository.MoveToTrash();
        repository.RestoreTrash(trash.Id);

        Assert.True(Assert.Single(repository.Load()).HasTrustedOriginal);
        Assert.Throws<InvalidOperationException>(() => repository.RestoreTrash(trash.Id));
    }

    private static DocumentRecord Record(string fileName, string content) =>
        DocumentCaptureService.Create(new DocumentCaptureDraft(
            fileName,
            "text/plain",
            Encoding.UTF8.GetBytes(content),
            DocumentType.GeneralEvidence),
            DateTimeOffset.UtcNow);

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "lifeos-document-intake-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }
}
