using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Grocery;

namespace LifeOS.Desktop;

public sealed class GroceryPlanningView : UserControl
{
    private readonly GroceryPlanningService _service = new();
    private readonly (IReadOnlyList<GroceryItem> Items, IReadOnlyList<GroceryList> Lists, IReadOnlyList<RecurringEssential> Essentials) _data = GroceryProofData.Build();
    private readonly ContentControl _content = new();

    public GroceryPlanningView()
    {
        Background = Brush("#0B1020");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        Content = Build();
        ShowOverview();
    }

    private UIElement Build()
    {
        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var nav = new StackPanel { Margin = new Thickness(18,22,18,18) };
        nav.Children.Add(Text("LIFEOS v13", 12, "#9AA9C7"));
        nav.Children.Add(Text("Grocery Planning", 28, "#FFFFFF", FontWeights.Bold));
        nav.Children.Add(Text("Household workspace • review-first", 13, "#9AA9C7"));
        nav.Children.Add(Button("Overview", ShowOverview));
        nav.Children.Add(Button("Active list", ShowActiveList));
        nav.Children.Add(Button("Recurring essentials", ShowEssentials));
        nav.Children.Add(Button("Templates", () => ShowMessage("Templates", "Reusable list templates clone into draft lists only.")));
        nav.Children.Add(Button("Catalogue", () => ShowMessage("Catalogue", "Items, categories, brands, alternatives, units and pack sizes.")));
        nav.Children.Add(Button("History", () => ShowMessage("History", "Manual changes and overrides remain auditable.")));

        var rail = new Border { Background = Brush("#11182B"), BorderBrush = Brush("#27324A"), BorderThickness = new Thickness(0,0,1,0), Child = nav };
        root.Children.Add(rail);
        Grid.SetColumn(_content, 1);
        _content.Margin = new Thickness(28,22,28,24);
        root.Children.Add(_content);
        return root;
    }

    private void ShowOverview()
    {
        var dashboard = _service.BuildDashboard(_data.Lists, _data.Essentials, new DateOnly(2026,7,22));
        var panel = Stack("Grocery Planning overview", "Active shopping, due essentials and estimates remain reviewable.");
        panel.Children.Add(Card("Active list", $"{dashboard.ActiveList!.Name}\nState: {dashboard.ActiveList.State}\nUnresolved required: {dashboard.UnresolvedRequired}\nEstimated total: {dashboard.ActiveList.Currency} {dashboard.ActiveList.EstimatedTotal:0.00}\nSource: {dashboard.ActiveList.EstimateSource}", "ACTIVE"));
        panel.Children.Add(Card("Due essentials", $"Due or due soon: {dashboard.DueEssentials.Count}\nMilk • overdue\nBread • due soon\nCandidates require explicit acceptance.", "REVIEW REQUIRED"));
        panel.Children.Add(Card("Boundary", dashboard.Boundary + "\nPlanned spend is not a trusted Money transaction.", "NO PURCHASE"));
        _content.Content = Scroll(panel);
    }

    private void ShowActiveList()
    {
        var list = _data.Lists[0];
        var panel = Stack(list.Name, $"Grouped items • estimated {list.Currency} {list.EstimatedTotal:0.00} • {list.EstimateSource}");
        foreach (var item in list.Items)
            panel.Children.Add(Card(item.RequestedName,
                $"Category: {_data.Items.Single(i => i.Id == item.GroceryItemId).Category}\nQuantity: {item.Quantity.Quantity} {item.Quantity.Unit} • Pack: {item.Quantity.PackSize ?? "Not set"}\nPriority: {item.Priority}\nRequired by: {item.RequiredBy?.ToString("dd MMM") ?? "Not set"}\nState: {item.State}\nPurchased/substitute: {item.PurchasedName ?? "None"}",
                item.State.ToString().ToUpperInvariant()));
        _content.Content = Scroll(panel);
    }

    private void ShowEssentials()
    {
        var panel = Stack("Recurring essentials review", "Due, deferred and skipped states create planning candidates only.");
        foreach (var item in _data.Essentials)
        {
            var grocery = _data.Items.Single(i => i.Id == item.GroceryItemId);
            panel.Children.Add(Card(grocery.Name,
                $"Cadence: every {item.CadenceDays} days\nNext due: {item.NextDue:dd MMM yyyy}\nUsual quantity: {item.UsualQuantity.Quantity} {item.UsualQuantity.Unit}\nPreferred store: {item.PreferredStore ?? "Not set"}\nAvailable actions: accept, defer, skip once",
                item.ReviewState.ToString().ToUpperInvariant()));
        }
        _content.Content = Scroll(panel);
    }

    private void ShowMessage(string title, string body) => _content.Content = Scroll(Stack(title, body));

    private static StackPanel Stack(string title, string subtitle)
    {
        var p = new StackPanel();
        p.Children.Add(Text(title, 31, "#FFFFFF", FontWeights.Bold));
        p.Children.Add(Text(subtitle, 14, "#A9B8D5"));
        return p;
    }

    private static Button Button(string text, Action action)
    {
        var b = new Button { Content = text, Margin = new Thickness(0,14,0,0), Padding = new Thickness(12), HorizontalContentAlignment = HorizontalAlignment.Left };
        b.Click += (_,_) => action();
        return b;
    }

    private static Border Card(string title, string body, string badge)
    {
        var panel = new StackPanel();
        panel.Children.Add(Text($"{title}   [{badge}]", 20, "#FFFFFF", FontWeights.SemiBold));
        panel.Children.Add(Text(body, 14, "#D7E0F2"));
        return new Border { Background = Brush("#152039"), BorderBrush = Brush("#2A3857"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(14), Padding = new Thickness(18), Margin = new Thickness(0,0,0,14), Child = panel };
    }

    private static TextBlock Text(string value, double size, string color, FontWeight? weight = null) =>
        new() { Text = value, FontSize = size, Foreground = Brush(color), FontWeight = weight ?? FontWeights.Normal, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,0,0,6) };

    private static ScrollViewer Scroll(UIElement child) => new() { Content = child, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    private static SolidColorBrush Brush(string hex) => (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
}
