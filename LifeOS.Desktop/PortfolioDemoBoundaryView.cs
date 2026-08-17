using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LifeOS.Desktop;

public sealed class PortfolioDemoBoundaryView : UserControl
{
    public PortfolioDemoBoundaryView(string moduleTitle, Action openSettings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleTitle);
        ArgumentNullException.ThrowIfNull(openSettings);

        Background = new SolidColorBrush(Color.FromRgb(12, 18, 32));
        Foreground = Brushes.White;

        Button settings = new()
        {
            Content = "Open Settings",
            Padding = new Thickness(18, 10, 18, 10),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromRgb(124, 92, 252)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0)
        };
        settings.Click += (_, _) => openSettings();

        Content = new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(32),
                MaxWidth = 760,
                HorizontalAlignment = HorizontalAlignment.Left,
                Children =
                {
                    new TextBlock
                    {
                        Text = moduleTitle,
                        FontSize = 30,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = "No ordinary-mode records are available in this module yet.",
                        Margin = new Thickness(0, 16, 0, 0),
                        FontSize = 18,
                        FontWeight = FontWeights.SemiBold,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = "This surface currently depends on fictional proof records. LifeOS keeps those records out of ordinary use. You can enable Portfolio Demo explicitly in Settings to inspect the proof workflow; doing so does not connect providers or make the records real.",
                        Margin = new Thickness(0, 10, 0, 22),
                        FontSize = 15,
                        Foreground = new SolidColorBrush(Color.FromRgb(190, 200, 220)),
                        TextWrapping = TextWrapping.Wrap
                    },
                    settings
                }
            }
        };
    }
}
