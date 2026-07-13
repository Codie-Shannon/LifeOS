using LifeOS.Companion.Security;
using Microsoft.Extensions.Logging;

namespace LifeOS.Companion;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<SecureStorageKeyStore>();
        builder.Services.AddSingleton<AppBootstrapper>();

        return builder.Build();
    }
}
