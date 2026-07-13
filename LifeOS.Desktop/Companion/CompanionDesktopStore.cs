using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
namespace LifeOS.Desktop.Companion;
public sealed class CompanionDesktopStore
{
 private static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web){WriteIndented=true};
 private readonly string _root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"LifeOS","mobile-companion");
 private string Pairings=>Path.Combine(_root,"pairings.json"); private string Intakes=>Path.Combine(_root,"intakes.json");
 public CompanionDesktopStore(){Directory.CreateDirectory(_root);}
 public List<DesktopPairing> LoadPairings(){ if(!File.Exists(Pairings))return []; try{return JsonSerializer.Deserialize<List<DesktopPairing>>(Unprotect(File.ReadAllBytes(Pairings)),Json)??[];}catch{return[];} }
 public void SavePairings(List<DesktopPairing> value)=>Atomic(Pairings,Protect(JsonSerializer.SerializeToUtf8Bytes(value,Json)));
 public List<MobileCompanionIntake> LoadIntakes()=>File.Exists(Intakes)?JsonSerializer.Deserialize<List<MobileCompanionIntake>>(File.ReadAllText(Intakes),Json)??[]:[];
 public void SaveIntakes(List<MobileCompanionIntake> value)=>Atomic(Intakes,JsonSerializer.SerializeToUtf8Bytes(value,Json));
 private static byte[] Protect(byte[] b)=>ProtectedData.Protect(b,null,DataProtectionScope.CurrentUser); private static byte[] Unprotect(byte[] b)=>ProtectedData.Unprotect(b,null,DataProtectionScope.CurrentUser);
 private static void Atomic(string path,byte[] bytes){var temp=path+".tmp";File.WriteAllBytes(temp,bytes);File.Move(temp,path,true);}
}
