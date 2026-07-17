using LifeOS.Mobile.Core.Home;
using LifeOS.Mobile.Core.Services;

namespace LifeOS.Mobile.Views;

public sealed class QuickCapturePage : ContentPage
{
    private readonly MobileFoundationService _foundation;
    private readonly HomeDailyService _home = new();
    private readonly Picker _kind;
    private readonly Editor _text;
    private readonly Label _status;

    public QuickCapturePage(MobileFoundationService foundation)
    {
        _foundation = foundation;

        Title = "Quick capture";
        BackgroundColor = HomeVisuals.Background;

        _kind = new Picker
        {
            Title = "Capture type",
            TitleColor = HomeVisuals.Secondary,
            TextColor = Colors.White,
            ItemsSource = Enum
                .GetNames<MobileCaptureKind>()
                .Select(Humanize)
                .ToArray(),
            SelectedIndex = 0,
            HeightRequest = 52
        };

        _text = new Editor
        {
            Placeholder = "Write a fictional or sanitized capture…",
            PlaceholderColor = Color.FromArgb("#8E93A3"),
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#1B1E28"),
            HeightRequest = 130,
            MaxLength = 240
        };

        _status = new Label
        {
            Text =
                "Nothing is saved until you explicitly queue this draft.",
            TextColor = HomeVisuals.Secondary
        };

        var save = HomeVisuals.Action("Save and queue");
        save.Clicked += SaveAsync;

        var cancel = HomeVisuals.Action(
            "Cancel draft",
            Color.FromArgb("#2C3140"));

        cancel.Clicked += async (_, _) =>
            await Navigation.PopAsync();

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 16,
                Children =
                {
                    new Label
                    {
                        Text = "Quick capture",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    new Label
                    {
                        Text =
                            "Task • note • expense-evidence placeholder • " +
                            "person follow-up • project idea",
                        TextColor = HomeVisuals.Secondary
                    },
                    new Border
                    {
                        Padding = new Thickness(14, 0),
                        BackgroundColor = Color.FromArgb("#1B1E28"),
                        Stroke = Color.FromArgb("#303441"),
                        StrokeShape =
                            new Microsoft.Maui.Controls.Shapes
                                .RoundRectangle
                            {
                                CornerRadius = 14
                            },
                        Content = _kind
                    },
                    _text,
                    _status,
                    save,
                    cancel
                }
            }
        };
    }

    private async void SaveAsync(
        object? sender,
        EventArgs args)
    {
        try
        {
            var kind = (MobileCaptureKind)_kind.SelectedIndex;
            var draft = _home.CreateDraft(
                kind,
                _text.Text ?? string.Empty,
                DateTimeOffset.UtcNow);

            await _foundation.QueueQuickCaptureAsync(draft);

            _status.Text =
                $"{Humanize(kind.ToString())} queued safely. " +
                "It remains reviewable before synchronization.";

            _status.TextColor = Color.FromArgb("#8FE3B0");
            _text.IsEnabled = false;
        }
        catch (ArgumentException ex)
        {
            _status.Text = ex.Message;
            _status.TextColor = Color.FromArgb("#FF9A9A");
        }
    }

    private static string Humanize(string value)
    {
        var result = new System.Text.StringBuilder();

        foreach (var character in value)
        {
            if (result.Length > 0 && char.IsUpper(character))
            {
                result.Append(' ');
            }

            result.Append(character);
        }

        return result.ToString();
    }
}
