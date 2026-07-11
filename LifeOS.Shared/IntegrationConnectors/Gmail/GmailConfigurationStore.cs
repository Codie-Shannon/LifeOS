using System.Text.Json;
namespace LifeOS.Shared.IntegrationConnectors.Gmail;
public static class GmailConfigurationStore
{
 static readonly JsonSerializerOptions Options=new(){WriteIndented=true};
 public static string DirectoryPath=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"LifeOS","connectors","gmail");
 public static string FilePath=>Path.Combine(DirectoryPath,"configuration.json");
 public static GmailConnectorConfiguration Load(){EnsureTemplate(); return JsonSerializer.Deserialize<GmailConnectorConfiguration>(File.ReadAllText(FilePath),Options)??new();}
 public static void EnsureTemplate(){Directory.CreateDirectory(DirectoryPath); if(!File.Exists(FilePath)) File.WriteAllText(FilePath,JsonSerializer.Serialize(new GmailConnectorConfiguration{ClientId="REPLACE_WITH_LOCAL_GOOGLE_OAUTH_CLIENT_ID",ClientSecret="REPLACE_WITH_LOCAL_GOOGLE_OAUTH_CLIENT_SECRET",RedirectUri="http://127.0.0.1:53683/"},Options));}
}
