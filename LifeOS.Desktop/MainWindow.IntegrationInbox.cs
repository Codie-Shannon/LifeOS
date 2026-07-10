using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.IntegrationConnectors;
using LifeOS.Core.IntegrationInbox;
using LifeOS.Shared.IntegrationInbox;
using Microsoft.Win32;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private void ShowIntegrationInboxPage()
    {
        _integrationInboxItems = IntegrationInboxStorage.Load();
        var summary = IntegrationInboxCalculator.Calculate(_integrationInboxItems);
        var readiness = IntegrationReadinessMatrix.Create();

        SetHeader("Integration Inbox", $"Integration Inbox - v4.9 - {summary.NeedsReview} review - {summary.DuplicateSuspected} duplicate");

        var root = new StackPanel();

        root.Children.Add(CreateInfoPanel(
            "Integration Inbox + v5 Readiness",
            "v4.9 stages external-looking data as read-only previews. Every preview keeps source provenance, trust, duplicate state, suggested target, and a manual handoff gate. No raw connector record can update LifeOS automatically."));

        var metricsPanel = new WrapPanel { Margin = new Thickness(0, 22, 0, 0) };
        metricsPanel.Children.Add(CreateDashboardCard("Engine", "Active", "v4.9"));
        metricsPanel.Children.Add(CreateDashboardCard("Needs review", summary.NeedsReview.ToString(), "Review"));
        metricsPanel.Children.Add(CreateDashboardCard("Suspected", summary.DuplicateSuspected.ToString(), "Duplicate"));
        metricsPanel.Children.Add(CreateDashboardCard("Manual gate", summary.Untrusted.ToString(), "Untrusted"));
        metricsPanel.Children.Add(CreateDashboardCard("Reviewed", summary.Accepted.ToString(), "Accepted"));
        metricsPanel.Children.Add(CreateDashboardCard("Handoffs", summary.Linked.ToString(), "Linked"));
        metricsPanel.Children.Add(CreateDashboardCard("Preview money", FormatMoney(summary.PreviewMoney), "Not safe"));
        metricsPanel.Children.Add(CreateDashboardCard("Contracts ready", readiness.Count(x => x.ContractReady).ToString(), "v5"));
        root.Children.Add(metricsPanel);

        root.Children.Add(CreateInfoPanel(
            "v4.9 Integration Inbox rule",
            "- External records enter as read-only previews, never trusted items.\n" +
            "- Source evidence and provenance stay attached.\n" +
            "- Duplicate suspicion blocks acceptance.\n" +
            "- Accept, reject, defer, and link are deliberate review states.\n" +
            "- Accepted previews still require a separate manual handoff.\n" +
            "- Expected or imported money remains excluded from safe money.\n" +
            "- No OAuth, live API, bank feed, inbox scan, or automatic mutation is active."));

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
        var import = CreateEvidenceActionButton("Import CSV/JSON preview file", ImportIntegrationInboxPreviewFile);
        controlsStack.Children.Add(import);
        var reset = CreateEvidenceActionButton("Reset v4.9 integration previews", ResetIntegrationInboxDemo);
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
        AddIntegrationReadinessMatrix(root, readiness);

        root.Children.Add(CreateInfoPanel(
            "Integration-to-module handoff contract",
            "Accepted previews may later be handed to Item State, Payment Calendar, Work Pipeline, Evidence Vault, Bills / Payments, Follow-Ups, or Search / Knowledge. The target module remains authoritative and must validate its own rules."));

        root.Children.Add(CreateInfoPanel(
            "v4.9 audit and provenance contract",
            "Every preview keeps a local ID, source type, source label, external reference, duplicate key, source evidence, review note, trust state, status, suggested target, timestamps, and optional link reference."));

        root.Children.Add(CreateInfoPanel(
            "v4.9 boundary",
            "Local preview and readiness modelling only. No Gmail, Outlook, Google Calendar, Microsoft Calendar, Xero, SharePoint, Drive, OCR provider, banking provider, OAuth flow, live polling, automatic item creation, automatic updates, or AI actions are active."));

        root.Children.Add(CreateInfoPanel(
            "After v4.9",
            "The v4 spine is complete. v5.0 can begin with one narrow read-only connector, strict scopes, explicit preview review, duplicate detection, provenance, and a reversible manual handoff."));

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
                $"External reference: {item.ExternalReference}\nDuplicate key: {item.DuplicateKey}\nSource evidence: {item.SourceEvidence}\n" +
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
                    IntegrationInboxReviewEngine.Accept(item, "Accepted through local v4.9 review control.");
                    break;
                case IntegrationPreviewStatus.Deferred:
                    IntegrationInboxReviewEngine.Defer(item, "Deferred through local v4.9 review control.");
                    break;
                case IntegrationPreviewStatus.Rejected:
                    IntegrationInboxReviewEngine.Reject(item, "Rejected through local v4.9 review control.");
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

    private void ResetIntegrationInboxDemo()
    {
        if (!ConfirmRiskyAction(
                "Reset v4.9 Integration Inbox?",
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
            Filter = "CSV or JSON preview files (*.csv;*.json)|*.csv;*.json|CSV files (*.csv)|*.csv|JSON files (*.json)|*.json",
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
                _ => throw new InvalidOperationException("Choose a .csv or .json file.")
            };

            var items = IntegrationInboxStorage.Load();
            items.AddRange(result.Previews);
            IntegrationInboxStorage.Save(items);

            var message = $"Imported {result.Previews.Count} preview record(s) through {result.ConnectorKey}.";
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
}
