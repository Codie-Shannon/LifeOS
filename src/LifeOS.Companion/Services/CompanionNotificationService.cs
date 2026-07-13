using LifeOS.Companion.Core.Models;
using LifeOS.Companion.Core.Services;

namespace LifeOS.Companion.Services;

public sealed class CompanionNotificationService
{
    public async Task<bool> RequestPermissionAsync()
    {
        return await Permissions.RequestAsync<Permissions.PostNotifications>() == PermissionStatus.Granted;
    }

    public async Task ShowAsync(NotificationCategory category, bool privacySafe)
    {
#if ANDROID
        var context = Android.App.Application.Context;
        const string channelId = "lifeos_companion_private";

        var manager = (Android.App.NotificationManager?)context.GetSystemService(
            Android.Content.Context.NotificationService);
        if (manager is null)
        {
            return;
        }

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            var channel = new Android.App.NotificationChannel(
                channelId,
                "LifeOS Companion",
                Android.App.NotificationImportance.Default)
            {
                Description = "Private LifeOS Companion status only",
                LockscreenVisibility = Android.App.NotificationVisibility.Private
            };

            manager.CreateNotificationChannel(channel);
        }

        var builder = OperatingSystem.IsAndroidVersionAtLeast(26)
            ? new Android.App.Notification.Builder(context, channelId)
            : new Android.App.Notification.Builder(context);

        var notification = builder
            .SetContentTitle("LifeOS Companion")
            .SetContentText(BetaPolicy.NotificationText(category, privacySafe))
            // Android rejects notifications without a valid small icon. The app icon
            // resource can resolve to 0 on this MAUI build, so use a guaranteed
            // platform drawable for the beta checkpoint notification.
            .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
            .SetAutoCancel(true)
            .SetVisibility(Android.App.NotificationVisibility.Private)
            .Build();

        manager.Notify((int)category + 3400, notification);
#else
        await Task.CompletedTask;
#endif
    }
}
