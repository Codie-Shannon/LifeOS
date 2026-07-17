using LifeOS.Mobile.Core.Beta;

namespace LifeOS.Mobile.Views;

public sealed class BetaClosurePage : ContentPage
{
    private readonly BetaClosureService _service = new();
    private readonly VerticalStackLayout _content = new();

    public BetaClosurePage()
    {
        Title = "Beta closure";
        BackgroundColor = BetaVisuals.Background;

        Content = new ScrollView
        {
            Content = _content
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Render();
    }

    private void Render()
    {
        var snapshot = _service.BuildSnapshot(DateTimeOffset.Now);

        _content.Children.Clear();
        _content.Padding = new Thickness(18, 14, 18, 30);
        _content.Spacing = 13;

        _content.Children.Add(
            new Label
            {
                Text = "Full Mobile beta closure",
                FontSize = 32,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            });

        _content.Children.Add(
            new Label
            {
                Text =
                    "Sync, recovery, accessibility, performance and release readiness",
                FontSize = 15,
                TextColor = BetaVisuals.Secondary
            });

        _content.Children.Add(
            BetaVisuals.CardView(
                "Sync health",
                $"Last successful sync " +
                $"{snapshot.Sync.LastSuccessfulSyncUtc.ToLocalTime():h:mm tt} • " +
                $"Pending queue {snapshot.Sync.PendingQueue}",
                snapshot.Sync.Health.ToString()));

        _content.Children.Add(
            BetaVisuals.CardView(
                "Provider writes",
                snapshot.Sync.ProviderWritesEnabled
                    ? "Enabled"
                    : "Disabled",
                snapshot.Sync.ProviderWritesEnabled ? "Review" : "Safe"));

        var sync = BetaVisuals.Action("Open sync and offline proof");
        sync.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new BetaSyncPage(_service, snapshot.Sync));

        var recovery = BetaVisuals.Action(
            "Open recovery checkpoints",
            Color.FromArgb("#2C3140"));

        recovery.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new BetaRecoveryPage(_service, snapshot.Recovery));

        var access = BetaVisuals.Action(
            "Open accessibility audit",
            Color.FromArgb("#2C3140"));

        access.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new BetaAccessibilityPage(snapshot.Accessibility));

        var performance = BetaVisuals.Action(
            "Open performance budget",
            Color.FromArgb("#2C3140"));

        performance.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new BetaPerformancePage(
                    _service,
                    snapshot.Performance));

        var release = BetaVisuals.Action(
            "Open release checklist",
            Color.FromArgb("#2C3140"));

        release.Clicked += async (_, _) =>
            await Navigation.PushAsync(
                new BetaReleaseChecklistPage(snapshot.ReleaseChecks));

        _content.Children.Add(sync);
        _content.Children.Add(recovery);
        _content.Children.Add(access);
        _content.Children.Add(performance);
        _content.Children.Add(release);

        _content.Children.Add(
            BetaVisuals.CardView(
                "Release boundary",
                "No silent provider writes, no destructive recovery and no beta claim without complete proof."));
    }
}
