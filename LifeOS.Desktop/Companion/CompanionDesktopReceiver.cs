using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LifeOS.Companion.Core.Pairing;
namespace LifeOS.Desktop.Companion;
public sealed class CompanionDesktopReceiver : IDisposable
{
 private readonly CompanionDesktopStore _store; private HttpListener? _listener; private CancellationTokenSource? _cts;
 private string? _code; private DateTimeOffset _expires; private readonly Dictionary<string,(PairRequest request,string verification)> _pending=[]; private readonly HashSet<string> _nonces=[];
 public bool IsReceiving=>_listener?.IsListening==true; public string? CurrentCode=>_code; public DateTimeOffset ExpiresAtUtc=>_expires;
 public event Action? Changed;
 public CompanionDesktopReceiver(CompanionDesktopStore store){_store=store;}
 public void Start()
 { if(IsReceiving)return; _code=RandomNumberGenerator.GetInt32(100000,1000000).ToString();_expires=DateTimeOffset.UtcNow.AddMinutes(5);_listener=new HttpListener();_listener.Prefixes.Add($"http://+:{CompanionProtocol.Port}/");_listener.Start();_cts=new();_=Loop(_cts.Token);Changed?.Invoke(); }
 public void Stop(){_cts?.Cancel();try{_listener?.Stop();}catch{} _listener=null;_code=null;_pending.Clear();Changed?.Invoke();}
 private async Task Loop(CancellationToken ct){while(!ct.IsCancellationRequested&&_listener is not null){try{var c=await _listener.GetContextAsync();_=Handle(c,ct);}catch when(ct.IsCancellationRequested){break;}catch{}}}
 private async Task Handle(HttpListenerContext ctx,CancellationToken ct)
 { try { if(ctx.Request.HttpMethod!="POST"){ctx.Response.StatusCode=405;return;} var body=await new StreamReader(ctx.Request.InputStream,Encoding.UTF8).ReadToEndAsync(ct);
   if(ctx.Request.Url?.AbsolutePath=="/pair/request"){await PairRequestAsync(ctx,body);return;} if(ctx.Request.Url?.AbsolutePath=="/pair/confirm"){await PairConfirmAsync(ctx,body);return;} if(ctx.Request.Url?.AbsolutePath=="/transfer"){await TransferAsync(ctx,body);return;} ctx.Response.StatusCode=404; }
   catch{ctx.Response.StatusCode=400;} finally{try{ctx.Response.Close();}catch{}} }
 private async Task PairRequestAsync(HttpListenerContext ctx,string body)
 { var r=JsonSerializer.Deserialize<PairRequest>(body,new JsonSerializerOptions(JsonSerializerDefaults.Web)); if(r is null||r.ProtocolVersion!=CompanionProtocol.Version||DateTimeOffset.UtcNow>_expires||r.Code!=_code){ctx.Response.StatusCode=403;return;}
   var id=Guid.NewGuid().ToString("N");var verification=RandomNumberGenerator.GetInt32(100000,1000000).ToString();_pending[id]=(r,verification);await Write(ctx,new PairChallenge(CompanionProtocol.Version,id,Environment.MachineName,verification,_expires));Changed?.Invoke(); }
 private async Task PairConfirmAsync(HttpListenerContext ctx,string body)
 { var r=JsonSerializer.Deserialize<PairConfirm>(body,new JsonSerializerOptions(JsonSerializerDefaults.Web)); if(r is null||!_pending.TryGetValue(r.PairingId,out var p)||p.verification!=r.Verification||DateTimeOffset.UtcNow>_expires){ctx.Response.StatusCode=403;return;}
   var secret=Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));var list=_store.LoadPairings();list.RemoveAll(x=>x.PairingId==r.PairingId);list.Add(new DesktopPairing(r.PairingId,p.request.DeviceId,p.request.DeviceLabel,secret,DateTimeOffset.UtcNow,DateTimeOffset.UtcNow,false));_store.SavePairings(list);_pending.Remove(r.PairingId);await Write(ctx,new PairConfirmed(CompanionProtocol.Version,r.PairingId,Environment.MachineName,secret,DateTimeOffset.UtcNow));Changed?.Invoke(); }
 private async Task TransferAsync(HttpListenerContext ctx,string body)
 { var payload=JsonSerializer.Deserialize<CaptureTransfer>(body,new JsonSerializerOptions(JsonSerializerDefaults.Web));if(payload is null||payload.ProtocolVersion!=CompanionProtocol.Version||payload.Body.Length>20000||payload.Title?.Length>200||payload.Category?.Length>80){await Write(ctx,new TransferAcknowledgement(CompanionProtocol.Version,payload?.TransferId??"",payload?.CaptureId??"","",DateTimeOffset.UtcNow,TransferResult.Invalid));return;}
   var pairing=_store.LoadPairings().FirstOrDefault(x=>x.PairingId==payload.PairingId&&!x.Revoked);var ts=ctx.Request.Headers["X-LifeOS-Timestamp"]??"";var nonce=ctx.Request.Headers["X-LifeOS-Nonce"]??"";var sig=ctx.Request.Headers["X-LifeOS-Signature"]??"";
   if(pairing is null||!long.TryParse(ts,out var seconds)||Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds()-seconds)>120||string.IsNullOrWhiteSpace(nonce)||!_nonces.Add(nonce)||!CompanionAuthenticator.Verify(pairing.SecretBase64,ts,nonce,body,sig)){ctx.Response.StatusCode=401;return;}
   var intakes=_store.LoadIntakes();var existing=intakes.FirstOrDefault(x=>x.IdempotencyKey==payload.IdempotencyKey||x.CaptureId==payload.CaptureId);if(existing is not null){await Write(ctx,new TransferAcknowledgement(CompanionProtocol.Version,payload.TransferId,payload.CaptureId,existing.IntakeId,existing.ReceivedAtUtc,TransferResult.Duplicate));return;}
   var intake=new MobileCompanionIntake(Guid.NewGuid().ToString("N"),payload.CaptureId,pairing.DeviceLabel,payload.TransferId,payload.IdempotencyKey,payload.ContentHash,payload.Title,payload.Body,payload.Category,payload.CreatedAtUtc,DateTimeOffset.UtcNow,IntakeReviewState.NeedsReview,$"Mobile Companion Â· {pairing.DeviceLabel} Â· protocol {payload.ProtocolVersion}");intakes.Add(intake);_store.SaveIntakes(intakes);
   await Write(ctx,new TransferAcknowledgement(CompanionProtocol.Version,payload.TransferId,payload.CaptureId,intake.IntakeId,intake.ReceivedAtUtc,TransferResult.AcceptedForReview));Changed?.Invoke(); }
 private static async Task Write<T>(HttpListenerContext ctx,T value){ctx.Response.StatusCode=200;ctx.Response.ContentType="application/json";var bytes=JsonSerializer.SerializeToUtf8Bytes(value,new JsonSerializerOptions(JsonSerializerDefaults.Web));await ctx.Response.OutputStream.WriteAsync(bytes);}
 public void Dispose()=>Stop();
}
