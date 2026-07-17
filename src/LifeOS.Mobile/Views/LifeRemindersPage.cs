using LifeOS.Mobile.Core.Life;

namespace LifeOS.Mobile.Views;

public sealed class LifeRemindersPage : ContentPage
{
    public LifeRemindersPage(
        LifeService service,
        IReadOnlyList<LifeReminder> reminders)
    {
        Title = "Reminders";
        BackgroundColor = LifeProjectVisuals.Background;

        var content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 14,
            Children =
            {
                new Label
                {
                    Text = "Life reminders",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                }
            }
        };

        foreach (var reminder in reminders)
        {
            var status = new Label
            {
                Text = $"Status: {reminder.Status}",
                TextColor = LifeProjectVisuals.Warning
            };

            var captured = reminder;
            var defer = LifeProjectVisuals.Action(
                "Defer one day",
                Color.FromArgb("#2C3140"));

            defer.Clicked += (_, _) =>
            {
                var result = service.DeferReminder(
                    captured.Id,
                    DateTimeOffset.UtcNow.AddDays(1),
                    DateTimeOffset.UtcNow);

                status.Text = result.State;
                status.TextColor = LifeProjectVisuals.Success;
            };

            content.Children.Add(
                LifeProjectVisuals.CardView(
                    reminder.Title,
                    $"{reminder.Area} • " +
                    reminder.DueUtc.ToLocalTime()
                        .ToString("ddd d MMM, h:mm tt")));

            content.Children.Add(status);
            content.Children.Add(defer);
        }

        Content = new ScrollView
        {
            Content = content
        };
    }
}
