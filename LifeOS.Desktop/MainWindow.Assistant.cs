using LifeOS.Core.Assistant;
using LifeOS.Core.AssistantPlanning;
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
    private readonly ReviewOnlyPlanningService _planningService = new();
    private readonly SessionPlanningReviewArtifactStore _planningReviewStore = new();
    private ReviewOnlyPlan? _currentPlan;
    private AssistantResponse? _currentAssistantResponse;

    private void AssistantNavButton_Click(object sender, RoutedEventArgs e) => ShowAssistantPage();

    private void ShowAssistantPage()
    {
        PageTitleTextBlock.Text = "Assistant planning";
        PageSubtitleTextBlock.Text = "Review-only planning blocks • controlled review handoff • Desktop v7.0.0-alpha.3";
        CurrentSectionTextBlock.Text = "Current section: Assistant planning";
        var root = new StackPanel();
        root.Children.Add(CreateAssistantBoundaryCard());
        root.Children.Add(CreateAssistantConfigurationCard());
        root.Children.Add(CreateAssistantQuestionCard());
        _assistantResponsePanel = new StackPanel { Margin = new Thickness(0, 18, 0, 0) };
        root.Children.Add(_assistantResponsePanel);
        if (_currentAssistantResponse is not null) _assistantResponsePanel.Children.Add(CreateAssistantResponseCard(_currentAssistantResponse));
        if (_currentPlan is not null) _assistantResponsePanel.Children.Add(CreatePlanEditorCard(_currentPlan));
        MainContentControl.Content = root;
    }

    private Border CreateAssistantBoundaryCard()
    {
        var panel = CreatePanel(); panel.BorderBrush = new SolidColorBrush(Color.FromRgb(56,189,248));
        panel.Child = new StackPanel { Children =
        {
            new TextBlock { Text="REVIEW-ONLY • EXECUTABLE: NO", Foreground=new SolidColorBrush(Color.FromRgb(125,211,252)), FontWeight=FontWeights.Bold, FontSize=13 },
            new TextBlock { Text="Source-backed answers can become editable planning blocks and one controlled review artifact — never operational state.", Foreground=new SolidColorBrush(Color.FromRgb(226,232,240)), FontSize=17, FontWeight=FontWeights.SemiBold, TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,8,0,0) },
            new TextBlock { Text="No tasks, projects, Follow-Ups, payments, calendar items, emails, automations, orchestration runs, tools, scripts, durable memory or external writes.", Foreground=new SolidColorBrush(Color.FromRgb(148,163,184)), TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,8,0,0) }
        }}; return panel;
    }

    private Border CreateAssistantConfigurationCard()
    {
        var card=CreatePanel(); card.Margin=new Thickness(0,18,0,0); var stack=new StackPanel();
        stack.Children.Add(new TextBlock { Text="Assistant source permissions", Foreground=Brushes.White, FontSize=19, FontWeight=FontWeights.Bold });
        var enabled=new CheckBox { Content="Enable read-only assistant", IsChecked=_assistantConfiguration.Enabled, Foreground=new SolidColorBrush(Color.FromRgb(226,232,240)), Margin=new Thickness(0,12,0,8) }; stack.Children.Add(enabled);
        var checks=new Dictionary<AssistantSourceType,CheckBox>();
        foreach(var source in Enum.GetValues<AssistantSourceType>()) { var check=new CheckBox { Content=SourceLabel(source), IsChecked=_assistantConfiguration.IsSourceEnabled(source), Foreground=new SolidColorBrush(Color.FromRgb(203,213,225)), Margin=new Thickness(14,3,0,0) }; checks[source]=check; stack.Children.Add(check); }
        var save=new Button { Content="Save assistant configuration", Margin=new Thickness(0,14,0,0), Padding=new Thickness(12,8,12,8), HorizontalAlignment=HorizontalAlignment.Left };
        save.Click += (_,_) => { _assistantConfiguration=new AssistantConfiguration(enabled.IsChecked==true, checks.Select(p=>new AssistantSourcePermission(p.Key,p.Value.IsChecked==true)).ToArray()); AssistantConfigurationStorage.Save(_assistantConfiguration); ShowAssistantPage(); };
        stack.Children.Add(save); card.Child=stack; return card;
    }

    private Border CreateAssistantQuestionCard()
    {
        var card=CreatePanel(); card.Margin=new Thickness(0,18,0,0); var stack=new StackPanel();
        stack.Children.Add(new TextBlock { Text="Ask, then build a review-only plan", Foreground=Brushes.White, FontSize=19, FontWeight=FontWeights.Bold });
        _assistantQuestionTextBox=new TextBox { Margin=new Thickness(0,12,0,0), MinHeight=72, TextWrapping=TextWrapping.Wrap, AcceptsReturn=true, Padding=new Thickness(10), Text="Create a review-only recovery plan for fictional Project Zephyr Quill." }; stack.Children.Add(_assistantQuestionTextBox);
        var ask=new Button { Content="Ask read-only assistant", Margin=new Thickness(0,12,0,0), Padding=new Thickness(12,8,12,8), HorizontalAlignment=HorizontalAlignment.Left }; ask.Click += (_,_)=>RunAssistantQuestion(); stack.Children.Add(ask); card.Child=stack; return card;
    }

    private void RunAssistantQuestion()
    {
        if(_assistantQuestionTextBox is null) return;
        var response=new AssistantService(LocalAssistantEvidenceSources.Create(),new LocalRuleAssistantAnswerProvider()).Ask(new AssistantRequest(_assistantQuestionTextBox.Text,_assistantConfiguration));
        _assistantSessionHistory.Add(response); _currentAssistantResponse=response; _currentPlan=null; ShowAssistantPage();
    }

    private Border CreateAssistantResponseCard(AssistantResponse response)
    {
        var card=CreatePanel(); var stack=new StackPanel();
        stack.Children.Add(new TextBlock { Text=response.Refused?"Request refused safely":"Assistant answer", Foreground=response.Refused?new SolidColorBrush(Color.FromRgb(251,146,60)):new SolidColorBrush(Color.FromRgb(134,239,172)), FontSize=19, FontWeight=FontWeights.Bold });
        stack.Children.Add(new TextBlock { Text=response.Answer, Foreground=Brushes.White, FontSize=15, TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,10,0,0) });
        stack.Children.Add(Section("Intent classified",response.Intent.ToString()));
        stack.Children.Add(Section("Sources searched",response.SourcesSearched is { Count:>0 }?string.Join(", ",response.SourcesSearched):"None"));
        stack.Children.Add(Section("Records used",response.SourcesUsed.Count==0?"None":string.Join("\n",response.SourcesUsed.Select(s=>$"• [{s.Source}] {s.Title} — {s.RecordId} — {s.Timestamp:yyyy-MM-dd HH:mm} — {s.Provenance}"))));
        stack.Children.Add(Section("Facts / assumptions / gaps / conflicts",string.Join("\n",response.Statements.Select(s=>$"• {s.Kind}: {s.Text}"))));
        stack.Children.Add(Section("Confidence",$"{response.Confidence}\nReason: {response.ConfidenceReason}"));
        stack.Children.Add(Section("Safety boundary",response.SafetyBoundary));
        var build=new Button { Content="Build review-only plan", IsEnabled=!response.Refused && response.SourcesUsed.Count>0, Margin=new Thickness(0,14,0,0), Padding=new Thickness(12,8,12,8), HorizontalAlignment=HorizontalAlignment.Left };
        build.Click += (_,_) => { if(_assistantQuestionTextBox is not null) { _currentPlan=_planningService.Build(new PlanningRequest(_assistantQuestionTextBox.Text,response)); ShowAssistantPage(); } };
        stack.Children.Add(build); card.Child=stack; return card;
    }

    private Border CreatePlanEditorCard(ReviewOnlyPlan plan)
    {
        var card=CreatePanel(); card.Margin=new Thickness(0,18,0,0); var stack=new StackPanel();
        stack.Children.Add(new TextBlock { Text="REVIEW-ONLY PLAN • EXECUTABLE: NO", Foreground=new SolidColorBrush(Color.FromRgb(250,204,21)), FontSize=19, FontWeight=FontWeights.Bold });
        stack.Children.Add(Section("Readiness",$"{plan.Readiness} — {plan.ReadinessReason}"));
        stack.Children.Add(Section("Plan summary",$"{plan.Blocks.Count} blocks • {plan.Blocks.SelectMany(b=>b.Sources).Select(s=>s.RecordId).Distinct().Count()} sources • {plan.Conflicts.Count} conflicts • {plan.MissingData.Count} missing-data items"));
        foreach(var block in plan.Blocks) stack.Children.Add(CreatePlanningBlockCard(plan,block));
        var target=new ComboBox { Width=260, HorizontalAlignment=HorizontalAlignment.Left, Margin=new Thickness(0,16,0,0), ItemsSource=Enum.GetValues<PlanningReviewSurface>(), SelectedIndex=0 };
        stack.Children.Add(new TextBlock { Text="Target review surface", Foreground=new SolidColorBrush(Color.FromRgb(125,211,252)), FontWeight=FontWeights.Bold, Margin=new Thickness(0,16,0,0) }); stack.Children.Add(target);
        var controls=new StackPanel { Orientation=Orientation.Horizontal, Margin=new Thickness(0,12,0,0) };
        var preview=new Button { Content="Preview handoff", Padding=new Thickness(12,8,12,8), Margin=new Thickness(0,0,8,0) };
        var cancel=new Button { Content="Cancel", Padding=new Thickness(12,8,12,8) };
        preview.Click += (_,_) => ShowHandoffPreview(plan,(PlanningReviewSurface)(target.SelectedItem??PlanningReviewSurface.AssistantReviewQueue));
        cancel.Click += (_,_) => { _currentPlan=null; ShowAssistantPage(); };
        controls.Children.Add(preview); controls.Children.Add(cancel); stack.Children.Add(controls); card.Child=stack; return card;
    }

    private Border CreatePlanningBlockCard(ReviewOnlyPlan plan, PlanningBlock block)
    {
        var border=new Border { BorderBrush=new SolidColorBrush(block.Status==PlanningBlockStatus.Blocked?Color.FromRgb(248,113,113):Color.FromRgb(71,85,105)), BorderThickness=new Thickness(1), CornerRadius=new CornerRadius(8), Padding=new Thickness(12), Margin=new Thickness(0,10,0,0) };
        var stack=new StackPanel();
        stack.Children.Add(new TextBlock { Text=$"{block.Type} • {block.Status}", Foreground=new SolidColorBrush(Color.FromRgb(125,211,252)), FontWeight=FontWeights.Bold });
        var title=new TextBox { Text=block.Title, Margin=new Thickness(0,8,0,0), Padding=new Thickness(6) }; var content=new TextBox { Text=block.Content, TextWrapping=TextWrapping.Wrap, AcceptsReturn=true, MinHeight=54, Margin=new Thickness(0,6,0,0), Padding=new Thickness(6) };
        stack.Children.Add(title); stack.Children.Add(content);
        stack.Children.Add(new TextBlock { Text=$"Why: {block.Rationale}", Foreground=new SolidColorBrush(Color.FromRgb(148,163,184)), TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,7,0,0) });
        stack.Children.Add(new TextBlock { Text=$"Evidence: {(block.Sources.Count==0?"None":string.Join(", ",block.Sources.Select(s=>$"{s.Source}/{s.RecordId}")))} • Assumption: {block.IsAssumption} • Conflict: {block.HasConflict} • Missing: {block.HasMissingData}", Foreground=new SolidColorBrush(Color.FromRgb(203,213,225)), TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,6,0,0) });
        var row=new StackPanel { Orientation=Orientation.Horizontal, Margin=new Thickness(0,8,0,0) };
        Button Btn(string text){ return new Button { Content=text, Padding=new Thickness(8,4,8,4), Margin=new Thickness(0,0,6,0) }; }
        var save=Btn("Save edit"); var up=Btn("Move up"); var down=Btn("Move down"); var remove=Btn("Remove");
        save.Click += (_,_)=>{ _currentPlan=_planningService.Edit(plan,block.BlockId,title.Text,content.Text); ShowAssistantPage(); };
        up.Click += (_,_)=>{ var i=plan.Blocks.ToList().FindIndex(b=>b.BlockId==block.BlockId); _currentPlan=_planningService.Reorder(plan,block.BlockId,i-1); ShowAssistantPage(); };
        down.Click += (_,_)=>{ var i=plan.Blocks.ToList().FindIndex(b=>b.BlockId==block.BlockId); _currentPlan=_planningService.Reorder(plan,block.BlockId,i+1); ShowAssistantPage(); };
        remove.Click += (_,_)=>{ _currentPlan=_planningService.Remove(plan,block.BlockId); ShowAssistantPage(); };
        row.Children.Add(save); row.Children.Add(up); row.Children.Add(down); row.Children.Add(remove); stack.Children.Add(row); border.Child=stack; return border;
    }

    private void ShowHandoffPreview(ReviewOnlyPlan plan, PlanningReviewSurface target)
    {
        var service=new PlanningReviewTransferService(_planningReviewStore); var preview=service.Preview(plan,target);
        var payload=$"Target: {preview.Target}\nOriginal question: {preview.OriginalQuestion}\nBlocks: {preview.Blocks.Count}\nAssumptions: {preview.Assumptions.Count}\nConflicts: {preview.Conflicts.Count}\nMissing data: {preview.MissingData.Count}\nProvenance: {preview.Provenance}\nExecutable: No\n\nCreate one review artifact only?";
        var result=MessageBox.Show(payload,"Controlled handoff preview",MessageBoxButton.YesNoCancel,MessageBoxImage.Question);
        if(result==MessageBoxResult.Yes) { var artifact=service.Confirm(preview,true); MessageBox.Show($"Review artifact created: {artifact.Status}\nTarget: {artifact.Target}\nExecutable: No\nNo operational record changed.","Review handoff complete",MessageBoxButton.OK,MessageBoxImage.Information); }
        else service.Cancel(preview);
        ShowAssistantPage();
    }

    private static string SourceLabel(AssistantSourceType source) => source switch { AssistantSourceType.MoneyPressure=>"Money Pressure / payment attention", AssistantSourceType.WaitingOn=>"Waiting On", AssistantSourceType.WorkSessions=>"Work Sessions", AssistantSourceType.DailyState=>"Daily State", _=>source.ToString() };
    private static FrameworkElement Section(string title,string text) { var stack=new StackPanel { Margin=new Thickness(0,14,0,0) }; stack.Children.Add(new TextBlock { Text=title, Foreground=new SolidColorBrush(Color.FromRgb(125,211,252)), FontWeight=FontWeights.Bold }); stack.Children.Add(new TextBlock { Text=text, Foreground=new SolidColorBrush(Color.FromRgb(203,213,225)), TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,5,0,0) }); return stack; }
}
