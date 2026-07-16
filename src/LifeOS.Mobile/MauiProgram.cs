using LifeOS.Mobile.Security;
using Microsoft.Extensions.Logging;

namespace LifeOS.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<MobileKeyStore>();

        return builder.Build();
    }
}
