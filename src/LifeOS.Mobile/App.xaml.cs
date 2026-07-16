namespace LifeOS.Mobile;

public partial class App : Application
{
    private readonly MobileBootstrapper _bootstrapper;
    public App(MobileBootstrapper bootstrapper) { InitializeComponent(); _bootstrapper = bootstrapper; }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loading = new ContentPage { BackgroundColor = Color.FromArgb("#11131A"), Content = new VerticalStackLayout
        {
            Padding = 28, Spacing = 18, VerticalOptions = LayoutOptions.Center,
            Children = { new ActivityIndicator { IsRunning = true }, new Label { Text = "Starting LifeOS Full Mobile…", TextColor = Colors.White, FontSize = 22, HorizontalTextAlignment = TextAlignment.Center } }
        }};
        var window = new Window(loading);
        Dispatcher.Dispatch(async () => window.Page = await _bootstrapper.CreatePageAsync());
        return window;
    }
}
