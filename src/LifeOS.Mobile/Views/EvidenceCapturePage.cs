using LifeOS.Mobile.Core.Money;
using Microsoft.Maui.Networking;

namespace LifeOS.Mobile.Views;

public sealed class EvidenceCapturePage : ContentPage
{
    private readonly MoneyService _service;
    private readonly MoneyDashboardSnapshot _snapshot;
    private readonly Entry _fileName;
    private readonly Picker _category;
    private readonly Label _status;
    private EvidenceDraft? _draft;

    public EvidenceCapturePage(
        MoneyService service,
        MoneyDashboardSnapshot snapshot)
    {
        _service = service;
        _snapshot = snapshot;

        Title = "Receipt capture";
        BackgroundColor = MoneyVisuals.Background;

        _fileName = new Entry
        {
            Placeholder = "receipt-demo.jpg",
            Text = "receipt-demo.jpg",
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#8E93A3"),
            BackgroundColor = Color.FromArgb("#1B1E28")
        };

        _category = new Picker
        {
            Title = "Category",
            TitleColor = MoneyVisuals.Secondary,
            TextColor = Colors.White,
            ItemsSource = new[]
            {
                "Software",
                "Travel",
                "Supplies",
                "Other"
            },
            SelectedIndex = 0,
            HeightRequest = 52
        };

        _status = new Label
        {
            Text =
                "Evidence stays as a local draft until explicitly queued.",
            TextColor = MoneyVisuals.Secondary
        };

        var preview = MoneyVisuals.Action("Create safe preview");
        preview.Clicked += (_, _) =>
        {
            _draft = _service.CreateEvidenceDraft(
                _fileName.Text ?? "receipt-demo.jpg",
                183240,
                _category.SelectedItem?.ToString() ?? "Other",
                "Sanitized receipt preview",
                DateTimeOffset.UtcNow);

            _status.Text =
                $"{_draft.FileName} • {_draft.SizeBytes} bytes • draft";
            _status.TextColor = MoneyVisuals.Success;
        };

        var queue = MoneyVisuals.Action("Save and queue");
        queue.Clicked += (_, _) =>
        {
            if (_draft is null)
            {
                _status.Text = "Create a safe preview first.";
                _status.TextColor = Color.FromArgb("#FF9A9A");
                return;
            }

            _draft = _service.QueueEvidence(_draft);

            var offline =
                Connectivity.Current.NetworkAccess !=
                NetworkAccess.Internet;

            _status.Text = offline
                ? "Evidence queued securely offline."
                : "Evidence queued securely for deliberate sync.";

            _status.TextColor = MoneyVisuals.Success;
        };

        var discard = MoneyVisuals.Action(
            "Discard draft",
            Color.FromArgb("#2C3140"));

        discard.Clicked += (_, _) =>
        {
            _draft = null;
            _status.Text =
                "Draft discarded. Original source was not deleted.";
            _status.TextColor = MoneyVisuals.Warning;
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 14,
                Children =
                {
                    new Label
                    {
                        Text = "Receipt evidence",
                        FontSize = 30,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White
                    },
                    new Label
                    {
                        Text =
                            "Camera/file-picker proof boundary • sanitized demo",
                        TextColor = MoneyVisuals.Secondary
                    },
                    _fileName,
                    _category,
                    MoneyVisuals.CardView(
                        "Preview",
                        "Safe label only • original evidence preserved"),
                    _status,
                    preview,
                    queue,
                    discard
                }
            }
        };
    }
}
