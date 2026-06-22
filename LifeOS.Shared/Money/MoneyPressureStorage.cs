using System.Text.Json;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.Money;

public static class MoneyPressureStorage
{
    private const string FileName = "money-pressure-input.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath => LocalAppDataPath.GetFilePath(FileName);

    public static MoneyPressureManualInput Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new MoneyPressureManualInput();
            }

            var json = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new MoneyPressureManualInput();
            }

            return JsonSerializer.Deserialize<MoneyPressureManualInput>(json, JsonOptions)
                ?? new MoneyPressureManualInput();
        }
        catch
        {
            return new MoneyPressureManualInput();
        }
    }

    public static void Save(MoneyPressureManualInput input)
    {
        var json = JsonSerializer.Serialize(input, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Reset()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}