using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LifeOS.Core.EmailRadar;

public static class CommunicationImportService
{
    public static CommunicationImportPreview PreviewJson(string content, string sourceFile)
    {
        if (string.IsNullOrWhiteSpace(content)) return new([], []);
        using var doc = JsonDocument.Parse(content);
        if (doc.RootElement.ValueKind != JsonValueKind.Array) throw new JsonException("Communication JSON must be an array of objects.");
        var rows = doc.RootElement.EnumerateArray().Select((e, i) => (Row: i + 1, Values: (IReadOnlyDictionary<string, string>)ToDictionary(e))).ToList();
        return ParseRows(rows, sourceFile, "local-json");
    }

    public static CommunicationImportPreview PreviewCsv(string content, string sourceFile)
    {
        if (string.IsNullOrWhiteSpace(content)) return new([], []);
        var rows = ParseCsv(content);
        if (rows.Count == 0) return new([], []);
        var headers = rows[0].Select(x => x.Trim()).ToArray();
        var mapped = rows.Skip(1).Where(r => r.Any(x => !string.IsNullOrWhiteSpace(x))).Select((row, i) =>
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Length; c++) values[headers[c]] = c < row.Count ? row[c].Trim() : "";
            return (Row: i + 2, Values: (IReadOnlyDictionary<string,string>)values);
        }).ToList();
        return ParseRows(mapped, sourceFile, "local-csv");
    }

    public static int MarkDuplicates(IEnumerable<ImportedCommunicationRecord> existing, IEnumerable<ImportedCommunicationRecord> incoming)
    {
        var keys = existing.Select(x => x.DuplicateKey).Where(x => x.Length > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var count = 0;
        foreach (var record in incoming)
        {
            if (!keys.Add(record.DuplicateKey) || !seen.Add(record.DuplicateKey))
            {
                record.ReviewState = CommunicationReviewState.DuplicateSuspected;
                count++;
            }
        }
        return count;
    }

    private static CommunicationImportPreview ParseRows(IEnumerable<(int Row, IReadOnlyDictionary<string,string> Values)> rows, string sourceFile, string sourceKind)
    {
        var records = new List<ImportedCommunicationRecord>();
        var errors = new List<CommunicationImportError>();
        foreach (var row in rows)
        {
            try
            {
                var sent = Required(row.Values, "sentAt", "sent_at", "date", "timestamp");
                if (!DateTimeOffset.TryParse(sent, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var sentAt)) throw new FormatException("sentAt is invalid.");
                var sender = Required(row.Values, "sender", "from");
                var subject = Required(row.Values, "subject", "title");
                var recipients = Value(row.Values, "recipients", "to").Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                var text = SanitizeText(Value(row.Values, "text", "body", "snippet", "summary"));
                var external = Value(row.Values, "externalReference", "external_reference", "messageId", "message_id", "id");
                var thread = Value(row.Values, "threadReference", "thread_reference", "threadId", "thread_id", "conversationId");
                var record = new ImportedCommunicationRecord
                {
                    SourceKind = sourceKind, SourceLabel = Path.GetFileName(sourceFile), SourceFile = sourceFile,
                    ExternalReference = external, ThreadReference = thread, SentAt = sentAt, Sender = sender,
                    Recipients = recipients, Subject = subject.Trim(), Text = text,
                    HasAttachments = bool.TryParse(Value(row.Values, "hasAttachments", "has_attachments"), out var has) && has,
                    Provenance = $"{sourceKind}:{Path.GetFileName(sourceFile)}#{row.Row}"
                };
                record.DuplicateKey = BuildDuplicateKey(record);
                records.Add(record);
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException)
            {
                errors.Add(new(row.Row, ex.Message));
            }
        }
        return new(records, errors);
    }

    public static string SanitizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var decoded = WebUtility.HtmlDecode(value);
        var noTags = System.Text.RegularExpressions.Regex.Replace(decoded, "<[^>]+>", " ");
        var noScripts = System.Text.RegularExpressions.Regex.Replace(noTags, "(?is)(javascript:|data:text/html|on\\w+\\s*=)", "[removed]");
        return System.Text.RegularExpressions.Regex.Replace(noScripts, "\\s+", " ").Trim();
    }

    public static string BuildDuplicateKey(ImportedCommunicationRecord record)
    {
        var raw = string.Join('|', record.SourceKind, record.ExternalReference, record.ThreadReference, record.SentAt.UtcDateTime.ToString("O"), record.Sender.Trim().ToLowerInvariant(), record.Subject.Trim().ToLowerInvariant());
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
    }

    private static Dictionary<string,string> ToDictionary(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) throw new JsonException("Each communication record must be an object.");
        return element.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() ?? "" : p.Value.GetRawText(), StringComparer.OrdinalIgnoreCase);
    }
    private static string Required(IReadOnlyDictionary<string,string> row, params string[] keys) => Value(row, keys) is { Length: > 0 } value ? value : throw new ArgumentException($"{keys[0]} is required.");
    private static string Value(IReadOnlyDictionary<string,string> row, params string[] keys) { foreach (var key in keys) if (row.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v)) return v.Trim(); return ""; }
    private static List<List<string>> ParseCsv(string content)
    {
        var rows = new List<List<string>>(); var row = new List<string>(); var field = new StringBuilder(); var quoted = false;
        for (var i=0;i<content.Length;i++) { var ch=content[i]; if(ch=='"'){if(quoted&&i+1<content.Length&&content[i+1]=='"'){field.Append('"');i++;}else quoted=!quoted;continue;} if(ch==','&&!quoted){row.Add(field.ToString());field.Clear();continue;} if((ch=='\r'||ch=='\n')&&!quoted){if(ch=='\r'&&i+1<content.Length&&content[i+1]=='\n')i++;row.Add(field.ToString());field.Clear();rows.Add(row);row=[];continue;} field.Append(ch);} row.Add(field.ToString()); rows.Add(row); return rows;
    }
}
