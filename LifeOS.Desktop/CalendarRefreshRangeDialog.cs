using System.Windows;
using System.Windows.Controls;
using LifeOS.Core.IntegrationConnectors.GoogleCalendar;

namespace LifeOS.Desktop;

public sealed class CalendarRefreshRangeDialog : Window
{
    private readonly DatePicker _from = new() { SelectedDate = DateTime.Today.AddDays(-7), Margin = new Thickness(0, 4, 0, 12) };
    private readonly DatePicker _to = new() { SelectedDate = DateTime.Today.AddDays(14), Margin = new Thickness(0, 4, 0, 12) };

    public CalendarRefreshRange SelectedRange => new(
        new DateTimeOffset((_from.SelectedDate ?? DateTime.Today).Date),
        new DateTimeOffset((_to.SelectedDate ?? DateTime.Today.AddDays(1)).Date.AddDays(1)));

    public CalendarRefreshRangeDialog()
    {
        Title = "Manual Google Calendar refresh";
        Width = 430;
        Height = 310;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        var root = new StackPanel { Margin = new Thickness(24) };
        root.Children.Add(new TextBlock { Text = "Choose a bounded date range", FontSize = 20, FontWeight = FontWeights.Bold });
        root.Children.Add(new TextBlock { Text = "Manual refresh only. Maximum 31 days. Events become untrusted previews.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 16) });
        root.Children.Add(new TextBlock { Text = "From" });
        root.Children.Add(_from);
        root.Children.Add(new TextBlock { Text = "Through" });
        root.Children.Add(_to);
        var actions = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
        var cancel = new Button { Content = "Cancel", MinWidth = 90, Margin = new Thickness(4) };
        cancel.Click += (_, _) => DialogResult = false;
        var refresh = new Button { Content = "Review refresh", MinWidth = 120, Margin = new Thickness(4) };
        refresh.Click += (_, _) =>
        {
            try { SelectedRange.Validate(DateTimeOffset.Now); DialogResult = true; }
            catch (ArgumentException ex) { MessageBox.Show(ex.Message, "Calendar range", MessageBoxButton.OK, MessageBoxImage.Warning); }
        };
        actions.Children.Add(cancel);
        actions.Children.Add(refresh);
        root.Children.Add(actions);
        Content = root;
    }
}
