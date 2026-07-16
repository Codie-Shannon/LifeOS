using LifeOS.Mobile.Core.Foundation;
using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Core.Storage;

namespace LifeOS.Mobile.Views;

public sealed class HomePage : ContentPage
{
    private readonly MobileFoundationService _foundation;
    private readonly Label _sync = new();
    public HomePage(MobileFoundationService foundation)
    {
        _foundation = foundation; Title = "Home"; BackgroundColor = Color.FromArgb("#11131A");
        var stop = new Button { Text = "Emergency Stop", BackgroundColor = Color.FromArgb("#B3261E"), TextColor = Colors.White };
        SemanticProperties.SetDescription(stop, "Immediately stop network and synchronization activity");
        stop.Clicked += async (_, _) => { await _foundation.ActivateEmergencyStopAsync(); await RefreshAsync(); };
        Content = new ScrollView { Content = new VerticalStackLayout { Padding = 20, Spacing = 16, Children =
        {
            new Label { Text = "LifeOS", FontSize = 34, FontAttributes = FontAttributes.Bold, TextColor = Colors.White },
            new Label { Text = "Full Mobile foundation", FontSize = 18, TextColor = Color.FromArgb("#C7C9D3") },
            Card("Ready", "Local-first demo mode • fictional data only"),
            Card("Eight workspaces", string.Join(" • ", MobileWorkspaceCatalog.Permanent)),
            Card("Freshness", "Last successful sync 4 minutes ago"),
            _sync,
            stop,
            new Label { Text = "No provider writes. No silent merges. Pending work remains reviewable.", TextColor = Color.FromArgb("#C7C9D3") }
        }}};
    }
    protected override async void OnAppearing() { base.OnAppearing(); await RefreshAsync(); }
    private async Task RefreshAsync()
    {
        var diagnostics = await _foundation.GetSafeDiagnosticsAsync();
        _sync.Text = string.Join("  •  ", diagnostics.Where(x => x.Name is "Sync state" or "Pending queue").Select(x => $"{x.Name}: {x.Value}"));
        _sync.TextColor = Colors.White; _sync.FontAttributes = FontAttributes.Bold;
    }
    private static Border Card(string heading, string body) => new() { Padding = 16, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 }, BackgroundColor = Color.FromArgb("#1B1E28"), Content = new VerticalStackLayout { Spacing = 6, Children = { new Label { Text = heading, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Colors.White }, new Label { Text = body, TextColor = Color.FromArgb("#C7C9D3") } } } };
}
