using System.Windows;
using LifeOS.Core.IntegrationControlCentre;

namespace LifeOS.Desktop;

public partial class IntegrationDisconnectReviewDialog : Window
{
    public IntegrationDisconnectReviewDialog(string accountName)
    {
        InitializeComponent();
        AccountText.Text = $"Account: {accountName}. Select one explicit retention outcome before disconnecting.";
        SelectedChoice = DisconnectRetentionChoice.KeepAcceptedLifeOsRecords;
    }

    public DisconnectRetentionChoice? SelectedChoice { get; private set; }

    private void Retention_Changed(object sender, RoutedEventArgs e)
    {
        if (KeepAcceptedRadio.IsChecked == true)
        {
            SelectedChoice = DisconnectRetentionChoice.KeepAcceptedLifeOsRecords;
        }
        else if (ArchiveLinksRadio.IsChecked == true)
        {
            SelectedChoice = DisconnectRetentionChoice.ArchiveProviderLinks;
        }
        else if (RemoveCandidatesRadio.IsChecked == true)
        {
            SelectedChoice = DisconnectRetentionChoice.RemoveUnacceptedImportedCandidates;
        }

        UpdateConfirmState();
    }

    private void Confirmation_Changed(object sender, RoutedEventArgs e) => UpdateConfirmState();

    private void UpdateConfirmState()
    {
        if (DisconnectButton is not null)
        {
            DisconnectButton.IsEnabled = SelectedChoice is not null && ConfirmCheckBox.IsChecked == true;
        }
    }

    private void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
