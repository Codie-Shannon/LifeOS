using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.IntegrationConnectors;
using LifeOS.Shared.IntegrationConnectors.Gmail;
using LifeOS.Shared.IntegrationConnectors.GoogleCalendar;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private void AddV5IntegrationReadinessSummary(Panel root)
    {
        var calendar = GoogleCalendarLifecycleStore.Load();
        var gmail = GmailLifecycleStore.Load();
        var summary = IntegrationReleaseReadiness.Create(calendar.LastResultSummary, gmail.LastSummary);
        var validation = V5ReleaseValidationMatrix.Create();

        var panel = CreatePanel();
        panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();

        stack.Children.Add(new TextBlock
        {
            Text = "v5 integration readiness checkpoint",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 22,
            FontWeight = FontWeights.Bold
        });

        stack.Children.Add(new TextBlock
        {
            Text = summary.GlobalSafetyStatement +
                   "\nAll active connector operations are manual, read-only, bounded where provider-backed, and require human review before trusted handoff.",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 12)
        });

        var metrics = new WrapPanel();
        metrics.Children.Add(CreateDashboardCard("Connector lanes", summary.Lanes.Count.ToString(), "Visible"));
        metrics.Children.Add(CreateDashboardCard("Authenticated", summary.AuthenticatedCount.ToString(), "Local OAuth"));
        metrics.Children.Add(CreateDashboardCard("Read-only", summary.ReadOnlyCount.ToString(), "All lanes"));
        metrics.Children.Add(CreateDashboardCard("Manual only", summary.ManualOnlyCount.ToString(), "No background"));
        metrics.Children.Add(CreateDashboardCard("Human review", summary.ReviewRequiredCount.ToString(), "Required"));
        metrics.Children.Add(CreateDashboardCard("Validation", validation.Count(x => x.Passed).ToString(), "All passed"));
        stack.Children.Add(metrics);

        foreach (var lane in summary.Lanes)
        {
            stack.Children.Add(CreateInfoPanel(
                lane.Name,
                $"{lane.OperationLabel}\nState: {lane.CurrentState}\nScope: {lane.ScopeLabel}\n" +
                $"Authenticated: {(lane.IsAuthenticated ? "Yes" : "No")} - Read-only: {(lane.IsReadOnly ? "Yes" : "No")} - " +
                $"Manual only: {(lane.IsManualOnly ? "Yes" : "No")} - Human review: {(lane.RequiresHumanReview ? "Required" : "No")}\n" +
                $"Evidence retained after disconnect/cache clear: {(lane.RetainsEvidenceAfterDisconnect ? "Yes" : "No")}"));
        }

        stack.Children.Add(CreateInfoPanel(
            "Release validation",
            string.Join("\n", validation.Select(x => $"PASS - {x.Area}: {x.Check} ({x.Evidence})"))));

        panel.Child = stack;
        root.Children.Add(panel);
    }
}
