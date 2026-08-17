using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Forms;
using LifeOS.Core.Grocery;
using LifeOS.Shared.Grocery;
using LifeOS.Shared.Storage;

namespace LifeOS.Desktop;

public sealed class HouseholdGroceryWorkspaceView : UserControl
{
    private readonly bool _portfolioDemo;
    private readonly GroceryPlanningService _service = new();
    private HouseholdGroceryState _state = HouseholdGroceryState.Empty;
    private IReadOnlyList<FormFieldIssue> _issues = [];
    private UserFacingProblem? _problem;
    private string? _notice;
    private string _listName = string.Empty;
    private string _itemName = string.Empty;
    private string _quantity = "1";
    private string _unit = "each";
    private string _cadence = "7";
    private string _nextDue = DateOnly.FromDateTime(DateTime.Today).AddDays(7).ToString("yyyy-MM-dd");
    private string _brand = string.Empty;
    private string _notes = string.Empty;
    private GroceryCategory _category = GroceryCategory.Custom;
    private GroceryPriority _priority = GroceryPriority.Normal;
    private bool _required = true;
    private bool _recurring;

    public HouseholdGroceryWorkspaceView(bool portfolioDemo)
    {
        _portfolioDemo = portfolioDemo;
        Background = Brush("#0C1220");
        Foreground = Brushes.White;
        FontFamily = new FontFamily("Segoe UI");
        LoadState();
        Render();
    }

    private void LoadState()
    {
        try { _state = HouseholdGroceryStorage.Load(); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            _state = HouseholdGroceryState.Empty;
            _problem = UserFacingProblemFactory.FromException(exception, "load household grocery planning");
        }
    }

    private void Render()
    {
        StackPanel root = new() { Margin = new Thickness(24) };
        root.Children.Add(Text(
            _portfolioDemo ? "PORTFOLIO DEMO • ISOLATED" : "ORDINARY MODE • LOCAL HOUSEHOLD",
            11, "#AFA4FF", FontWeights.SemiBold));
        root.Children.Add(Text("Household & Grocery Planning", 30, "#FFFFFF", FontWeights.Bold));
        root.Children.Add(Text(
            "Capture grocery needs and recurring essentials, then move each list through explicit local shopping states. Nothing is ordered, paid for or added to an external cart.",
            14, "#B8C5D8"));
        if (!string.IsNullOrWhiteSpace(_notice))
            root.Children.Add(Text(_notice, 12, "#83D4B3", FontWeights.SemiBold));
        if (_problem is not null) root.Children.Add(ProblemPanel(_problem));

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int activeLists = _state.Lists.Count(list => list.State is GroceryListState.Draft or GroceryListState.Ready or GroceryListState.Shopping or GroceryListState.Paused);
        int dueEssentials = _state.Essentials.Count(item => _service.CalculateEssentialState(item, today) is EssentialReviewState.Due or EssentialReviewState.DueSoon);
        int unresolved = _state.Lists.Sum(list => list.Items.Count(item => item.Required && item.State == ShoppingItemState.Pending));
        int duplicates = _service.FindDuplicateCandidates(_state.Items).Count;
        WrapPanel metrics = new() { Margin = new Thickness(0, 16, 0, 8) };
        metrics.Children.Add(Metric("Active lists", activeLists.ToString(), "Draft, ready or shopping"));
        metrics.Children.Add(Metric("Due essentials", dueEssentials.ToString(), "Review required"));
        metrics.Children.Add(Metric("Unresolved", unresolved.ToString(), "Required and pending"));
        metrics.Children.Add(Metric("Duplicates", duplicates.ToString(), "Review candidates only"));
        metrics.Children.Add(Metric("Estimated spend", "Not set", "No trusted transaction"));
        root.Children.Add(metrics);
        root.Children.Add(BuildCapture());

        root.Children.Add(Heading("Grocery lists", 21, new Thickness(0, 20, 0, 6)));
        if (_state.Lists.Count == 0)
        {
            root.Children.Add(Card(
                "No grocery lists yet",
                "Ordinary mode does not seed shopping, household or spending records. Add an item only when you want a local list.",
                "#151F30"));
        }
        else
        {
            foreach (GroceryList list in _state.Lists)
                root.Children.Add(ListCard(list));
        }

        root.Children.Add(Heading("Recurring essentials", 21, new Thickness(0, 20, 0, 6)));
        if (_state.Essentials.Count == 0)
            root.Children.Add(Card("No recurring essentials", "Cadence remains optional and review-first.", "#151F30"));
        else
            foreach (RecurringEssential essential in _state.Essentials)
                root.Children.Add(EssentialCard(essential));

        LocalStoreHealth health = HouseholdGroceryStorage.Inspect();
        root.Children.Add(Heading("Local-data boundary", 21, new Thickness(0, 20, 0, 6)));
        root.Children.Add(Card(
            "Versioned household-grocery store",
            $"State: {health.State}. Recovery is available in Local Data & Recovery. Duplicate names remain review candidates; no automatic list mutation, ordering, payment, price trust, substitution or external-cart write is exposed.",
            "#152437"));
        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = root
        };
    }

    private UIElement BuildCapture()
    {
        StackPanel body = new();
        body.Children.Add(Heading("Add a local grocery need", 20));
        body.Children.Add(Text("Required fields are validated before memory or disk changes.", 12, "#A9B6CA"));

        WrapPanel first = new();
        first.Children.Add(Input("List name *", "grocery-list-name", "Household.ListName", _listName, value => _listName = value, 490));
        first.Children.Add(Input("Item name *", "grocery-item-name", "Household.ItemName", _itemName, value => _itemName = value, 490));
        body.Children.Add(first);

        WrapPanel second = new();
        second.Children.Add(Input("Quantity *", "grocery-quantity", "Household.Quantity", _quantity, value => _quantity = value, 235));
        second.Children.Add(Input("Unit *", "grocery-unit", "Household.Unit", _unit, value => _unit = value, 235));
        second.Children.Add(Choice("Category", "Household.Category", Enum.GetValues<GroceryCategory>(), _category, value => _category = value, 235));
        second.Children.Add(Choice("Priority", "Household.Priority", Enum.GetValues<GroceryPriority>(), _priority, value => _priority = value, 235));
        body.Children.Add(second);

        WrapPanel options = new() { Margin = new Thickness(0, 4, 0, 0) };
        CheckBox required = Check("Required item", "Household.Required", _required);
        required.Checked += (_, _) => _required = true;
        required.Unchecked += (_, _) => _required = false;
        CheckBox recurring = Check("Recurring essential", "Household.Recurring", _recurring);
        recurring.Checked += (_, _) => { _recurring = true; Render(); };
        recurring.Unchecked += (_, _) => { _recurring = false; Render(); };
        options.Children.Add(required); options.Children.Add(recurring);
        body.Children.Add(options);

        if (_recurring)
        {
            WrapPanel cadence = new();
            cadence.Children.Add(Input("Cadence days *", "grocery-cadence", "Household.Cadence", _cadence, value => _cadence = value, 235));
            cadence.Children.Add(Input("Next due (YYYY-MM-DD) *", "grocery-next-due", "Household.NextDue", _nextDue, value => _nextDue = value, 490));
            body.Children.Add(cadence);
        }

        WrapPanel detail = new();
        detail.Children.Add(Input("Preferred brand", "grocery-brand", "Household.Brand", _brand, value => _brand = value, 490));
        detail.Children.Add(Input("Notes", "grocery-notes", "Household.Notes", _notes, value => _notes = value, 490));
        body.Children.Add(detail);
        Button add = ActionButton("Add to local list", "Household.Add", false);
        add.Click += (_, _) => AddItem();
        body.Children.Add(add);
        return Panel(body, new Thickness(0, 14, 0, 0));
    }

    private void AddItem()
    {
        HouseholdGroceryItemDraft draft = Draft();
        FormValidationResult validation = HouseholdGroceryCaptureService.Validate(draft);
        _issues = validation.Issues;
        if (!validation.IsValid)
        {
            _problem = new UserFacingProblem(
                "household-grocery-validation-failed", "Review the grocery fields",
                "No grocery record was added because one or more fields are invalid.",
                "Correct the highlighted fields, then try again.", true);
            Render();
            return;
        }
        try
        {
            HouseholdGroceryState candidate = HouseholdGroceryCaptureService.AddItem(_state, draft, DateTimeOffset.UtcNow);
            HouseholdGroceryStorage.Save(candidate);
            _state = candidate;
            _itemName = string.Empty; _brand = string.Empty; _notes = string.Empty;
            _issues = []; _problem = null;
            _notice = $"Added {draft.ItemName!.Trim()} to {draft.ListName!.Trim()}.";
            Render();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            _problem = UserFacingProblemFactory.FromException(exception, "save the household grocery item");
            Render();
        }
    }

    private HouseholdGroceryItemDraft Draft() => new(
        _listName, _itemName, _category, _quantity, _unit, _priority,
        _required, _recurring, _cadence, _nextDue, _brand, _notes);

    private UIElement ListCard(GroceryList list)
    {
        StackPanel body = new();
        DockPanel header = new();
        TextBlock state = Text(list.State.ToString().ToUpperInvariant(), 11, "#AFA4FF", FontWeights.SemiBold);
        DockPanel.SetDock(state, Dock.Right); header.Children.Add(state); header.Children.Add(Heading(list.Name, 18));
        body.Children.Add(header);
        int pending = list.Items.Count(item => item.Required && item.State == ShoppingItemState.Pending);
        body.Children.Add(Text(
            $"{list.Items.Count} items • {pending} required pending • {list.Currency} {list.EstimatedTotal:0.00} • {list.EstimateSource}",
            12, "#A9B6CA"));

        WrapPanel transitions = new();
        AddTransitions(transitions, list);
        body.Children.Add(transitions);
        foreach (GroceryListItem item in list.Items)
        {
            StackPanel itemBody = new();
            itemBody.Children.Add(Text(
                $"{item.RequestedName} • {item.Quantity.Quantity:0.###} {item.Quantity.Unit} • {item.Priority} • {item.State}" +
                (item.Required ? " • required" : string.Empty),
                13, "#E7ECF4", FontWeights.SemiBold));
            if (!string.IsNullOrWhiteSpace(item.Note)) itemBody.Children.Add(Text(item.Note, 11, "#A9B6CA"));
            WrapPanel actions = new();
            if (item.State == ShoppingItemState.Pending)
            {
                actions.Children.Add(ItemButton("Check", list, item, GroceryActionKind.Check));
                actions.Children.Add(ItemButton("Unavailable", list, item, GroceryActionKind.MarkUnavailable));
                actions.Children.Add(ItemButton("Skip", list, item, GroceryActionKind.Skip));
            }
            else
            {
                actions.Children.Add(ItemButton("Undo", list, item, GroceryActionKind.Undo));
                if (item.State == ShoppingItemState.Unavailable)
                    actions.Children.Add(ItemButton("Skip", list, item, GroceryActionKind.Skip));
            }
            itemBody.Children.Add(actions);
            body.Children.Add(new Border
            {
                Background = Brush("#101827"), CornerRadius = new CornerRadius(7),
                Padding = new Thickness(12), Margin = new Thickness(0, 8, 0, 0), Child = itemBody
            });
        }
        return Panel(body, new Thickness(0, 8, 0, 0));
    }

    private void AddTransitions(Panel transitions, GroceryList list)
    {
        if (list.State == GroceryListState.Draft)
            transitions.Children.Add(ListButton("Mark ready", list, GroceryListState.Ready));
        else if (list.State == GroceryListState.Ready)
        {
            transitions.Children.Add(ListButton("Start shopping", list, GroceryListState.Shopping));
            transitions.Children.Add(ListButton("Cancel", list, GroceryListState.Cancelled));
        }
        else if (list.State == GroceryListState.Shopping)
        {
            transitions.Children.Add(ListButton("Pause", list, GroceryListState.Paused));
            Button complete = ListButton("Complete", list, GroceryListState.Completed);
            complete.IsEnabled = _service.CanComplete(list);
            transitions.Children.Add(complete);
        }
        else if (list.State == GroceryListState.Paused)
        {
            transitions.Children.Add(ListButton("Resume", list, GroceryListState.Shopping));
            transitions.Children.Add(ListButton("Cancel", list, GroceryListState.Cancelled));
        }
        else if (list.State is GroceryListState.Completed or GroceryListState.Cancelled)
            transitions.Children.Add(ListButton("Archive", list, GroceryListState.Archived));
    }

    private Button ListButton(string label, GroceryList list, GroceryListState next)
    {
        Button button = ActionButton(label, $"Household.List.{next}.{list.Id}", true);
        button.Click += (_, _) => ChangeListState(list, next);
        return button;
    }

    private Button ItemButton(string label, GroceryList list, GroceryListItem item, GroceryActionKind action)
    {
        Button button = ActionButton(label, $"Household.Item.{action}.{item.Id}", true);
        button.Click += (_, _) => ChangeItem(list, item, action);
        return button;
    }

    private void ChangeListState(GroceryList list, GroceryListState next)
    {
        try
        {
            GroceryList changed = _service.Transition(list, next);
            Save(_state with { Lists = _state.Lists.Select(value => value.Id == list.Id ? changed : value).ToList() },
                $"{list.Name} is now {next.ToString().ToLowerInvariant()}.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or InvalidOperationException)
        {
            _problem = UserFacingProblemFactory.FromException(exception, "change the grocery-list state"); Render();
        }
    }

    private void ChangeItem(GroceryList list, GroceryListItem item, GroceryActionKind action)
    {
        try
        {
            GroceryList changed = _service.ApplyAction(list, action, item.Id);
            Save(_state with { Lists = _state.Lists.Select(value => value.Id == list.Id ? changed : value).ToList() },
                $"{item.RequestedName} is now {changed.Items.Single(value => value.Id == item.Id).State.ToString().ToLowerInvariant()}.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            _problem = UserFacingProblemFactory.FromException(exception, "update the grocery item"); Render();
        }
    }

    private UIElement EssentialCard(RecurringEssential essential)
    {
        GroceryItem? item = _state.Items.FirstOrDefault(value => value.Id == essential.GroceryItemId);
        StackPanel body = new();
        body.Children.Add(Heading(item?.Name ?? "Unknown item", 17));
        EssentialReviewState state = _service.CalculateEssentialState(essential, DateOnly.FromDateTime(DateTime.Today));
        body.Children.Add(Text($"Every {essential.CadenceDays} days • next due {essential.NextDue:yyyy-MM-dd} • {state}", 12, "#A9B6CA"));
        WrapPanel actions = new();
        Button defer = ActionButton("Defer 7 days", $"Household.Essential.Defer.{essential.Id}", true);
        defer.Click += (_, _) => ChangeEssential(essential, defer: true);
        Button skip = ActionButton("Skip once", $"Household.Essential.Skip.{essential.Id}", true);
        skip.Click += (_, _) => ChangeEssential(essential, defer: false);
        actions.Children.Add(defer); actions.Children.Add(skip); body.Children.Add(actions);
        return Panel(body, new Thickness(0, 8, 0, 0));
    }

    private void ChangeEssential(RecurringEssential essential, bool defer)
    {
        try
        {
            RecurringEssential changed = defer
                ? _service.Defer(essential, essential.NextDue.AddDays(7))
                : _service.SkipOnce(essential);
            Save(_state with { Essentials = _state.Essentials.Select(value => value.Id == essential.Id ? changed : value).ToList() },
                defer ? "Recurring essential deferred by seven days." : "Recurring essential skipped once.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            _problem = UserFacingProblemFactory.FromException(exception, "update the recurring essential"); Render();
        }
    }

    private void Save(HouseholdGroceryState candidate, string notice)
    {
        HouseholdGroceryStorage.Save(candidate);
        _state = candidate; _notice = notice; _problem = null; _issues = []; Render();
    }

    private UIElement Input(
        string label, string fieldId, string automationId, string value,
        Action<string> changed, double width)
    {
        StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) };
        field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        TextBox input = new()
        {
            Text = value, MinHeight = 38, Background = Brush("#101827"), Foreground = Brushes.White,
            BorderBrush = Brush("#3A4B66"), Padding = new Thickness(10, 7, 10, 7)
        };
        AutomationProperties.SetAutomationId(input, automationId);
        input.TextChanged += (_, _) => changed(input.Text);
        field.Children.Add(input);
        string error = string.Join(" ", _issues.Where(issue => issue.FieldId == fieldId).Select(issue => issue.Message));
        if (!string.IsNullOrWhiteSpace(error)) field.Children.Add(Text(error, 11, "#FF7788", FontWeights.SemiBold));
        return field;
    }

    private UIElement Choice<T>(
        string label, string automationId, IReadOnlyList<T> values, T selected,
        Action<T> changed, double width) where T : struct
    {
        StackPanel field = new() { Width = width, Margin = new Thickness(0, 7, 10, 0) };
        field.Children.Add(Text(label, 12, "#C7D2E3", FontWeights.SemiBold));
        ComboBox input = new() { ItemsSource = values, SelectedItem = selected, MinHeight = 38 };
        AutomationProperties.SetAutomationId(input, automationId);
        input.SelectionChanged += (_, _) => { if (input.SelectedItem is T value) changed(value); };
        field.Children.Add(input); return field;
    }

    private static CheckBox Check(string label, string automationId, bool value)
    {
        CheckBox check = new()
        {
            Content = label, IsChecked = value, Foreground = Brushes.White,
            Margin = new Thickness(0, 8, 24, 0), Padding = new Thickness(4)
        };
        AutomationProperties.SetAutomationId(check, automationId); return check;
    }

    private static Border ProblemPanel(UserFacingProblem problem)
    {
        StackPanel body = new();
        body.Children.Add(Heading($"{problem.Title} ({problem.Code})", 16));
        body.Children.Add(Text(problem.Detail, 12, "#E1E7F0"));
        body.Children.Add(Text($"Next: {problem.RecoveryAction}", 12, "#C5AECF"));
        Border panel = new()
        {
            Background = Brush("#251925"), BorderBrush = Brush("#C95F75"), BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9), Padding = new Thickness(15), Margin = new Thickness(0, 12, 0, 0), Child = body
        };
        AutomationProperties.SetAutomationId(panel, "Household.Problem"); return panel;
    }

    private static Button ActionButton(string label, string automationId, bool secondary)
    {
        Button button = new()
        {
            Content = label, Background = Brush(secondary ? "#25334A" : "#315E91"), Foreground = Brushes.White,
            BorderBrush = Brush(secondary ? "#405472" : "#477DB4"), Padding = new Thickness(14, 7, 14, 7),
            Margin = new Thickness(0, 8, 8, 0), MinHeight = 36
        };
        AutomationProperties.SetAutomationId(button, automationId); return button;
    }

    private static Border Metric(string label, string value, string detail)
    {
        StackPanel content = new(); content.Children.Add(Text(label, 11, "#9EACC0"));
        content.Children.Add(Text(value, 22, "#FFFFFF", FontWeights.SemiBold)); content.Children.Add(Text(detail, 11, "#9EACC0"));
        return new Border { Width = 190, MinHeight = 100, Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(14), Margin = new Thickness(0, 0, 10, 10), Child = content };
    }

    private static Border Card(string title, string body, string background)
    {
        StackPanel content = new(); content.Children.Add(Heading(title, 16)); content.Children.Add(Text(body, 12, "#C2CDDC"));
        return new Border { Background = Brush(background), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = new Thickness(0, 8, 0, 0), Child = content };
    }

    private static Border Panel(UIElement content, Thickness margin) => new() { Background = Brush("#151F30"), BorderBrush = Brush("#31445F"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(9), Padding = new Thickness(16), Margin = margin, Child = content };
    private static TextBlock Heading(string text, double size, Thickness? margin = null) => new() { Text = text, FontSize = size, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, Margin = margin ?? new Thickness(0, 0, 0, 4) };
    private static TextBlock Text(string text, double size, string color, FontWeight? weight = null) => new() { Text = text, FontSize = size, FontWeight = weight ?? FontWeights.Normal, Foreground = Brush(color), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
    private static SolidColorBrush Brush(string value) => new((Color)ColorConverter.ConvertFromString(value));
}
