using LifeOS.Website.Content;
namespace LifeOS.Website.Services;
public sealed class PublicContentService
{
    public IReadOnlyList<DocEntry> Search(string? query, string? category = null)
    {
        IEnumerable<DocEntry> results = PublicContent.Docs;
        if (!string.IsNullOrWhiteSpace(category)) results = results.Where(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(query))
        {
            var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            results = results.Where(x => terms.All(term => SearchText(x).Contains(term, StringComparison.OrdinalIgnoreCase)));
        }
        return results.OrderBy(x => x.Title, StringComparer.OrdinalIgnoreCase).ToArray();
    }
    private static string SearchText(DocEntry x) => string.Join(' ', x.Title, x.Summary, x.Audience, x.Category, x.Product, x.Version, string.Join(' ', x.Keywords));
}
