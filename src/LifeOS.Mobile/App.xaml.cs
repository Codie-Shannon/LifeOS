using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Core.Storage;
using LifeOS.Mobile.Security;
using LifeOS.Mobile.Views;

namespace LifeOS.Mobile;

public partial class App : Application
{
    private readonly MobileKeyStore _keyStore;

    public App(MobileKeyStore keyStore)
    {
        InitializeComponent();
        _keyStore = keyStore;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loadingPage = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#11131A"),
            Content = new VerticalStackLayout
            {
                Padding = 28,
                Spacing = 18,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new ActivityIndicator
                    {
                        IsRunning = true,
                        Color = Color.FromArgb("#7C5CFC")
                    },
                    new Label
                    {
                        Text = "Starting LifeOS Full Mobile…",
                        TextColor = Colors.White,
                        FontSize = 22,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            }
        };

        var window = new Window(loadingPage);

        Dispatcher.Dispatch(async () =>
        {
            try
            {
                var key = await _keyStore.GetOrCreateKeyAsync();

                var storePath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    "lifeos-full-mobile.secure");

                IMobileLocalStore store =
                    new JsonMobileLocalStore(storePath, key);

                var foundation = new MobileFoundationService(store);
                var bootstrapper = new MobileBootstrapper(foundation);

                window.Page = await bootstrapper.CreatePageAsync();
            }
            catch (Exception ex)
            {
                window.Page = new RecoveryPage(
                    $"{ex.GetType().Name}: {ex.Message}");
            }
        });

        return window;
    }
}
