using System;
using System.IO;
using LifeOS.Shared.V8;
using Xunit;

namespace LifeOS.Core.Tests;

public sealed class V8PreferencesTests
{
    [Fact]
    public void Defaults_AreApprovedAndHomeFirst()
    {
        V8Preferences preferences = new V8Preferences().Normalize();

        Assert.Equal(V8Theme.Dark, preferences.Theme);
        Assert.Equal(V8Accent.Purple, preferences.Accent);
        Assert.Equal(V8Density.Comfortable, preferences.Density);
        Assert.Equal(V8StartupMode.Home, preferences.StartupMode);
        Assert.Equal("Home", preferences.LastWorkspace);
        Assert.Equal(1.0, preferences.TextScale);
        Assert.True(preferences.ContextPanelAutoOpen);
        Assert.Equal(V8EmergencyStopState.Idle, preferences.EmergencyStopState);
    }

    [Fact]
    public void CorruptJson_FallsBackToApprovedDefaults()
    {
        string path = CreateTemporaryPath();

        try
        {
            File.WriteAllText(path, "{ this is not valid json");

            V8Preferences preferences = V8PreferenceStore.Load(path);

            Assert.Equal("Home", preferences.LastWorkspace);
            Assert.Equal(V8Theme.Dark, preferences.Theme);
            Assert.Equal(V8Accent.Purple, preferences.Accent);
            Assert.Equal(V8Density.Comfortable, preferences.Density);
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    [Fact]
    public void InvalidPreferenceValues_NormalizeSafely()
    {
        V8Preferences preferences = new()
        {
            Theme = (V8Theme)999,
            Accent = (V8Accent)999,
            Density = (V8Density)999,
            StartupMode = (V8StartupMode)999,
            EmergencyStopState = (V8EmergencyStopState)999,
            LastWorkspace = "Legacy",
            TextScale = 9.9,
            ProfileName = "   ",
            ActiveContext = "   "
        };

        preferences.Normalize();

        Assert.Equal(V8Theme.Dark, preferences.Theme);
        Assert.Equal(V8Accent.Purple, preferences.Accent);
        Assert.Equal(V8Density.Comfortable, preferences.Density);
        Assert.Equal(V8StartupMode.Home, preferences.StartupMode);
        Assert.Equal(V8EmergencyStopState.Idle, preferences.EmergencyStopState);
        Assert.Equal("Home", preferences.LastWorkspace);
        Assert.Equal(1.0, preferences.TextScale);
        Assert.Equal("Codie Shannon", preferences.ProfileName);
        Assert.Equal("Personal", preferences.ActiveContext);
    }

    [Fact]
    public void SaveAndLoad_RoundTripsApprovedPreferences()
    {
        string path = CreateTemporaryPath();

        try
        {
            V8Preferences expected = new()
            {
                Theme = V8Theme.HighContrast,
                Accent = V8Accent.Teal,
                Density = V8Density.Compact,
                StartupMode = V8StartupMode.LastUsed,
                EmergencyStopState = V8EmergencyStopState.Armed,
                LastWorkspace = "Projects",
                ContextPanelOpen = true,
                ContextPanelAutoOpen = false,
                ReducedMotion = true,
                TextScale = 1.25,
                ProfileName = "Evidence Profile",
                ActiveContext = "Demo"
            };

            V8PreferenceStore.Save(expected, path);
            V8Preferences actual = V8PreferenceStore.Load(path);

            Assert.Equal(expected.Theme, actual.Theme);
            Assert.Equal(expected.Accent, actual.Accent);
            Assert.Equal(expected.Density, actual.Density);
            Assert.Equal(expected.StartupMode, actual.StartupMode);
            Assert.Equal(expected.EmergencyStopState, actual.EmergencyStopState);
            Assert.Equal(expected.LastWorkspace, actual.LastWorkspace);
            Assert.Equal(expected.ContextPanelOpen, actual.ContextPanelOpen);
            Assert.Equal(expected.ContextPanelAutoOpen, actual.ContextPanelAutoOpen);
            Assert.Equal(expected.ReducedMotion, actual.ReducedMotion);
            Assert.Equal(expected.TextScale, actual.TextScale);
            Assert.Equal(expected.ProfileName, actual.ProfileName);
            Assert.Equal(expected.ActiveContext, actual.ActiveContext);
        }
        finally
        {
            DeleteIfPresent(path);
        }
    }

    private static string CreateTemporaryPath() => Path.Combine(
        Path.GetTempPath(),
        $"lifeos-v8-preferences-{Guid.NewGuid():N}.json");

    private static void DeleteIfPresent(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        string temporaryPath = path + ".tmp";

        if (File.Exists(temporaryPath))
        {
            File.Delete(temporaryPath);
        }
    }
}
