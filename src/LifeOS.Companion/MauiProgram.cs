using LifeOS.Companion.Core.Security;
using LifeOS.Companion.Core.Services;
using LifeOS.Companion.Core.Storage;
using LifeOS.Companion.Security;
using LifeOS.Companion.Views;
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
        builder.Services.AddSingleton<IFieldProtector>(sp =>
        {
            var key = sp.GetRequiredService<SecureStorageKeyStore>().GetOrCreateKeyAsync().GetAwaiter().GetResult();
            return new AesGcmFieldProtector(key);
        });
        builder.Services.AddSingleton<ICompanionStore>(sp =>
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "lifeos-companion.db3");
            return new SQLiteCompanionStore(path, sp.GetRequiredService<IFieldProtector>());
        });
        builder.Services.AddSingleton<CaptureService>();
        builder.Services.AddSingleton<AppBootstrapper>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<OutboxPage>();
        builder.Services.AddTransient<DeviceStatusPage>();
        builder.Services.AddTransient<SettingsPage>();
        return builder.Build();
    }
}
