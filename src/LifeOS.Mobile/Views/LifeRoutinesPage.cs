using LifeOS.Mobile.Core.Life;

namespace LifeOS.Mobile.Views;

public sealed class LifeRoutinesPage : ContentPage
{
    public LifeRoutinesPage(
        LifeService service,
        IReadOnlyList<LifeRoutine> routines)
    {
        Title = "Routines";
        BackgroundColor = LifeProjectVisuals.Background;

        var content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 14,
            Children =
            {
                new Label
                {
                    Text = "Routines",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                }
            }
        };

        foreach (var routine in routines)
        {
            var status = new Label
            {
                Text = routine.CompletedToday
                    ? "Completed today"
                    : "Not completed",
                TextColor = routine.CompletedToday
                    ? LifeProjectVisuals.Success
                    : LifeProjectVisuals.Warning
            };

            var captured = routine;
            var complete = LifeProjectVisuals.Action(
                "Mark complete",
                Color.FromArgb("#2C3140"));

            complete.Clicked += (_, _) =>
            {
                var result = service.CompleteRoutine(
                    captured.Id,
                    DateTimeOffset.UtcNow);

                status.Text = result.State;
                status.TextColor = LifeProjectVisuals.Success;
            };

            content.Children.Add(
                LifeProjectVisuals.CardView(
                    routine.Title,
                    $"{routine.Area} • {routine.Frequency}"));

            content.Children.Add(status);
            content.Children.Add(complete);
        }

        Content = new ScrollView
        {
            Content = content
        };
    }
}
