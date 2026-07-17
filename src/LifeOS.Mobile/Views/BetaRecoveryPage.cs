using LifeOS.Mobile.Core.Beta;

namespace LifeOS.Mobile.Views;

public sealed class BetaRecoveryPage : ContentPage
{
    public BetaRecoveryPage(
        BetaClosureService service,
        IReadOnlyList<RecoveryCheckpoint> checkpoints)
    {
        Title = "Recovery";
        BackgroundColor = BetaVisuals.Background;

        var content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 14,
            Children =
            {
                new Label
                {
                    Text = "Recovery checkpoints",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                },
                new Label
                {
                    Text =
                        "Explicit restore only • current state is preserved until review",
                    TextColor = BetaVisuals.Secondary
                }
            }
        };

        foreach (var checkpoint in checkpoints)
        {
            var status = new Label
            {
                Text = $"State: {checkpoint.State}",
                TextColor = BetaVisuals.Warning
            };

            var captured = checkpoint;
            var restore = BetaVisuals.Action(
                "Restore checkpoint",
                Color.FromArgb("#2C3140"));

            restore.Clicked += (_, _) =>
            {
                var result = service.RestoreCheckpoint(
                    captured,
                    DateTimeOffset.UtcNow);

                status.Text = $"State: {result.State}";
                status.TextColor = BetaVisuals.Success;
            };

            content.Children.Add(
                BetaVisuals.CardView(
                    checkpoint.Label,
                    $"{checkpoint.CreatedUtc.ToLocalTime():ddd d MMM, h:mm tt} • " +
                    $"Pending {checkpoint.PendingItems} • " +
                    $"Hash {checkpoint.IntegrityHash[..12]}",
                    checkpoint.State.ToString()));

            content.Children.Add(status);
            content.Children.Add(restore);
        }

        content.Children.Add(
            new Label
            {
                Text =
                    "No destructive overwrite, automatic rollback or hidden state replacement.",
                TextColor = BetaVisuals.Secondary
            });

        Content = new ScrollView
        {
            Content = content
        };
    }
}
