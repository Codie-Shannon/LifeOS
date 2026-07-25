namespace LifeOS.Core.PublicPackaging;

public enum PublicAssetAudience
{
    Public,
    PrivateBeta,
    Internal
}

public sealed record PublicAsset(
    string Id,
    string Title,
    string Route,
    PublicAssetAudience Audience,
    string Status,
    string Copy,
    IReadOnlyList<string> ScreenshotPaths);

public sealed record PackagingResult(
    bool Ready,
    IReadOnlyList<string> Errors,
    IReadOnlyList<PublicAsset> PublicAssets);

public sealed class PublicPackagingService
{
    public PackagingResult Validate(IEnumerable<PublicAsset> assets)
    {
        ArgumentNullException.ThrowIfNull(assets);
        PublicAsset[] assetList = assets.ToArray();
        List<string> errors = new();

        foreach (IGrouping<string, PublicAsset> duplicate in assetList
                     .GroupBy(asset => asset.Id, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
        {
            errors.Add($"Duplicate asset id: {duplicate.Key}.");
        }

        foreach (PublicAsset asset in assetList)
        {
            if (asset.Audience == PublicAssetAudience.Public &&
                (ContainsPrivatePath(asset.Copy) ||
                 asset.ScreenshotPaths.Any(ContainsPrivatePath) ||
                 asset.Copy.Contains("private Codex", StringComparison.OrdinalIgnoreCase) ||
                 asset.Copy.Contains("production-ready", StringComparison.OrdinalIgnoreCase) &&
                 asset.Status.Contains("beta", StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add($"{asset.Id} contains private context or an unsupported claim.");
            }
            if (asset.Audience == PublicAssetAudience.Public &&
                (!asset.Route.StartsWith("/", StringComparison.Ordinal) ||
                 asset.Route.StartsWith("//", StringComparison.Ordinal) ||
                 asset.Route.Contains("..", StringComparison.Ordinal)))
            {
                errors.Add($"{asset.Id} does not have a public route.");
            }
        }

        foreach (IGrouping<string, PublicAsset> duplicate in assetList
                     .Where(asset => asset.Audience == PublicAssetAudience.Public)
                     .GroupBy(asset => asset.Route, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
        {
            errors.Add($"Duplicate public route: {duplicate.Key}.");
        }

        return new PackagingResult(
            errors.Count == 0,
            errors,
            assetList.Where(asset => asset.Audience == PublicAssetAudience.Public).ToArray());
    }

    private static bool ContainsPrivatePath(string value) =>
        value.Contains(@":\", StringComparison.OrdinalIgnoreCase) ||
        value.Contains(@"\\", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("file://", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("/Users/", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("/home/", StringComparison.OrdinalIgnoreCase);
}
