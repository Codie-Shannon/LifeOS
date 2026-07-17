using LifeOS.Mobile.Core.Money;

namespace LifeOS.Mobile.Views;

public sealed class InvoiceDetailPage : ContentPage
{
    public InvoiceDetailPage(InvoiceRecord invoice)
    {
        Title = "Invoice detail";
        BackgroundColor = MoneyVisuals.Background;

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
                        Text = invoice.Client,
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    MoneyVisuals.CardView(
                        "Amount",
                        $"{invoice.Currency} {invoice.Amount:N2}"),
                    MoneyVisuals.CardView(
                        "Issued",
                        invoice.IssueDate.ToString("ddd d MMM yyyy")),
                    MoneyVisuals.CardView(
                        "Due",
                        invoice.DueDate.ToString("ddd d MMM yyyy")),
                    MoneyVisuals.CardView(
                        "Status",
                        invoice.Status.ToString()),
                    MoneyVisuals.CardView(
                        "Linked proof",
                        invoice.LinkedEvidence.Count == 0
                            ? "No linked evidence"
                            : string.Join(" • ", invoice.LinkedEvidence)),
                    new Label
                    {
                        Text =
                            "No payment action or external accounting write is available.",
                        TextColor = MoneyVisuals.Secondary
                    }
                }
            }
        };
    }
}
