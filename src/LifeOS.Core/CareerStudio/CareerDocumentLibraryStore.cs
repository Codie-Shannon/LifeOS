using System.Text.Json;

namespace LifeOS.Core.CareerStudio;

public static class CareerDocumentLibraryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static string DefaultFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LifeOS",
        "career",
        "career-documents.json");

    public static CareerDocumentLibrary Load(string? filePath = null)
    {
        string path = Resolve(filePath);
        if (!File.Exists(path))
            return CareerDocumentLibrary.Empty;

        try
        {
            CareerDocumentLibrary? library = JsonSerializer.Deserialize<CareerDocumentLibrary>(
                File.ReadAllText(path),
                JsonOptions);
            return Normalize(library);
        }
        catch (JsonException)
        {
            PreserveUnreadableFile(path);
            return LoadBackup(path + ".backup");
        }
        catch (IOException)
        {
            return CareerDocumentLibrary.Empty;
        }
        catch (UnauthorizedAccessException)
        {
            return CareerDocumentLibrary.Empty;
        }
    }

    public static void Save(CareerDocumentLibrary library, string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(library);
        CareerDocumentLibrary normalized = Normalize(library);
        string path = Resolve(filePath);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        string temporaryPath = path + ".tmp";
        string backupPath = path + ".backup";
        File.WriteAllText(
            temporaryPath,
            JsonSerializer.Serialize(normalized, JsonOptions) + Environment.NewLine);

        if (File.Exists(path))
            File.Copy(path, backupPath, overwrite: true);
        File.Move(temporaryPath, path, overwrite: true);
    }

    private static CareerDocumentLibrary Normalize(CareerDocumentLibrary? library)
    {
        if (library is null || library.SchemaVersion is < 1 or > CareerDocumentLibrary.CurrentSchemaVersion)
            return CareerDocumentLibrary.Empty;

        CvBuilderDocument[] documents = (library.Documents ?? [])
            .Where(document => document is not null && !string.IsNullOrWhiteSpace(document.Id))
            .GroupBy(document => document.Id, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(document => document.UpdatedUtc).First())
            .ToArray();
        string activeDocumentId = documents.Any(document => document.Id == library.ActiveDocumentId)
            ? library.ActiveDocumentId
            : documents.FirstOrDefault()?.Id ?? string.Empty;
        CvStoredVersion[] versions = (library.Versions ?? [])
            .Where(version =>
                version is not null &&
                version.Snapshot is not null &&
                version.Document is not null &&
                documents.Any(document => document.Id == version.DocumentId))
            .OrderBy(version => version.Snapshot.SavedUtc)
            .ToArray();

        return new CareerDocumentLibrary(
            CareerDocumentLibrary.CurrentSchemaVersion,
            documents,
            activeDocumentId,
            versions);
    }

    private static void PreserveUnreadableFile(string path)
    {
        try
        {
            string preservedPath = path + $".unreadable-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            File.Copy(path, preservedPath, overwrite: false);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            // The original remains untouched even when a diagnostic copy cannot be created.
        }
    }

    private static CareerDocumentLibrary LoadBackup(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
                return CareerDocumentLibrary.Empty;

            CareerDocumentLibrary? library =
                JsonSerializer.Deserialize<CareerDocumentLibrary>(
                    File.ReadAllText(backupPath),
                    JsonOptions);
            return Normalize(library);
        }
        catch (Exception exception) when (
            exception is JsonException or IOException or UnauthorizedAccessException)
        {
            return CareerDocumentLibrary.Empty;
        }
    }

    private static string Resolve(string? filePath) =>
        string.IsNullOrWhiteSpace(filePath) ? DefaultFilePath : Path.GetFullPath(filePath);
}
