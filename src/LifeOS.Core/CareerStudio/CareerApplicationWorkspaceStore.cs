using System.Text.Json;

namespace LifeOS.Core.CareerStudio;

public static class CareerApplicationWorkspaceStore
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
        "career-applications.json");

    public static CareerApplicationWorkspace Load(string? filePath = null)
    {
        string path = Resolve(filePath);
        if (!File.Exists(path))
            return CareerApplicationWorkspace.Empty;

        try
        {
            CareerApplicationWorkspace? workspace =
                JsonSerializer.Deserialize<CareerApplicationWorkspace>(
                    File.ReadAllText(path),
                    JsonOptions);
            return Normalize(workspace);
        }
        catch (JsonException)
        {
            PreserveUnreadableFile(path);
            return LoadBackup(path + ".backup");
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            return CareerApplicationWorkspace.Empty;
        }
    }

    public static void Save(
        CareerApplicationWorkspace workspace,
        string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        CareerApplicationWorkspace normalized = Normalize(workspace);
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

    private static CareerApplicationWorkspace Normalize(
        CareerApplicationWorkspace? workspace)
    {
        if (workspace is null ||
            workspace.SchemaVersion is < 1 or > CareerApplicationWorkspace.CurrentSchemaVersion)
            return CareerApplicationWorkspace.Empty;

        CareerOpportunity[] opportunities = Unique(
            workspace.Opportunities,
            opportunity => opportunity.Id);
        CoverLetterDocument[] letters = Unique(
                workspace.CoverLetters,
                letter => letter.Id)
            .Where(letter => opportunities.Any(opportunity =>
                opportunity.Id == letter.OpportunityId))
            .ToArray();
        CareerApplication[] applications = Unique(
            workspace.Applications,
            application => application.Id);
        CareerApplicationPack[] packs = Unique(
            workspace.Packs,
            pack => pack.Id);
        CareerFact[] facts = Unique(workspace.Facts, fact => fact.Id);

        string activeOpportunityId = opportunities.Any(opportunity =>
            opportunity.Id == workspace.ActiveOpportunityId)
            ? workspace.ActiveOpportunityId
            : opportunities.FirstOrDefault()?.Id ?? string.Empty;
        string activeLetterId = letters.Any(letter =>
            letter.Id == workspace.ActiveCoverLetterId)
            ? workspace.ActiveCoverLetterId
            : letters.FirstOrDefault()?.Id ?? string.Empty;

        return new CareerApplicationWorkspace(
            CareerApplicationWorkspace.CurrentSchemaVersion,
            opportunities,
            letters,
            applications.Where(application => opportunities.Any(opportunity =>
                opportunity.Id == application.OpportunityId)).ToArray(),
            packs.Where(pack => opportunities.Any(opportunity =>
                opportunity.Id == pack.OpportunityId)).ToArray(),
            facts,
            activeOpportunityId,
            activeLetterId);
    }

    private static T[] Unique<T>(IReadOnlyList<T>? values, Func<T, string> id) =>
        (values ?? [])
            .Where(value => value is not null && !string.IsNullOrWhiteSpace(id(value)))
            .GroupBy(id, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToArray();

    private static CareerApplicationWorkspace LoadBackup(string path)
    {
        try
        {
            if (!File.Exists(path))
                return CareerApplicationWorkspace.Empty;
            return Normalize(JsonSerializer.Deserialize<CareerApplicationWorkspace>(
                File.ReadAllText(path),
                JsonOptions));
        }
        catch (Exception exception) when (
            exception is JsonException or IOException or UnauthorizedAccessException)
        {
            return CareerApplicationWorkspace.Empty;
        }
    }

    private static void PreserveUnreadableFile(string path)
    {
        try
        {
            File.Copy(
                path,
                path + $".unreadable-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                overwrite: false);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
        }
    }

    private static string Resolve(string? filePath) =>
        string.IsNullOrWhiteSpace(filePath)
            ? DefaultFilePath
            : Path.GetFullPath(filePath);
}
