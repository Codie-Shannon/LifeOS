using System.Windows;
using System.Windows.Controls;

namespace LifeOS.Desktop;

public partial class Group51GoogleWorkspaceView : UserControl
{
    public event EventHandler? BackRequested;

    public Group51GoogleWorkspaceView() => InitializeComponent();

    private void Back_Click(object sender, RoutedEventArgs e) =>
        BackRequested?.Invoke(this, EventArgs.Empty);
}
