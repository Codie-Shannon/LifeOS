using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Core.Grocery;

namespace LifeOS.Desktop;

public sealed class GroceryPlanningView : UserControl
{
    private readonly GroceryPlanningService _service = new();
    private readonly HouseholdInventoryService _inventoryService = new();
    private readonly HouseholdWorkflowService _workflowService = new();

    private readonly (
        IReadOnlyList<GroceryItem> Items,
        IReadOnlyList<GroceryList> Lists,
        IReadOnlyList<RecurringEssential> Essentials) _data =
            GroceryProofData.Build();

    private readonly (
        IReadOnlyList<HouseholdInventoryItem> Inventory,
        IReadOnlyList<StoreProfile> Stores,
        IReadOnlyList<MealRecipe> Recipes) _household =
            HouseholdInventoryProofData.Build();

    private readonly (
        IReadOnlyList<HouseholdRoutine> Routines,
        IReadOnlyList<HouseholdAssignment> Assignments,
        IReadOnlyList<HouseholdReceiptCandidate> Receipts,
        IReadOnlyList<HouseholdSpendReview> SpendReviews,
        IReadOnlyList<V13ClosureCheck> ClosureChecks) _workflow =
            HouseholdWorkflowProofData.Build();

    private readonly ContentControl _content = new();

    private readonly Dictionary<string, Button> _navigationButtons =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly SolidColorBrush PageBackground = Brush("#0B1020");
    private static readonly SolidColorBrush RailBackground = Brush("#11182B");
    private static readonly SolidColorBrush CardBackground = Brush("#152039");
    private static readonly SolidColorBrush CardBorderBrush = Brush("#2A3857");
    private static readonly SolidColorBrush PrimaryText = Brush("#FFFFFF");
    private static readonly SolidColorBrush SecondaryText = Brush("#B8C5DF");
    private static readonly SolidColorBrush Accent = Brush("#6C63FF");
    private static readonly SolidColorBrush AccentSoft = Brush("#2A3567");
    private static readonly SolidColorBrush ButtonBackground = Brush("#171F34");
    private static readonly SolidColorBrush ButtonHover = Brush("#232D48");

    public GroceryPlanningView()
    {
        Background = PageBackground;
        Foreground = PrimaryText;
        FontFamily = new FontFamily("Segoe UI");

        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;

        Content = BuildLayout();
        ShowOverview();
    }

    private UIElement BuildLayout()
    {
        var root = new Grid
        {
            Background = PageBackground
        };

        root.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(280)
            });

        root.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

        var rail = BuildNavigationRail();

        Grid.SetColumn(rail, 0);
        root.Children.Add(rail);

        _content.Margin = new Thickness(30, 24, 30, 30);
        _content.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        _content.VerticalContentAlignment = VerticalAlignment.Stretch;

        Grid.SetColumn(_content, 1);
        root.Children.Add(_content);

        return root;
    }

    private Border BuildNavigationRail()
    {
        var navigation = new StackPanel
        {
            Margin = new Thickness(20, 24, 20, 24)
        };

        navigation.Children.Add(
            Text(
                "LIFEOS v13 - Group 66",
                12,
                SecondaryText));

        navigation.Children.Add(
            Text(
                "Grocery Planning",
                29,
                PrimaryText,
                FontWeights.Bold,
                new Thickness(0, 6, 0, 4)));

        navigation.Children.Add(
            Text(
                "Household workspace • review-first",
                13,
                SecondaryText,
                null,
                new Thickness(0, 0, 0, 18)));

        navigation.Children.Add(
            NavigationButton(
                "Overview",
                ShowOverview));

        navigation.Children.Add(
            NavigationButton(
                "Active list",
                ShowActiveList));

        navigation.Children.Add(
            NavigationButton(
                "Recurring essentials",
                ShowEssentials));

        navigation.Children.Add(
            NavigationButton(
                "Household inventory",
                ShowInventory));

        navigation.Children.Add(
            NavigationButton(
                "Meal ingredients",
                ShowMealIngredients));

        navigation.Children.Add(
            NavigationButton(
                "Store profiles",
                ShowStoreProfiles));

        navigation.Children.Add(
            NavigationButton(
                "Household routines",
                ShowHouseholdRoutines));

        navigation.Children.Add(
            NavigationButton(
                "Replenishment review",
                ShowReplenishmentReview));

        navigation.Children.Add(
            NavigationButton(
                "Receipts & spending",
                ShowReceiptsAndSpending));

        navigation.Children.Add(
            NavigationButton(
                "v13 closure",
                ShowV13Closure));

        navigation.Children.Add(
            NavigationButton(
                "Templates",
                () => ShowMessage(
                    "Grocery-list templates",
                    "Reusable templates clone into draft lists. No external cart or provider order is created.")));

        navigation.Children.Add(
            NavigationButton(
                "Catalogue",
                () => ShowMessage(
                    "Grocery catalogue",
                    "Items retain category, preferred brand, acceptable alternatives, quantity units, pack size and notes.")));

        navigation.Children.Add(
            NavigationButton(
                "History",
                () => ShowMessage(
                    "Grocery history",
                    "Manual changes, overrides, deferrals, skipped essentials and list-state changes remain auditable.")));

        navigation.Children.Add(
            BoundaryCard());

        return new Border
        {
            Background = RailBackground,
            BorderBrush = CardBorderBrush,
            BorderThickness = new Thickness(0, 0, 1, 0),
            Child = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = navigation
            }
        };
    }

    private Button NavigationButton(
        string label,
        Action action)
    {
        var button = new Button
        {
            Content = label,
            Height = 46,
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(14, 0, 14, 0),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            Foreground = PrimaryText,
            Background = ButtonBackground,
            BorderBrush = CardBorderBrush,
            BorderThickness = new Thickness(1),
            FontSize = 14,
            Cursor = System.Windows.Input.Cursors.Hand,
            Template = BuildButtonTemplate()
        };

        button.Click += (_, _) =>
        {
            SetSelectedNavigation(label);
            action();
        };

        _navigationButtons[label] = button;

        return button;
    }

    private static ControlTemplate BuildButtonTemplate()
    {
        var template = new ControlTemplate(typeof(Button));

        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "ButtonBorder";
        border.SetBinding(
            Border.BackgroundProperty,
            new System.Windows.Data.Binding("Background")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });

        border.SetBinding(
            Border.BorderBrushProperty,
            new System.Windows.Data.Binding("BorderBrush")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });

        border.SetBinding(
            Border.BorderThicknessProperty,
            new System.Windows.Data.Binding("BorderThickness")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });

        border.SetValue(
            Border.CornerRadiusProperty,
            new CornerRadius(9));

        border.SetBinding(
            Border.PaddingProperty,
            new System.Windows.Data.Binding("Padding")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });

        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));

        presenter.SetBinding(
            ContentPresenter.ContentProperty,
            new System.Windows.Data.Binding("Content")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });

        presenter.SetBinding(
            ContentPresenter.HorizontalAlignmentProperty,
            new System.Windows.Data.Binding(
                "HorizontalContentAlignment")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });

        presenter.SetBinding(
            ContentPresenter.VerticalAlignmentProperty,
            new System.Windows.Data.Binding(
                "VerticalContentAlignment")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });

        border.AppendChild(presenter);
        template.VisualTree = border;

        var hoverTrigger = new Trigger
        {
            Property = Button.IsMouseOverProperty,
            Value = true
        };

        hoverTrigger.Setters.Add(
            new Setter(
                Border.BackgroundProperty,
                ButtonHover,
                "ButtonBorder"));

        template.Triggers.Add(hoverTrigger);

        return template;
    }

    private void SetSelectedNavigation(string selected)
    {
        foreach ((string label, Button button) in _navigationButtons)
        {
            bool isSelected = string.Equals(
                label,
                selected,
                StringComparison.OrdinalIgnoreCase);

            button.Background = isSelected
                ? AccentSoft
                : ButtonBackground;

            button.BorderBrush = isSelected
                ? Accent
                : CardBorderBrush;
        }
    }

    private void ShowOverview()
    {
        SetSelectedNavigation("Overview");

        GroceryDashboard dashboard =
            _service.BuildDashboard(
                _data.Lists,
                _data.Essentials,
                new DateOnly(2026, 7, 22));

        var panel = Page(
            "Grocery Planning overview",
            "Active shopping, due essentials and planned spend remain explicit and reviewable.");

        panel.Children.Add(
            Card(
                "Active grocery list",
                $"""
                {dashboard.ActiveList!.Name}
                State: {dashboard.ActiveList.State}
                Unresolved required items: {dashboard.UnresolvedRequired}
                Estimated total: {dashboard.ActiveList.Currency} {dashboard.ActiveList.EstimatedTotal:0.00}
                Source: {dashboard.ActiveList.EstimateSource}
                """,
                "ACTIVE"));

        panel.Children.Add(
            Card(
                "Recurring essentials requiring review",
                $"""
                Due or due soon: {dashboard.DueEssentials.Count}
                Milk: overdue
                Bread: due soon
                Candidates require explicit acceptance, deferral or skip-once review.
                """,
                "REVIEW REQUIRED"));

        HouseholdInventoryDashboard inventory =
            _inventoryService.BuildDashboard(
                _household.Inventory,
                _household.Stores,
                _household.Recipes,
                new DateOnly(2026, 7, 22));

        panel.Children.Add(
            Card(
                "Household inventory context",
                $"""
                Out: {inventory.OutCount}
                Low: {inventory.LowCount}
                Unknown: {inventory.UnknownCount}
                Overstocked: {inventory.OverstockedCount}
                Review candidates: {inventory.ReviewCandidates.Count}
                """,
                "GROUP 65"));

        HouseholdWorkflowDashboard workflow =
            _workflowService.BuildDashboard(
                _workflow.Routines,
                _workflow.Assignments,
                _workflow.Receipts,
                _workflow.SpendReviews,
                _workflow.ClosureChecks,
                new DateOnly(2026, 7, 22));

        panel.Children.Add(
            Card(
                "Shared household workflow",
                $"""
                Due routines: {workflow.DueRoutines.Count}
                Assignments: {workflow.Assignments.Count(assignment => assignment.RequiresReview)} review
                Receipts: {workflow.ReceiptCandidates.Count(candidate => candidate.RequiresReview)} review
                Spend reviews: {workflow.SpendReviews.Count(review => review.RequiresReview)} review
                v13 closure: {workflow.ClosureState}
                """,
                "GROUP 66"));

        panel.Children.Add(
            Card(
                "Planning boundary",
                $"""
                {dashboard.Boundary}
                Planned grocery spend remains an estimate and is not a trusted Money transaction.
                """,
                "NO PURCHASE"));

        _content.Content = Scroll(panel);
    }

    private void ShowActiveList()
    {
        SetSelectedNavigation("Active list");

        GroceryList list = _data.Lists[0];

        var panel = Page(
            list.Name,
            $"Grouped grocery items • estimated {list.Currency} {list.EstimatedTotal:0.00} • {list.EstimateSource}");

        foreach (
            IGrouping<GroceryCategory, GroceryListItem> category in
            list.Items.GroupBy(
                item =>
                    _data.Items
                        .Single(catalogueItem =>
                            catalogueItem.Id == item.GroceryItemId)
                        .Category))
        {
            panel.Children.Add(
                Text(
                    category.Key.ToString(),
                    20,
                    PrimaryText,
                    FontWeights.SemiBold,
                    new Thickness(0, 8, 0, 10)));

            foreach (GroceryListItem item in category)
            {
                panel.Children.Add(
                    Card(
                        item.RequestedName,
                        $"""
                        Quantity: {item.Quantity.Quantity} {item.Quantity.Unit}
                        Pack size: {item.Quantity.PackSize ?? "Not set"}
                        Priority: {item.Priority}
                        Required by: {item.RequiredBy?.ToString("dd MMM yyyy") ?? "Not set"}
                        State: {item.State}
                        Purchased or substitute: {item.PurchasedName ?? "None"}
                        """,
                        item.State.ToString().ToUpperInvariant()));
            }
        }

        panel.Children.Insert(
            2,
            Card(
                "Estimated spend",
                $"""
                {list.Currency} {list.EstimatedTotal:0.00}
                Source and freshness: {list.EstimateSource}
                This is planning context only and not a trusted financial transaction.
                """,
                "ESTIMATE ONLY"));

        _content.Content = Scroll(panel);
    }

    private void ShowEssentials()
    {
        SetSelectedNavigation("Recurring essentials");

        var panel = Page(
            "Recurring essentials review",
            "Due, deferred and skipped items remain reviewable planning candidates.");

        foreach (RecurringEssential essential in _data.Essentials)
        {
            GroceryItem grocery =
                _data.Items.Single(
                    item => item.Id == essential.GroceryItemId);

            panel.Children.Add(
                Card(
                    grocery.Name,
                    $"""
                    Cadence: every {essential.CadenceDays} days
                    Next due: {essential.NextDue:dd MMM yyyy}
                    Usual quantity: {essential.UsualQuantity.Quantity} {essential.UsualQuantity.Unit}
                    Pack size: {essential.UsualQuantity.PackSize ?? "Not set"}
                    Preferred store: {essential.PreferredStore ?? "Not set"}
                    Available actions: accept into list, defer or skip once
                    """,
                    FormatEssentialState(essential.ReviewState)));
        }

        _content.Content = Scroll(panel);
    }

    private void ShowMessage(
        string title,
        string body)
    {
        var panel = Page(
            title,
            body);

        panel.Children.Add(
            Card(
                "Review-first boundary",
                "No grocery item, recurring essential or template creates a purchase, payment or external cart automatically.",
                "LOCAL ONLY"));

        _content.Content = Scroll(panel);
    }

    private void ShowInventory()
    {
        SetSelectedNavigation("Household inventory");

        HouseholdInventoryDashboard dashboard =
            _inventoryService.BuildDashboard(
                _household.Inventory,
                _household.Stores,
                _household.Recipes,
                new DateOnly(2026, 7, 22));

        var panel = Page(
            "Household inventory",
            "Stock state, expiry and cupboard checks become review candidates, not automatic purchases.");

        panel.Children.Add(
            Card(
                "Inventory summary",
                $"""
                Out: {dashboard.OutCount}
                Low: {dashboard.LowCount}
                Enough: {dashboard.EnoughCount}
                Overstocked: {dashboard.OverstockedCount}
                Unknown: {dashboard.UnknownCount}
                """,
                "STOCK STATES"));

        foreach (HouseholdInventoryItem item in _household.Inventory)
        {
            panel.Children.Add(
                Card(
                    item.Name,
                    $"""
                    Category: {item.Category}
                    Stock state: {item.StockState}
                    On hand: {item.OnHand:0.##} {item.Unit}
                    Storage: {item.StorageLocation ?? "Not set"}
                    Last checked: {item.LastChecked?.ToString("dd MMM yyyy") ?? "Needs check"}
                    Expires: {item.ExpiresOn?.ToString("dd MMM yyyy") ?? "Not set"}
                    Source: {item.SourceEvidence}
                    """,
                    item.StockState.ToString().ToUpperInvariant()));
        }

        panel.Children.Add(
            Card(
                "Inventory boundary",
                dashboard.Boundary,
                "REVIEW FIRST"));

        _content.Content = Scroll(panel);
    }

    private void ShowMealIngredients()
    {
        SetSelectedNavigation("Meal ingredients");

        HouseholdInventoryDashboard dashboard =
            _inventoryService.BuildDashboard(
                _household.Inventory,
                _household.Stores,
                _household.Recipes,
                new DateOnly(2026, 7, 22));

        var panel = Page(
            "Meal ingredient planning",
            "Recipes can reveal ingredient gaps, but they cannot clone grocery items without review.");

        foreach (MealRecipe recipe in dashboard.Recipes)
        {
            string ingredients = string.Join(
                Environment.NewLine,
                recipe.Ingredients.Select(
                    item =>
                        $"- {item.Name}: {item.Quantity.Quantity:0.##} {item.Quantity.Unit} ({(item.Required ? "required" : "optional")})"));

            panel.Children.Add(
                Card(
                    recipe.Name,
                    $"""
                    Source: {recipe.SourceEvidence}
                    Ingredients:
                    {ingredients}
                    """,
                    "MEAL PLAN"));
        }

        foreach (
            HouseholdInventoryCandidate candidate in dashboard.ReviewCandidates.Where(
                candidate => candidate.Kind == HouseholdInventoryCandidateKind.RecipeIngredientGap))
        {
            panel.Children.Add(
                Card(
                    candidate.Title,
                    $"""
                    Suggested target: {candidate.SuggestedTarget}
                    Suggested action: {candidate.SuggestedAction}
                    Source: {candidate.SourceEvidence}
                    Requires review: {candidate.RequiresReview}
                    """,
                    "INGREDIENT GAP"));
        }

        _content.Content = Scroll(panel);
    }

    private void ShowStoreProfiles()
    {
        SetSelectedNavigation("Store profiles");

        HouseholdInventoryDashboard dashboard =
            _inventoryService.BuildDashboard(
                _household.Inventory,
                _household.Stores,
                _household.Recipes,
                new DateOnly(2026, 7, 22));

        var panel = Page(
            "Store profiles and price context",
            "Store profiles help planning only. They are not live feeds, trusted prices or cart writes.");

        foreach (StoreProfile store in dashboard.StoreProfiles)
        {
            panel.Children.Add(
                Card(
                    store.Name,
                    $"""
                    Kind: {store.Kind}
                    Area: {store.Area ?? "Not set"}
                    Strong categories: {string.Join(", ", store.StrongCategories)}
                    Price context only: {store.PriceContextOnly}
                    Source: {store.SourceEvidence}
                    """,
                    "CONTEXT ONLY"));
        }

        foreach (
            HouseholdInventoryCandidate candidate in dashboard.ReviewCandidates.Where(
                candidate => candidate.Kind == HouseholdInventoryCandidateKind.StorePriceContext))
        {
            panel.Children.Add(
                Card(
                    candidate.Title,
                    $"""
                    Suggested action: {candidate.SuggestedAction}
                    Source: {candidate.SourceEvidence}
                    Requires review: {candidate.RequiresReview}
                    """,
                    "NOT TRUSTED"));
        }

        _content.Content = Scroll(panel);
    }

    private void ShowHouseholdRoutines()
    {
        SetSelectedNavigation("Household routines");

        HouseholdWorkflowDashboard dashboard =
            BuildWorkflowDashboard();

        var panel = Page(
            "Household routines",
            "Shared checks and assignments remain explicit review states.");

        foreach (HouseholdRoutine routine in dashboard.DueRoutines)
        {
            panel.Children.Add(
                Card(
                    routine.Title,
                    $"""
                    Area: {routine.Area}
                    Cadence: every {routine.CadenceDays} days
                    Next due: {routine.NextDue:dd MMM yyyy}
                    State: {routine.State}
                    Assigned to: {routine.AssignedTo ?? "Unassigned"}
                    Source: {routine.SourceEvidence}
                    """,
                    routine.State.ToString().ToUpperInvariant()));
        }

        foreach (HouseholdAssignment assignment in dashboard.Assignments)
        {
            panel.Children.Add(
                Card(
                    $"Assignment: {assignment.RoutineId}",
                    $"""
                    Assigned to: {assignment.AssignedTo}
                    Proposed for: {assignment.ProposedFor:dd MMM yyyy}
                    State: {assignment.State}
                    Requires review: {assignment.RequiresReview}
                    Source: {assignment.SourceEvidence}
                    """,
                    assignment.State.ToString().ToUpperInvariant()));
        }

        _content.Content = Scroll(panel);
    }

    private void ShowReplenishmentReview()
    {
        SetSelectedNavigation("Replenishment review");

        HouseholdInventoryDashboard inventory =
            _inventoryService.BuildDashboard(
                _household.Inventory,
                _household.Stores,
                _household.Recipes,
                new DateOnly(2026, 7, 22));

        HouseholdWorkflowDashboard workflow = BuildWorkflowDashboard();

        var panel = Page(
            "Replenishment review",
            "Inventory gaps, routines and meal needs can suggest review candidates only.");

        foreach (HouseholdInventoryCandidate candidate in _inventoryService.BuildGroceryReviewCandidates(inventory))
        {
            panel.Children.Add(
                Card(
                    candidate.Title,
                    $"""
                    Kind: {candidate.Kind}
                    Suggested target: {candidate.SuggestedTarget}
                    Suggested action: {candidate.SuggestedAction}
                    Source: {candidate.SourceEvidence}
                    Requires review: {candidate.RequiresReview}
                    """,
                    "REVIEW ONLY"));
        }

        panel.Children.Add(
            Card(
                "Routine connection",
                $"""
                Due household routines: {workflow.DueRoutines.Count}
                Replenishment suggestions stay separate from grocery-list mutation.
                """,
                "NO AUTO LIST"));

        panel.Children.Add(
            Card(
                "Replenishment boundary",
                workflow.Boundary,
                "NO ORDER"));

        _content.Content = Scroll(panel);
    }

    private void ShowReceiptsAndSpending()
    {
        SetSelectedNavigation("Receipts & spending");

        HouseholdWorkflowDashboard dashboard = BuildWorkflowDashboard();

        var panel = Page(
            "Receipt and spending review",
            "Household receipts and planned-vs-actual spending remain review candidates.");

        foreach (HouseholdReceiptCandidate receipt in dashboard.ReceiptCandidates)
        {
            panel.Children.Add(
                Card(
                    receipt.Title,
                    $"""
                    Amount: {receipt.Currency} {receipt.Amount:0.00}
                    Captured on: {receipt.CapturedOn:dd MMM yyyy}
                    State: {receipt.State}
                    Document target: {receipt.SuggestedDocumentTarget}
                    Money target: {receipt.SuggestedMoneyTarget}
                    Requires review: {receipt.RequiresReview}
                    Source: {receipt.SourceEvidence}
                    """,
                    "RECEIPT REVIEW"));
        }

        foreach (HouseholdSpendReview review in dashboard.SpendReviews)
        {
            panel.Children.Add(
                Card(
                    review.Category,
                    $"""
                    Planned: {review.Currency} {review.PlannedAmount:0.00}
                    Actual: {review.Currency} {review.ActualAmount:0.00}
                    State: {review.State}
                    Requires review: {review.RequiresReview}
                    Source: {review.SourceEvidence}
                    """,
                    review.State.ToString().ToUpperInvariant()));
        }

        _content.Content = Scroll(panel);
    }

    private void ShowV13Closure()
    {
        SetSelectedNavigation("v13 closure");

        HouseholdWorkflowDashboard dashboard = BuildWorkflowDashboard();

        var panel = Page(
            "v13 household release closure",
            "Group 66 closes the household/grocery release after proof and validation are captured.");

        panel.Children.Add(
            Card(
                "Closure state",
                $"""
                State: {dashboard.ClosureState}
                Pending proof blocks release closure until screenshots and validation are complete.
                """,
                dashboard.ClosureState.ToString().ToUpperInvariant()));

        foreach (V13ClosureCheck check in dashboard.ClosureChecks)
        {
            panel.Children.Add(
                Card(
                    check.Title,
                    $"""
                    Passed: {check.Passed}
                    Evidence: {check.Evidence}
                    """,
                    check.Passed ? "PASSED" : "PENDING"));
        }

        panel.Children.Add(
            Card(
                "Release boundary",
                dashboard.Boundary,
                "FAIL CLOSED"));

        _content.Content = Scroll(panel);
    }

    private HouseholdWorkflowDashboard BuildWorkflowDashboard() =>
        _workflowService.BuildDashboard(
            _workflow.Routines,
            _workflow.Assignments,
            _workflow.Receipts,
            _workflow.SpendReviews,
            _workflow.ClosureChecks,
            new DateOnly(2026, 7, 22));

    private static StackPanel Page(
        string title,
        string subtitle)
    {
        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        panel.Children.Add(
            Text(
                title,
                32,
                PrimaryText,
                FontWeights.Bold,
                new Thickness(0, 0, 0, 5)));

        panel.Children.Add(
            Text(
                subtitle,
                14,
                SecondaryText,
                null,
                new Thickness(0, 0, 0, 22)));

        return panel;
    }

    private static Border Card(
        string title,
        string body,
        string badge)
    {
        var layout = new Grid();

        layout.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });

        layout.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = GridLength.Auto
            });

        var content = new StackPanel();

        content.Children.Add(
            Text(
                title,
                19,
                PrimaryText,
                FontWeights.SemiBold,
                new Thickness(0, 0, 0, 8)));

        content.Children.Add(
            Text(
                body.Trim(),
                14,
                SecondaryText));

        layout.Children.Add(content);

        var badgeBorder = new Border
        {
            Background = AccentSoft,
            BorderBrush = Accent,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(9),
            Padding = new Thickness(11, 6, 11, 6),
            Margin = new Thickness(18, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Child = Text(
                badge,
                11,
                PrimaryText,
                FontWeights.SemiBold)
        };

        Grid.SetColumn(badgeBorder, 1);
        layout.Children.Add(badgeBorder);

        return new Border
        {
            Background = CardBackground,
            BorderBrush = CardBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(19),
            Margin = new Thickness(0, 0, 0, 14),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = layout
        };
    }

    private static Border BoundaryCard()
    {
        return new Border
        {
            Background = Brush("#121A2D"),
            BorderBrush = CardBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(14),
            Margin = new Thickness(0, 12, 0, 0),
            Child = new StackPanel
            {
                Children =
                {
                    Text(
                        "Review-first boundary",
                        15,
                        PrimaryText,
                        FontWeights.SemiBold,
                        new Thickness(0, 0, 0, 7)),

                    Text(
                        "No automatic purchases\nNo payment initiation\nNo external-cart mutation\nEstimates remain separate from Money",
                        12,
                        SecondaryText)
                }
            }
        };
    }

    private static TextBlock Text(
        string value,
        double size,
        Brush colour,
        FontWeight? weight = null,
        Thickness? margin = null)
    {
        return new TextBlock
        {
            Text = value,
            FontSize = size,
            Foreground = colour,
            FontWeight = weight ?? FontWeights.Normal,
            TextWrapping = TextWrapping.Wrap,
            Margin = margin ?? new Thickness(0)
        };
    }

    private static ScrollViewer Scroll(UIElement child)
    {
        return new ScrollViewer
        {
            Content = child,
            VerticalScrollBarVisibility =
                ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility =
                ScrollBarVisibility.Disabled
        };
    }

    private static string FormatEssentialState(
        EssentialReviewState state)
    {
        return state switch
        {
            EssentialReviewState.Due => "DUE",
            EssentialReviewState.DueSoon => "DUE SOON",
            EssentialReviewState.Deferred => "DEFERRED",
            EssentialReviewState.SkippedOnce => "SKIPPED ONCE",
            EssentialReviewState.Accepted => "ACCEPTED",
            _ => "CURRENT"
        };
    }

    private static SolidColorBrush Brush(string value)
    {
        return (SolidColorBrush)
            new BrushConverter()
                .ConvertFromString(value)!;
    }
}
