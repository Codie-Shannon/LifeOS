using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LifeOS.Core.FinancialRecords;
using LifeOS.Core.FinancialReview;

namespace LifeOS.Desktop;

public sealed class MoneyV11View : UserControl
{
    private static readonly DateTimeOffset ProofNow = new(2026, 7, 22, 12, 0, 0, TimeSpan.FromHours(12));
    private readonly FinancialReviewProof _proof = FinancialReviewProofData.Build(ProofNow);
    private readonly FinancialReviewService _reviewService = new();
    private readonly ContentControl _content = new();
    private readonly TextBlock _pageTitle = new();
    private readonly TextBlock _pageSubtitle = new();
    private readonly Dictionary<string, Button> _navigation = new(StringComparer.Ordinal);
    private string _activePage = "Overview";

    public MoneyV11View()
    {
        Background = Brush("#0B1020");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        Content = BuildShell();
        ShowPage("Overview");
    }

    private UIElement BuildShell()
    {
        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var nav = new StackPanel();
        nav.Children.Add(Text("LIFEOS v11", 12, Brush("#9AA9C7"), FontWeights.SemiBold));
        nav.Children.Add(Text("Money", 30, Brushes.White, FontWeights.Bold, new Thickness(0, 5, 0, 2)));
        nav.Children.Add(Text("Review and records", 14, Brush("#9AA9C7"), FontWeights.Normal, new Thickness(0, 0, 0, 20)));
        foreach (var page in new[]
        {
            "Overview", "Review queue", "Review detail", "Resolve candidate",
            "Reports", "Evidence report", "Export preview",
            "Accounts", "Transactions", "Invoices & payments", "Linked detail",
            "Manual review", "Audit & protection"
        })
        {
            var button = new Button
            {
                Content = page,
                Tag = page,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(13, 9, 13, 9),
                Margin = new Thickness(0, 0, 0, 5),
                Background = Brushes.Transparent,
                Foreground = Brush("#D9E2F3"),
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                Template = NavigationButtonTemplate()
            };
            button.MouseEnter += (_, _) => ApplyNavigationVisual(button, true);
            button.MouseLeave += (_, _) => ApplyNavigationVisual(button, false);
            button.Click += (_, _) => ShowPage((string)button.Tag);
            _navigation[page] = button;
            nav.Children.Add(button);
        }
        nav.Children.Add(new Border
        {
            Background = Brush("#17213A"), BorderBrush = Brush("#31405F"), BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10), Padding = new Thickness(12), Margin = new Thickness(0, 14, 0, 0),
            Child = Text("Review-first\nNo automatic reconciliation\nNo bank or provider writes\nFictional proof data only", 12, Brush("#A9B8D5"), FontWeights.Normal)
        });
        var rail = new Border
        {
            Background = Brush("#11182B"), BorderBrush = Brush("#27324A"), BorderThickness = new Thickness(0, 0, 1, 0),
            Padding = new Thickness(18, 22, 18, 18), Child = new ScrollViewer { Content = nav, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }
        };
        root.Children.Add(rail);

        var main = new Grid { Margin = new Thickness(28, 22, 28, 24) };
        main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _pageTitle.FontSize = 31; _pageTitle.FontWeight = FontWeights.Bold; _pageTitle.Foreground = Brushes.White;
        _pageSubtitle.FontSize = 14; _pageSubtitle.Foreground = Brush("#A9B8D5"); _pageSubtitle.Margin = new Thickness(0, 5, 0, 20);
        var heading = new StackPanel(); heading.Children.Add(_pageTitle); heading.Children.Add(_pageSubtitle); main.Children.Add(heading);
        Grid.SetRow(_content, 1); main.Children.Add(_content); Grid.SetColumn(main, 1); root.Children.Add(main);
        return root;
    }

    private void ShowPage(string page)
    {
        _activePage = page;
        foreach (var pair in _navigation) ApplyNavigationVisual(pair.Value, false);
        (_pageTitle.Text, _pageSubtitle.Text, _content.Content) = page switch
        {
            "Review queue" => ("Financial review queue", "Unmatched, allocation, duplicate, mismatch and evidence-gap candidates remain review-only.", ReviewQueue()),
            "Review detail" => ("Review candidate detail", "Trusted financial records and preserved evidence are compared side by side.", ReviewDetail()),
            "Resolve candidate" => ("Explicit candidate resolution", "Trusted-state actions require confirmation and every result is auditable.", ResolveCandidate()),
            "Reports" => ("Financial reports", "Bounded reporting periods with explicit currency grouping and trusted-record drill-down.", Reports()),
            "Evidence report" => ("Evidence completeness", "Complete, missing, pending-review and conflicting evidence remain visible and drillable.", EvidenceReport()),
            "Export preview" => ("Safe export preview", "CSV and PDF outputs are explicit derivatives with destination, confirmation and redaction controls.", ExportPreview()),
            "Accounts" => ("Accounts and financial status", "Manual and tracked accounts never pretend to be live-connected.", Accounts()),
            "Transactions" => ("Transactions", "Searchable records with category, direction and explicit review state.", Transactions()),
            "Invoices & payments" => ("Invoices and payments", "Due, overdue, paid and partially paid states remain deterministic.", Invoices()),
            "Linked detail" => ("Invoice and payment detail", "Read-only links preserve Work, Project and evidence context.", LinkedDetail()),
            "Manual review" => ("Manual financial record", "A proposed record becomes trusted only after explicit confirmation.", ManualReview()),
            "Audit & protection" => ("Audit history and protection", "Meaningful state changes are visible while sensitive proof values remain minimized.", Audit()),
            _ => ("Money v11 overview", "Authoritative financial records • review-first • fictional proof data", Overview())
        };
    }

    private UIElement Overview()
    {
        var financial = FinancialRecordService.Summarize(_proof.FinancialRecords, "NZD");
        var review = _reviewService.Summarize(_proof.Candidates);
        var panel = Vertical();
        var metrics = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 8) };
        metrics.Children.Add(Metric("Current balance", Money(financial.CurrentBalance), "Confirmed tracked accounts"));
        metrics.Children.Add(Metric("Income", Money(financial.Income), "Confirmed transactions"));
        metrics.Children.Add(Metric("Expenses", Money(financial.Expenses), "Pending review excluded"));
        metrics.Children.Add(Metric("Open review", review.Open.ToString(CultureInfo.InvariantCulture), "Candidate-only queue"));
        metrics.Children.Add(Metric("Critical", review.Critical.ToString(CultureInfo.InvariantCulture), "Requires explicit review"));
        metrics.Children.Add(Metric("Evidence gaps", review.EvidenceGaps.ToString(CultureInfo.InvariantCulture), "Original-preserving resolution"));
        panel.Children.Add(metrics);
        panel.Children.Add(Card("Trust boundary", "LifeOS organizes local financial information. It does not approve accounting, initiate payments, write to banks or reconcile autonomously.", "REVIEW-FIRST"));
        return Scroll(panel);
    }

    private UIElement ReviewQueue()
    {
        var panel = Vertical();
        var filters = new WrapPanel { Margin = new Thickness(0, 0, 0, 13) };
        filters.Children.Add(FilterBox("All candidate types", Enum.GetNames<ReconciliationCandidateType>()));
        filters.Children.Add(FilterBox("All severities", Enum.GetNames<FinancialReviewSeverity>()));
        filters.Children.Add(FilterBox("Open", Enum.GetNames<FinancialReviewStatus>()));
        filters.Children.Add(FilterBox("All parties", _proof.FinancialRecords.Parties.Select(x => x.DisplayName)));
        filters.Children.Add(FilterBox("Bounded date", new[] { "Last 7 days", "Last 30 days", "Custom" }));
        filters.Children.Add(FilterBox("All linked records", new[] { "Transactions", "Invoices", "Payments", "Documents" }));
        panel.Children.Add(filters);
        foreach (var candidate in _proof.Candidates.Take(12))
        {
            var sourceText = string.Join(", ", candidate.Sources.Select(x => $"{x.Kind} {x.RecordId}"));
            panel.Children.Add(Card(candidate.Type.ToString(),
                $"{candidate.Explanation}\n{candidate.Currency} • {candidate.RecordDate:dd MMM yyyy} • party {candidate.PartyId}\nSources: {sourceText}\nConfidence: {(candidate.Confidence.HasValue ? candidate.Confidence.Value.ToString("P0", CultureInfo.InvariantCulture) : "reason-only")}",
                $"{candidate.Severity} • {candidate.Status}"));
        }
        return Scroll(panel);
    }

    private UIElement ReviewDetail()
    {
        var candidate = _proof.Candidates.First(x => x.EvidenceGap is not null);
        var financialSource = candidate.Sources[0];
        var acceptedDocument = _proof.Documents.First(x => x.State == LifeOS.Core.Documents.DocumentIntakeState.Accepted);
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var left = Vertical();
        left.Children.Add(Text("Trusted financial record", 20, Brushes.White, FontWeights.SemiBold, new Thickness(0, 0, 0, 10)));
        left.Children.Add(Card(financialSource.Label,
            $"{financialSource.Kind} • {financialSource.RecordId}\nProvenance {financialSource.Provenance}\nFingerprint {financialSource.Fingerprint}\nFreshness {financialSource.FreshnessUtc:dd MMM yyyy HH:mm zzz}", "AUTHORITATIVE"));
        left.Children.Add(Card("Candidate explanation", candidate.Explanation + "\nNo inference of external bank truth is made.", candidate.Type.ToString()));
        var right = Vertical();
        right.Children.Add(Text("Preserved evidence", 20, Brushes.White, FontWeights.SemiBold, new Thickness(0, 0, 0, 10)));
        right.Children.Add(Card(acceptedDocument.Original.FileName,
            $"Document {acceptedDocument.Id} • {acceptedDocument.Type} • {acceptedDocument.State}\nSHA-256 {acceptedDocument.Original.Sha256}\nOriginal trusted: {acceptedDocument.HasTrustedOriginal}\nLinks {acceptedDocument.Links.Count} • original remains preserved", "ACCEPTED ORIGINAL"));
        right.Children.Add(Card("Evidence gap", candidate.EvidenceGap!.Explanation + "\nLinking creates a reviewed reference; it never replaces the original.", "CANDIDATE ONLY"));
        grid.Children.Add(left); Grid.SetColumn(right, 2); grid.Children.Add(right);
        return Scroll(grid);
    }

    private UIElement ResolveCandidate()
    {
        var candidate = _proof.Candidates.First(x => x.AllocationIssue is not null && x.AllocationIssue.ProposedAllocation > 0m);
        var issue = candidate.AllocationIssue!;
        var panel = Vertical();
        panel.Children.Add(Card("Partial allocation candidate",
            $"Invoice {issue.InvoiceId} outstanding {Money(issue.InvoiceOutstanding)}\nPayment {issue.PaymentId} unallocated {Money(issue.PaymentUnallocated)}\nDeterministic proposed allocation {Money(issue.ProposedAllocation)}\nRemaining invoice {Money(issue.RemainingInvoiceBalance)}",
            "CONFIRMATION REQUIRED"));
        var confirm = new CheckBox { Content = "I reviewed the trusted records and confirm this local allocation", Foreground = Brushes.White, Margin = new Thickness(0, 5, 0, 12) };
        var status = Text("No trusted financial state has changed.", 14, Brush("#F3C969"), FontWeights.SemiBold, new Thickness(0, 10, 0, 0));
        var audit = Text("Audit: generated from trusted local records only.", 13, Brush("#B8C5DD"), FontWeights.Normal, new Thickness(0, 8, 0, 0));
        var button = ActionButton("Allocate reviewed amount");
        button.Click += (_, _) =>
        {
            var result = _reviewService.Resolve(candidate,
                new CandidateResolutionRequest(ReviewResolutionAction.Allocate, "Fictional proof allocation reviewed", confirm.IsChecked == true, AllocationAmount: issue.ProposedAllocation),
                ProofNow.AddMinutes(12));
            status.Text = result.Message;
            status.Foreground = result.Applied ? Brush("#72D6A0") : Brush("#F3C969");
            audit.Text = string.Join("\n", result.Candidate.Audit.Select(x => $"{x.OccurredUtc:HH:mm} • {x.Action} • {x.SafeSummary}"));
            if (result.Applied) button.IsEnabled = false;
        };
        panel.Children.Add(confirm); panel.Children.Add(button); panel.Children.Add(status); panel.Children.Add(audit);
        panel.Children.Add(Card("Other explicit actions", "Link accepted evidence • correct local record • accept as exception • defer • dismiss • reopen\nNo automatic approval, merge, provider write or payment initiation exists.", "AUDITABLE"));
        return Scroll(panel);
    }


    private UIElement Reports()
    {
        var service = new FinancialReportingService();
        var report = service.Build(_proof.FinancialRecords, _proof.Candidates, new FinancialReportFilter(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), ProofNow);
        var panel = Vertical();
        panel.Children.Add(Card("Bounded period", $"01 Jan 2026 to 31 Dec 2026\n{report.FreshnessNote}", "NO MIXED-CURRENCY TOTAL"));
        foreach (var group in report.CurrencyGroups)
            panel.Children.Add(Card($"{group.Currency} financial summary", $"Income {group.Income:N2}\nExpenses {group.Expenses:N2}\nOutstanding invoices {group.OutstandingInvoices:N2}\nPayments received {group.PaymentsReceived:N2}\nTrusted-record rows {group.TransactionCount + group.InvoiceCount + group.PaymentCount}", "DRILL-DOWN READY"));
        panel.Children.Add(Card("Unresolved review candidates", report.UnresolvedCandidates.Count.ToString(CultureInfo.InvariantCulture), "REVIEW-FIRST"));
        return Scroll(panel);
    }

    private UIElement EvidenceReport()
    {
        var report = new FinancialReportingService().Build(_proof.FinancialRecords, _proof.Candidates, new FinancialReportFilter(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), ProofNow);
        var panel = Vertical();
        foreach (var state in Enum.GetValues<EvidenceCompletenessState>())
            panel.Children.Add(Metric(state.ToString(), report.Evidence.Count(x => x.State == state).ToString(CultureInfo.InvariantCulture), "Linked preserved evidence"));
        foreach (var item in report.Evidence.Take(10))
            panel.Children.Add(Card($"{item.RecordType} • {item.Label}", $"{item.Explanation}\nRecord {item.RecordId}\nLinked Work/Project records: {item.Links.Count}", item.State.ToString()));
        return Scroll(panel);
    }

    private UIElement ExportPreview()
    {
        var service = new FinancialReportingService();
        var report = service.Build(_proof.FinancialRecords, _proof.Candidates, new FinancialReportFilter(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), ProofNow);
        var preview = service.Preview(report, new FinancialExportRequest(FinancialExportFormat.Pdf, @"C:\Exports", false, true, ["summary", "evidence", "review-candidates"]));
        var panel = Vertical();
        panel.Children.Add(Card("Export preview", $"{preview.FileName}\n{preview.MediaType}\nDestination: {preview.Destination}\nEstimated size: {preview.EstimatedBytes:N0} bytes", "NOT EXPORTED"));
        panel.Children.Add(Card("Privacy controls", $"Redaction enabled: {preview.RedactionEnabled}\nSections: {string.Join(", ", preview.Sections)}\n{preview.Warning}", "CONFIRMATION REQUIRED"));
        panel.Children.Add(Card("Failure and cancellation", "Cancel leaves no derivative. Failed exports return a safe error and authoritative records remain unchanged.", "DERIVATIVE ONLY"));
        return Scroll(panel);
    }
    private UIElement Accounts()
    {
        var panel = Vertical();
        foreach (var account in _proof.FinancialRecords.Accounts)
            panel.Children.Add(Card(account.Name, $"{account.Type} account • {Money(account.Balance, account.Currency)}\nReview {account.ReviewState} • Live connected: {account.IsLiveConnected}", account.ReviewState.ToString()));
        return Scroll(panel);
    }

    private UIElement Transactions()
    {
        var panel = Vertical();
        foreach (var transaction in _proof.FinancialRecords.Transactions)
        {
            var category = _proof.FinancialRecords.Categories.Single(x => x.Id == transaction.CategoryId);
            panel.Children.Add(Card(transaction.Description, $"{transaction.Direction} • {Money(transaction.Amount, transaction.Currency)} • {transaction.Date:dd MMM yyyy}\nCategory {category.Name} • Party {transaction.PartyId}\nReview {transaction.ReviewState} • Evidence {transaction.Evidence.Count}", transaction.ReviewState.ToString()));
        }
        return Scroll(panel);
    }

    private UIElement Invoices()
    {
        var panel = Vertical();
        foreach (var invoice in _proof.FinancialRecords.Invoices)
            panel.Children.Add(Card(invoice.InvoiceNumber, $"Total {Money(invoice.Total, invoice.Currency)} • Outstanding {Money(invoice.Outstanding, invoice.Currency)}\nIssued {invoice.IssueDate:dd MMM yyyy} • Due {invoice.DueDate:dd MMM yyyy}\nLinks {invoice.Links.Count} • Evidence {invoice.Evidence.Count}", invoice.State.ToString()));
        panel.Children.Add(Text("Payments", 22, Brushes.White, FontWeights.SemiBold, new Thickness(0, 9, 0, 10)));
        foreach (var payment in _proof.FinancialRecords.Payments)
            panel.Children.Add(Card(payment.Id, $"Amount {Money(payment.Amount, payment.Currency)} • Allocated {Money(payment.AllocatedAmount, payment.Currency)} • Unallocated {Money(payment.Unallocated, payment.Currency)}\nInvoice {payment.InvoiceId} • {payment.Date:dd MMM yyyy}", payment.State.ToString()));
        return Scroll(panel);
    }

    private UIElement LinkedDetail()
    {
        var invoice = _proof.FinancialRecords.Invoices[0];
        var panel = Vertical();
        panel.Children.Add(Card($"Invoice {invoice.InvoiceNumber}", $"Party {invoice.PartyId}\nTotal {Money(invoice.Total, invoice.Currency)} • Outstanding {Money(invoice.Outstanding, invoice.Currency)}\nState {invoice.State} • Review {invoice.ReviewState}", "INVOICE"));
        panel.Children.Add(Card("Linked context", string.Join("\n", invoice.Links.Select(x => $"{x.Type}: {x.Label} ({x.RecordId})")), "READ-ONLY LINK"));
        panel.Children.Add(Card("Evidence", string.Join("\n", invoice.Evidence.Select(x => $"{x.Label} • SHA-256 {x.Sha256} • {x.ReviewState}")), "ORIGINAL PRESERVED"));
        return Scroll(panel);
    }

    private UIElement ManualReview()
    {
        var panel = Vertical();
        panel.Children.Add(Card("Proposed fictional transaction", "NZD 125.00 • Income • Client work • Fictional Engineering\nLinked Work record • no provider write", "PENDING REVIEW"));
        var state = Text("Draft has not been committed.", 14, Brush("#F3C969"), FontWeights.SemiBold, new Thickness(0, 10, 0, 0));
        var button = ActionButton("Review and confirm fictional transaction");
        button.Click += (_, _) => { state.Text = "Confirmed explicitly: NZD 125.00 • local proof only"; state.Foreground = Brush("#72D6A0"); };
        panel.Children.Add(button); panel.Children.Add(state);
        return Scroll(panel);
    }

    private UIElement Audit()
    {
        var panel = Vertical();
        foreach (var entry in _proof.FinancialRecords.Audit.OrderByDescending(x => x.OccurredUtc))
            panel.Children.Add(Card(entry.Action, $"{entry.RecordType} {entry.RecordId}\n{entry.SafeSummary}\n{entry.OccurredUtc:dd MMM yyyy HH:mm zzz}", "AUDIT"));
        panel.Children.Add(Card("Sensitive-data protection", _reviewService.Redact("email test@example.com account 123456789") + "\nFictional parties, minimized values and safe summaries only.", "REDACTED"));
        panel.Children.Add(Card("Capability boundary", "No bank feed write • No payment initiation • No accounting-provider write • No automatic reconciliation", "ENFORCED"));
        return Scroll(panel);
    }

    private ComboBox FilterBox(string selected, IEnumerable<string> options)
    {
        var items = new[] { selected }.Concat(options.Where(x => !string.Equals(x, selected, StringComparison.Ordinal))).ToArray();
        return new ComboBox { Width = 178, Height = 36, Margin = new Thickness(0, 0, 9, 8), ItemsSource = items, SelectedIndex = 0 };
    }

    private static Button ActionButton(string label) => new() { Content = label, Padding = new Thickness(16, 10, 16, 10), HorizontalAlignment = HorizontalAlignment.Left, Background = Brush("#526DFF"), Foreground = Brushes.White, BorderThickness = new Thickness(0), FontWeight = FontWeights.SemiBold, Cursor = System.Windows.Input.Cursors.Hand };

    private static ControlTemplate NavigationButtonTemplate()
    {
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding(nameof(Button.Background)) { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding(nameof(Button.BorderBrush)) { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding(nameof(Button.BorderThickness)) { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetBinding(ContentPresenter.ContentProperty, new System.Windows.Data.Binding(nameof(Button.Content)) { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        presenter.SetBinding(ContentPresenter.MarginProperty, new System.Windows.Data.Binding(nameof(Button.Padding)) { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        presenter.SetBinding(ContentPresenter.HorizontalAlignmentProperty, new System.Windows.Data.Binding(nameof(Button.HorizontalContentAlignment)) { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center); border.AppendChild(presenter);
        return new ControlTemplate(typeof(Button)) { VisualTree = border };
    }

    private void ApplyNavigationVisual(Button button, bool hover)
    {
        bool active = string.Equals(button.Tag as string, _activePage, StringComparison.Ordinal);
        button.Background = active ? Brush("#23335A") : hover ? Brush("#19233B") : Brushes.Transparent;
        button.BorderBrush = active ? Brush("#607DFF") : hover ? Brush("#31405F") : Brushes.Transparent;
        button.Foreground = active || hover ? Brushes.White : Brush("#D9E2F3");
    }

    private static ScrollViewer Scroll(UIElement child) => new() { Content = child, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
    private static StackPanel Vertical() => new() { Orientation = Orientation.Vertical };
    private static string Money(decimal value, string currency = "NZD") => $"{currency} {value:N2}";
    private static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
    private static TextBlock Text(string text, double size, Brush foreground, FontWeight weight, Thickness? margin = null) => new() { Text = text, FontSize = size, Foreground = foreground, FontWeight = weight, Margin = margin ?? new Thickness(0), TextWrapping = TextWrapping.Wrap };

    private static Border Metric(string label, string value, string detail)
    {
        var stack = Vertical(); stack.Children.Add(Text(label.ToUpperInvariant(), 11, Brush("#95A5C5"), FontWeights.SemiBold)); stack.Children.Add(Text(value, 27, Brushes.White, FontWeights.Bold, new Thickness(0, 7, 0, 5))); stack.Children.Add(Text(detail, 12, Brush("#A9B8D5"), FontWeights.Normal));
        return new Border { Background = Brush("#141D32"), BorderBrush = Brush("#293754"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(16), Margin = new Thickness(0, 0, 12, 12), Child = stack };
    }

    private static Border Card(string title, string body, string badge)
    {
        var grid = new Grid(); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var text = Vertical(); text.Children.Add(Text(title, 18, Brushes.White, FontWeights.SemiBold)); text.Children.Add(Text(body, 13, Brush("#B8C5DD"), FontWeights.Normal, new Thickness(0, 7, 0, 0))); grid.Children.Add(text);
        var marker = new Border { Background = Brush("#223052"), BorderBrush = Brush("#40547B"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Padding = new Thickness(10, 5, 10, 5), VerticalAlignment = VerticalAlignment.Top, Child = Text(badge.ToUpperInvariant(), 10, Brush("#C8D4EC"), FontWeights.Bold) };
        Grid.SetColumn(marker, 1); grid.Children.Add(marker);
        return new Border { Background = Brush("#141D32"), BorderBrush = Brush("#293754"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(17), Margin = new Thickness(0, 0, 0, 11), Child = grid };
    }
}
