using LifeOS.Mobile.Core.Services;

namespace LifeOS.Mobile.Views;

public sealed class DiagnosticsPage : ContentPage
{
    private readonly MobileFoundationService _foundation;
    private readonly VerticalStackLayout _items = new() { Spacing = 10 };
    public DiagnosticsPage(MobileFoundationService foundation)
    {
        _foundation = foundation; Title = "Diagnostics"; BackgroundColor = Color.FromArgb("#11131A");
        Content = new ScrollView { Content = new VerticalStackLayout { Padding = 20, Spacing = 14, Children =
        {
            new Label { Text = "Redacted diagnostics", FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Colors.White },
            new Label { Text = "Safe summary only — tokens, secrets, addresses and raw provider payloads are never displayed.", TextColor = Color.FromArgb("#C7C9D3") },
            _items
        }}};
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing(); _items.Children.Clear();
        foreach (var item in await _foundation.GetSafeDiagnosticsAsync())
            _items.Children.Add(new Border { Padding = 12, BackgroundColor = Color.FromArgb("#1B1E28"), Content = new Label { Text = $"{item.Name}: {item.Value}", TextColor = Colors.White } });
    }
}
