using LifeOS.Companion.Core.Pairing;
using LifeOS.Companion.Core.Services;
using LifeOS.Companion.Core.Storage;
using LifeOS.Companion.Security;
namespace LifeOS.Companion.Views;
public sealed class PairDevicePage : ContentPage
{
    private readonly ICompanionStore _store; private readonly PairingCredentialStore _credentials; private readonly CompanionTransferClient _client;
    private readonly Entry _endpoint = new() { Placeholder = "http://192.168.x.x:43133", Keyboard = Keyboard.Url };
    private readonly Entry _code = new() { Placeholder = "6-digit code", Keyboard = Keyboard.Numeric, MaxLength = 6 };
    private readonly Label _status = new(); private PairChallenge? _challenge;
    private readonly Button _confirm = new() { Text = "Confirm matching verification", IsEnabled = false };
    public PairDevicePage(ICompanionStore store, PairingCredentialStore credentials, CompanionTransferClient client)
    {
        _store=store; _credentials=credentials; _client=client; Title="Pair";
        var begin=new Button { Text="Verify one-time code" }; begin.Clicked += async (_,__) => await BeginAsync();
        _confirm.Clicked += async (_,__) => await ConfirmAsync();
        var revoke=new Button { Text="Revoke paired Desktop" }; revoke.Clicked += (_,__) => { _credentials.Revoke(); _status.Text="Pairing revoked on this phone."; };
        Content=new ScrollView { Content=new VerticalStackLayout { Padding=16, Spacing=12, Children={
            new Label { Text="Pair Desktop", FontSize=24, FontAttributes=FontAttributes.Bold },
            new Label { Text="Same LAN only. Desktop receiving must be explicitly enabled. No background sending." },
            _endpoint,_code,begin,_status,_confirm,revoke } } };
    }
    private async Task BeginAsync()
    {
        try { var device=await _store.GetOrCreateDeviceAsync("Galaxy S9 Companion"); _challenge=await _client.BeginPairingAsync(_endpoint.Text??"",_code.Text??"",device); _status.Text=$"Desktop: {_challenge.DesktopLabel}\nVerification: {_challenge.Verification}\nExpires: {_challenge.ExpiresAtUtc.LocalDateTime:t}"; _confirm.IsEnabled=true; }
        catch(Exception ex){ _status.Text="Pairing failed: "+ex.Message; _confirm.IsEnabled=false; }
    }
    private async Task ConfirmAsync()
    {
        if(_challenge is null)return;
        try { var result=await _client.ConfirmPairingAsync(_endpoint.Text??"",_challenge); await _credentials.SaveAsync(new PairingCredential(result.PairingId,result.DesktopLabel,(_endpoint.Text??"").TrimEnd('/'),result.SecretBase64,result.ConfirmedAtUtc)); _status.Text=$"Paired with {result.DesktopLabel}."; _confirm.IsEnabled=false; }
        catch(Exception ex){ _status.Text="Confirmation failed: "+ex.Message; }
    }
}
