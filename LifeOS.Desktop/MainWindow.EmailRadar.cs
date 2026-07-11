using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core;
using LifeOS.Core.EmailRadar;
using LifeOS.Shared.EmailRadar;
using Microsoft.Win32;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private CommunicationImportPreview? _emailRadarPendingImport;

    private void ShowEmailRadarPage()
    {
        var profiles = EmailRadarStorage.LoadProfiles();
        var records = EmailRadarStorage.LoadRecords();
        var profile = profiles.FirstOrDefault(x => x.Status == EmailRadarProfileStatus.Active);
        var candidates = profile is null ? [] : EmailRadarService.FindCandidates(profile, records).ToList();
        var timeline = profile is null ? [] : EmailRadarService.BuildTimeline(profile, records, ["owner@example.invalid"]).ToList();
        var suggestion = profile is null ? new CommunicationSuggestion(CommunicationSuggestionKind.InsufficientEvidence, "Insufficient evidence", "Create an active profile first.") : EmailRadarService.Suggest(profile, records, ["owner@example.invalid"]);

        SetHeader("Email Radar", $"{ProductVersion.Display} • {ProductVersion.ReleaseName}");
        var root = new StackPanel();
        root.Children.Add(EmailRadarPanel("Local/imported evidence only", "No Gmail or Outlook account is connected. Candidate matches require confirmation. No LifeOS state changes automatically.", "#7C2D12"));

        var metrics = new WrapPanel { Margin = new Thickness(0, 18, 0, 0) };
        metrics.Children.Add(CreateDashboardCard("Profiles", profiles.Count.ToString(), "Local"));
        metrics.Children.Add(CreateDashboardCard("Imported evidence", records.Count.ToString(), "Inert text"));
        metrics.Children.Add(CreateDashboardCard("Candidates", candidates.Count.ToString(), "Explainable"));
        metrics.Children.Add(CreateDashboardCard("Confirmed timeline", timeline.Count.ToString(), "Confirmed only"));
        root.Children.Add(metrics);

        var buttons = new WrapPanel { Margin = new Thickness(0, 18, 0, 0) };
        buttons.Children.Add(EmailRadarButton("Create fictional profile", (_, _) => CreateEmailRadarProfile()));
        buttons.Children.Add(EmailRadarButton("Preview JSON/CSV import", (_, _) => PreviewEmailRadarImport()));
        buttons.Children.Add(EmailRadarButton("Confirm pending import", (_, _) => ConfirmEmailRadarImport()));
        root.Children.Add(buttons);

        if (profile is not null)
        {
            root.Children.Add(EmailRadarPanel("Active profile", $"{profile.Name} • {profile.RelatedLabel}\nAddresses: {string.Join(", ", profile.EmailAddresses)}\nSubject phrases: {string.Join(", ", profile.SubjectPhrases)}\nKeywords: {string.Join(", ", profile.Keywords)}\nExclude terms: {string.Join(", ", profile.ExcludeTerms)}\nFollow-up interval: {profile.FollowUpDays} days", "#1E3A5F"));
            root.Children.Add(EmailRadarPanel("Review-first suggestion", $"{suggestion.Label}\n{suggestion.Explanation}\nSuggested only. Requires review. No Follow-Up or Work Pipeline record was created.", "#3F3F46"));
        }

        foreach (var candidate in candidates.Take(8))
        {
            var panel = EmailRadarPanel($"{candidate.Record.ReviewState}: {candidate.Record.Subject}", $"{candidate.Record.SentAt:g} • {candidate.Record.Sender}\nScore: {candidate.Score}\n{string.Join("\n", candidate.Reasons.Select(x => "• " + x))}\nSource: {candidate.Record.Provenance}", candidate.Record.ReviewState == CommunicationReviewState.DuplicateSuspected ? "#7F1D1D" : "#1E293B");
            var actions = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
            actions.Children.Add(EmailRadarButton("Confirm match", (_, _) => ReviewEmailRadarMatch(profile!, candidate.Record, true)));
            actions.Children.Add(EmailRadarButton("Reject match", (_, _) => ReviewEmailRadarMatch(profile!, candidate.Record, false)));
            ((StackPanel)panel.Child).Children.Add(actions);
            root.Children.Add(panel);
        }

        if (timeline.Count > 0)
        {
            root.Children.Add(EmailRadarPanel("Confirmed communication timeline", string.Join("\n\n", timeline.Select(x => $"{x.SentAt:g} • {x.Direction}\n{x.Subject}\n{x.Snippet}\n{x.Provenance}")), "#14532D"));
        }
        MainContentControl.Content = root;
    }

    private static Border EmailRadarPanel(string title, string body, string background)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = title, Foreground = Brushes.White, FontSize = 17, FontWeight = FontWeights.SemiBold });
        stack.Children.Add(new TextBlock { Text = body, Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 7, 0, 0) });
        return new Border { Child = stack, Background = (Brush)new BrushConverter().ConvertFromString(background)!, BorderBrush = new SolidColorBrush(Color.FromRgb(51, 65, 85)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(16), Margin = new Thickness(0, 14, 0, 0) };
    }

    private static Button EmailRadarButton(string label, RoutedEventHandler handler)
    {
        var button = new Button { Content = label, Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 8, 8), Background = new SolidColorBrush(Color.FromRgb(30, 41, 59)), Foreground = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromRgb(71, 85, 105)) };
        button.Click += handler; return button;
    }

    private void CreateEmailRadarProfile()
    {
        var profile = new EmailRadarProfile { Name = "Northline Repairs", RelatedLabel = "Northline Repairs", People = ["Taylor Morgan"], EmailAddresses = ["taylor@northline-repairs.example.invalid"], SubjectPhrases = ["Repair update"], Keywords = ["proof", "invoice", "access", "status"], ExcludeTerms = ["newsletter"], FollowUpDays = 7, Notes = "Safe fictional profile created in Group 24." };
        EmailRadarStorage.UpsertProfile(profile); EmailRadarStorage.AppendAudit(new(DateTimeOffset.Now, "profile-created", profile.Name, profile.Id)); ShowEmailRadarPage();
    }

    private void PreviewEmailRadarImport()
    {
        var dialog = new OpenFileDialog { Filter = "Communication evidence (*.json;*.csv)|*.json;*.csv", Multiselect = false };
        if (dialog.ShowDialog() != true) return;
        try
        {
            var content = File.ReadAllText(dialog.FileName);
            _emailRadarPendingImport = Path.GetExtension(dialog.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase) ? CommunicationImportService.PreviewJson(content, dialog.FileName) : CommunicationImportService.PreviewCsv(content, dialog.FileName);
            MessageBox.Show($"Preview only. {_emailRadarPendingImport.Records.Count} valid inert records; {_emailRadarPendingImport.Errors.Count} malformed rows skipped. Nothing has been saved. Use Confirm pending import to continue.", "Email Radar import preview", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        { MessageBox.Show(ex.Message, "Email Radar import", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void ConfirmEmailRadarImport()
    {
        if (_emailRadarPendingImport is null || !_emailRadarPendingImport.RequiresConfirmation) { MessageBox.Show("Preview a JSON or CSV file first.", "Email Radar", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        if (!ConfirmRiskyAction("Confirm local communication import?", "The previewed records will be saved as untrusted inert text. They cannot mutate Work Pipeline, Follow-Ups, money, tasks, or other LifeOS state.")) return;
        EmailRadarStorage.ConfirmImport(_emailRadarPendingImport); _emailRadarPendingImport = null; ShowEmailRadarPage();
    }

    private void ReviewEmailRadarMatch(EmailRadarProfile profile, ImportedCommunicationRecord record, bool confirm)
    {
        try
        {
            if (confirm) EmailRadarService.Confirm(profile, record, "Confirmed manually in Email Radar."); else EmailRadarService.Reject(record, "Rejected manually in Email Radar.");
            var records = EmailRadarStorage.LoadRecords(); var index = records.FindIndex(x => x.Id == record.Id); if (index >= 0) records[index] = record; EmailRadarStorage.SaveRecords(records);
            EmailRadarStorage.AppendAudit(new(DateTimeOffset.Now, confirm ? "match-confirmed" : "match-rejected", record.Subject, profile.Id, record.Id)); ShowEmailRadarPage();
        }
        catch (InvalidOperationException ex) { MessageBox.Show(ex.Message, "Email Radar review", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
