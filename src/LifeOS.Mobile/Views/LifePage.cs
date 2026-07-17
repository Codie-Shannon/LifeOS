using LifeOS.Mobile.Core.Life;

namespace LifeOS.Mobile.Views;

public sealed class LifePage : ContentPage
{
    private readonly LifeService _service = new();
    private readonly VerticalStackLayout _content = new();

    public LifePage()
    {
        Title = "Life";
        BackgroundColor = LifeProjectVisuals.Background;

        Content = new ScrollView
        {
            Content = _content
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Render();
    }

    private void Render()
    {
        var snapshot = _service.BuildSnapshot(DateTimeOffset.Now);

        _content.Children.Clear();
        _content.Padding = new Thickness(18, 14, 18, 30);
        _content.Spacing = 13;

        _content.Children.Add(
            new Label
            {
                Text = "Life",
                FontSize = 34,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            });

        _content.Children.Add(
            new Label
            {
                Text =
                    "Private personal context, routines and reminders with minimized mobile detail",
                FontSize = 15,
                TextColor = LifeProjectVisuals.Secondary
            });

        _content.Children.Add(
            LifeProjectVisuals.Section("Areas"));

        foreach (var area in snapshot.Areas)
        {
            _content.Children.Add(
                LifeProjectVisuals.CardView(
                    area.Name,
                    $"{area.Summary} • Open {area.OpenCount}",
                    area.Status.ToString()));
        }

        var routines = LifeProjectVisuals.Action("Open routines");
        routines.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new LifeRoutinesPage(_service, snapshot.Routines));

        var reminders = LifeProjectVisuals.Action(
            "Open reminders",
            Color.FromArgb("#2C3140"));

        reminders.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new LifeRemindersPage(_service, snapshot.Reminders));

        var privacy = LifeProjectVisuals.Action(
            "Open minimized-detail proof",
            Color.FromArgb("#2C3140"));

        privacy.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new LifePrivacyPage());

        _content.Children.Add(routines);
        _content.Children.Add(reminders);
        _content.Children.Add(privacy);

        _content.Children.Add(
            LifeProjectVisuals.CardView(
                "Boundary",
                "No diagnosis, treatment decision, hidden family detail or silent external action."));
    }
}
