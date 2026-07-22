using LifeOS.Core.Grocery;
using Microsoft.Maui.Controls.Shapes;

namespace LifeOS.Mobile.Views;

public sealed class GroceryPage : ContentPage
{
    private readonly GroceryPlanningService _service = new();
    private readonly (IReadOnlyList<GroceryItem> Items, IReadOnlyList<GroceryList> Lists, IReadOnlyList<RecurringEssential> Essentials) _data = GroceryProofData.Build();
    private readonly VerticalStackLayout _content = new();

    public GroceryPage()
    {
        Title = "Grocery";
        BackgroundColor = Color.FromArgb("#101018");
        Content = new ScrollView { Content = _content };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var dashboard = _service.BuildDashboard(_data.Lists, _data.Essentials, new DateOnly(2026,7,22));
        _content.Children.Clear();
        _content.Padding = new Thickness(12,14,12,30);
        _content.Spacing = 11;
        _content.Children.Add(Heading("Grocery dashboard", 28));
        _content.Children.Add(Text("Review-first planning and in-store execution â€¢ no orders or payments"));
        _content.Children.Add(Card("Active list", $"{dashboard.ActiveList!.Name}\nState {dashboard.ActiveList.State}\nEstimated {dashboard.ActiveList.Currency} {dashboard.ActiveList.EstimatedTotal:0.00}\n{dashboard.ActiveList.EstimateSource}", "OPEN"));
        _content.Children.Add(Card("Due essentials", $"{dashboard.DueEssentials.Count} require review\nAccept, defer or skip explicitly", "REVIEW"));
        _content.Children.Add(Card("Recently completed", $"{dashboard.RecentlyCompleted.Count} recent list", "HISTORY"));
        AddAction("Open shopping list", () => Navigation.PushAsync(new GroceryShoppingPage(_data.Items, _data.Lists[0])));
        AddAction("Quick add item", () => DisplayAlert("Quick add", "Text capture creates a local draft item only.", "OK"));
        AddAction("Review due essentials", () => DisplayAlert("Recurring essentials", "Candidates remain reviewable and are never purchased automatically.", "OK"));
    }

    private void AddAction(string label, Func<Task> action)
    {
        var b = new Button { Text = label, BackgroundColor = Color.FromArgb("#4057D6"), TextColor = Colors.White, CornerRadius = 12, HeightRequest = 48 };
        b.Clicked += async (_,_) => await action();
        _content.Children.Add(b);
    }

    internal static Label Heading(string text, double size) => new() { Text=text, FontSize=size, FontAttributes=FontAttributes.Bold, TextColor=Colors.White };
    internal static Label Text(string text) => new() { Text=text, FontSize=13, TextColor=Color.FromArgb("#D6D3E6") };
    internal static Border Card(string title, string body, string badge)
    {
        var p = new VerticalStackLayout { Spacing=6 };
        p.Children.Add(Heading(title, 17)); p.Children.Add(Text(body));
        p.Children.Add(new Label { Text=badge, FontSize=11, TextColor=Color.FromArgb("#9A7CFF") });
        return new Border { BackgroundColor=Color.FromArgb("#1B1A25"), Stroke=Color.FromArgb("#343143"), StrokeShape=new RoundRectangle { CornerRadius=14 }, Padding=14, Content=p };
    }
}

public sealed class GroceryShoppingPage : ContentPage
{
    private GroceryList _list;
    private readonly IReadOnlyList<GroceryItem> _catalogue;
    private readonly GroceryPlanningService _service = new();
    private readonly VerticalStackLayout _content = new();

    public GroceryShoppingPage(IReadOnlyList<GroceryItem> catalogue, GroceryList list)
    {
        _catalogue = catalogue; _list = list;
        Title = "Shopping list"; BackgroundColor = Color.FromArgb("#101018");
        Content = new ScrollView { Content = _content };
    }

    protected override void OnAppearing() { base.OnAppearing(); Render(); }

    private void Render()
    {
        _content.Children.Clear(); _content.Padding=new Thickness(12,14,12,30); _content.Spacing=10;
        _content.Children.Add(GroceryPage.Heading(_list.Name, 26));
        _content.Children.Add(GroceryPage.Text($"Estimated {_list.Currency} {_list.EstimatedTotal:0.00} â€¢ {_list.EstimateSource}"));
        foreach (var group in _list.Items.GroupBy(i => _catalogue.Single(c => c.Id == i.GroceryItemId).Category))
        {
            _content.Children.Add(GroceryPage.Heading(group.Key.ToString(), 19));
            foreach (var item in group)
            {
                var requested = item.RequestedName;
                var detail = $"Requested: {requested}\nQuantity: {item.Quantity.Quantity} {item.Quantity.Unit}\nState: {item.State}\nPurchased/substitute: {item.PurchasedName ?? "None"}\n{item.Note}";
                _content.Children.Add(GroceryPage.Card(requested, detail, item.State.ToString().ToUpperInvariant()));
            }
        }
        _content.Children.Add(GroceryPage.Card("Offline-safe actions", "Check-off, undo, quantity, unavailable, substitute and skip actions queue until replay. Same-item Desktop/Mobile edits require conflict review.", "QUEUED / CONFLICT SAFE"));
    }
}
