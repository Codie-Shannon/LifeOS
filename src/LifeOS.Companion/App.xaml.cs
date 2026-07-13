namespace LifeOS.Companion;

public partial class App : Application
{
    private readonly AppBootstrapper _bootstrapper;

    public App(AppBootstrapper bootstrapper)
    {
        InitializeComponent();
        _bootstrapper = bootstrapper;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loadingPage = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#101820"),
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 16,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new ActivityIndicator
                    {
                        IsRunning = true,
                        HeightRequest = 48,
                        WidthRequest = 48
                    },
                    new Label
                    {
                        Text = "Starting LifeOS Companion…",
                        FontSize = 20,
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Colors.White
                    }
                }
            }
        };

        var window = new Window(loadingPage);

        Dispatcher.Dispatch(async () =>
        {
            window.Page = await _bootstrapper.CreatePageAsync();
        });

        return window;
    }
}
