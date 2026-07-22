using LifeOS.Core.CareerStudio;

namespace LifeOS.Mobile.Views;

public sealed class CareerApplicationPage : ContentPage
{
    public CareerApplicationPage(CareerApplication application)
    {
        Title = "Application";
        BackgroundColor = Color.FromArgb("#101018");
        var stack = new VerticalStackLayout { Padding = 14, Spacing = 12 };
        stack.Children.Add(CareerPage.Heading("Application detail", 28));
        stack.Children.Add(CareerPage.Text("Explicit status, checklist, defer and offline-safe note actions.", 14, "#B6B3C8"));
        stack.Children.Add(CareerPage.Card("Current application",
            $"State: {application.State}\nChannel: {application.SubmissionChannel}\nConfirmation: {application.ConfirmationReference}\nNext follow-up: {application.NextFollowUpUtc:dd MMM HH:mm}",
            "NO AUTOMATIC SUBMISSION"));
        foreach (var item in application.Checklist)
            stack.Children.Add(CareerPage.Card(item.Label,
                $"Required: {item.IsRequired}\nEvidence: {item.EvidenceLinkId ?? "Not linked"}",
                item.IsComplete ? "COMPLETE" : "OPEN"));
        foreach (var follow in application.FollowUps)
            stack.Children.Add(CareerPage.Card("Follow-up",
                $"{follow.Description}\nDue: {follow.DueUtc:dd MMM HH:mm}\nNo message is sent.",
                follow.State.ToString().ToUpperInvariant()));
        var update = new Button { Text = "Queue explicit stage update", BackgroundColor = Color.FromArgb("#4057D6"), TextColor = Colors.White, CornerRadius = 12 };
        var defer = new Button { Text = "Defer", BackgroundColor = Color.FromArgb("#2C3140"), TextColor = Colors.White, CornerRadius = 12 };
        stack.Children.Add(update);
        stack.Children.Add(defer);
        stack.Children.Add(CareerPage.Text("Queued updates remain pending until replay and conflict review completes.", 12, "#A9A5B9"));
        Content = new ScrollView { Content = stack };
    }
}
