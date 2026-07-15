using LifeOS.Website.Content;

namespace LifeOS.Website.Services;

public sealed class PublicContentService
{
    private static readonly string[] UserFirst = ["User", "All", "Business evaluator", "Technical reviewer", "Developer"];

    public IReadOnlyList<DocEntry> Search(string? query, string? category = null, string? audience = null, string? product = null)
    {
        IEnumerable<DocEntry> results = PublicContent.Docs.Where(x => x.PublicStatus == "Public");
        if (!string.IsNullOrWhiteSpace(category)) results = results.Where(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(audience)) results = results.Where(x => x.Audience.Equals(audience, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(product)) results = results.Where(x => x.Product.Equals(product, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(query))
        {
            var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            results = results.Where(x => terms.All(term => SearchText(x).Contains(term, StringComparison.OrdinalIgnoreCase)));
        }
        return results.OrderBy(x => AudienceRank(x.Audience)).ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public DocEntry? FindByRoute(string route) => PublicContent.Docs.SingleOrDefault(x => x.Route.Equals(Normalize(route), StringComparison.OrdinalIgnoreCase));
    public DocEntry? FindById(string id) => PublicContent.Docs.SingleOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    public IReadOnlyList<DocEntry> Related(DocEntry entry) => entry.RelatedIds.Select(FindById).Where(x => x is not null).Cast<DocEntry>().ToArray();
    public IReadOnlyList<string> Categories => PublicContent.Docs.Select(x => x.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray();
    public IReadOnlyList<string> Audiences => PublicContent.Docs.Select(x => x.Audience).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(AudienceRank).ToArray();
    public IReadOnlyList<string> Products => PublicContent.Docs.Select(x => x.Product).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray();

    private static int AudienceRank(string audience)
    {
        var index = Array.FindIndex(UserFirst, x => x.Equals(audience, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? UserFirst.Length : index;
    }

    private static string Normalize(string route)
    {
        var clean = route.Split('?', '#')[0];
        if (clean.Length > 1) clean = clean.TrimEnd('/');
        return clean;
    }

    private static string SearchText(DocEntry x) => string.Join(' ', x.Title, x.Summary, x.Audience, x.Category, x.Product, x.Version, string.Join(' ', x.Keywords));
}
