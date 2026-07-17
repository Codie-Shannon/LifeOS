using LifeOS.Mobile.Core.Money;

namespace LifeOS.Mobile.Views;

public sealed class MoneyPage : ContentPage
{
    private readonly MoneyService _service = new();
    private readonly VerticalStackLayout _content = new();

    public MoneyPage()
    {
        Title = "Money";
        BackgroundColor = MoneyVisuals.Background;

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
                Text = "Money",
                FontSize = 34,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            });

        _content.Children.Add(
            new Label
            {
                Text =
                    "Conservative tracking, invoice status and original-preserving evidence",
                FontSize = 15,
                TextColor = MoneyVisuals.Secondary
            });

        _content.Children.Add(MoneyVisuals.Section("Overview"));

        _content.Children.Add(
            MoneyVisuals.CardView(
                "Income",
                $"{snapshot.Summary.Currency} {snapshot.Summary.Income:N2}",
                "Trusted"));

        _content.Children.Add(
            MoneyVisuals.CardView(
                "Expenses",
                $"{snapshot.Summary.Currency} {snapshot.Summary.Expenses:N2}",
                "Review"));

        _content.Children.Add(
            MoneyVisuals.CardView(
                "Outstanding invoices",
                $"{snapshot.Summary.Currency} {snapshot.Summary.OutstandingInvoices:N2}",
                "Due"));

        _content.Children.Add(
            MoneyVisuals.CardView(
                "Payments received",
                $"{snapshot.Summary.Currency} {snapshot.Summary.PaymentsReceived:N2}",
                "Received"));

        var invoices = MoneyVisuals.Action("Invoices and payments");
        invoices.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new InvoiceListPage(snapshot.Invoices));

        var capture = MoneyVisuals.Action(
            "Capture receipt evidence",
            Color.FromArgb("#2C3140"));

        capture.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new EvidenceCapturePage(_service, snapshot));

        var review = MoneyVisuals.Action(
            "Evidence review queue",
            Color.FromArgb("#2C3140"));

        review.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new EvidenceReviewPage(_service, snapshot));

        var privacy = MoneyVisuals.Action(
            "Open privacy mode",
            Color.FromArgb("#2C3140"));

        privacy.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new MoneyPrivacyPage());

        _content.Children.Add(invoices);
        _content.Children.Add(capture);
        _content.Children.Add(review);
        _content.Children.Add(privacy);

        _content.Children.Add(
            MoneyVisuals.CardView(
                "Boundary",
                "LifeOS tracks and organizes. It does not approve accounting, initiate payments or write to external accounting systems."));
    }
}
