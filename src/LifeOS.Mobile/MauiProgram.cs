using LifeOS.Mobile.Core.Services;
using LifeOS.Mobile.Core.Storage;
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
        builder.Services.AddSingleton<IMobileLocalStore>(sp =>
        {
            var key = sp.GetRequiredService<MobileKeyStore>().GetOrCreateKeyAsync().GetAwaiter().GetResult();
            return new JsonMobileLocalStore(Path.Combine(FileSystem.AppDataDirectory, "lifeos-full-mobile.secure"), key);
        });
        builder.Services.AddSingleton<MobileFoundationService>();
        builder.Services.AddSingleton<MobileBootstrapper>();
        return builder.Build();
    }
}
