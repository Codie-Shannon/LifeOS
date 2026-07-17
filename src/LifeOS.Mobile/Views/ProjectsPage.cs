using LifeOS.Mobile.Core.Projects;

namespace LifeOS.Mobile.Views;

public sealed class ProjectsPage : ContentPage
{
    private readonly ProjectService _service = new();
    private readonly VerticalStackLayout _content = new();

    public ProjectsPage()
    {
        Title = "Projects";
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
                Text = "Projects",
                FontSize = 34,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            });

        _content.Children.Add(
            new Label
            {
                Text =
                    "Milestones, next actions, progress, evidence and deliberate project state",
                FontSize = 15,
                TextColor = LifeProjectVisuals.Secondary
            });

        var summary = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        AddSummary(summary, 0, 0, "Active", snapshot.ActiveCount);
        AddSummary(summary, 1, 0, "Waiting", snapshot.WaitingCount);
        AddSummary(summary, 0, 1, "Blocked", snapshot.BlockedCount);
        AddSummary(summary, 1, 1, "Parked", snapshot.ParkedCount);

        _content.Children.Add(summary);
        _content.Children.Add(LifeProjectVisuals.Section("Project list"));

        foreach (var project in snapshot.Projects)
        {
            var captured = project;
            var open = LifeProjectVisuals.Action(
                $"{project.Name} • {project.Status}",
                Color.FromArgb("#2C3140"));

            open.Clicked += async (_, _) =>
                await Navigation.PushAsync(
                    new ProjectDetailPage(
                        _service,
                        captured));

            _content.Children.Add(
                LifeProjectVisuals.CardView(
                    project.Name,
                    $"{project.CurrentMilestone} • " +
                    $"{project.ProgressPercent}% • Next: {project.NextAction}",
                    project.Status.ToString()));

            _content.Children.Add(open);
        }
    }

    private static void AddSummary(
        Grid grid,
        int column,
        int row,
        string label,
        int count)
    {
        var card = LifeProjectVisuals.CardView(
            label,
            count.ToString());

        Grid.SetColumn(card, column);
        Grid.SetRow(card, row);
        grid.Children.Add(card);
    }
}
