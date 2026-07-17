using LifeOS.Mobile.Core.Beta;

namespace LifeOS.Mobile.Views;

public sealed class BetaAccessibilityPage : ContentPage
{
    public BetaAccessibilityPage(AccessibilityAudit audit)
    {
        Title = "Accessibility";
        BackgroundColor = BetaVisuals.Background;

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 14,
                Children =
                {
                    new Label
                    {
                        Text = "Accessibility audit",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    BetaVisuals.CardView(
                        "Touch targets",
                        $"Minimum {audit.MinimumTouchTargetDp}dp",
                        "Passed"),
                    BetaVisuals.CardView(
                        "Screen-reader labels",
                        audit.ScreenReaderLabels
                            ? "Supported"
                            : "Needs review",
                        audit.ScreenReaderLabels ? "Passed" : "Review"),
                    BetaVisuals.CardView(
                        "Large text",
                        audit.LargeTextSupported
                            ? "Supported"
                            : "Needs review",
                        audit.LargeTextSupported ? "Passed" : "Review"),
                    BetaVisuals.CardView(
                        "Sensitive previews",
                        audit.SensitivePreviewsHidden
                            ? "Hidden"
                            : "Visible",
                        audit.SensitivePreviewsHidden ? "Passed" : "Review"),
                    BetaVisuals.CardView(
                        "App lock",
                        audit.AppLockRequired
                            ? "Required"
                            : "Optional",
                        audit.AppLockRequired ? "Passed" : "Review"),
                    new Label
                    {
                        Text =
                            "Reading order is top to bottom. Controls retain minimum touch targets.",
                        TextColor = BetaVisuals.Secondary
                    }
                }
            }
        };
    }
}
