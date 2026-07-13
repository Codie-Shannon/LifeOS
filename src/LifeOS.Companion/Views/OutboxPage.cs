using LifeOS.Companion.Core.Models;
using LifeOS.Companion.Core.Storage;

namespace LifeOS.Companion.Views;

public sealed class OutboxPage : ContentPage
{
    private readonly ICompanionStore _store;
    private readonly CollectionView _list = new();
    private readonly Label _summary = new();
    public OutboxPage(ICompanionStore store)
    {
        _store = store;
        Title = "Outbox";
        _list.ItemTemplate = new DataTemplate(() =>
        {
            var title = new Label { FontAttributes = FontAttributes.Bold };
            title.SetBinding(Label.TextProperty, nameof(QuickCapture.Title));
            var body = new Label { MaxLines = 2 };
            body.SetBinding(Label.TextProperty, nameof(QuickCapture.Body));
            var state = new Label();
            state.SetBinding(Label.TextProperty, nameof(QuickCapture.DeliveryState), stringFormat: "State: {0}");
            return new VerticalStackLayout { Padding = 10, Children = { title, body, state } };
        });
        Content = new VerticalStackLayout
        {
            Padding = 16, Spacing = 12,
            Children =
            {
                new Label { Text = "Offline outbox", FontSize = 24, FontAttributes = FontAttributes.Bold },
                new Label { Text = "Not paired · No automatic sending · No background retry" },
                _summary, _list,
                new Button { Text = "Pair device (Group 33)", IsEnabled = false }
            }
        };
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var items = await _store.GetOutboxAsync();
        _list.ItemsSource = items;
        _summary.Text = $"{items.Count} Pending item(s)";
    }
}
