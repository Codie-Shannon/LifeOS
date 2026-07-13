using LifeOS.Companion.Core.Storage;
using LifeOS.Companion.Views;

namespace LifeOS.Companion;

public sealed class AppBootstrapper
{
    private readonly IServiceProvider _services;
    private readonly ICompanionStore _store;
    public AppBootstrapper(IServiceProvider services, ICompanionStore store)
    {
        _services = services;
        _store = store;
    }

    public Shell CreateShell()
    {
        try
        {
            _store.InitializeAsync().GetAwaiter().GetResult();
            _store.GetOrCreateDeviceAsync("Galaxy S9 Companion").GetAwaiter().GetResult();
            return new CompanionShell(_services);
        }
        catch (Exception ex)
        {
            return new Shell
            {
                Items = { new ShellContent { Title = "Recovery", Content = new RecoveryPage(ex.Message) } }
            };
        }
    }
}
