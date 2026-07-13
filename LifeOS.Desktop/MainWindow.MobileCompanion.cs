using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LifeOS.Desktop.Companion;
namespace LifeOS.Desktop;
public partial class MainWindow
{
 private readonly CompanionDesktopStore _companionStore=new(); private CompanionDesktopReceiver? _companionReceiver;
 private void AddMobileCompanionPanel(Panel root)
 {
  _companionReceiver ??= new CompanionDesktopReceiver(_companionStore); _companionReceiver.Changed -= RefreshCompanionInbox; _companionReceiver.Changed += RefreshCompanionInbox;
  var panel=CreatePanel();panel.Margin=new Thickness(0,16,0,0);var stack=new StackPanel();
  stack.Children.Add(new TextBlock{Text="Mobile Companion lane",Foreground=Brushes.White,FontSize=20,FontWeight=FontWeights.Bold});
  var status=_companionReceiver.IsReceiving?$"Receiving enabled · code {_companionReceiver.CurrentCode} · expires {_companionReceiver.ExpiresAtUtc.LocalDateTime:t} · endpoint http://{GetLanAddress()}:{LifeOS.Companion.Core.Pairing.CompanionProtocol.Port}":"Receiving disabled by default";
  stack.Children.Add(new TextBlock{Text=status,Foreground=new SolidColorBrush(Color.FromRgb(148,163,184)),Margin=new Thickness(0,6,0,10),TextWrapping=TextWrapping.Wrap});
  var actions=new WrapPanel();actions.Children.Add(CreateEvidenceActionButton(_companionReceiver.IsReceiving?"Pause receiving":"Pair Companion",()=>{try{if(_companionReceiver.IsReceiving)_companionReceiver.Stop();else _companionReceiver.Start();ShowIntegrationInboxPage();}catch(HttpListenerException ex){MessageBox.Show("Could not open local receiver. Run the included firewall command only after reviewing it.\n\n"+ex.Message,"LifeOS Companion");}}));
  actions.Children.Add(CreateEvidenceActionButton("Revoke all paired devices",()=>{var p=_companionStore.LoadPairings().Select(x=>x with{Revoked=true}).ToList();_companionStore.SavePairings(p);ShowIntegrationInboxPage();}));stack.Children.Add(actions);
  var intakes=_companionStore.LoadIntakes().OrderByDescending(x=>x.ReceivedAtUtc).ToList();stack.Children.Add(new TextBlock{Text=$"{intakes.Count(x=>x.ReviewState==IntakeReviewState.NeedsReview)} NeedsReview · {intakes.Count} retained",Foreground=Brushes.White,Margin=new Thickness(0,12,0,8)});
  foreach(var item in intakes.Take(20)){var card=CreatePanel();card.Margin=new Thickness(0,0,0,10);var c=new StackPanel();c.Children.Add(new TextBlock{Text=item.Title??"Quick Capture",Foreground=Brushes.White,FontWeight=FontWeights.Bold});c.Children.Add(new TextBlock{Text=$"{item.ReviewState} · {item.Provenance}\nCapture {Mask(item.CaptureId)} · hash {Mask(item.ContentHash)}\n{item.Body}",Foreground=new SolidColorBrush(Color.FromRgb(148,163,184)),TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,5,0,8)});var a=new WrapPanel();a.Children.Add(CreateEvidenceActionButton("Confirm evidence",()=>ReviewCompanion(item.IntakeId,IntakeReviewState.Confirmed)));a.Children.Add(CreateEvidenceActionButton("Reject evidence",()=>ReviewCompanion(item.IntakeId,IntakeReviewState.Rejected)));c.Children.Add(a);card.Child=c;stack.Children.Add(card);}
  panel.Child=stack;root.Children.Add(panel);
 }
 private void ReviewCompanion(string id,IntakeReviewState state){var list=_companionStore.LoadIntakes();var i=list.FindIndex(x=>x.IntakeId==id);if(i>=0){list[i]=list[i] with{ReviewState=state};_companionStore.SaveIntakes(list);}ShowIntegrationInboxPage();}
 private void RefreshCompanionInbox(){Dispatcher.Invoke(ShowIntegrationInboxPage);}
 private static string Mask(string value)=>value.Length<=8?value:value[..4]+"…"+value[^4..];
 private static string GetLanAddress(){foreach(var ni in NetworkInterface.GetAllNetworkInterfaces().Where(x=>x.OperationalStatus==OperationalStatus.Up)){foreach(var ua in ni.GetIPProperties().UnicastAddresses){if(ua.Address.AddressFamily==AddressFamily.InterNetwork&&!IPAddress.IsLoopback(ua.Address))return ua.Address.ToString();}}return "127.0.0.1";}
}
