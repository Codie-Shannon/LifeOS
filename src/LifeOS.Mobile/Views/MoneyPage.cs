using LifeOS.Mobile.Core.Money;

namespace LifeOS.Mobile.Views;

public sealed class MoneyPage : ContentPage
{
    private readonly V11MoneyService _service = new();
    private readonly VerticalStackLayout _content = new();

    public MoneyPage()
    {
        Title = "Money";
        BackgroundColor = MoneyVisuals.Background;
        Content = new ScrollView { Content = _content };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Render();
    }

    private void Render()
    {
        var now = DateTimeOffset.Now;
        var overview = _service.BuildOverview(now);
        var data = _service.BuildProofData(now);
        _content.Children.Clear();
        _content.Padding = new Thickness(18, 14, 18, 30);
        _content.Spacing = 13;
        _content.Children.Add(new Label { Text = "Money v11", FontSize = 34, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
        _content.Children.Add(new Label { Text = "Authoritative financial records • review-first • fictional proof data", FontSize = 15, TextColor = MoneyVisuals.Secondary });
        _content.Children.Add(MoneyVisuals.Section("Overview"));
        _content.Children.Add(MoneyVisuals.CardView("Current balance", $"{overview.Currency} {overview.CurrentBalance:N2}", "Trusted records"));
        _content.Children.Add(MoneyVisuals.CardView("Income / expenses", $"{overview.Currency} {overview.Income:N2} / {overview.Expenses:N2}", "Deterministic"));
        _content.Children.Add(MoneyVisuals.CardView("Invoices due", $"{overview.Currency} {overview.InvoicesDue:N2}", "Due and overdue"));
        _content.Children.Add(MoneyVisuals.CardView("Payments received", $"{overview.Currency} {overview.PaymentsReceived:N2}", "Reviewed"));
        _content.Children.Add(MoneyVisuals.CardView("Evidence gaps", overview.EvidenceGaps.ToString(), overview.EvidenceGaps == 0 ? "Complete" : "Needs review"));
        AddAction("Financial review summary", () => Navigation.PushAsync(new FinancialReviewPage()));
        AddAction("Document & evidence intake", () => Navigation.PushAsync(new DocumentIntakePage()));
        AddAction("Accounts and status", () => Navigation.PushAsync(new FinancialListPage("Accounts", data.Accounts.Select(x => $"{x.Name}\n{x.Type} • {x.Currency} {x.Balance:N2} • {x.ReviewState}\nLive connected: {x.IsLiveConnected}"))));
        AddAction("Transactions", () => Navigation.PushAsync(new FinancialListPage("Transactions", data.Transactions.Select(x => $"{x.Date:dd MMM} • {x.Description}\n{x.Direction} • {x.Currency} {x.Amount:N2} • {x.ReviewState}\nCategory: {x.CategoryId}"))));
        AddAction("Invoices and payments", () => Navigation.PushAsync(new FinancialListPage("Invoices", data.Invoices.Select(x => $"{x.InvoiceNumber} • {x.State}\n{x.Currency} {x.Total:N2} • outstanding {x.Outstanding:N2}\nLinks {x.Links.Count} • evidence {x.Evidence.Count}"))));
        AddAction("Manual record review", () => Navigation.PushAsync(new ManualFinancialReviewPage(_service, data)));
        AddAction("Audit and protection", () => Navigation.PushAsync(new FinancialListPage("Audit history", data.Audit.Select(x => $"{x.OccurredUtc:g} • {x.Action}\n{x.RecordType} {x.RecordId}\n{x.SafeSummary}"))));
        _content.Children.Add(MoneyVisuals.CardView("Boundary", "No bank feed, payment initiation, provider write, accounting approval or automatic reconciliation exists."));
    }

    private void AddAction(string text, Func<Task> action)
    {
        var button = MoneyVisuals.Action(text, Color.FromArgb("#2C3140"));
        button.Clicked += async (_, _) => await action();
        _content.Children.Add(button);
    }
}
