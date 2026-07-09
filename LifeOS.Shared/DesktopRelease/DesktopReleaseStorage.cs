using System.Text.Json;
using LifeOS.Core.DesktopRelease;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.DesktopRelease;

public static class DesktopReleaseStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("desktop-release-readiness.json");

    public static DesktopReleaseReadinessProfile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return DesktopReleaseDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? DesktopReleaseDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<DesktopReleaseReadinessProfile>(json, Options) ?? DesktopReleaseDemoData.CreateDefaultProfile();
        }
        catch
        {
            return DesktopReleaseDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(DesktopReleaseReadinessProfile profile)
    {
        profile.LastReviewedAt = DateTime.Now;
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(DesktopReleaseDemoData.CreateDefaultProfile());
    }
}
