using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LifeOS.Core.FinancialRecords;

namespace LifeOS.Desktop;

public sealed class MoneyV11View : UserControl
{
    private readonly FinancialDataset _data = FinancialProofData.Build(
        new DateTimeOffset(2026, 7, 20, 12, 0, 0, TimeSpan.FromHours(12)));
    private readonly ContentControl _content = new();
    private readonly TextBlock _pageTitle = new();
    private readonly TextBlock _pageSubtitle = new();
    private readonly Dictionary<string, Button> _navigation = new(StringComparer.Ordinal);

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
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(244) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var rail = new Border
        {
            Background = Brush("#11182B"),
            BorderBrush = Brush("#27324A"),
            BorderThickness = new Thickness(0, 0, 1, 0),
            Padding = new Thickness(18, 24, 18, 20)
        };
        var nav = new StackPanel();
        nav.Children.Add(Text("LIFEOS v11", 12, Brush("#9AA9C7"), FontWeights.SemiBold));
        nav.Children.Add(Text("Money", 30, Brushes.White, FontWeights.Bold, new Thickness(0, 5, 0, 2)));
        nav.Children.Add(Text("Financial records", 14, Brush("#9AA9C7"), FontWeights.Normal, new Thickness(0, 0, 0, 22)));

        foreach (string page in new[] { "Overview", "Accounts", "Transactions", "Invoices & payments", "Linked detail", "Manual review", "Audit & protection" })
        {
            var button = new Button
            {
                Content = page,
                Tag = page,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(14, 11, 14, 11),
                Margin = new Thickness(0, 0, 0, 7),
                Background = Brushes.Transparent,
                Foreground = Brush("#D9E2F3"),
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            button.Click += (_, _) => ShowPage((string)button.Tag);
            _navigation[page] = button;
            nav.Children.Add(button);
        }

        nav.Children.Add(new Border
        {
            Background = Brush("#17213A"),
            BorderBrush = Brush("#31405F"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 18, 0, 0),
            Child = Text("Review-first\nNo bank or provider writes\nFictional proof data only", 12, Brush("#A9B8D5"), FontWeights.Normal)
        });
        rail.Child = nav;
        root.Children.Add(rail);

        var main = new Grid { Margin = new Thickness(28, 22, 28, 24) };
        main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _pageTitle.FontSize = 31;
        _pageTitle.FontWeight = FontWeights.Bold;
        _pageTitle.Foreground = Brushes.White;
        _pageSubtitle.FontSize = 14;
        _pageSubtitle.Foreground = Brush("#A9B8D5");
        _pageSubtitle.Margin = new Thickness(0, 5, 0, 20);
        var heading = new StackPanel();
        heading.Children.Add(_pageTitle);
        heading.Children.Add(_pageSubtitle);
        main.Children.Add(heading);
        Grid.SetRow(_content, 1);
        main.Children.Add(_content);
        Grid.SetColumn(main, 1);
        root.Children.Add(main);
        return root;
    }

    private void ShowPage(string page)
    {
        foreach (var pair in _navigation)
        {
            bool active = pair.Key == page;
            pair.Value.Background = active ? Brush("#23335A") : Brushes.Transparent;
            pair.Value.BorderBrush = active ? Brush("#607DFF") : Brushes.Transparent;
            pair.Value.Foreground = active ? Brushes.White : Brush("#D9E2F3");
        }

        (_pageTitle.Text, _pageSubtitle.Text, _content.Content) = page switch
        {
            "Accounts" => ("Accounts and financial status", "Manual and tracked accounts never pretend to be live-connected.", Accounts()),
            "Transactions" => ("Transactions", "Searchable financial records with category, direction and explicit review state.", Transactions()),
            "Invoices & payments" => ("Invoices and payments", "Due, overdue, paid and partially paid states remain deterministic.", Invoices()),
            "Linked detail" => ("Invoice and payment detail", "Read-only links preserve Work, Project and evidence context.", LinkedDetail()),
            "Manual review" => ("Manual financial record", "A proposed record becomes trusted only after explicit confirmation.", ManualReview()),
            "Audit & protection" => ("Audit history and protection", "Meaningful state changes are visible while sensitive proof values remain minimized.", Audit()),
            _ => ("Money v11 overview", "Authoritative financial records • review-first • fictional proof data", Overview())
        };
    }

    private UIElement Overview()
    {
        var overview = FinancialRecordService.Summarize(_data, "NZD");
        var panel = Vertical();
        var metrics = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 8) };
        metrics.Children.Add(Metric("Current balance", Money(overview.CurrentBalance), "Confirmed tracked accounts"));
        metrics.Children.Add(Metric("Income", Money(overview.Income), "Confirmed transactions"));
        metrics.Children.Add(Metric("Expenses", Money(overview.Expenses), "Pending review excluded"));
        metrics.Children.Add(Metric("Invoices due", Money(overview.InvoicesDue), "Issued, overdue and partial"));
        metrics.Children.Add(Metric("Payments received", Money(overview.PaymentsReceived), "Received and allocated"));
        metrics.Children.Add(Metric("Evidence gaps", overview.EvidenceGaps.ToString(CultureInfo.InvariantCulture), "Review attention"));
        panel.Children.Add(metrics);
        panel.Children.Add(Card("Trust boundary", "LifeOS organizes financial information. It does not approve accounting, initiate payments, write to banks or reconcile autonomously.", "REVIEW-FIRST"));
        return Scroll(panel);
    }

    private UIElement Accounts()
    {
        var panel = Vertical();
        foreach (var account in _data.Accounts)
        {
            panel.Children.Add(Card(account.Name,
                $"{account.Type} account  •  {Money(account.Balance)}\nCurrency {account.Currency}  •  Review {account.ReviewState}\nLive connected: {account.IsLiveConnected}",
                account.ReviewState.ToString()));
        }
        return Scroll(panel);
    }

    private UIElement Transactions()
    {
        var root = Vertical();
        var tools = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var search = new TextBox { Width = 310, Height = 38, Padding = new Thickness(10, 7, 10, 7), Text = "Search fictional records", Foreground = Brush("#59657A") };
        var filter = new ComboBox { Width = 190, Height = 38, Margin = new Thickness(10, 0, 0, 0), SelectedIndex = 0, ItemsSource = new[] { "All review states", "Confirmed", "Pending review" } };
        tools.Children.Add(search); tools.Children.Add(filter); root.Children.Add(tools);
        foreach (var transaction in _data.Transactions)
        {
            var category = _data.Categories.Single(x => x.Id == transaction.CategoryId);
            root.Children.Add(Card(transaction.Description,
                $"{transaction.Direction}  •  {Money(transaction.Amount)}  •  {transaction.Date:dd MMM yyyy}\nCategory: {category.Name}  •  Counterparty: {transaction.PartyId}\nReview state: {transaction.ReviewState}",
                transaction.ReviewState.ToString()));
        }
        return Scroll(root);
    }

    private UIElement Invoices()
    {
        var panel = Vertical();
        foreach (var invoice in _data.Invoices)
        {
            panel.Children.Add(Card(invoice.InvoiceNumber,
                $"Total {Money(invoice.Total)}  •  Outstanding {Money(invoice.Outstanding)}\nIssued {invoice.IssueDate:dd MMM yyyy}  •  Due {invoice.DueDate:dd MMM yyyy}\nLinks {invoice.Links.Count}  •  Evidence {invoice.Evidence.Count}  •  Review {invoice.ReviewState}",
                invoice.State.ToString()));
        }
        panel.Children.Add(Text("Payments", 22, Brushes.White, FontWeights.SemiBold, new Thickness(0, 9, 0, 10)));
        foreach (var payment in _data.Payments)
            panel.Children.Add(Card(payment.Id, $"Amount {Money(payment.Amount)}  •  Allocated {Money(payment.AllocatedAmount)}  •  Unallocated {Money(payment.Unallocated)}\nInvoice {payment.InvoiceId}  •  {payment.Date:dd MMM yyyy}  •  Review {payment.ReviewState}", payment.State.ToString()));
        return Scroll(panel);
    }

    private UIElement LinkedDetail()
    {
        var invoice = _data.Invoices[0];
        var payment = _data.Payments[0];
        var panel = Vertical();
        panel.Children.Add(Card($"Invoice {invoice.InvoiceNumber}", $"Party: {invoice.PartyId}\nTotal {Money(invoice.Total)}  •  Outstanding {Money(invoice.Outstanding)}\nState {invoice.State}  •  Review {invoice.ReviewState}", "INVOICE"));
        panel.Children.Add(Card("Linked Work context", string.Join("\n", invoice.Links.Select(x => $"{x.Type}: {x.Label} ({x.RecordId})")), "READ-ONLY LINK"));
        panel.Children.Add(Card("Evidence", string.Join("\n", invoice.Evidence.Select(x => $"{x.Label}\nSHA-256 proof prefix: {x.Sha256}\nReview: {x.ReviewState}")), "ORIGINAL PRESERVED"));
        panel.Children.Add(Card("Allocated payment", $"{payment.Id}  •  {Money(payment.Amount)}\nState {payment.State}  •  Allocated {Money(payment.AllocatedAmount)}\nNo payment initiation capability exists.", "PAYMENT RECORD"));
        return Scroll(panel);
    }

    private UIElement ManualReview()
    {
        var panel = Vertical();
        var state = Text("Draft has not been committed.", 14, Brush("#F3C969"), FontWeights.SemiBold, new Thickness(0, 10, 0, 0));
        panel.Children.Add(Card("Proposed fictional transaction", "Amount NZD 125.00\nDirection Income\nCategory Client work\nParty Fictional Engineering\nLinked Work record\nNo provider write", "PENDING REVIEW"));
        var confirm = new Button
        {
            Content = "Review and confirm fictional transaction",
            Padding = new Thickness(16, 11, 16, 11),
            HorizontalAlignment = HorizontalAlignment.Left,
            Background = Brush("#526DFF"),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            FontWeight = FontWeights.SemiBold,
            Cursor = System.Windows.Input.Cursors.Hand
        };
        confirm.Click += (_, _) =>
        {
            var draft = new FinancialTransaction("manual-demo", "manual-proof-58", "acct-operating", TransactionDirection.Income, 125m, "NZD", DateOnly.FromDateTime(DateTime.Today), "Fictional reviewed work", "cat-work", "party-a", FinancialReviewState.PendingReview, [new(FinancialLinkType.Work, "work-demo", "Reviewed work")], []);
            if (FinancialRecordService.IsDuplicateSubmission(_data.Transactions, draft.SubmissionKey))
                state.Text = "Duplicate submission blocked; nothing changed.";
            else
                state.Text = "Confirmed explicitly: NZD 125.00 • Confirmed • local proof only";
            state.Foreground = Brush("#72D6A0");
            confirm.IsEnabled = false;
        };
        panel.Children.Add(confirm); panel.Children.Add(state);
        panel.Children.Add(Card("Confirmation boundary", "Nothing is saved, trusted or posted externally until explicit review succeeds. This proof performs no provider, bank or payment write.", "NO EXTERNAL WRITE"));
        return Scroll(panel);
    }

    private UIElement Audit()
    {
        var panel = Vertical();
        foreach (var entry in _data.Audit.OrderByDescending(x => x.OccurredUtc))
            panel.Children.Add(Card(entry.Action, $"{entry.RecordType} {entry.RecordId}\n{entry.SafeSummary}\n{entry.OccurredUtc:dd MMM yyyy HH:mm zzz}", "AUDIT"));
        panel.Children.Add(Card("Sensitive-data protection proof", FinancialRecordService.Redact("email test@example.com account 123456789") + "\nProof mode uses fictional parties, shortened hashes and safe summaries.", "REDACTED"));
        panel.Children.Add(Card("Capability boundary", "No bank feed write • No payment initiation • No accounting-provider write • No autonomous reconciliation", "ENFORCED"));
        return Scroll(panel);
    }

    private static ScrollViewer Scroll(UIElement child) => new() { Content = child, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
    private static StackPanel Vertical() => new() { Orientation = Orientation.Vertical };
    private static string Money(decimal value) => $"NZD {value:N2}";
    private static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
    private static TextBlock Text(string text, double size, Brush foreground, FontWeight weight, Thickness? margin = null) => new() { Text = text, FontSize = size, Foreground = foreground, FontWeight = weight, Margin = margin ?? new Thickness(0), TextWrapping = TextWrapping.Wrap };

    private static Border Metric(string label, string value, string detail)
    {
        var stack = Vertical();
        stack.Children.Add(Text(label.ToUpperInvariant(), 11, Brush("#95A5C5"), FontWeights.SemiBold));
        stack.Children.Add(Text(value, 27, Brushes.White, FontWeights.Bold, new Thickness(0, 7, 0, 5)));
        stack.Children.Add(Text(detail, 12, Brush("#A9B8D5"), FontWeights.Normal));
        return new Border { Background = Brush("#141D32"), BorderBrush = Brush("#293754"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(16), Margin = new Thickness(0, 0, 12, 12), Child = stack };
    }

    private static Border Card(string title, string body, string badge)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var text = Vertical();
        text.Children.Add(Text(title, 18, Brushes.White, FontWeights.SemiBold));
        text.Children.Add(Text(body, 13, Brush("#B8C5DD"), FontWeights.Normal, new Thickness(0, 7, 0, 0)));
        grid.Children.Add(text);
        var marker = new Border { Background = Brush("#223052"), BorderBrush = Brush("#40547B"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8), Padding = new Thickness(10, 5, 10, 5), VerticalAlignment = VerticalAlignment.Top, Child = Text(badge.ToUpperInvariant(), 10, Brush("#C8D4EC"), FontWeights.Bold) };
        Grid.SetColumn(marker, 1); grid.Children.Add(marker);
        return new Border { Background = Brush("#141D32"), BorderBrush = Brush("#293754"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(17), Margin = new Thickness(0, 0, 0, 11), Child = grid };
    }
}
