namespace LifeOS.Mobile.Views;

public sealed class WorkspacePage : ContentPage
{
    public WorkspacePage(string name)
    {
        Title = name; BackgroundColor = Color.FromArgb("#11131A");
        Content = new VerticalStackLayout { Padding = 22, Spacing = 14, VerticalOptions = LayoutOptions.Center, Children =
        {
            new Label { Text = name, FontSize = 32, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = "Foundation placeholder — full workspace scope begins in its dedicated group.", TextColor = Color.FromArgb("#C7C9D3"), HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = "Offline-safe • review-first • no external writes", TextColor = Color.FromArgb("#AFA4FF"), HorizontalTextAlignment = TextAlignment.Center }
        }};
    }
}
