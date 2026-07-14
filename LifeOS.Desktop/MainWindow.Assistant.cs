using LifeOS.Core.Assistant;
using LifeOS.Shared.Assistant;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LifeOS.Desktop;

public partial class MainWindow
{
    private AssistantConfiguration _assistantConfiguration = AssistantConfigurationStorage.Load();
    private TextBox? _assistantQuestionTextBox;
    private StackPanel? _assistantResponsePanel;
    private readonly List<AssistantResponse> _assistantSessionHistory = [];

    private void AssistantNavButton_Click(object sender, RoutedEventArgs e) => ShowAssistantPage();

    private void ShowAssistantPage()
    {
        PageTitleTextBlock.Text = "Assistant";
        PageSubtitleTextBlock.Text = "Source-backed local reasoning • read-only • Desktop v7.0.0-alpha.1";
        CurrentSectionTextBlock.Text = "Current section: Assistant";

        var root = new StackPanel();
        root.Children.Add(CreateAssistantBoundaryCard());
        root.Children.Add(CreateAssistantConfigurationCard());
        root.Children.Add(CreateAssistantQuestionCard());
        _assistantResponsePanel = new StackPanel { Margin = new Thickness(0, 18, 0, 0) };
        root.Children.Add(_assistantResponsePanel);
        MainContentControl.Content = root;
    }

    private Border CreateAssistantBoundaryCard()
    {
        var panel = CreatePanel();
        panel.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
        panel.Child = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "READ-ONLY ASSISTANT", Foreground = new SolidColorBrush(Color.FromRgb(125, 211, 252)), FontWeight = FontWeights.Bold, FontSize = 13 },
                new TextBlock { Text = "Answers use approved local LifeOS sources only. The assistant cannot send, execute, approve, confirm, continue orchestration or mutate state.", Foreground = new SolidColorBrush(Color.FromRgb(226, 232, 240)), FontSize = 17, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,0) },
                new TextBlock { Text = "No external web search, connector writes, scripts, processes, plugins, durable memory or hidden tools.", Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,8,0,0) }
            }
        };
        return panel;
    }

    private Border CreateAssistantConfigurationCard()
    {
        var card = CreatePanel(); card.Margin = new Thickness(0,18,0,0);
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = "Assistant configuration", Foreground = Brushes.White, FontSize = 19, FontWeight = FontWeights.Bold });
        var enabled = new CheckBox { Content = "Enable read-only assistant", IsChecked = _assistantConfiguration.Enabled, Foreground = new SolidColorBrush(Color.FromRgb(226,232,240)), Margin = new Thickness(0,12,0,8) };
        stack.Children.Add(enabled);
        var checks = new Dictionary<AssistantSourceType, CheckBox>();
        foreach (var source in Enum.GetValues<AssistantSourceType>())
        {
            var check = new CheckBox { Content = source.ToString(), IsChecked = _assistantConfiguration.IsSourceEnabled(source), Foreground = new SolidColorBrush(Color.FromRgb(203,213,225)), Margin = new Thickness(14,3,0,0) };
            checks[source] = check; stack.Children.Add(check);
        }
        var save = new Button { Content = "Save assistant configuration", Margin = new Thickness(0,14,0,0), Padding = new Thickness(12,8,12,8), HorizontalAlignment = HorizontalAlignment.Left };
        save.Click += (_, _) =>
        {
            _assistantConfiguration = new AssistantConfiguration(enabled.IsChecked == true, checks.Select(pair => new AssistantSourcePermission(pair.Key, pair.Value.IsChecked == true)).ToArray());
            AssistantConfigurationStorage.Save(_assistantConfiguration);
            ShowAssistantPage();
        };
        stack.Children.Add(save); card.Child = stack; return card;
    }

    private Border CreateAssistantQuestionCard()
    {
        var card = CreatePanel(); card.Margin = new Thickness(0,18,0,0);
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = "Ask about approved local state", Foreground = Brushes.White, FontSize = 19, FontWeight = FontWeights.Bold });
        _assistantQuestionTextBox = new TextBox { Margin = new Thickness(0,12,0,0), MinHeight = 72, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Padding = new Thickness(10), Text = "Why is the door invoice OCR proof waiting?" };
        stack.Children.Add(_assistantQuestionTextBox);
        var ask = new Button { Content = "Ask read-only assistant", Margin = new Thickness(0,12,0,0), Padding = new Thickness(12,8,12,8), HorizontalAlignment = HorizontalAlignment.Left };
        ask.Click += (_, _) => RunAssistantQuestion(); stack.Children.Add(ask);
        card.Child = stack; return card;
    }

    private void RunAssistantQuestion()
    {
        if (_assistantResponsePanel is null || _assistantQuestionTextBox is null) return;
        var service = new AssistantService(LocalAssistantEvidenceSources.Create(), new LocalRuleAssistantAnswerProvider());
        var response = service.Ask(new AssistantRequest(_assistantQuestionTextBox.Text, _assistantConfiguration));
        _assistantSessionHistory.Add(response);
        _assistantResponsePanel.Children.Clear();
        _assistantResponsePanel.Children.Add(CreateAssistantResponseCard(response));
    }

    private Border CreateAssistantResponseCard(AssistantResponse response)
    {
        var card = CreatePanel();
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = response.Refused ? "Request refused safely" : "Assistant answer", Foreground = response.Refused ? new SolidColorBrush(Color.FromRgb(251,146,60)) : new SolidColorBrush(Color.FromRgb(134,239,172)), FontSize = 19, FontWeight = FontWeights.Bold });
        stack.Children.Add(new TextBlock { Text = response.Answer, Foreground = Brushes.White, FontSize = 15, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,10,0,0) });
        stack.Children.Add(Section("Sources used", response.SourcesUsed.Count == 0 ? "None" : string.Join("\n", response.SourcesUsed.Select(s => $"• [{s.Source}] {s.Title} — {s.RecordId} — {s.Timestamp:yyyy-MM-dd HH:mm} — {s.Provenance}"))));
        stack.Children.Add(Section("Fact / inference / missing data / uncertainty", string.Join("\n", response.Statements.Select(s => $"• {s.Kind}: {s.Text}"))));
        stack.Children.Add(Section("Confidence", response.Confidence));
        if (response.Suggestion is not null)
            stack.Children.Add(Section("Non-executing suggestion for review", $"{response.Suggestion.Title}\n{response.Suggestion.Rationale}\nReview route: {response.Suggestion.ReviewRoute}\nExecutable: No"));
        stack.Children.Add(Section("Safety boundary", response.SafetyBoundary));
        card.Child = stack; return card;
    }

    private static FrameworkElement Section(string title, string text)
    {
        var stack = new StackPanel { Margin = new Thickness(0,14,0,0) };
        stack.Children.Add(new TextBlock { Text = title, Foreground = new SolidColorBrush(Color.FromRgb(125,211,252)), FontWeight = FontWeights.Bold });
        stack.Children.Add(new TextBlock { Text = text, Foreground = new SolidColorBrush(Color.FromRgb(203,213,225)), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0,5,0,0) });
        return stack;
    }
}
