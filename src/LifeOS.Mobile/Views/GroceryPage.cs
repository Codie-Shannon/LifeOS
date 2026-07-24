using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace LifeOS.Mobile.Views;

public sealed class GroceryPage : ContentPage
{
    private static readonly Color PageBackground = Color.FromArgb("#101010");
    private static readonly Color CardBackground = Color.FromArgb("#1D1C27");
    private static readonly Color CardBorder = Color.FromArgb("#353148");
    private static readonly Color PrimaryText = Colors.White;
    private static readonly Color SecondaryText = Color.FromArgb("#D0CADB");
    private static readonly Color Accent = Color.FromArgb("#5168E8");
    private static readonly Color AccentText = Color.FromArgb("#A990FF");

    public GroceryPage()
    {
        Title = "Grocery";
        BackgroundColor = PageBackground;

        Content = BuildDashboard();
    }

    private View BuildDashboard()
    {
        var content = new VerticalStackLayout
        {
            Padding = new Thickness(10, 14, 10, 24),
            Spacing = 10
        };

        content.Add(Heading("Grocery dashboard"));

        content.Add(
            Body(
                "Review-first planning and in-store execution • no orders or payments"));

        content.Add(
            CreateCard(
                "Active list",
                """
                Weekly household shop
                State: Shopping
                Estimated NZD 68.40
                Manual estimate • fresh 22 Jul 16:00
                """,
                "OPEN"));

        content.Add(
            CreateCard(
                "Due essentials",
                """
                2 require review
                Accept, defer or skip explicitly
                """,
                "REVIEW"));

        content.Add(
            CreateCard(
                "Household inventory",
                """
                Out: 1
                Low: 1
                Unknown: 1
                Meal gaps require review
                """,
                "GROUP 65"));

        content.Add(
            CreateCard(
                "Shared household review",
                """
                Due routines: 2
                Receipts: 2 review
                Spend reviews: 1 review
                v13 closure needs proof
                """,
                "GROUP 66"));

        content.Add(
            CreateCard(
                "Recently completed",
                "1 recent list",
                "HISTORY"));

        var openListButton = PrimaryButton("Open shopping list");
        openListButton.Clicked += (_, _) =>
        {
            Content = BuildShoppingList();
        };

        var quickAddButton = PrimaryButton("Quick add item");
        quickAddButton.Clicked += async (_, _) =>
        {
            await DisplayAlertAsync(
                "Quick add",
                "Quick-add creates a local pending grocery item. It does not place an order.",
                "OK");
        };

        var essentialsButton = PrimaryButton("Review due essentials");
        essentialsButton.Clicked += async (_, _) =>
        {
            await DisplayAlertAsync(
                "Recurring essentials",
                "Candidates remain reviewable and are never purchased automatically.",
                "OK");
        };

        var inventoryButton = PrimaryButton("Review household inventory");
        inventoryButton.Clicked += (_, _) =>
        {
            Content = BuildInventoryReview();
        };

        var routinesButton = PrimaryButton("Review household routines");
        routinesButton.Clicked += (_, _) =>
        {
            Content = BuildHouseholdRoutineReview();
        };

        var spendingButton = PrimaryButton("Review receipts and spending");
        spendingButton.Clicked += (_, _) =>
        {
            Content = BuildReceiptAndSpendingReview();
        };

        var conflictButton = PrimaryButton("Review offline conflict");
        conflictButton.Clicked += (_, _) =>
        {
            Content = BuildOfflineConflictReview();
        };

        content.Add(openListButton);
        content.Add(quickAddButton);
        content.Add(essentialsButton);
        content.Add(inventoryButton);
        content.Add(routinesButton);
        content.Add(spendingButton);
        content.Add(conflictButton);

        return Scroll(content);
    }

    private View BuildShoppingList()
    {
        var content = new VerticalStackLayout
        {
            Padding = new Thickness(10, 14, 10, 28),
            Spacing = 10
        };

        var backButton = TextButton("\u2190 Grocery dashboard");
        backButton.Clicked += (_, _) =>
        {
            Content = BuildDashboard();
        };

        content.Add(backButton);
        content.Add(Heading("Weekly household shop"));

        content.Add(
            Body(
                "Estimated NZD 68.40 • Manual estimate • fresh 22 Jul 16:00"));

        content.Add(SectionHeading("Dairy"));

        content.Add(
            CreateCard(
                "Milk",
                """
                Requested: Milk
                Quantity: 2 L
                State: Checked
                Purchased/substitute: None
                """,
                "CHECKED"));

        content.Add(SectionHeading("Bakery"));

        var breadButton = CreateCardButton(
            "Bread",
            """
            Requested: Bread
            Quantity: 1 loaf
            State: Substituted
            Purchased/substitute: Wholemeal loaf
            User-approved substitute
            """,
            "SUBSTITUTED");

        var breadTap = new TapGestureRecognizer();

        breadTap.Tapped += (_, _) =>
        {
            Content = BuildSubstitutionDetail();
        };

        breadButton.GestureRecognizers.Add(breadTap);

        content.Add(breadButton);

        content.Add(SectionHeading("Produce"));

        content.Add(
            CreateCard(
                "Bananas",
                """
                Requested: Bananas
                Quantity: 6 each
                State: Pending
                Purchased/substitute: None
                """,
                "PENDING"));

        content.Add(SectionHeading("Cleaning"));

        content.Add(
            CreateCard(
                "Surface cleaner",
                """
                Requested: Surface cleaner
                Quantity: 1 bottle
                State: Unavailable
                Purchased/substitute: None
                Requires explicit review
                """,
                "UNAVAILABLE"));

        return Scroll(content);
    }

    private View BuildInventoryReview()
    {
        var content = new VerticalStackLayout
        {
            Padding = new Thickness(10, 14, 10, 28),
            Spacing = 12
        };

        var backButton = TextButton("\u2190 Grocery dashboard");
        backButton.Clicked += (_, _) =>
        {
            Content = BuildDashboard();
        };

        content.Add(backButton);
        content.Add(Heading("Household inventory"));

        content.Add(
            Body(
                "Stock state, meals and store context are review-first planning signals."));

        content.Add(
            CreateCard(
                "Milk",
                """
                Category: Dairy
                Stock state: Low
                On hand: 0.5 L
                Expires: 23 Jul 2026
                Review before adding to a list
                """,
                "LOW"));

        content.Add(
            CreateCard(
                "Bread",
                """
                Category: Bakery
                Stock state: Out
                Meal impact: Simple breakfast
                Review candidate only
                """,
                "OUT"));

        content.Add(
            CreateCard(
                "Rice",
                """
                Category: Pantry
                Stock state: Overstocked
                On hand: 3 kg
                Do not buy more
                """,
                "OVERSTOCKED"));

        content.Add(
            CreateCard(
                "Rice bowl: Sauce",
                """
                Required recipe ingredient missing from inventory
                Suggested target: Meal ingredients
                Manual review required before any list change
                """,
                "INGREDIENT GAP"));

        content.Add(
            CreateBoundaryCard(
                "No automatic grocery mutation",
                "Inventory and meal gaps cannot order, pay, trust prices or alter grocery lists without explicit review."));

        return Scroll(content);
    }

    private View BuildHouseholdRoutineReview()
    {
        var content = new VerticalStackLayout
        {
            Padding = new Thickness(10, 14, 10, 28),
            Spacing = 12
        };

        var backButton = TextButton("\u2190 Grocery dashboard");
        backButton.Clicked += (_, _) =>
        {
            Content = BuildDashboard();
        };

        content.Add(backButton);
        content.Add(Heading("Household routines"));

        content.Add(
            Body(
                "Shared assignments and replenishment checks require explicit review."));

        content.Add(
            CreateCard(
                "Fridge and freezer check",
                """
                Area: Kitchen
                State: Overdue
                Assigned to: CS
                Source: Synthetic household routine
                """,
                "OVERDUE"));

        content.Add(
            CreateCard(
                "Bin night reset",
                """
                Area: Household
                State: Due soon
                Assigned to: Unassigned
                Source: Synthetic household routine
                """,
                "DUE SOON"));

        content.Add(
            CreateCard(
                "Assignment: fridge-check",
                """
                Assigned to: CS
                State: Proposed
                Requires review: True
                Explicit acceptance required
                """,
                "REVIEW"));

        content.Add(
            CreateBoundaryCard(
                "No automatic assignment",
                "Household assignments, routine completion and replenishment suggestions stay review-first."));

        return Scroll(content);
    }

    private View BuildReceiptAndSpendingReview()
    {
        var content = new VerticalStackLayout
        {
            Padding = new Thickness(10, 14, 10, 28),
            Spacing = 12
        };

        var backButton = TextButton("\u2190 Grocery dashboard");
        backButton.Clicked += (_, _) =>
        {
            Content = BuildDashboard();
        };

        content.Add(backButton);
        content.Add(Heading("Receipts and spending"));

        content.Add(
            Body(
                "Receipt links and planned-vs-actual spend are review candidates, not Money or Document mutations."));

        content.Add(
            CreateCard(
                "Local supermarket receipt",
                """
                Amount: NZD 74.30
                State: Needs review
                Documents target: receipt evidence candidate
                Money target: grocery spend candidate
                """,
                "RECEIPT REVIEW"));

        content.Add(
            CreateCard(
                "Groceries",
                """
                Planned: NZD 90.00
                Actual: NZD 93.20
                State: Over plan
                Requires review: True
                """,
                "OVER PLAN"));

        content.Add(
            CreateCard(
                "v13 closure",
                """
                Desktop proof: ready
                Mobile proof: ready
                Screenshot pack: pending capture
                Validation: pending final run
                """,
                "CLOSURE"));

        content.Add(
            CreateBoundaryCard(
                "No automatic posting",
                "Receipt candidates cannot mutate Documents or Money and cannot initiate payments."));

        return Scroll(content);
    }

    private View BuildSubstitutionDetail()
    {
        var content = new VerticalStackLayout
        {
            Padding = new Thickness(10, 14, 10, 28),
            Spacing = 12
        };

        var backButton = TextButton("\u2190 Shopping list");
        backButton.Clicked += (_, _) =>
        {
            Content = BuildShoppingList();
        };

        content.Add(backButton);
        content.Add(Heading("Substitution detail"));

        content.Add(
            Body(
                "Requested and purchased values remain separate. No substitution is accepted silently."));

        content.Add(
            CreateCard(
                "Original request",
                """
                Requested item: Bread
                Requested quantity: 1 loaf
                Requested category: Bakery
                """,
                "REQUESTED"));

        content.Add(
            CreateCard(
                "Chosen substitute",
                """
                Purchased/substitute: Wholemeal loaf
                Result state: Substituted
                Original request retained: Yes
                """,
                "SUBSTITUTE"));

        content.Add(
            CreateCard(
                "User decision",
                """
                Decision: Explicitly approved by the user
                Approval source: Mobile shopping action
                Automatic substitution: No
                Review required before any replacement: Yes
                """,
                "USER APPROVED"));

        content.Add(
            CreateBoundaryCard(
                "No autonomous purchase",
                "This record documents a user-approved in-store substitution. It does not initiate an order or payment."));

        return Scroll(content);
    }

    private View BuildOfflineConflictReview()
    {
        var content = new VerticalStackLayout
        {
            Padding = new Thickness(10, 14, 10, 28),
            Spacing = 12
        };

        var backButton = TextButton("\u2190 Grocery dashboard");
        backButton.Clicked += (_, _) =>
        {
            Content = BuildDashboard();
        };

        content.Add(backButton);
        content.Add(Heading("Offline conflict review"));

        content.Add(
            Body(
                "Queued mobile actions remain local until sync. Same-item conflicts require explicit review."));

        content.Add(
            CreateCard(
                "Queued mobile action",
                """
                Item: Bananas
                Mobile change: Mark purchased
                Queue state: Waiting for sync
                Captured offline: Yes
                """,
                "QUEUED"));

        content.Add(
            CreateCard(
                "Desktop version",
                """
                Item: Bananas
                Desktop change: Quantity changed from 6 to 8
                Source freshness: Newer server version
                """,
                "DESKTOP"));

        content.Add(
            CreateCard(
                "Mobile version",
                """
                Item: Bananas
                Mobile change: Mark purchased at quantity 6
                Source freshness: Offline queued action
                """,
                "MOBILE"));

        content.Add(
            CreateCard(
                "Resolution required",
                """
                Conflict: Same grocery item changed on Desktop and Mobile
                Silent overwrite: Blocked
                Available decisions:
                • Keep Desktop version
                • Keep Mobile version
                • Resolve manually
                """,
                "REVIEW REQUIRED"));

        content.Add(
            CreateBoundaryCard(
                "No silent overwrite",
                "LifeOS preserves both versions until the user chooses a resolution."));

        return Scroll(content);
    }

    private static ScrollView Scroll(View child)
    {
        return new ScrollView
        {
            BackgroundColor = PageBackground,
            Content = child
        };
    }

    private static Label Heading(string text)
    {
        return new Label
        {
            Text = text,
            TextColor = PrimaryText,
            FontSize = 25,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 0, 0, 2)
        };
    }

    private static Label SectionHeading(string text)
    {
        return new Label
        {
            Text = text,
            TextColor = PrimaryText,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 2, 0, 0)
        };
    }

    private static Label Body(string text)
    {
        return new Label
        {
            Text = text,
            TextColor = SecondaryText,
            FontSize = 13,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }

    private static Border CreateCard(
        string title,
        string body,
        string state)
    {
        return new Border
        {
            BackgroundColor = CardBackground,
            Stroke = CardBorder,
            StrokeThickness = 1,
            Padding = new Thickness(13),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(12)
            },
            Content = BuildCardContent(
                title,
                body,
                state)
        };
    }

    private static Border CreateCardButton(
        string title,
        string body,
        string state)
    {
        var card = new Border
        {
            BackgroundColor = CardBackground,
            Stroke = CardBorder,
            StrokeThickness = 1,
            Padding = new Thickness(13),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(12)
            },
            Content = BuildCardContent(
                title,
                $"{body.Trim()}\n\nTap for substitution detail",
                state)
        };

        return card;
    }

    private static View BuildCardContent(
        string title,
        string body,
        string state)
    {
        var layout = new VerticalStackLayout
        {
            Spacing = 5
        };

        layout.Add(
            new Label
            {
                Text = title,
                TextColor = PrimaryText,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold
            });

        layout.Add(
            new Label
            {
                Text = body.Trim(),
                TextColor = SecondaryText,
                FontSize = 13,
                LineBreakMode = LineBreakMode.WordWrap
            });

        layout.Add(
            new Label
            {
                Text = state,
                TextColor = AccentText,
                FontSize = 11,
                Margin = new Thickness(0, 5, 0, 0)
            });

        return layout;
    }

    private static Border CreateBoundaryCard(
        string title,
        string body)
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#171720"),
            Stroke = CardBorder,
            StrokeThickness = 1,
            Padding = new Thickness(13),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(12)
            },
            Content = BuildCardContent(
                title,
                body,
                "BOUNDARY")
        };
    }

    private static Button PrimaryButton(string text)
    {
        return new Button
        {
            Text = text,
            TextColor = Colors.White,
            BackgroundColor = Accent,
            CornerRadius = 10,
            HeightRequest = 48,
            FontSize = 13,
            HorizontalOptions = LayoutOptions.Fill
        };
    }

    private static Button TextButton(string text)
    {
        return new Button
        {
            Text = text,
            TextColor = AccentText,
            BackgroundColor = Colors.Transparent,
            Padding = new Thickness(0),
            HorizontalOptions = LayoutOptions.Start,
            FontSize = 14
        };
    }
}
