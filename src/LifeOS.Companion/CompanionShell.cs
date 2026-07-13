using LifeOS.Companion.Core.Services;
using LifeOS.Companion.Core.Storage;
using LifeOS.Companion.Security;
using LifeOS.Companion.Views;
namespace LifeOS.Companion;
public sealed class CompanionShell : Shell
{
 public CompanionShell(ICompanionStore store,CaptureService captureService,PairingCredentialStore credentials,CompanionTransferClient client)
 { Title="LifeOS Companion"; Items.Add(new TabBar { Items={
  new ShellContent {Title="Capture",Route="capture",Content=new HomePage(store,captureService)},
  new ShellContent {Title="Outbox",Route="outbox",Content=new OutboxPage(store,credentials,client)},
  new ShellContent {Title="Pair",Route="pair",Content=new PairDevicePage(store,credentials,client)},
  new ShellContent {Title="Device",Route="device",Content=new DeviceStatusPage(store)},
  new ShellContent {Title="Settings",Route="settings",Content=new SettingsPage(store)} } }); }
}
