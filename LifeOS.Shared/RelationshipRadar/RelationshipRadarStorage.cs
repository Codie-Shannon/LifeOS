using System.Text.Json;
using LifeOS.Core.RelationshipRadar;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.RelationshipRadar;

public static class RelationshipRadarStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("relationship-radar.json");

    public static List<RelationshipRadarProfile> Load()
    {
        if (!File.Exists(FilePath))
        {
            return RelationshipRadarDemoData.CreateDefaultProfiles();
        }

        var json = File.ReadAllText(FilePath);
        return string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<RelationshipRadarProfile>>(json, Options) ?? [];
    }

    public static void Save(IEnumerable<RelationshipRadarProfile> profiles)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profiles.ToList(), Options));
    }

    public static void ResetToDemoData()
    {
        Save(RelationshipRadarDemoData.CreateDefaultProfiles());
    }
}
