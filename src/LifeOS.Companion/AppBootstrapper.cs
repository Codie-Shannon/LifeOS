using LifeOS.Companion.Core.Security;
using LifeOS.Companion.Core.Services;
using LifeOS.Companion.Core.Storage;
using LifeOS.Companion.Security;
using LifeOS.Companion.Views;
namespace LifeOS.Companion;
public sealed class AppBootstrapper
{
 private readonly IServiceProvider _services; private readonly SecureStorageKeyStore _keyStore;
 public AppBootstrapper(IServiceProvider services,SecureStorageKeyStore keyStore){_services=services;_keyStore=keyStore;}
 public async Task<Page> CreatePageAsync()
 { try { var key=await _keyStore.GetOrCreateKeyAsync(); var databasePath=Path.Combine(FileSystem.AppDataDirectory,"lifeos-companion.db3"); var store=new SQLiteCompanionStore(databasePath,new AesGcmFieldProtector(key)); await store.InitializeAsync(); await store.NormalizeStaleSendingAsync(); await store.GetOrCreateDeviceAsync("Galaxy S9 Companion"); return new CompanionShell(store,new CaptureService(store),new PairingCredentialStore(),new CompanionTransferClient()); }
 catch(Exception ex){return new RecoveryPage($"{ex.GetType().Name}: {ex.Message}");} }
}
