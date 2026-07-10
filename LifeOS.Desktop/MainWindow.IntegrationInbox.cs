using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.IntegrationConnectors;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Core.IntegrationConnectors.GoogleCalendar;
using LifeOS.Shared.IntegrationConnectors.GoogleCalendar;
using LifeOS.Shared.IntegrationInbox;
using Microsoft.Win32;
using System.Net.Http;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private void ShowIntegrationInboxPage()
    {
        _integrationInboxItems = IntegrationInboxStorage.Load();
        var summary = IntegrationInboxCalculator.Calculate(_integrationInboxItems);
        var readiness = IntegrationReadinessMatrix.Create();

        SetHeader("Integration Inbox", $"Integration Inbox - v5.0-alpha - {summary.NeedsReview} review - {summary.DuplicateSuspected} duplicate");

        var root = new StackPanel();

        root.Children.Add(CreateInfoPanel(
            "Connector foundation",
            "v5.0-alpha imports local CSV, JSON, and ICS files and exposes one narrow Google Calendar read-only connector. Every preview keeps source provenance, trust, duplicate state, suggested target, confirmation state, and audit history. No imported record can update LifeOS automatically."));

        var metricsPanel = new WrapPanel { Margin = new Thickness(0, 22, 0, 0) };
        metricsPanel.Children.Add(CreateDashboardCard("Engine", "Active", "v5.0-alpha"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.NeedsReview.ToString(), "Review"));
        metricsPanel.Children.Add(CreateDashboardCard("Suspected", summary.DuplicateSuspected.ToString(), "Duplicate"));
        metricsPanel.Children.Add(CreateDashboardCard("Manual gate", summary.Untrusted.ToString(), "Untrusted"));
        metricsPanel.Children.Add(CreateDashboardCard("Reviewed", summary.Accepted.ToString(), "Accepted"));
        metricsPanel.Children.Add(CreateDashboardCard("Handoffs", summary.Linked.ToString(), "Linked"));
        metricsPanel.Children.Add(CreateDashboardCard("Preview money", FormatMoney(summary.PreviewMoney), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Contracts ready", readiness.Count(x => x.ContractReady).ToString(), "v5"));
        root.Children.Add(metricsPanel);

        root.Children.Add(CreateInfoPanel(
            "v5.0-alpha connector rule",
            "- External records enter as read-only previews, never trusted items.\n" +
            "- Source evidence and provenance stay attached.\n" +
            "- Duplicate suspicion blocks acceptance.\n" +
            "- Accept, reject, defer, and link are deliberate review states.\n" +
            "- Accepted previews still require a separate manual handoff.\n" +
            "- Expected or imported money remains excluded from safe money.\n" +
            "- Google Calendar uses the calendar.readonly scope, manual bounded refresh, and explicit disconnect.\n" +
            "- No calendar write, background polling, bank feed, inbox scan, or automatic mutation is active."));

        AddGoogleCalendarConnectorPanel(root);

        var controls = CreatePanel();
        controls.Margin = new Thickness(0, 16, 0, 0);
        var controlsStack = new StackPanel();
        controlsStack.Children.Add(new TextBlock
        {
            Text = "Local controls",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });
        controlsStack.Children.Add(new TextBlock
        {
            Text = "Reset restores fictional previews. It does not connect to or import from any external service.",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 12)
        });
        var import = CreateEvidenceActionButton("Import CSV/JSON/ICS preview file", ImportIntegrationInboxPreviewFile);
        controlsStack.Children.Add(import);
        var reset = CreateEvidenceActionButton("Reset local integration previews", ResetIntegrationInboxDemo);
        controlsStack.Children.Add(reset);
        controls.Child = controlsStack;
        root.Children.Add(controls);

        AddIntegrationPreviewSection(root, "Review queue", "New, uncertain, and duplicate-suspected previews requiring a human decision.",
            _integrationInboxItems.Where(x => x.Status is IntegrationPreviewStatus.New or IntegrationPreviewStatus.NeedsReview or IntegrationPreviewStatus.DuplicateSuspected));

        AddIntegrationPreviewSection(root, "Deferred / rejected", "Contained records that must not create pressure or mutate target modules.",
            _integrationInboxItems.Where(x => x.Status is IntegrationPreviewStatus.Deferred or IntegrationPreviewStatus.Rejected));

        AddIntegrationPreviewSection(root, "Accepted / linked handoffs", "Reviewed previews that are ready for a separate explicit target-module handoff.",
            _integrationInboxItems.Where(x => x.Status is IntegrationPreviewStatus.Accepted or IntegrationPreviewStatus.Linked));

        AddIntegrationSourceLanes(root, summary);
        AddIntegrationImportAuditSection(root);
        AddIntegrationReadinessMatrix(root, readiness);

        root.Children.Add(CreateInfoPanel(
            "Integration-to-module handoff contract",
            "Accepted previews may later be handed to Item State, Payment Calendar, Work Pipeline, Evidence Vault, Bills / Payments, Follow-Ups, or Search / Knowledge. The target module remains authoritative and must validate its own rules."));

        root.Children.Add(CreateInfoPanel(
            "v5.0-alpha audit and provenance contract",
            "Every preview keeps a local ID, source type, source label, external reference, duplicate key, source evidence, review note, trust state, status, suggested target, timestamps, and optional link reference."));

        root.Children.Add(CreateInfoPanel(
            "v5.0-alpha boundary",
            "Google Calendar is the only live provider boundary in Group 22. It is read-only, manually refreshed, date-bounded, and review-first. No Gmail, Outlook, Microsoft Calendar, Xero, SharePoint, Drive, OCR provider, banking provider, background polling, automatic item creation, automatic updates, or AI actions are active."));

        root.Children.Add(CreateInfoPanel(
            "Active connector state",
            "Local CSV, JSON, and ICS connectors remain active. Google Calendar can be configured locally and connected with the minimum calendar.readonly scope. Imports require confirmation, remain read-only previews, expose duplicate suspicion, preserve audit history, and require deliberate human review before any later handoff."));

        root.Children.Add(CreateInfoPanel(
            "Local Integration Inbox file",
            IntegrationInboxStorage.FilePath));

        MainContentControl.Content = root;
    }

    private void AddIntegrationPreviewSection(
        Panel root,
        string title,
        string description,
        IEnumerable<IntegrationPreviewItem> items)
    {
        var section = CreatePanel();
        section.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();

        stack.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        stack.Children.Add(new TextBlock
        {
            Text = description,
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            Margin = new Thickness(0, 6, 0, 12),
            TextWrapping = TextWrapping.Wrap
        });

        var list = items.ToList();
        if (list.Count == 0)
        {
            stack.Children.Add(new TextBlock
            {
                Text = "No integration previews in this lane.",
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184))
            });
        }
        else
        {
            foreach (var item in list)
            {
                stack.Children.Add(CreateIntegrationPreviewCard(item));
            }
        }

        section.Child = stack;
        root.Children.Add(section);
    }

    private Border CreateIntegrationPreviewCard(IntegrationPreviewItem item)
    {
        var card = CreatePanel();
        card.Margin = new Thickness(0, 0, 0, 12);
        var stack = new StackPanel();

        stack.Children.Add(new TextBlock
        {
            Text = item.Title,
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap
        });

        var amount = item.Amount.HasValue ? $" - Amount: {FormatMoney(item.Amount.Value)}" : "";
        stack.Children.Add(new TextBlock
        {
            Text = $"{item.SourceKind} - {item.SourceLabel} - {item.Status} - {item.TrustState}{amount}",
            Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            Margin = new Thickness(0, 6, 0, 0),
            TextWrapping = TextWrapping.Wrap
        });

        stack.Children.Add(new TextBlock
        {
            Text =
                $"{item.Summary}\nSuggested target: {item.SuggestedTarget}\nSuggested action: {item.SuggestedAction}\n" +
                $"Connector: {item.ConnectorKey}\nProvider account: {item.ProviderAccountLabel}\nProvider calendar: {item.ProviderContainerId}\n" +
                $"External reference: {item.ExternalReference}\nEvent end: {item.EndsAt:g}\nFetched: {item.FetchedAt:g}\nDuplicate key: {item.DuplicateKey}\nSource evidence: {item.SourceEvidence}\n" +
                $"Review note: {item.ReviewNote}\nRead-only preview: {(item.IsReadOnlyPreview ? "Yes" : "No")} - Human review: {(item.RequiresHumanReview ? "Required" : "Complete")}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            Margin = new Thickness(0, 6, 0, 10),
            TextWrapping = TextWrapping.Wrap
        });

        var actions = new WrapPanel();
        actions.Children.Add(CreateEvidenceActionButton("Accept", () => ReviewIntegrationPreview(item.Id, IntegrationPreviewStatus.Accepted)));
        actions.Children.Add(CreateEvidenceActionButton("Defer", () => ReviewIntegrationPreview(item.Id, IntegrationPreviewStatus.Deferred)));
        actions.Children.Add(CreateEvidenceActionButton("Reject", () => ReviewIntegrationPreview(item.Id, IntegrationPreviewStatus.Rejected)));
        actions.Children.Add(CreateEvidenceActionButton("Link demo", () => ReviewIntegrationPreview(item.Id, IntegrationPreviewStatus.Linked)));
        stack.Children.Add(actions);

        card.Child = stack;
        return card;
    }

    private void AddIntegrationSourceLanes(Panel root, IntegrationInboxSummary summary)
    {
        var sourcePanel = CreatePanel();
        sourcePanel.Margin = new Thickness(0, 16, 0, 0);
        var sourceStack = new StackPanel();
        sourceStack.Children.Add(new TextBlock
        {
            Text = "Source lanes",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });
        sourceStack.Children.Add(new TextBlock
        {
            Text =
                $"Email {summary.EmailPreviews} - Calendar {summary.CalendarPreviews} - Accounting {summary.AccountingPreviews} - " +
                $"Files {summary.FilePreviews} - OCR {summary.OcrPreviews} - Banking preview {summary.BankingPreviews}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            Margin = new Thickness(0, 8, 0, 0),
            TextWrapping = TextWrapping.Wrap
        });
        sourcePanel.Child = sourceStack;
        root.Children.Add(sourcePanel);
    }

    private void AddIntegrationReadinessMatrix(Panel root, IReadOnlyList<IntegrationReadinessItem> readiness)
    {
        var readinessPanel = CreatePanel();
        readinessPanel.Margin = new Thickness(0, 16, 0, 0);
        var readinessStack = new StackPanel();
        readinessStack.Children.Add(new TextBlock
        {
            Text = "v5 connector readiness matrix",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        foreach (var item in readiness)
        {
            readinessStack.Children.Add(CreateInfoPanel(
                item.Connector,
                $"{item.Capability} - {item.CurrentState}\nBefore v5: {item.RequiredBeforeV5}\nContract ready: {(item.ContractReady ? "Yes" : "No")} - Live connection: {(item.LiveConnectionActive ? "Yes" : "No")}"));
        }

        readinessPanel.Child = readinessStack;
        root.Children.Add(readinessPanel);
    }

    private void AddIntegrationImportAuditSection(Panel root)
    {
        var audits = IntegrationImportAuditStorage.Load().Take(5).ToList();
        var auditPanel = CreatePanel();
        auditPanel.Margin = new Thickness(0, 16, 0, 0);
        var auditStack = new StackPanel();
        auditStack.Children.Add(new TextBlock
        {
            Text = "Manual import audit trail",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });

        auditStack.Children.Add(new TextBlock
        {
            Text = $"Local audit file: {IntegrationImportAuditStorage.FilePath}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            Margin = new Thickness(0, 6, 0, 12),
            TextWrapping = TextWrapping.Wrap
        });

        if (audits.Count == 0)
        {
            auditStack.Children.Add(new TextBlock
            {
                Text = "No manual import audit entries yet.",
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184))
            });
        }
        else
        {
            foreach (var audit in audits)
            {
                auditStack.Children.Add(CreateInfoPanel(
                    $"{audit.ConnectorKey} - {audit.SourceFileName}",
                    $"Imported: {audit.ImportedCount} - Duplicates suspected: {audit.DuplicateSuspectedCount} - Skipped rows: {audit.SkippedRowCount} - Rows seen: {audit.TotalRowsSeen}\n" +
                    $"File kind: {audit.FileKind} - Imported at: {audit.ImportedAt:g}\n" +
                    $"Source file: {audit.SourceFilePath}\nSHA-256: {audit.FileSha256}\n" +
                    $"Preview IDs: {string.Join(", ", audit.PreviewIds.Take(3))}" +
                    (audit.Errors.Count > 0 ? $"\nFirst error: Row {audit.Errors[0].RowNumber}: {audit.Errors[0].Message}" : "")));
            }
        }

        auditPanel.Child = auditStack;
        root.Children.Add(auditPanel);
    }

    private void ReviewIntegrationPreview(Guid id, IntegrationPreviewStatus target)
    {
        var items = IntegrationInboxStorage.Load();
        var item = items.FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            return;
        }

        try
        {
            switch (target)
            {
                case IntegrationPreviewStatus.Accepted:
                    IntegrationInboxReviewEngine.Accept(item, "Accepted through local v5.0-alpha review control.");
                    break;
                case IntegrationPreviewStatus.Deferred:
                    IntegrationInboxReviewEngine.Defer(item, "Deferred through local v5.0-alpha review control.");
                    break;
                case IntegrationPreviewStatus.Rejected:
                    IntegrationInboxReviewEngine.Reject(item, "Rejected through local v5.0-alpha review control.");
                    break;
                case IntegrationPreviewStatus.Linked:
                    if (item.Status != IntegrationPreviewStatus.Accepted)
                    {
                        MessageBox.Show("Accept the preview before linking it.", "LifeOS", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    IntegrationInboxReviewEngine.Link(item, $"demo-link-{item.Id:N}");
                    break;
            }

            IntegrationInboxStorage.Save(items);
            ShowIntegrationInboxPage();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "LifeOS review gate", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AddGoogleCalendarConnectorPanel(Panel root)
    {
        GoogleCalendarConfigurationStore.EnsureTemplate();
        var connected = GoogleOAuthTokenStore.Exists;
        var panel = CreatePanel();
        panel.Margin = new Thickness(0, 16, 0, 0);
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = "Read-only Google Calendar connector",
            Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });
        stack.Children.Add(new TextBlock
        {
            Text = $"Status: {(connected ? "Connected locally" : "Disconnected / configuration required")}\n" +
                   "Scope: calendar.readonly only. Refresh: manual only. Maximum range: 31 days.\n" +
                   "Fetched events enter as untrusted Integration Inbox previews. No external event or LifeOS module is changed automatically.\n" +
                   $"Local configuration: {GoogleCalendarConfigurationStore.FilePath}",
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            Margin = new Thickness(0, 8, 0, 12),
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(CreateEvidenceActionButton("Open local connector configuration", OpenGoogleCalendarConfiguration));
        stack.Children.Add(CreateEvidenceActionButton("Connect read-only Google Calendar", ConnectGoogleCalendar));
        stack.Children.Add(CreateEvidenceActionButton("Manual bounded calendar refresh", RefreshGoogleCalendar));
        stack.Children.Add(CreateEvidenceActionButton("Disconnect and delete local token cache", DisconnectGoogleCalendar));
        panel.Child = stack;
        root.Children.Add(panel);
    }

    private void OpenGoogleCalendarConfiguration()
    {
        GoogleCalendarConfigurationStore.EnsureTemplate();
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(GoogleCalendarConfigurationStore.FilePath) { UseShellExecute = true });
    }

    private async void ConnectGoogleCalendar()
    {
        try
        {
            var configuration = GoogleCalendarConfigurationStore.Load();
            configuration.Validate();
            if (!ConfirmRiskyAction("Connect Google Calendar read-only?", "LifeOS will open Google authorization and request only calendar.readonly. It will not edit calendar events.")) return;
            using var httpClient = new HttpClient();
            var client = new GoogleOAuthPkceClient(httpClient, configuration);
            await client.ConnectAsync();
            MessageBox.Show("Google Calendar connected locally with read-only scope. Refresh remains manual.", "LifeOS", MessageBoxButton.OK, MessageBoxImage.Information);
            ShowIntegrationInboxPage();
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or TimeoutException or HttpRequestException or PlatformNotSupportedException)
        {
            MessageBox.Show(ex.Message, "Google Calendar connection", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void RefreshGoogleCalendar()
    {
        var rangeDialog = new CalendarRefreshRangeDialog { Owner = this };
        if (rangeDialog.ShowDialog() != true) return;

        try
        {
            var range = rangeDialog.SelectedRange;
            range.Validate(DateTimeOffset.Now);
            if (!ConfirmRiskyAction("Refresh Google Calendar previews?", $"Read events from {range.From:d} through {range.To:d}? Results remain untrusted previews and no calendar or LifeOS module will be changed.")) return;

            var configuration = GoogleCalendarConfigurationStore.Load();
            configuration.Validate();
            using var httpClient = new HttpClient();
            var oauth = new GoogleOAuthPkceClient(httpClient, configuration);
            var provider = new GoogleCalendarApiProvider(httpClient, configuration, () => oauth.GetAccessTokenAsync());
            var service = new GoogleCalendarRefreshService(provider);
            var result = await service.RefreshAsync(range);
            var items = IntegrationInboxStorage.Load();
            var duplicateCount = IntegrationImportDuplicateDetector.MarkDuplicateSuspicions(items, result.Previews);
            items.AddRange(result.Previews);
            IntegrationInboxStorage.Save(items);

            var auditResult = new ManualIntegrationImportResult("google-calendar", result.Previews, result.Errors.Select((error, index) => new ManualIntegrationImportError(index + 1, error)).ToArray());
            IntegrationImportAuditStorage.Append(IntegrationImportAudit.CreateManualImportEntry(
                auditResult,
                $"google-calendar://{configuration.CalendarId}/{range.From:yyyy-MM-dd}/{range.To:yyyy-MM-dd}",
                "API-READ-ONLY",
                string.Join("\n", result.Previews.Select(x => x.DuplicateKey))));

            MessageBox.Show($"Received {result.Previews.Count} read-only calendar preview(s).\nDuplicate-suspected: {duplicateCount}.\nSkipped malformed events: {result.Errors.Count}.", "Google Calendar manual refresh", MessageBoxButton.OK, MessageBoxImage.Information);
            ShowIntegrationInboxPage();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or HttpRequestException or IOException or PlatformNotSupportedException)
        {
            MessageBox.Show(ex.Message, "Google Calendar refresh", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void DisconnectGoogleCalendar()
    {
        if (!ConfirmRiskyAction("Disconnect Google Calendar?", "This deletes the local encrypted token cache. Existing Integration Inbox previews remain for provenance and review.")) return;
        GoogleCalendarConnectorCache.Disconnect();
        MessageBox.Show("Google Calendar disconnected. Local token cache deleted. Existing previews were retained.", "LifeOS", MessageBoxButton.OK, MessageBoxImage.Information);
        ShowIntegrationInboxPage();
    }

    private void ResetIntegrationInboxDemo()
    {
        if (!ConfirmRiskyAction(
                "Reset local integration previews?",
                "This restores fictional local preview records only. It does not contact external services."))
        {
            return;
        }

        _integrationInboxItems = IntegrationInboxStorage.Reset();
        ShowIntegrationInboxPage();
    }

    private void ImportIntegrationInboxPreviewFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import LifeOS preview file",
            Filter = "Preview files (*.csv;*.json;*.ics)|*.csv;*.json;*.ics|CSV files (*.csv)|*.csv|JSON files (*.json)|*.json|ICS calendar files (*.ics)|*.ics",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
            var content = File.ReadAllText(dialog.FileName);
            var result = extension switch
            {
                ".csv" => ManualIntegrationImportConnector.ImportCsv(content, dialog.FileName),
                ".json" => ManualIntegrationImportConnector.ImportJson(content, dialog.FileName),
                ".ics" => IcsCalendarImportConnector.Import(content, dialog.FileName),
                _ => throw new InvalidOperationException("Choose a .csv, .json, or .ics file.")
            };

            var items = IntegrationInboxStorage.Load();
            var duplicateCount = IntegrationImportDuplicateDetector.MarkDuplicateSuspicions(items, result.Previews);
            var summary = ManualIntegrationImportPreviewSummary.Create(result, dialog.FileName, extension);
            if (!ConfirmManualImportPreview(summary))
            {
                return;
            }

            items.AddRange(result.Previews);
            IntegrationInboxStorage.Save(items);
            IntegrationImportAuditStorage.Append(IntegrationImportAudit.CreateManualImportEntry(
                result,
                dialog.FileName,
                extension,
                content));

            var message = $"Imported {result.Previews.Count} preview record(s) through {result.ConnectorKey}.";
            if (duplicateCount > 0)
            {
                message += $"\n\nMarked {duplicateCount} imported preview(s) as duplicate-suspected.";
            }

            if (result.Errors.Count > 0)
            {
                message += $"\n\nSkipped {result.Errors.Count} row(s):\n" +
                    string.Join("\n", result.Errors.Take(5).Select(error => $"Row {error.RowNumber}: {error.Message}"));
            }

            MessageBox.Show(message, "LifeOS manual import", MessageBoxButton.OK, MessageBoxImage.Information);
            ShowIntegrationInboxPage();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.Text.Json.JsonException)
        {
            MessageBox.Show(ex.Message, "LifeOS manual import", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private bool ConfirmManualImportPreview(ManualIntegrationImportPreviewSummary summary)
    {
        var message =
            $"File: {summary.SourceFileName}\n" +
            $"Kind: {summary.FileKind}\n" +
            $"Connector: {summary.ConnectorKey}\n" +
            $"Previews to create: {summary.PreviewCount}\n" +
            $"Duplicate-suspected: {summary.DuplicateSuspectedCount}\n" +
            $"Skipped rows: {summary.SkippedRowCount}\n" +
            $"Preview money: {FormatMoney(summary.PreviewMoney)}";

        if (summary.Errors.Count > 0)
        {
            message += "\n\nFirst skipped rows:\n" +
                string.Join("\n", summary.Errors.Take(5).Select(error => $"Row {error.RowNumber}: {error.Message}"));
        }

        message += "\n\nImport these previews into the review queue?";

        return MessageBox.Show(message, "Review manual import preview", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
