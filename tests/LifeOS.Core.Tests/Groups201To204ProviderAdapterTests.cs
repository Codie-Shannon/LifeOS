using System.Text.Json;using LifeOS.Core.ConfigurationReadiness;using LifeOS.Core.ProviderAdapters;using LifeOS.Shared.ProviderAdapters;using Xunit;
namespace LifeOS.Core.Tests;
public sealed class Groups201To204ProviderAdapterTests
{
 static readonly DateTimeOffset Now=new(2026,8,18,3,4,5,TimeSpan.Zero);static ProviderAdapterDraft Draft()=>new("Calendar adapter",ProviderFamily.Google,"Integration owner",ConfigurationEnvironment.Test,ProviderCapability.CalendarRead|ProviderCapability.DraftProposal,"LIFEOS_GOOGLE_TOKEN","Reference only.");
 [Fact]public void Required_fields_and_capabilities_are_validated(){var r=ProviderAdapterService.Validate(Draft()with{Name=null,Owner=null,Capabilities=ProviderCapability.None});Assert.False(r.IsValid);Assert.NotEmpty(r.ForField("adapter-name"));Assert.NotEmpty(r.ForField("adapter-owner"));Assert.NotEmpty(r.ForField("adapter-capabilities"));}
 [Fact]public void Credential_value_is_rejected(){var r=ProviderAdapterService.Validate(Draft()with{CredentialReferenceName="private-value"});Assert.Equal("reference-name",Assert.Single(r.ForField("adapter-credential-reference")).Code);}
 [Fact]public void Secret_like_notes_are_rejected(){var r=ProviderAdapterService.Validate(Draft()with{Notes="api_key=private"});Assert.Equal("secret-like-value",Assert.Single(r.ForField("adapter-notes")).Code);}
 [Fact]public void Reference_marks_adapter_ready_for_test(){var r=ProviderAdapterService.Create(Draft(),Now);Assert.Equal(ProviderAdapterState.AvailableForCredentialedTest,r.State);}
 [Fact]public void Provider_writes_remain_disabled_even_when_declared(){var r=ProviderAdapterService.Create(Draft()with{Capabilities=ProviderCapability.ProviderWrite},Now);Assert.False(ProviderAdapterService.ProviderWritesEnabled(r));}
 [Fact]public void Missing_repository_is_empty_without_file(){using Temp t=new();string p=Path.Combine(t.Path,"a.json");var r=new ProviderAdapterRepository(p).LoadResult();Assert.Empty(r.Value);Assert.False(File.Exists(p));}
 [Fact]public void Repository_is_versioned(){using Temp t=new();string p=Path.Combine(t.Path,"a.json");var repo=new ProviderAdapterRepository(p);repo.Save([ProviderAdapterService.Create(Draft(),Now)]);using var j=JsonDocument.Parse(File.ReadAllText(p));Assert.Equal("provider-adapters",j.RootElement.GetProperty("storeId").GetString());}
 [Fact]public void Unsafe_record_does_not_overwrite(){using Temp t=new();string p=Path.Combine(t.Path,"a.json");var repo=new ProviderAdapterRepository(p);var v=ProviderAdapterService.Create(Draft(),Now);repo.Save([v]);Assert.Throws<InvalidDataException>(()=>repo.Save([v with{Notes="secret=private"}]));Assert.Single(repo.Load());}
 sealed class Temp:IDisposable{public Temp(){Path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"lifeos-adapter-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Path);}public string Path{get;}public void Dispose(){if(Directory.Exists(Path))Directory.Delete(Path,true);}}
}
