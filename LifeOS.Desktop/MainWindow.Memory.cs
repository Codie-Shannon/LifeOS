using LifeOS.Core.AssistantMemory;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private readonly ProtectedAssistantMemoryStore _assistantMemoryStore = new();
    private AssistantMemoryService? _assistantMemoryService;
    private ProposedAssistantMemory? _pendingMemoryProposal;
    private string _memoryDraftTitle = "Fictional preference";
    private string _memoryDraftStatement = "For fictional Project Zephyr Quill, use short evidence-first status summaries.";
    private string _memoryDraftSource = "Manual entry";

    private AssistantMemoryService MemoryService => _assistantMemoryService ??= new AssistantMemoryService(_assistantMemoryStore);

    private void MemoryNavButton_Click(object sender, RoutedEventArgs e) => ShowMemoryPage();

    private void PrepareMemoryFromAssistant(string answer, string sourceQuestion)
    {
        _memoryDraftTitle = "Assistant context from reviewed answer";
        _memoryDraftStatement = answer;
        _memoryDraftSource = string.IsNullOrWhiteSpace(sourceQuestion) ? "Assistant answer" : sourceQuestion;
        _pendingMemoryProposal = null;
        ShowMemoryPage();
    }

    private void ShowMemoryPage()
    {
        PageTitleTextBlock.Text = "Assistant memory";
        PageSubtitleTextBlock.Text = "Explicit durable context • permission-controlled • Desktop v7.0.0-alpha.4";
        CurrentSectionTextBlock.Text = "Current section: Assistant memory";
        var root = new StackPanel();
        root.Children.Add(CreateMemoryBoundaryCard());
        root.Children.Add(CreateMemoryPermissionsCard());
        root.Children.Add(CreateMemoryProposalCard());
        root.Children.Add(CreateMemoryRecordsCard());
        MainContentControl.Content = root;
    }

    private Border CreateMemoryBoundaryCard()
    {
        var card = CreatePanel();
        card.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
        card.Child = new StackPanel { Children =
        {
            new TextBlock { Text = "MEMORY IS EXPLICIT • NO AUTOMATIC RETENTION", Foreground = new SolidColorBrush(Color.FromRgb(125,211,252)), FontWeight = FontWeights.Bold, FontSize = 13 },
            new TextBlock { Text = "Every remembered item is previewed, scoped, confirmed, auditable and removable.", Foreground = Brushes.White, FontSize = 18, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,0) },
            new TextBlock { Text = "Memory cannot approve, confirm or execute work. Current trusted LifeOS records outrank conflicting memory. No memory is sent to external services.", Foreground = new SolidColorBrush(Color.FromRgb(148,163,184)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,0) }
        }};
        return card;
    }

    private Border CreateMemoryPermissionsCard()
    {
        var card = CreatePanel(); card.Margin = new Thickness(0,18,0,0);
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = "Memory permissions", Foreground = Brushes.White, FontSize = 19, FontWeight = FontWeights.Bold });
        stack.Children.Add(new TextBlock { Text = "Enabled types: Preference, Fact, Constraint, Decision, Relationship context and Project context. Sensitive items require explicit acknowledgement. Secret-bearing memory is always rejected.", Foreground = new SolidColorBrush(Color.FromRgb(203,213,225)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,0) });
        stack.Children.Add(new TextBlock { Text = "Permitted scopes: Global, Workspace, Project, Relationship and Session-limited. Retrieval includes only Active, unexpired, relevant memories in the current permitted scope.", Foreground = new SolidColorBrush(Color.FromRgb(203,213,225)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,6,0,0) });
        card.Child = stack; return card;
    }

    private Border CreateMemoryProposalCard()
    {
        var card = CreatePanel(); card.Margin = new Thickness(0,18,0,0);
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = "Propose memory — exact preview required", Foreground = Brushes.White, FontSize = 19, FontWeight = FontWeights.Bold });
        var title = new TextBox { Text = _memoryDraftTitle, Margin = new Thickness(0,10,0,0), Padding = new Thickness(8) };
        var statement = new TextBox { Text = _memoryDraftStatement, Margin = new Thickness(0,8,0,0), Padding = new Thickness(8), MinHeight = 72, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };
        var source = new TextBox { Text = _memoryDraftSource, Margin = new Thickness(0,8,0,0), Padding = new Thickness(8) };
        var type = EnumCombo<AssistantMemoryType>(AssistantMemoryType.Preference);
        var scope = EnumCombo<AssistantMemoryScopeType>(AssistantMemoryScopeType.Project);
        var scopeName = new TextBox { Text = "Zephyr Quill", Width = 190, Margin = new Thickness(8,0,12,0), Padding = new Thickness(8) };
        var sensitivity = EnumCombo<AssistantMemorySensitivity>(AssistantMemorySensitivity.Standard);
        var expiry = new TextBox { Text = DateTimeOffset.UtcNow.AddDays(30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Width = 120, Margin = new Thickness(8,0,0,0), Padding = new Thickness(8) };

        stack.Children.Add(Labeled("Title", title));
        stack.Children.Add(Labeled("Exact remembered statement", statement));
        stack.Children.Add(Labeled("Origin / source title", source));
        var row1 = new WrapPanel { Margin = new Thickness(0,10,0,0) };
        row1.Children.Add(new TextBlock { Text = "Type", Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center }); row1.Children.Add(type);
        row1.Children.Add(new TextBlock { Text = "Scope", Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center }); row1.Children.Add(scope); row1.Children.Add(scopeName);
        row1.Children.Add(new TextBlock { Text = "Sensitivity", Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center }); row1.Children.Add(sensitivity);
        row1.Children.Add(new TextBlock { Text = "Expiry", Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center }); row1.Children.Add(expiry);
        stack.Children.Add(row1);
        var acknowledge = new CheckBox { Content = "I acknowledge the selected sensitivity and exact text", Foreground = new SolidColorBrush(Color.FromRgb(203,213,225)), Margin = new Thickness(0,10,0,0) };
        stack.Children.Add(acknowledge);
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,12,0,0) };
        var preview = ActionButton("Preview exact memory"); var cancel = ActionButton("Cancel proposal");
        preview.Click += (_,_) =>
        {
            try
            {
                _memoryDraftTitle = title.Text; _memoryDraftStatement = statement.Text; _memoryDraftSource = source.Text;
                var expiryAt = ParseExpiry(expiry.Text);
                var selectedScope = (AssistantMemoryScopeType)scope.SelectedItem!;
                var proposal = MemoryService.Propose(title.Text, statement.Text, (AssistantMemoryType)type.SelectedItem!,
                    new AssistantMemoryScope(selectedScope, selectedScope == AssistantMemoryScopeType.Global ? null : scopeName.Text),
                    (AssistantMemorySensitivity)sensitivity.SelectedItem!,
                    new AssistantMemoryOrigin("ManualOrReviewedOutput", Guid.NewGuid().ToString("N"), source.Text, [], "User-selected explicit memory capture"),
                    expiryAt);
                _pendingMemoryProposal = proposal;
                var candidates = proposal.DuplicateCandidates.Count + proposal.ConflictingCandidates.Count;
                var payload = $"Exact text:\n{proposal.Statement}\n\nType: {proposal.Type}\nScope: {proposal.Scope.Display}\nExpiry: {proposal.ExpiresAt:yyyy-MM-dd}\nSensitivity: {proposal.Sensitivity}\nSource: {proposal.Origin.SourceTitle}\nDuplicate/conflict candidates: {candidates}\n\nSave this memory?";
                var result = MessageBox.Show(payload, "Proposed memory preview", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var saved = MemoryService.ConfirmResolved(proposal.ProposalId, Environment.UserName, acknowledge.IsChecked == true, candidates > 0);
                    _pendingMemoryProposal = null;
                    MessageBox.Show($"Memory confirmed: {saved.Status}\nScope: {saved.Scope.Display}\nAudit: {saved.ConfirmedBy} at {saved.ConfirmedAt:yyyy-MM-dd HH:mm}\nNo operational state changed.", "Memory saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else MemoryService.Cancel(proposal.ProposalId);
                ShowMemoryPage();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Memory not saved", MessageBoxButton.OK, MessageBoxImage.Warning); }
        };
        cancel.Click += (_,_) => { if (_pendingMemoryProposal is not null) MemoryService.Cancel(_pendingMemoryProposal.ProposalId); _pendingMemoryProposal = null; _memoryDraftStatement = ""; ShowMemoryPage(); };
        buttons.Children.Add(preview); buttons.Children.Add(cancel); stack.Children.Add(buttons);
        card.Child = stack; return card;
    }

    private Border CreateMemoryRecordsCard()
    {
        var card = CreatePanel(); card.Margin = new Thickness(0,18,0,0);
        var stack = new StackPanel();
        var records = MemoryService.List().OrderByDescending(x => x.UpdatedAt).ToArray();
        stack.Children.Add(new TextBlock { Text = $"Memory workspace • {records.Length} inspectable record(s)", Foreground = Brushes.White, FontSize = 19, FontWeight = FontWeights.Bold });
        if (records.Length == 0)
        {
            stack.Children.Add(new TextBlock { Text = "No memory is stored. Nothing is retained automatically.", Foreground = new SolidColorBrush(Color.FromRgb(148,163,184)), Margin = new Thickness(0,10,0,0) });
        }
        foreach (var memory in records) stack.Children.Add(CreateMemoryRecordCard(memory));
        card.Child = stack; return card;
    }

    private Border CreateMemoryRecordCard(AssistantMemoryRecord memory)
    {
        var border = new Border { BorderBrush = memory.Status == AssistantMemoryStatus.Active ? new SolidColorBrush(Color.FromRgb(56,189,248)) : new SolidColorBrush(Color.FromRgb(100,116,139)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), Padding = new Thickness(12), Margin = new Thickness(0,12,0,0) };
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = $"{memory.Type} • {memory.Status} • {memory.Scope.Display}", Foreground = new SolidColorBrush(Color.FromRgb(125,211,252)), FontWeight = FontWeights.Bold });
        var title = new TextBox { Text = memory.Title, Margin = new Thickness(0,8,0,0), Padding = new Thickness(7) };
        var statement = new TextBox { Text = memory.Statement, Margin = new Thickness(0,6,0,0), Padding = new Thickness(7), TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, MinHeight = 48 };
        stack.Children.Add(title); stack.Children.Add(statement);
        stack.Children.Add(new TextBlock { Text = $"Origin: {memory.Origin.SourceTitle} • {memory.Origin.Provenance}\nConfirmed: {memory.ConfirmedBy} at {memory.ConfirmedAt:yyyy-MM-dd HH:mm} • Expiry: {(memory.ExpiresAt is null ? "None" : memory.ExpiresAt.Value.ToString("yyyy-MM-dd"))}\nUsage: {memory.UsageCount} • Last used: {(memory.LastUsedAt is null ? "Never" : memory.LastUsedAt.Value.ToString("yyyy-MM-dd HH:mm"))} • Revisions: {memory.Revisions.Count}", Foreground = new SolidColorBrush(Color.FromRgb(148,163,184)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,0) });
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,10,0,0) };
        var save = ActionButton("Save auditable edit"); var revoke = ActionButton("Revoke"); var delete = ActionButton("Delete / forget"); var review = ActionButton("Review source and audit");
        save.IsEnabled = memory.Status == AssistantMemoryStatus.Active;
        revoke.IsEnabled = memory.Status == AssistantMemoryStatus.Active;
        delete.IsEnabled = memory.Status != AssistantMemoryStatus.Deleted;
        save.Click += (_,_) => { try { MemoryService.Edit(memory.MemoryId, title.Text, statement.Text, Environment.UserName, "User-edited in Memory workspace"); ShowMemoryPage(); } catch(Exception ex){ MessageBox.Show(ex.Message); } };
        revoke.Click += (_,_) => { MemoryService.Revoke(memory.MemoryId, Environment.UserName); ShowMemoryPage(); };
        delete.Click += (_,_) => { if(MessageBox.Show("Delete this memory? The original trusted source is preserved and an audit tombstone remains.", "Delete memory", MessageBoxButton.YesNo, MessageBoxImage.Warning)==MessageBoxResult.Yes){ MemoryService.Delete(memory.MemoryId, Environment.UserName); ShowMemoryPage(); } };
        review.Click += (_,_) => MessageBox.Show($"Memory ID: {memory.MemoryId}\nSource: {memory.Origin.SourceTitle}\nReferences: {string.Join(", ", memory.Origin.SourceReferences)}\nScope: {memory.Scope.Display}\nSensitivity: {memory.Sensitivity}\nRevisions: {memory.Revisions.Count}\nOriginal trusted LifeOS source is not deleted by memory actions.", "Memory audit", MessageBoxButton.OK, MessageBoxImage.Information);
        row.Children.Add(save); row.Children.Add(revoke); row.Children.Add(delete); row.Children.Add(review); stack.Children.Add(row);
        border.Child = stack; return border;
    }

    private static ComboBox EnumCombo<T>(T selected) where T : struct, Enum
    {
        var combo = new ComboBox { ItemsSource = Enum.GetValues<T>(), SelectedItem = selected, Margin = new Thickness(8,0,12,0), MinWidth = 120, Padding = new Thickness(6) };
        return combo;
    }

    private static FrameworkElement Labeled(string label, FrameworkElement control)
    {
        var stack = new StackPanel { Margin = new Thickness(0,8,0,0) };
        stack.Children.Add(new TextBlock { Text = label, Foreground = new SolidColorBrush(Color.FromRgb(203,213,225)), FontWeight = FontWeights.SemiBold });
        stack.Children.Add(control); return stack;
    }

    private static Button ActionButton(string text) => new() { Content = text, Padding = new Thickness(10,6,10,6), Margin = new Thickness(0,0,8,0) };

    private static DateTimeOffset? ParseExpiry(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!DateTimeOffset.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result))
            throw new FormatException("Expiry must use YYYY-MM-DD or be blank.");
        return result.Date.AddDays(1);
    }

    private IReadOnlyList<AssistantMemoryUse> GetDisclosedMemories(string question)
    {
        var project = question.Contains("Zephyr", StringComparison.OrdinalIgnoreCase) ? "Zephyr Quill" : null;
        return MemoryService.Retrieve(new AssistantMemoryQuery(question, Workspace: "Assistant", Project: project, RecordUsage: false));
    }
}
