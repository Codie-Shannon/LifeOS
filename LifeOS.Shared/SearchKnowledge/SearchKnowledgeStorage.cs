using System.Text.Json;
using LifeOS.Core.SearchKnowledge;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.SearchKnowledge;

public static class SearchKnowledgeStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("search-knowledge.json");

    public static SearchKnowledgeProfile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return SearchKnowledgeDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? SearchKnowledgeDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<SearchKnowledgeProfile>(json, Options) ?? SearchKnowledgeDemoData.CreateDefaultProfile();
        }
        catch
        {
            return SearchKnowledgeDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(SearchKnowledgeProfile profile)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(SearchKnowledgeDemoData.CreateDefaultProfile());
    }
}
