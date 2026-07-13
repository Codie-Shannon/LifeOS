using LifeOS.Companion.Core.Models;using LifeOS.Companion.Core.Services;
namespace LifeOS.Companion.Services;
public sealed class CompanionNotificationService
{
 public async Task<bool> RequestPermissionAsync()=>await Permissions.RequestAsync<Permissions.PostNotifications>()==PermissionStatus.Granted;
 public async Task ShowAsync(NotificationCategory category,bool privacySafe)
 {
#if ANDROID
  if(!OperatingSystem.IsAndroidVersionAtLeast(26))return; var context=Android.App.Application.Context;const string channelId="lifeos_companion_private";var manager=(Android.App.NotificationManager?)context.GetSystemService(Android.Content.Context.NotificationService);if(manager is null)return;var channel=new Android.App.NotificationChannel(channelId,"LifeOS Companion",Android.App.NotificationImportance.Default){Description="Private LifeOS Companion status only"};channel.LockscreenVisibility=Android.App.NotificationVisibility.Private;manager.CreateNotificationChannel(channel);var notification=new Android.App.Notification.Builder(context,channelId).SetContentTitle("LifeOS Companion").SetContentText(BetaPolicy.NotificationText(category,privacySafe)).SetSmallIcon(context.ApplicationInfo!.Icon).SetAutoCancel(true).SetVisibility(Android.App.NotificationVisibility.Private).Build();manager.Notify((int)category+3400,notification);
#else
  await Task.CompletedTask;
#endif
 }
}
