namespace LifeOS.Shared.Storage;

public static class LocalAppDataPath
{
    public static string GetLifeOSFolder()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LifeOS");

        Directory.CreateDirectory(folder);

        return folder;
    }

    public static string GetFilePath(string fileName)
    {
        return Path.Combine(GetLifeOSFolder(), fileName);
    }
}