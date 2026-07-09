using System.Text.Json;
using LifeOS.Core.FinalOfflineOs;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.FinalOfflineOs;

public static class FinalOfflineOsStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("final-offline-os.json");

    public static FinalOfflineOsProfile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return FinalOfflineOsDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? FinalOfflineOsDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<FinalOfflineOsProfile>(json, Options) ?? FinalOfflineOsDemoData.CreateDefaultProfile();
        }
        catch
        {
            return FinalOfflineOsDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(FinalOfflineOsProfile profile)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(FinalOfflineOsDemoData.CreateDefaultProfile());
    }
}
