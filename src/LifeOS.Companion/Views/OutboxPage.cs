using LifeOS.Companion.Core.Models;
using LifeOS.Companion.Core.Pairing;
using LifeOS.Companion.Core.Services;
using LifeOS.Companion.Core.Storage;
using LifeOS.Companion.Security;
namespace LifeOS.Companion.Views;
public sealed class OutboxPage : ContentPage
{
    private readonly ICompanionStore _store; private readonly PairingCredentialStore _credentials; private readonly CompanionTransferClient _client;
    private readonly VerticalStackLayout _items = new(); private readonly Label _summary = new();
    public OutboxPage(ICompanionStore store, PairingCredentialStore credentials, CompanionTransferClient client)
    { _store=store; _credentials=credentials; _client=client; Title="Outbox"; Content=new ScrollView { Content=new VerticalStackLayout { Padding=16,Spacing=12,Children={new Label {Text="Manual outbox",FontSize=24,FontAttributes=FontAttributes.Bold},new Label {Text="Pending → Sending → Delivered only after verified Desktop acknowledgement."},_summary,_items} } }; }
    protected override async void OnAppearing(){base.OnAppearing(); await RefreshAsync();}
    private async Task RefreshAsync()
    {
        await _store.NormalizeStaleSendingAsync(); var list=await _store.GetOutboxAsync(); _summary.Text=$"{list.Count} item(s) retained"; _items.Children.Clear();
        foreach(var capture in list)
        { var state=new Label {Text=$"State: {capture.DeliveryState}"}; var send=new Button {Text=capture.DeliveryState==DeliveryState.Delivered?"Resend duplicate proof":"Send manually",IsEnabled=capture.DeliveryState!=DeliveryState.Sending};
          send.Clicked += async (_,__) => { await SendAsync(capture,state,send); };
          _items.Children.Add(new Border {Padding=12,StrokeThickness=1,Content=new VerticalStackLayout {Children={new Label {Text=capture.Title??"Quick Capture",FontAttributes=FontAttributes.Bold},new Label {Text=capture.Body,MaxLines=3},state,send}}}); }
    }
    private async Task SendAsync(QuickCapture capture, Label state, Button button)
    {
        var credential=await _credentials.LoadAsync(); if(credential is null){state.Text="Failed: Not paired";return;}
        try { button.IsEnabled=false; await _store.UpdateDeliveryStateAsync(capture.CaptureId,DeliveryState.Sending); state.Text="State: Sending"; var ack=await _client.SendAsync(credential,capture);
          if(ack.CaptureId==capture.CaptureId && ack.Result is TransferResult.AcceptedForReview or TransferResult.Duplicate){await _store.UpdateDeliveryStateAsync(capture.CaptureId,DeliveryState.Delivered);state.Text=$"State: Delivered ({ack.Result})";}
          else {await _store.UpdateDeliveryStateAsync(capture.CaptureId,DeliveryState.Rejected);state.Text=$"State: Rejected ({ack.Result})";} }
        catch(Exception ex){await _store.UpdateDeliveryStateAsync(capture.CaptureId,DeliveryState.Failed);state.Text="State: Failed — "+ex.Message;}
        finally{button.IsEnabled=true;}
    }
}
