using System.Globalization;
using System.Text;
using System.Text.Json;
using LifeOS.Core.IntegrationInbox;

namespace LifeOS.Core.IntegrationConnectors;

public static class ManualIntegrationImportConnector
{
    public static ManualIntegrationImportResult ImportCsv(string content, string sourceEvidence)
    {
        return ImportRows("manual-csv", ParseCsv(content), sourceEvidence);
    }

    public static ManualIntegrationImportResult ImportJson(string content, string sourceEvidence)
    {
        var rows = ParseJson(content);
        return ImportRows("manual-json", rows, sourceEvidence);
    }

    private static ManualIntegrationImportResult ImportRows(
        string connectorKey,
        IReadOnlyList<IReadOnlyDictionary<string, string>> rows,
        string sourceEvidence)
    {
        var connector = IntegrationConnectorRegistry.GetRequired(connectorKey);
        var previews = new List<IntegrationPreviewItem>();
        var errors = new List<ManualIntegrationImportError>();
        var evidence = string.IsNullOrWhiteSpace(sourceEvidence) ? connector.DisplayName : sourceEvidence.Trim();

        for (var index = 0; index < rows.Count; index++)
        {
            var rowNumber = index + 1;
            var row = rows[index];

            try
            {
                var externalReference = FirstValue(row, "externalReference", "external_reference", "reference", "ref", "id");
                if (string.IsNullOrWhiteSpace(externalReference))
                {
                    externalReference = $"row-{rowNumber}";
                }

                var title = FirstValue(row, "title", "name", "subject", "description", "summary");
                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new ArgumentException("Title is required. Provide a title, name, subject, description, or summary column.");
                }

                var amount = ParseDecimal(FirstValue(row, "amount", "value", "total"));
                var occurredAt = ParseDate(FirstValue(row, "occurredAt", "occurred_at", "date", "dueDate", "due_date"));

                var draft = new IntegrationPreviewDraft
                {
                    SourceKind = connector.SourceKind,
                    SourceLabel = connector.Key,
                    ExternalReference = externalReference,
                    Title = title,
                    Summary = FirstValue(row, "summary", "description", "notes", "note"),
                    OccurredAt = occurredAt,
                    Amount = amount,
                    Currency = FirstValue(row, "currency") is { Length: > 0 } currency ? currency : "NZD",
                    SuggestedTarget = ParseTarget(FirstValue(row, "suggestedTarget", "suggested_target", "target")),
                    SuggestedAction = FirstValue(row, "suggestedAction", "suggested_action", "action"),
                    SourceEvidence = $"{evidence}#{rowNumber}",
                    DuplicateKey = BuildDuplicateKey(connector.Key, externalReference, occurredAt, amount)
                };

                previews.Add(IntegrationPreviewIntake.CreatePreview(draft));
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException or JsonException)
            {
                errors.Add(new ManualIntegrationImportError(rowNumber, ex.Message));
            }
        }

        return new ManualIntegrationImportResult(connector.Key, previews, errors);
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, string>> ParseCsv(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var rows = ParseCsvRows(content);
        if (rows.Count == 0)
        {
            return [];
        }

        var headers = rows[0].Select(NormalizeHeader).ToArray();
        var parsed = new List<IReadOnlyDictionary<string, string>>();

        foreach (var row in rows.Skip(1))
        {
            if (row.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < headers.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(headers[index]))
                {
                    continue;
                }

                values[headers[index]] = index < row.Count ? row[index].Trim() : string.Empty;
            }

            parsed.Add(values);
        }

        return parsed;
    }

    private static List<List<string>> ParseCsvRows(string content)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < content.Length; index++)
        {
            var current = content[index];

            if (current == '"')
            {
                if (inQuotes && index + 1 < content.Length && content[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
                continue;
            }

            if (current == ',' && !inQuotes)
            {
                row.Add(field.ToString());
                field.Clear();
                continue;
            }

            if ((current == '\r' || current == '\n') && !inQuotes)
            {
                if (current == '\r' && index + 1 < content.Length && content[index + 1] == '\n')
                {
                    index++;
                }

                row.Add(field.ToString());
                field.Clear();
                rows.Add(row);
                row = [];
                continue;
            }

            field.Append(current);
        }

        row.Add(field.ToString());
        rows.Add(row);

        return rows.Where(csvRow => csvRow.Any(value => !string.IsNullOrWhiteSpace(value))).ToList();
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, string>> ParseJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        using var document = JsonDocument.Parse(content);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Manual JSON import expects an array of objects.");
        }

        var rows = new List<IReadOnlyDictionary<string, string>>();
        foreach (var element in document.RootElement.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Manual JSON import records must be objects.");
            }

            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in element.EnumerateObject())
            {
                row[NormalizeHeader(property.Name)] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                    JsonValueKind.Number => property.Value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => string.Empty,
                    _ => property.Value.GetRawText()
                };
            }

            rows.Add(row);
        }

        return rows;
    }

    private static string NormalizeHeader(string value)
    {
        return value.Trim();
    }

    private static string FirstValue(IReadOnlyDictionary<string, string> row, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private static decimal? ParseDecimal(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : null;
    }

    private static DateTime? ParseDate(string value)
    {
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date)
            ? date
            : null;
    }

    private static IntegrationTargetKind ParseTarget(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return IntegrationTargetKind.None;
        }

        return Enum.TryParse<IntegrationTargetKind>(value.Replace(" ", string.Empty).Replace("/", string.Empty), true, out var target)
            ? target
            : IntegrationTargetKind.None;
    }

    private static string BuildDuplicateKey(string connectorKey, string externalReference, DateTime? occurredAt, decimal? amount)
    {
        var datePart = occurredAt?.ToString("yyyyMMdd", CultureInfo.InvariantCulture) ?? "nodate";
        var amountPart = amount?.ToString("0.00", CultureInfo.InvariantCulture) ?? "noamount";

        return string.Join(
            ":",
            connectorKey.Trim().ToLowerInvariant(),
            externalReference.Trim().ToLowerInvariant(),
            datePart,
            amountPart);
    }
}
