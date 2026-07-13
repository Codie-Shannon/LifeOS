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
        var shell = _bootstrapper.CreateShell();
        return new Window(shell);
    }
}
