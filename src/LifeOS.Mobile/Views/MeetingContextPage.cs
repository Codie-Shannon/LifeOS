using LifeOS.Mobile.Core.Work;

namespace LifeOS.Mobile.Views;

public sealed class MeetingContextPage : ContentPage
{
    public MeetingContextPage(MeetingContext meeting)
    {
        Title = "Meeting context";
        BackgroundColor = WorkVisuals.Background;

        var actions = new VerticalStackLayout
        {
            Spacing = 10
        };

        foreach (var action in meeting.CapturedActions)
        {
            actions.Children.Add(
                WorkVisuals.CardView(
                    "Captured action",
                    action));
        }

        var capture = WorkVisuals.Action("Capture action");
        var status = new Label
        {
            Text =
                "Actions remain local until explicitly saved and queued.",
            TextColor = WorkVisuals.Secondary
        };

        capture.Clicked += (_, _) =>
        {
            status.Text =
                "Fictional post-meeting action captured locally.";
            status.TextColor = Color.FromArgb("#8FE3B0");
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 14,
                Children =
                {
                    new Label
                    {
                        Text = meeting.Title,
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    WorkVisuals.CardView(
                        "Time",
                        meeting.StartsUtc.ToLocalTime()
                            .ToString("ddd d MMM, h:mm tt")),
                    WorkVisuals.CardView(
                        "Attendees",
                        string.Join(" • ", meeting.Attendees)),
                    WorkVisuals.CardView(
                        "Safe context",
                        meeting.SafeContext),
                    WorkVisuals.Section("Captured follow-up"),
                    actions,
                    capture,
                    status
                }
            }
        };
    }
}
