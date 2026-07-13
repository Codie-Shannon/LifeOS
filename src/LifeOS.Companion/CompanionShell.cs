using Microsoft.Extensions.DependencyInjection;
using LifeOS.Companion.Views;

namespace LifeOS.Companion;

public sealed class CompanionShell : Shell
{
    public CompanionShell(IServiceProvider services)
    {
        Title = "LifeOS Companion";
        Items.Add(new TabBar
        {
            Items =
            {
                new ShellContent { Title = "Capture", Route = "capture", ContentTemplate = new DataTemplate(() => services.GetRequiredService<HomePage>()) },
                new ShellContent { Title = "Outbox", Route = "outbox", ContentTemplate = new DataTemplate(() => services.GetRequiredService<OutboxPage>()) },
                new ShellContent { Title = "Device", Route = "device", ContentTemplate = new DataTemplate(() => services.GetRequiredService<DeviceStatusPage>()) },
                new ShellContent { Title = "Settings", Route = "settings", ContentTemplate = new DataTemplate(() => services.GetRequiredService<SettingsPage>()) }
            }
        });
    }
}
