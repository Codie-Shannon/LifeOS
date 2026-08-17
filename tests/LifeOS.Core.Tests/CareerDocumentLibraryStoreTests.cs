using LifeOS.Core.CareerStudio;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class CareerDocumentLibraryStoreTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 18, 9, 0, 0, TimeSpan.FromHours(12));

    [Fact]
    public void Missing_library_opens_as_an_honest_empty_state()
    {
        string path = TemporaryPath();

        CareerDocumentLibrary library = CareerDocumentLibraryStore.Load(path);

        Assert.Empty(library.Documents);
        Assert.Empty(library.Versions);
        Assert.Equal(string.Empty, library.ActiveDocumentId);
    }

    [Fact]
    public void Library_round_trip_preserves_document_and_version_history()
    {
        string path = TemporaryPath();
        try
        {
            CareerDocumentBuilderService builder = new();
            CareerDocumentLayoutService layout = new();
            CvBuilderDocument document = builder.CreateBlank("cv-1", "Primary CV", Now);
            CvVersionSnapshot snapshot = layout.CreateSnapshot(document, "Initial save");
            CareerDocumentLibrary expected = new(
                CareerDocumentLibrary.CurrentSchemaVersion,
                [document],
                document.Id,
                [new CvStoredVersion(document.Id, snapshot, document)]);

            CareerDocumentLibraryStore.Save(expected, path);
            CareerDocumentLibrary actual = CareerDocumentLibraryStore.Load(path);

            CvBuilderDocument stored = Assert.Single(actual.Documents);
            Assert.Equal(document.Id, stored.Id);
            Assert.Equal(document.Name, stored.Name);
            Assert.Equal(document.Version, stored.Version);
            Assert.Equal(
                System.Text.Json.JsonSerializer.Serialize(document),
                System.Text.Json.JsonSerializer.Serialize(stored));
            CvStoredVersion version = Assert.Single(actual.Versions);
            Assert.Equal("Initial save", version.Snapshot.Label);
            Assert.Equal(document.Id, version.Document.Id);
            Assert.Equal(
                System.Text.Json.JsonSerializer.Serialize(document),
                System.Text.Json.JsonSerializer.Serialize(version.Document));
        }
        finally
        {
            DeleteTemporaryFiles(path);
        }
    }

    [Fact]
    public void Saving_again_preserves_a_recoverable_backup()
    {
        string path = TemporaryPath();
        try
        {
            CareerDocumentBuilderService builder = new();
            CvBuilderDocument original = builder.CreateBlank("cv-1", "Original", Now);
            CareerDocumentLibraryStore.Save(
                new(CareerDocumentLibrary.CurrentSchemaVersion, [original], original.Id, []),
                path);
            CvBuilderDocument revised = original with { Name = "Revised", Version = 2 };

            CareerDocumentLibraryStore.Save(
                new(CareerDocumentLibrary.CurrentSchemaVersion, [revised], revised.Id, []),
                path);

            Assert.True(File.Exists(path + ".backup"));
            Assert.Equal("Original", CareerDocumentLibraryStore.Load(path + ".backup").Documents.Single().Name);
            Assert.Equal("Revised", CareerDocumentLibraryStore.Load(path).Documents.Single().Name);
        }
        finally
        {
            DeleteTemporaryFiles(path);
        }
    }

    [Fact]
    public void Corrupt_library_is_preserved_and_does_not_seed_demo_records()
    {
        string path = TemporaryPath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "{ not valid json");

            CareerDocumentLibrary library = CareerDocumentLibraryStore.Load(path);

            Assert.Empty(library.Documents);
            Assert.True(File.Exists(path));
            Assert.NotEmpty(Directory.GetFiles(Path.GetDirectoryName(path)!, Path.GetFileName(path) + ".unreadable-*"));
        }
        finally
        {
            DeleteTemporaryFiles(path);
        }
    }

    [Fact]
    public void Null_collections_in_valid_json_recover_as_an_empty_library()
    {
        string path = TemporaryPath();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(
                path,
                """
                {
                  "schemaVersion": 1,
                  "documents": null,
                  "activeDocumentId": "cv-missing",
                  "versions": null
                }
                """);

            CareerDocumentLibrary library = CareerDocumentLibraryStore.Load(path);

            Assert.Empty(library.Documents);
            Assert.Empty(library.Versions);
            Assert.Equal(string.Empty, library.ActiveDocumentId);
        }
        finally
        {
            DeleteTemporaryFiles(path);
        }
    }

    [Fact]
    public void Corrupt_primary_recovers_the_last_valid_backup()
    {
        string path = TemporaryPath();
        try
        {
            CareerDocumentBuilderService builder = new();
            CvBuilderDocument original = builder.CreateBlank("cv-1", "Recover me", Now);
            CareerDocumentLibraryStore.Save(
                new(CareerDocumentLibrary.CurrentSchemaVersion, [original], original.Id, []),
                path);
            CareerDocumentLibraryStore.Save(
                new(CareerDocumentLibrary.CurrentSchemaVersion, [original with { Version = 2 }], original.Id, []),
                path);
            File.WriteAllText(path, "{ damaged primary");

            CareerDocumentLibrary recovered = CareerDocumentLibraryStore.Load(path);

            Assert.Equal("Recover me", Assert.Single(recovered.Documents).Name);
            Assert.Equal(1, recovered.Documents.Single().Version);
            Assert.NotEmpty(Directory.GetFiles(
                Path.GetDirectoryName(path)!,
                Path.GetFileName(path) + ".unreadable-*"));
        }
        finally
        {
            DeleteTemporaryFiles(path);
        }
    }

    private static string TemporaryPath() => Path.Combine(
        Path.GetTempPath(),
        $"lifeos-career-library-{Guid.NewGuid():N}",
        "career-documents.json");

    private static void DeleteTemporaryFiles(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }
}
