using LifeOS.Mobile.Core.Money;

namespace LifeOS.Mobile.Views;

public sealed class EvidenceReviewPage : ContentPage
{
    public EvidenceReviewPage(
        MoneyService service,
        MoneyDashboardSnapshot snapshot)
    {
        Title = "Evidence review";
        BackgroundColor = MoneyVisuals.Background;

        var content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 14,
            Children =
            {
                new Label
                {
                    Text = "Evidence review queue",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                },
                new Label
                {
                    Text =
                        "Captured and imported evidence remains explicit and reviewable.",
                    TextColor = MoneyVisuals.Secondary
                }
            }
        };

        foreach (var item in snapshot.Evidence)
        {
            var badge = item.ReviewState ==
                EvidenceReviewState.DuplicateCandidate
                    ? "Duplicate"
                    : item.ReviewState.ToString();

            content.Children.Add(
                MoneyVisuals.CardView(
                    item.FileName,
                    $"{item.SafePreviewLabel} • {item.Category} • " +
                    $"{item.SizeBytes} bytes",
                    badge));
        }

        var duplicate = snapshot.Evidence[1];
        var original = snapshot.Evidence[0];

        content.Children.Add(
            MoneyVisuals.CardView(
                "Duplicate candidate",
                service.IsDuplicateCandidate(original, duplicate)
                    ? "Same SHA-256 source data • review required • no automatic deletion"
                    : "No exact duplicate candidate"));

        var link = MoneyVisuals.Action("Categorize and link");
        var result = new Label
        {
            Text = "No evidence has been linked automatically.",
            TextColor = MoneyVisuals.Secondary
        };

        link.Clicked += (_, _) =>
        {
            var linked = service.LinkEvidence(
                original,
                "txn-expense",
                "Software");

            result.Text =
                $"Linked to {linked.LinkedRecordId} • {linked.Category}";
            result.TextColor = MoneyVisuals.Success;
        };

        content.Children.Add(link);
        content.Children.Add(result);

        Content = new ScrollView
        {
            Content = content
        };
    }
}
