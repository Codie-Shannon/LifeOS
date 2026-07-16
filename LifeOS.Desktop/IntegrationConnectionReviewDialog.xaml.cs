using System.Windows;
using LifeOS.Core.IntegrationControlCentre;

namespace LifeOS.Desktop;

public enum IntegrationConnectionReviewMode
{
    Reconnect,
    Revoke
}

public partial class IntegrationConnectionReviewDialog : Window
{
    public IntegrationConnectionReviewDialog(
        string accountName,
        IntegrationConnectionState currentState,
        IntegrationConnectionReviewMode mode)
    {
        InitializeComponent();

        AccountText.Text = $"Account: {accountName} · Current state: {Format(currentState)}";

        if (mode == IntegrationConnectionReviewMode.Reconnect)
        {
            EyebrowText.Text = "RECONNECT REVIEW";
            TitleText.Text = "Reconnect fictional account";
            SummaryText.Text = "Reconnect re-verifies identity, permission state and the first safe read.";
            DetailText.Text = "The operation is explicit, rerunnable and audited. It does not grant unrequested permissions, trust imported records or enable external writes.";
            ConfirmButton.Content = "Confirm reconnect";
        }
        else
        {
            EyebrowText.Text = "REVOKE REVIEW";
            TitleText.Text = "Revoke fictional consent";
            SummaryText.Text = "Revoke marks granted permissions as revoked and blocks refresh until reconnect.";
            DetailText.Text = "Accepted LifeOS records remain intact. Revoked, expired and missing permission states stay visible instead of being hidden behind a provider-wide green status.";
            ConfirmButton.Content = "Confirm revoke";
        }
    }

    private void ConfirmCheckBox_Changed(object sender, RoutedEventArgs e) =>
        ConfirmButton.IsEnabled = ConfirmCheckBox.IsChecked == true;

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static string Format(IntegrationConnectionState state) =>
        string.Concat(state.ToString().Select((character, index) =>
            index > 0 && char.IsUpper(character) ? $" {character}" : character.ToString()));
}
