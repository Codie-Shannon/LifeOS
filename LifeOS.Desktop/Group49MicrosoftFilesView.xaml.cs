using System.Windows;
using System.Windows.Controls;
namespace LifeOS.Desktop;
public partial class Group49MicrosoftFilesView : UserControl
{
 public event EventHandler? BackRequested;
 public Group49MicrosoftFilesView() => InitializeComponent();
 private void Back_Click(object sender, RoutedEventArgs e) => BackRequested?.Invoke(this, EventArgs.Empty);
}
