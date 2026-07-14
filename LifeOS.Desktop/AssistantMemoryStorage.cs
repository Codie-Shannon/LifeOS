using LifeOS.Core.AssistantMemory;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LifeOS.Desktop;

internal sealed class ProtectedAssistantMemoryStore : IAssistantMemoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _path;
    private readonly object _sync = new();

    public ProtectedAssistantMemoryStore(string? path = null)
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LifeOS", "AssistantMemory");
        Directory.CreateDirectory(root);
        _path = path ?? Path.Combine(root, "assistant-memory.bin");
    }

    public IReadOnlyList<AssistantMemoryRecord> Load()
    {
        lock (_sync)
        {
            try
            {
                if (!File.Exists(_path)) return [];
                var protectedBytes = File.ReadAllBytes(_path);
                var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return JsonSerializer.Deserialize<List<AssistantMemoryRecord>>(bytes, JsonOptions) ?? [];
            }
            catch
            {
                return []; // Fail closed: unreadable memory is never retrieved.
            }
        }
    }

    public void Save(IReadOnlyList<AssistantMemoryRecord> records)
    {
        lock (_sync)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(records, JsonOptions);
            var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            var temp = _path + ".tmp";
            File.WriteAllBytes(temp, protectedBytes);
            File.Move(temp, _path, true);
        }
    }
}
