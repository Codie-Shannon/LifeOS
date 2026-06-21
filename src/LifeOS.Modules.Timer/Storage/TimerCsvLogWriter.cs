using System.Globalization;
using System.Text;

namespace LifeOS.Modules.Timer.Storage;

public sealed class TimerCsvLogWriter
{
    private const string Header =
        "Id,Date,StartTime,EndTime,DurationMinutes,ClientName,ProjectName,WorkType,IsBillable,HourlyRate,EarnedAmount,TaxSetAsidePercent,TaxSetAsideAmount,SafeAfterTaxAmount,Notes";

    public string LogFilePath { get; }

    public TimerCsvLogWriter(string logFilePath)
    {
        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            throw new ArgumentException("Log file path cannot be empty.", nameof(logFilePath));
        }

        LogFilePath = logFilePath;
    }

    public void Append(TimerLogEntry entry)
    {
        var directory = Path.GetDirectoryName(LogFilePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileExists = File.Exists(LogFilePath);

        using var stream = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        if (!fileExists)
        {
            writer.WriteLine(Header);
        }

        writer.WriteLine(ToCsvLine(entry));
    }

    public IReadOnlyList<TimerLogEntry> ReadAll()
    {
        if (!File.Exists(LogFilePath))
        {
            return Array.Empty<TimerLogEntry>();
        }

        var lines = File.ReadAllLines(LogFilePath, Encoding.UTF8);

        if (lines.Length <= 1)
        {
            return Array.Empty<TimerLogEntry>();
        }

        var entries = new List<TimerLogEntry>();

        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseCsvLine(line);

            if (values.Count < 15)
            {
                continue;
            }

            entries.Add(new TimerLogEntry
            {
                Id = Guid.TryParse(values[0], out var id) ? id : Guid.Empty,
                Date = DateOnly.Parse(values[1], CultureInfo.InvariantCulture),
                StartTime = TimeOnly.Parse(values[2], CultureInfo.InvariantCulture),
                EndTime = TimeOnly.Parse(values[3], CultureInfo.InvariantCulture),
                DurationMinutes = double.Parse(values[4], CultureInfo.InvariantCulture),
                ClientName = values[5],
                ProjectName = values[6],
                WorkType = values[7],
                IsBillable = bool.Parse(values[8]),
                HourlyRate = decimal.Parse(values[9], CultureInfo.InvariantCulture),
                EarnedAmount = decimal.Parse(values[10], CultureInfo.InvariantCulture),
                TaxSetAsidePercent = decimal.Parse(values[11], CultureInfo.InvariantCulture),
                TaxSetAsideAmount = decimal.Parse(values[12], CultureInfo.InvariantCulture),
                SafeAfterTaxAmount = decimal.Parse(values[13], CultureInfo.InvariantCulture),
                Notes = values[14]
            });
        }

        return entries;
    }

    private static string ToCsvLine(TimerLogEntry entry)
    {
        return string.Join(",",
            Escape(entry.Id.ToString()),
            Escape(entry.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            Escape(entry.StartTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
            Escape(entry.EndTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
            Escape(entry.DurationMinutes.ToString("0.##", CultureInfo.InvariantCulture)),
            Escape(entry.ClientName),
            Escape(entry.ProjectName),
            Escape(entry.WorkType),
            Escape(entry.IsBillable.ToString()),
            Escape(entry.HourlyRate.ToString("0.##", CultureInfo.InvariantCulture)),
            Escape(entry.EarnedAmount.ToString("0.##", CultureInfo.InvariantCulture)),
            Escape(entry.TaxSetAsidePercent.ToString("0.##", CultureInfo.InvariantCulture)),
            Escape(entry.TaxSetAsideAmount.ToString("0.##", CultureInfo.InvariantCulture)),
            Escape(entry.SafeAfterTaxAmount.ToString("0.##", CultureInfo.InvariantCulture)),
            Escape(entry.Notes)
        );
    }

    private static string Escape(string value)
    {
        value ??= string.Empty;

        var mustQuote = value.Contains(',')
            || value.Contains('"')
            || value.Contains('\n')
            || value.Contains('\r');

        if (!mustQuote)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];

            if (character == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }

                continue;
            }

            if (character == ',' && !insideQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        values.Add(current.ToString());

        return values;
    }
}