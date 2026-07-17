using LifeOS.Mobile.Core.Money;

namespace LifeOS.Mobile.Views;

public sealed class InvoiceListPage : ContentPage
{
    public InvoiceListPage(IReadOnlyList<InvoiceRecord> invoices)
    {
        Title = "Invoices";
        BackgroundColor = MoneyVisuals.Background;

        var content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 14,
            Children =
            {
                new Label
                {
                    Text = "Invoices and payments",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                }
            }
        };

        foreach (var invoice in invoices.OrderBy(x => x.DueDate))
        {
            var captured = invoice;
            var button = MoneyVisuals.Action(
                $"{invoice.Client} • {invoice.Status}",
                Color.FromArgb("#2C3140"));

            button.Clicked += async (_, _) =>
                await Navigation.PushAsync(
                    new InvoiceDetailPage(captured));

            content.Children.Add(
                MoneyVisuals.CardView(
                    invoice.Client,
                    $"{invoice.Currency} {invoice.Amount:N2} • " +
                    $"Due {invoice.DueDate:ddd d MMM}",
                    invoice.Status.ToString()));

            content.Children.Add(button);
        }

        Content = new ScrollView
        {
            Content = content
        };
    }
}
