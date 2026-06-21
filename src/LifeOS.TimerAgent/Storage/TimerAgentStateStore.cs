using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LifeOS.TimerAgent.Storage;

public sealed class TimerAgentStateStore
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public string StateFilePath { get; }

    public TimerAgentStateStore(string stateFilePath)
    {
        if (string.IsNullOrWhiteSpace(stateFilePath))
        {
            throw new ArgumentException("State file path cannot be empty.", nameof(stateFilePath));
        }

        StateFilePath = stateFilePath;
    }

    public TimerAgentAppState Load()
    {
        if (!File.Exists(StateFilePath))
        {
            return new TimerAgentAppState();
        }

        try
        {
            var json = File.ReadAllText(StateFilePath);
            var state = JsonSerializer.Deserialize<TimerAgentAppState>(json, _jsonOptions);

            return state ?? new TimerAgentAppState();
        }
        catch
        {
            var backupPath = $"{StateFilePath}.broken-{DateTime.Now:yyyyMMdd-HHmmss}.json";

            try
            {
                File.Copy(StateFilePath, backupPath, overwrite: false);
            }
            catch
            {
                // If backup fails, still return a safe empty state.
            }

            return new TimerAgentAppState();
        }
    }

    public void Save(TimerAgentAppState state)
    {
        var directory = Path.GetDirectoryName(StateFilePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        state.SavedAt = DateTimeOffset.Now;

        var json = JsonSerializer.Serialize(state, _jsonOptions);
        File.WriteAllText(StateFilePath, json);
    }
}