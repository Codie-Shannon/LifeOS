using System.Windows;
using System.Windows.Controls;

namespace LifeOS.Desktop;

public partial class Group50TeamsView : UserControl
{
    public event EventHandler? BackRequested;

    public Group50TeamsView() => InitializeComponent();

    private void Back_Click(object sender, RoutedEventArgs e) =>
        BackRequested?.Invoke(this, EventArgs.Empty);
}
