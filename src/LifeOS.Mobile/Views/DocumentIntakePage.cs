using LifeOS.Core.Documents;
using LifeOS.Mobile.Core.Documents;

namespace LifeOS.Mobile.Views;

public sealed class DocumentIntakePage : ContentPage
{
    private readonly V11DocumentIntakeService service = new();
    private readonly VerticalStackLayout stack = new()
    {
        Padding = 18,
        Spacing = 12
    };

    public DocumentIntakePage()
    {
        Title = "Documents";
        BackgroundColor = Color.FromArgb("#101017");
        Content = new ScrollView { Content = stack };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RenderDashboard();
    }

    private void RenderDashboard()
    {
        var (records, duplicates) = service.BuildProofData(DateTimeOffset.Now);

        stack.Children.Clear();
        stack.Children.Add(Heading("Document intake", 32));
        stack.Children.Add(Body("Capture • review • preserve originals"));

        AddNavigationButton(
            "Capture draft",
            records.Single(record => record.Id == "doc-mobile-timesheet"),
            "Draft retained locally until deliberate review and upload.",
            record => RenderCaptureDraft(record));

        AddNavigationButton(
            "Extraction suggestions",
            records.Single(record => record.State == DocumentIntakeState.ReviewRequired),
            "Suggestions remain untrusted.",
            record => RenderSummary(
                "Extraction suggestions",
                record,
                "Extracted values remain suggestions until explicitly reviewed."));

        AddNavigationButton(
            "Exact-hash duplicate",
            records.Single(record => record.Id == "doc-receipt-copy"),
            $"Candidate only • {duplicates.Count} exact match • no automatic merge.",
            record => RenderSummary(
                "Exact-hash duplicate candidate",
                record,
                "Candidate only. No automatic merge, replacement or deletion."));

        AddNavigationButton(
            "Accepted and linked",
            records.Single(record => record.State == DocumentIntakeState.Accepted),
            "Linked to Money and Work after explicit review.",
            record => RenderSummary(
                "Accepted and linked",
                record,
                "Accepted only after explicit review; links remain read-only."));
    }

    private void RenderCaptureDraft(DocumentRecord record)
    {
        stack.Children.Clear();
        stack.Children.Add(BackButton());
        stack.Children.Add(Heading("Mobile capture draft", 30));
        stack.Children.Add(Body("Local capture queue • review-first • original-preserving"));

        stack.Children.Add(Card(
            record.Original.FileName,
            $"Document type: {record.Type}\n" +
            $"State: {record.State}\n" +
            $"Source: {record.Original.Source}\n" +
            $"Media type: {record.Original.MediaType}\n" +
            $"Size: {record.Original.SizeBytes} bytes\n" +
            "Stored locally until deliberate review and upload.",
            "DRAFT"));

        stack.Children.Add(Card(
            "Upload boundary",
            "The draft can be reviewed, corrected, cancelled or deliberately uploaded. " +
            "Nothing is automatically accepted, posted to Money or written to a provider.",
            "REVIEW REQUIRED"));
    }

    private void RenderSummary(string title, DocumentRecord record, string boundary)
    {
        stack.Children.Clear();
        stack.Children.Add(BackButton());
        stack.Children.Add(Heading(title, 30));
        stack.Children.Add(Card(
            record.Original.FileName,
            $"Type: {record.Type}\nState: {record.State}\nSource: {record.Original.Source}\n{boundary}",
            record.State.ToString().ToUpperInvariant()));
    }

    private void AddNavigationButton(
        string heading,
        DocumentRecord record,
        string note,
        Action<DocumentRecord> action)
    {
        var button = new Button
        {
            Text = $"{heading}\n{record.Original.FileName} • {record.State}\n{note}",
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#1A2030"),
            HorizontalOptions = LayoutOptions.Fill,
            Padding = 14
        };

        button.Clicked += (_, _) => action(record);
        stack.Children.Add(button);
    }

    private Button BackButton()
    {
        var button = new Button
        {
            Text = "← Back to document intake",
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#1A2030"),
            HorizontalOptions = LayoutOptions.Start,
            Padding = new Thickness(14, 10)
        };

        button.Clicked += (_, _) => RenderDashboard();
        return button;
    }

    private static Label Heading(string text, double size) => new()
    {
        Text = text,
        FontSize = size,
        FontAttributes = FontAttributes.Bold,
        TextColor = Colors.White
    };

    private static Label Body(string text) => new()
    {
        Text = text,
        TextColor = Color.FromArgb("#B8C5DE")
    };

    private static Border Card(string title, string body, string status)
    {
        return new Border
        {
            BackgroundColor = Color.FromArgb("#1A1B26"),
            Stroke = Color.FromArgb("#303347"),
            StrokeThickness = 1,
            Padding = 14,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Content = new VerticalStackLayout
            {
                Spacing = 7,
                Children =
                {
                    Heading(title, 18),
                    Body(body),
                    new Label
                    {
                        Text = status,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 12,
                        TextColor = Color.FromArgb("#AFC6FF")
                    }
                }
            }
        };
    }
}
