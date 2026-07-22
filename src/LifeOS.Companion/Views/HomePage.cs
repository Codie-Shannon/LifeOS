using LifeOS.Companion.Core.Models;
using LifeOS.Companion.Core.Services;
using LifeOS.Companion.Core.Storage;

namespace LifeOS.Companion.Views;

public sealed class HomePage : ContentPage
{
    private readonly ICompanionStore _store;
    private readonly CaptureService _captureService;
    private readonly Entry _title = new() { Placeholder = "Optional title", MaxLength = CaptureValidator.MaxTitleLength };
    private readonly Editor _body = new() { Placeholder = "Capture a thought, task, or evidence note", AutoSize = EditorAutoSizeOption.TextChanges, HeightRequest = 150, MaxLength = CaptureValidator.MaxBodyLength };
    private readonly Picker _category = new() { Title = "Category", ItemsSource = new[] { "General", "Work", "Personal", "Evidence Draft" } };
    private readonly Label _validation = new() { TextColor = Colors.OrangeRed };
    private readonly CollectionView _list = new() { SelectionMode = SelectionMode.Single };
    private string? _editingCaptureId;

    public HomePage(ICompanionStore store, CaptureService captureService)
    {
        _store = store;
        _captureService = captureService;
        Title = "Quick Capture";
        AutomationProperties.SetHelpText(_body, "Quick Capture note body");
        _list.ItemTemplate = new DataTemplate(() =>
        {
            var title = new Label { FontAttributes = FontAttributes.Bold };
            title.SetBinding(Label.TextProperty, nameof(QuickCapture.Title));
            var preview = new Label { MaxLines = 2 };
            preview.SetBinding(Label.TextProperty, nameof(QuickCapture.Body));
            var state = new Label { FontSize = 12 };
            state.SetBinding(Label.TextProperty, nameof(QuickCapture.DeliveryState));
            return new VerticalStackLayout { Padding = new Thickness(4, 8), Children = { title, preview, state } };
        });
        _list.SelectionChanged += OnSelectionChanged;
        var save = new Button { Text = "Save to offline outbox" };
        save.Clicked += SaveClicked;
        var clear = new Button { Text = "Clear editor" };
        clear.Clicked += (_, _) => ClearEditor();
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 16, Spacing = 12,
                Children =
                {
                    new Label { Text = "LifeOS Mobile Companion", FontSize = 24, FontAttributes = FontAttributes.Bold },
                    new Label { Text = "v0.1.0-alpha.2 · Local-first · No cloud account" },
                    _title, _body, _category, _validation, save, clear,
                    new Label { Text = "Saved locally", FontSize = 20, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 12, 0, 0) },
                    _list
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshAsync();
    }

    private async void SaveClicked(object? sender, EventArgs e)
    {
        try
        {
            _validation.Text = string.Empty;
            var device = await _store.GetOrCreateDeviceAsync("Galaxy S9 Companion");
            await _captureService.SavePendingAsync(_editingCaptureId, _title.Text, _body.Text, _category.SelectedItem?.ToString(), device);
            ClearEditor();
            await RefreshAsync();
            await DisplayAlertAsync("Saved", "Capture is encrypted locally and queued as Pending. Nothing was sent.", "OK");
        }
        catch (CaptureValidationException ex) { _validation.Text = ex.Message; }
        catch (Exception ex) { await DisplayAlertAsync("Save failed", ex.Message, "OK"); }
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not QuickCapture capture) return;
        _editingCaptureId = capture.CaptureId;
        _title.Text = capture.Title;
        _body.Text = capture.Body;
        _category.SelectedItem = capture.Category;
        _validation.Text = "Editing existing Pending capture. Identity and created time will be preserved.";
        _list.SelectedItem = null;
    }

    private async Task RefreshAsync() => _list.ItemsSource = await _store.GetCapturesAsync();
    private void ClearEditor()
    {
        _editingCaptureId = null;
        _title.Text = string.Empty;
        _body.Text = string.Empty;
        _category.SelectedItem = null;
        _validation.Text = string.Empty;
    }
}
