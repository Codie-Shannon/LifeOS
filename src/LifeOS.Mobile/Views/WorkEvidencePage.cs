using LifeOS.Mobile.Core.Work;

namespace LifeOS.Mobile.Views;

public sealed class WorkEvidencePage : ContentPage
{
    public WorkEvidencePage(
        IReadOnlyList<WorkEvidenceItem> evidence)
    {
        Title = "Evidence";
        BackgroundColor = WorkVisuals.Background;

        var content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 14,
            Children =
            {
                new Label
                {
                    Text = "Evidence checklist",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                },
                new Label
                {
                    Text =
                        "Attachment metadata only • no automatic body download",
                    TextColor = WorkVisuals.Secondary
                }
            }
        };

        foreach (var item in evidence)
        {
            content.Children.Add(
                WorkVisuals.CardView(
                    item.Label,
                    $"{(item.Complete ? "Complete" : "Needs review")} • " +
                    $"{item.FileName} • {item.Source} • " +
                    $"{item.SizeBytes} bytes",
                    item.Complete ? "Ready" : "Review"));
        }

        Content = new ScrollView
        {
            Content = content
        };
    }
}
