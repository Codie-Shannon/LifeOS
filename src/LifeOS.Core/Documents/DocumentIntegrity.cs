using System.Security.Cryptography;
namespace LifeOS.Core.Documents;
public static class DocumentIntegrity
{
 public static string Sha256(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
 public static OriginalDocument PreserveOriginal(string id,string fileName,string mediaType,byte[] bytes,DateTimeOffset importedUtc,string source,string provenance)
 {
  ArgumentException.ThrowIfNullOrWhiteSpace(fileName); ArgumentException.ThrowIfNullOrWhiteSpace(mediaType); ArgumentNullException.ThrowIfNull(bytes);
  if(bytes.Length==0) throw new ArgumentException("Original document cannot be empty.",nameof(bytes));
  return new(id,fileName,mediaType,bytes.LongLength,Sha256(bytes),bytes.ToArray(),importedUtc,source,provenance);
 }
 public static void Verify(OriginalDocument original)
 {
  if(original.Bytes.LongLength!=original.SizeBytes || Sha256(original.Bytes)!=original.Sha256) throw new InvalidDataException("Original document integrity verification failed.");
 }
}
