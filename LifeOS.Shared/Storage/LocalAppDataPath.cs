namespace LifeOS.Shared.Storage;

public static class LocalAppDataPath
{
    public static bool IsPortfolioDemoMode { get; private set; }

    public static void SetPortfolioDemoMode(bool enabled) =>
        IsPortfolioDemoMode = enabled;

    public static string GetLifeOSFolder()
    {
        string root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LifeOS");
        string folder = IsPortfolioDemoMode
            ? Path.Combine(root, "portfolio-demo")
            : root;

        Directory.CreateDirectory(folder);

        return folder;
    }

    public static string GetFilePath(string fileName)
    {
        return Path.Combine(GetLifeOSFolder(), fileName);
    }
}
