using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using LifeOS.Shared.V8;
using Microsoft.Win32;

namespace LifeOS.Desktop;

internal static class V8ThemeManager
{
    private const string ThemePrefix = "Resources/Themes/";

    public static V8Theme Apply(V8Preferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        preferences.Normalize();

        V8Theme effectiveTheme = preferences.Theme == V8Theme.System
            ? ResolveSystemTheme()
            : preferences.Theme;

        string themeFile = effectiveTheme switch
        {
            V8Theme.Light => "Theme.Light.xaml",
            V8Theme.HighContrast => "Theme.HighContrast.xaml",
            _ => "Theme.Dark.xaml"
        };

        ResourceDictionary replacement = new()
        {
            Source = new Uri($"{ThemePrefix}{themeFile}", UriKind.Relative)
        };

        var dictionaries = Application.Current.Resources.MergedDictionaries;
        ResourceDictionary? existingTheme = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains("Resources/Themes/Theme.", StringComparison.OrdinalIgnoreCase) == true &&
            dictionary.Source.OriginalString.Contains("Theme.Base", StringComparison.OrdinalIgnoreCase) == false);

        if (existingTheme is null)
        {
            dictionaries.Add(replacement);
        }
        else
        {
            int index = dictionaries.IndexOf(existingTheme);
            dictionaries[index] = replacement;
        }

        ApplyAccent(preferences.Accent, effectiveTheme == V8Theme.HighContrast);
        ApplyTextScale(preferences.TextScale);
        Application.Current.Resources["LifeOS.ReducedMotion"] = preferences.ReducedMotion;

        return effectiveTheme;
    }

    private static V8Theme ResolveSystemTheme()
    {
        try
        {
            object? value = Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme",
                0);

            return value is int lightValue && lightValue == 1
                ? V8Theme.Light
                : V8Theme.Dark;
        }
        catch
        {
            return V8Theme.Dark;
        }
    }

    private static void ApplyAccent(V8Accent accent, bool highContrast)
    {
        Color color = highContrast
            ? Color.FromRgb(255, 255, 0)
            : accent switch
            {
                V8Accent.Blue => Color.FromRgb(59, 130, 246),
                V8Accent.Teal => Color.FromRgb(20, 184, 166),
                _ => Color.FromRgb(139, 92, 246)
            };

        Color softColor = highContrast
            ? Color.FromArgb(70, 255, 255, 0)
            : Color.FromArgb(52, color.R, color.G, color.B);

        Application.Current.Resources["LifeOS.Color.Accent"] = color;
        Application.Current.Resources["LifeOS.Brush.Accent"] = new SolidColorBrush(color);
        Application.Current.Resources["LifeOS.Brush.AccentSoft"] = new SolidColorBrush(softColor);
        Application.Current.Resources["LifeOS.Brush.Focus"] = new SolidColorBrush(color);
    }

    private static void ApplyTextScale(double scale)
    {
        Application.Current.Resources["LifeOS.FontSize.Caption"] = 12.0 * scale;
        Application.Current.Resources["LifeOS.FontSize.Body"] = 14.0 * scale;
        Application.Current.Resources["LifeOS.FontSize.Subheading"] = 18.0 * scale;
        Application.Current.Resources["LifeOS.FontSize.Heading"] = 24.0 * scale;
        Application.Current.Resources["LifeOS.FontSize.Hero"] = 31.0 * scale;
    }
}
