using System.Text; using System.Text.Json; using LifeOS.Shared.IntegrationConnectors.GoogleCalendar;
namespace LifeOS.Shared.IntegrationConnectors.Gmail;
public static class GmailOAuthTokenStore
{
 static readonly JsonSerializerOptions Options=new(); public static string FilePath=>Path.Combine(GmailConfigurationStore.DirectoryPath,"token.cache"); public static bool Exists=>File.Exists(FilePath);
 public static GoogleOAuthToken? Load(){if(!Exists)return null; var json=Encoding.UTF8.GetString(WindowsProtectedData.Unprotect(File.ReadAllBytes(FilePath))); return JsonSerializer.Deserialize<GoogleOAuthToken>(json,Options);}
 public static void Save(GoogleOAuthToken token){Directory.CreateDirectory(GmailConfigurationStore.DirectoryPath); File.WriteAllBytes(FilePath,WindowsProtectedData.Protect(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(token,Options))));}
 public static void Delete(){if(Exists)File.Delete(FilePath);}
}
