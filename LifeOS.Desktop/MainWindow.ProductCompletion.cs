using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LifeOS.Core.ProductCompletion;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private void ShowProductLanePage(string route)
    {
        ProductLaneDefinition lane = ProductLaneCatalog.Get(route);
        SetHeader(lane.Title, lane.Subtitle);

        StackPanel root = new()
        {
            Margin = new Thickness(28, 22, 28, 40)
        };

        Border boundary = new()
        {
            Background = new SolidColorBrush(Color.FromRgb(30, 58, 138)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(96, 165, 250)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 18)
        };
        boundary.Child = new TextBlock
        {
            Text = $"{lane.ScreenshotGroup} review boundary\n{lane.Boundary}",
            Foreground = Brushes.White,
            FontSize = 15,
            TextWrapping = TextWrapping.Wrap
        };
        root.Children.Add(boundary);

        UniformGrid metrics = new()
        {
            Columns = Math.Min(4, lane.Metrics.Count),
            Margin = new Thickness(0, 0, 0, 18)
        };
        foreach (ProductLaneMetric metric in lane.Metrics)
        {
            Border card = new()
            {
                Background = new SolidColorBrush(Color.FromRgb(18, 24, 39)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 12, 0)
            };
            StackPanel text = new();
            text.Children.Add(new TextBlock { Text = metric.Label, Foreground = new SolidColorBrush(Color.FromRgb(147, 197, 253)), FontSize = 13 });
            text.Children.Add(new TextBlock { Text = metric.Value, Foreground = Brushes.White, FontWeight = FontWeights.SemiBold, FontSize = 27, Margin = new Thickness(0, 6, 0, 2) });
            text.Children.Add(new TextBlock { Text = metric.Detail, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), TextWrapping = TextWrapping.Wrap });
            card.Child = text;
            metrics.Children.Add(card);
        }
        root.Children.Add(metrics);

        TextBlock heading = new()
        {
            Text = "Reviewable work records",
            Foreground = Brushes.White,
            FontWeight = FontWeights.SemiBold,
            FontSize = 22,
            Margin = new Thickness(0, 4, 0, 12)
        };
        root.Children.Add(heading);

        foreach (ProductLaneAction action in lane.Actions)
        {
            Border item = new()
            {
                Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                BorderBrush = new SolidColorBrush(action.RequiresReview ? Color.FromRgb(139, 92, 246) : Color.FromRgb(51, 65, 85)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(18),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid content = new();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            StackPanel details = new();
            details.Children.Add(new TextBlock { Text = action.Title, Foreground = Brushes.White, FontSize = 17, FontWeight = FontWeights.SemiBold });
            details.Children.Add(new TextBlock { Text = action.Detail, Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), Margin = new Thickness(0, 5, 0, 0), TextWrapping = TextWrapping.Wrap });
            content.Children.Add(details);
            Border state = new()
            {
                Background = new SolidColorBrush(action.RequiresReview ? Color.FromRgb(76, 29, 149) : Color.FromRgb(30, 41, 59)),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12, 7, 12, 7),
                Child = new TextBlock { Text = action.State, Foreground = Brushes.White }
            };
            Grid.SetColumn(state, 1);
            content.Children.Add(state);
            item.Child = content;
            root.Children.Add(item);
        }

        MainContentControl.Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = root
        };
    }
}
