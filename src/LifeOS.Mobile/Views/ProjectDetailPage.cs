using LifeOS.Mobile.Core.Projects;

namespace LifeOS.Mobile.Views;

public sealed class ProjectDetailPage : ContentPage
{
    public ProjectDetailPage(
        ProjectService service,
        MobileProject project)
    {
        Title = "Project detail";
        BackgroundColor = LifeProjectVisuals.Background;

        var status = new Label
        {
            Text = $"Status: {project.Status}",
            TextColor = LifeProjectVisuals.Warning,
            FontAttributes = FontAttributes.Bold
        };

        var park = LifeProjectVisuals.Action(
            "Park project",
            Color.FromArgb("#2C3140"));

        park.Clicked += (_, _) =>
        {
            var result = service.Park(
                project.Id,
                DateTimeOffset.UtcNow);

            status.Text = $"Status: {result.State}";
            status.TextColor = LifeProjectVisuals.Success;
        };

        var resume = LifeProjectVisuals.Action("Resume project");

        resume.Clicked += (_, _) =>
        {
            var result = service.Resume(
                project.Id,
                DateTimeOffset.UtcNow);

            status.Text = $"Status: {result.State}";
            status.TextColor = LifeProjectVisuals.Success;
        };

        var content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 14,
            Children =
            {
                new Label
                {
                    Text = project.Name,
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                },
                status,
                LifeProjectVisuals.CardView(
                    "Current milestone",
                    project.CurrentMilestone),
                LifeProjectVisuals.CardView(
                    "Next action",
                    project.NextAction),
                LifeProjectVisuals.CardView(
                    "Progress",
                    $"{project.ProgressPercent}%")
            }
        };

        content.Children.Add(
            LifeProjectVisuals.Section("Milestones"));

        if (project.Milestones.Count == 0)
        {
            content.Children.Add(
                LifeProjectVisuals.CardView(
                    "No active milestone",
                    "Project remains parked or waiting."));
        }
        else
        {
            foreach (var milestone in project.Milestones)
            {
                content.Children.Add(
                    LifeProjectVisuals.CardView(
                        milestone.Title,
                        $"{(milestone.IsComplete ? "Complete" : "Open")} • " +
                        milestone.DueUtc.ToLocalTime()
                            .ToString("ddd d MMM, h:mm tt")));
            }
        }

        content.Children.Add(
            LifeProjectVisuals.Section("Evidence"));

        if (project.Evidence.Count == 0)
        {
            content.Children.Add(
                LifeProjectVisuals.CardView(
                    "No linked evidence",
                    "Nothing is inferred or generated automatically."));
        }
        else
        {
            foreach (var evidence in project.Evidence)
            {
                content.Children.Add(
                    LifeProjectVisuals.CardView(
                        evidence.Label,
                        $"{evidence.FileName} • {evidence.Source}",
                        evidence.Complete ? "Ready" : "Review"));
            }
        }

        content.Children.Add(park);
        content.Children.Add(resume);

        content.Children.Add(
            new Label
            {
                Text =
                    "Project state changes require an explicit user action.",
                TextColor = LifeProjectVisuals.Secondary
            });

        Content = new ScrollView
        {
            Content = content
        };
    }
}
