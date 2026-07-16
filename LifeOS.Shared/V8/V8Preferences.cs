using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LifeOS.Shared.V8;

public enum V8Theme
{
    Light,
    Dark,
    System,
    HighContrast
}

public enum V8Accent
{
    Purple,
    Blue,
    Teal
}

public enum V8Density
{
    Comfortable,
    Compact
}

public enum V8StartupMode
{
    Home,
    LastUsed
}

public enum V8EmergencyStopState
{
    Idle,
    Armed,
    Stopped
}

public sealed class V8Preferences
{
    public const double DefaultTextScale = 1.0;

    private static readonly string[] LockedWorkspaces =
    {
        "Home",
        "Work",
        "Career",
        "Money",
        "Life",
        "Projects",
        "Assistant",
        "Settings"
    };

    private static readonly double[] ApprovedTextScales = { 1.0, 1.1, 1.25, 1.4 };

    public V8Theme Theme { get; set; } = V8Theme.Dark;

    public V8Accent Accent { get; set; } = V8Accent.Purple;

    public V8Density Density { get; set; } = V8Density.Comfortable;

    public V8StartupMode StartupMode { get; set; } = V8StartupMode.Home;

    public V8EmergencyStopState EmergencyStopState { get; set; } = V8EmergencyStopState.Idle;

    public string LastWorkspace { get; set; } = "Home";

    public bool ContextPanelOpen { get; set; }

    public bool ContextPanelAutoOpen { get; set; } = true;

    public bool ReducedMotion { get; set; }

    public double TextScale { get; set; } = DefaultTextScale;

    public string ProfileName { get; set; } = "Codie Shannon";

    public string ActiveContext { get; set; } = "Personal";

    public static IReadOnlyList<string> WorkspaceNames => LockedWorkspaces;

    public static IReadOnlyList<double> TextScales => ApprovedTextScales;

    public V8Preferences Normalize()
    {
        Theme = Enum.IsDefined(Theme) ? Theme : V8Theme.Dark;
        Accent = Enum.IsDefined(Accent) ? Accent : V8Accent.Purple;
        Density = Enum.IsDefined(Density) ? Density : V8Density.Comfortable;
        StartupMode = Enum.IsDefined(StartupMode) ? StartupMode : V8StartupMode.Home;
        EmergencyStopState = Enum.IsDefined(EmergencyStopState)
            ? EmergencyStopState
            : V8EmergencyStopState.Idle;

        LastWorkspace = LockedWorkspaces.Contains(LastWorkspace, StringComparer.OrdinalIgnoreCase)
            ? LockedWorkspaces.Single(name => string.Equals(name, LastWorkspace, StringComparison.OrdinalIgnoreCase))
            : "Home";

        TextScale = ApprovedTextScales.Any(scale => Math.Abs(scale - TextScale) < 0.001)
            ? ApprovedTextScales.Single(scale => Math.Abs(scale - TextScale) < 0.001)
            : DefaultTextScale;

        ProfileName = NormalizeDisplayText(ProfileName, "Codie Shannon", 80);
        ActiveContext = NormalizeDisplayText(ActiveContext, "Personal", 80);

        return this;
    }

    public static bool IsLockedWorkspace(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        LockedWorkspaces.Contains(value, StringComparer.OrdinalIgnoreCase);

    private static string NormalizeDisplayText(string? value, string fallback, int maxLength)
    {
        string normalized = (value ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return fallback;
        }

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }
}

public static class V8PreferenceStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static string DefaultFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LifeOS",
        "v8-preferences.json");

    public static V8Preferences Load(string? filePath = null)
    {
        string path = ResolvePath(filePath);

        try
        {
            if (!File.Exists(path))
            {
                return new V8Preferences().Normalize();
            }

            string json = File.ReadAllText(path);
            V8Preferences? preferences = JsonSerializer.Deserialize<V8Preferences>(json, JsonOptions);
            return (preferences ?? new V8Preferences()).Normalize();
        }
        catch (JsonException)
        {
            return new V8Preferences().Normalize();
        }
        catch (IOException)
        {
            return new V8Preferences().Normalize();
        }
        catch (UnauthorizedAccessException)
        {
            return new V8Preferences().Normalize();
        }
    }

    public static void Save(V8Preferences preferences, string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        string path = ResolvePath(filePath);
        string? directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        preferences.Normalize();
        string json = JsonSerializer.Serialize(preferences, JsonOptions) + Environment.NewLine;
        string temporaryPath = path + ".tmp";

        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, path, overwrite: true);
    }

    public static void Reset(string? filePath = null)
    {
        string path = ResolvePath(filePath);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string ResolvePath(string? filePath) =>
        string.IsNullOrWhiteSpace(filePath) ? DefaultFilePath : Path.GetFullPath(filePath);
}
