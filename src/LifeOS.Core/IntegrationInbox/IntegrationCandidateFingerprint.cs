using System.Security.Cryptography;
using System.Text;

namespace LifeOS.Core.IntegrationInbox;

public static class IntegrationCandidateFingerprint
{
    public static string Build(
        IntegrationCandidateType type,
        IEnumerable<KeyValuePair<string, string?>> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        string canonical = string.Join(
            "\n",
            values
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair =>
                    $"{Normalize(pair.Key)}={Normalize(pair.Value)}"));

        return Hash($"{type}\n{canonical}");
    }

    public static string BuildContentHash(
        IntegrationCandidateType type,
        IEnumerable<IntegrationCandidateField> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);

        return Build(
            type,
            fields.Select(field =>
                new KeyValuePair<string, string?>(
                    field.Key,
                    field.Value)));
    }

    public static string BuildExternalKey(IntegrationCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        return string.Join(
            "|",
            Normalize(candidate.Provenance.ProviderId),
            Normalize(candidate.Provenance.AccountId),
            Normalize(candidate.Provenance.ExternalId));
    }

    private static string Hash(string value)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : string.Join(
                " ",
                value
                    .Trim()
                    .ToLowerInvariant()
                    .Split(
                        (char[]?)null,
                        StringSplitOptions.RemoveEmptyEntries));
}
