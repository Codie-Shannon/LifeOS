using LifeOS.Mobile.Core.Foundation;
using LifeOS.Mobile.Core.Services;

namespace LifeOS.Mobile.Views;

public sealed class MorePage : ContentPage
{
    public MorePage(MobileFoundationService foundation)
    {
        Title = "More"; BackgroundColor = Color.FromArgb("#11131A");
        var layout = new VerticalStackLayout { Padding = 20, Spacing = 12 };
        layout.Children.Add(new Label { Text = "All workspaces", FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
        foreach (var workspace in MobileWorkspaceCatalog.Permanent.Where(x => x is not "Home" and not "Work" and not "Money" and not "Projects"))
        {
            var captured = workspace;
            var button = new Button { Text = captured, BackgroundColor = Color.FromArgb("#292D3A"), TextColor = Colors.White };
            button.Clicked += async (_, _) =>
{
    Page target = captured switch
    {
        "Life" => new LifePage(),
        "Settings" => new SettingsPage(foundation),
        _ => new WorkspacePage(captured)
    };

    await Navigation.PushAsync(target);
};
            layout.Children.Add(button);
        }
        var diagnostics = new Button { Text = "Diagnostics", BackgroundColor = Color.FromArgb("#7C5CFC"), TextColor = Colors.White };
        diagnostics.Clicked += async (_, _) => await Navigation.PushAsync(new DiagnosticsPage(foundation));
        layout.Children.Add(diagnostics);
        var betaClosure = new Button
        {
            Text = "Beta closure",
            BackgroundColor = Color.FromArgb("#7C5CFC"),
            TextColor = Colors.White
        };

        betaClosure.Clicked += async (_, _) =>
            await Navigation.PushAsync(new BetaClosurePage());

        layout.Children.Add(betaClosure);
        Content = new ScrollView { Content = layout };
    }
}
