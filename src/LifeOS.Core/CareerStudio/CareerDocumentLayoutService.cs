using System.IO.Compression;
using System.Security;
using System.Text;

namespace LifeOS.Core.CareerStudio;

public sealed class CareerDocumentLayoutService
{
    private static readonly CvTemplateDefinition[] Templates =
    [
        new(
            "professional",
            "Professional",
            "Confident colour header with clear section hierarchy.",
            true,
            new CvDocumentLayout("Aptos", 1.0, 18, "#315E91", CvPageDensity.Balanced, true)),
        new(
            "ats-classic",
            "ATS Classic",
            "Single-column, restrained styling for automated screening.",
            true,
            new CvDocumentLayout("Arial", 0.98, 17, "#243247", CvPageDensity.Compact, true)),
        new(
            "modern",
            "Modern",
            "Open spacing and a stronger contemporary accent.",
            true,
            new CvDocumentLayout("Segoe UI", 1.03, 19, "#6C4EE3", CvPageDensity.Spacious, false)),
        new(
            "compact",
            "Compact",
            "Reduced spacing for detailed experience without tiny text.",
            true,
            new CvDocumentLayout("Calibri", 0.94, 15, "#176B65", CvPageDensity.Compact, true))
    ];

    public IReadOnlyList<CvTemplateDefinition> GetTemplates() => Templates;

    public CvTemplateDefinition GetTemplate(string templateId) =>
        Templates.SingleOrDefault(template =>
            string.Equals(template.Id, templateId, StringComparison.Ordinal))
        ?? throw new ArgumentException("The requested CV template does not exist.", nameof(templateId));

    public CvBuilderDocument ApplyTemplate(
        CvBuilderDocument document,
        string templateId,
        DateTimeOffset now)
    {
        CvTemplateDefinition template = GetTemplate(templateId);
        return document with
        {
            TemplateId = template.Id,
            Layout = template.Layout,
            Version = document.Version + 1,
            UpdatedUtc = now,
            IsAutosaved = true
        };
    }

    public CvBuilderDocument UpdateLayout(
        CvBuilderDocument document,
        CvDocumentLayout layout,
        DateTimeOffset now)
    {
        if (layout.FontScale is < 0.85 or > 1.2)
            throw new ArgumentOutOfRangeException(nameof(layout), "Font scale must remain readable.");
        if (layout.PageMarginMillimetres is < 12 or > 25)
            throw new ArgumentOutOfRangeException(nameof(layout), "A4 margins must be between 12 and 25 mm.");
        if (!IsHexColour(layout.AccentHex))
            throw new ArgumentException("Accent colour must be a six-digit hexadecimal colour.", nameof(layout));

        return document with
        {
            Layout = layout,
            Version = document.Version + 1,
            UpdatedUtc = now,
            IsAutosaved = true
        };
    }

    public CvReadabilityReview Review(CvBuilderDocument document)
    {
        int wordCount = document.VisibleSections.Sum(section =>
            CountWords(section.Content) +
            (section.Entries?.Sum(entry => CountWords(entry.Description)) ?? 0));
        bool hasContact = HasContent(document, CvSectionKind.Contact);
        bool hasProfile = HasContent(document, CvSectionKind.Profile);
        bool hasEmployment = HasContent(document, CvSectionKind.Employment);
        bool hasSkills = HasContent(document, CvSectionKind.Skills);
        bool readableScale = document.EffectiveLayout.FontScale >= 0.9;
        bool sensibleLength = wordCount is >= 40 and <= 1100;
        bool atsTemplate = GetTemplate(document.TemplateId).AtsFriendly;
        int estimatedPages = Math.Max(
            1,
            (int)Math.Ceiling(wordCount /
                (document.EffectiveLayout.Density == CvPageDensity.Compact ? 650d : 520d)));

        CvReadabilityCheck[] checks =
        [
            Check("contact", "Contact details", "A recruiter can identify and contact the candidate.", hasContact, true),
            Check("profile", "Professional profile", "The opening summary establishes role relevance.", hasProfile, true),
            Check("employment", "Employment evidence", "At least one evidence-backed experience section is present.", hasEmployment, true),
            Check("skills", "Role skills", "Skills are available for human and ATS review.", hasSkills, true),
            Check("font-size", "Readable typography", "Body type remains at or above the safe readability floor.", readableScale, true),
            Check("length", "Focused length", $"{wordCount} words; recommended range is 40-1100 for this proof document.", sensibleLength, false),
            Check("ats-template", "ATS-safe structure", "Template uses one-column semantic headings and selectable text.", atsTemplate, false),
            Check("page-count", "A4 page estimate", $"Estimated {estimatedPages} A4 page(s).", estimatedPages <= 3, false)
        ];
        int score = (int)Math.Round(checks.Count(check => check.Passed) * 100d / checks.Length);
        return new CvReadabilityReview(score, estimatedPages, checks);
    }

    public CvVersionSnapshot CreateSnapshot(CvBuilderDocument document, string label) =>
        new(
            document.Version,
            document.UpdatedUtc,
            label,
            document.TemplateId,
            document.VisibleSections.Count);

    public CvExportArtifact Export(
        CvBuilderDocument document,
        CvBuilderReview sourceReview,
        CvExportFormat format,
        DateTimeOffset now)
    {
        CvReadabilityReview readability = Review(document);
        if (!sourceReview.CanExport || !readability.CanExport)
            throw new InvalidOperationException("Resolve blocking source and readability checks before export.");

        string safeName = SafeFileName(document.Name);
        return format switch
        {
            CvExportFormat.Pdf => new CvExportArtifact(
                format,
                $"{safeName}-v{document.Version}.pdf",
                "application/pdf",
                BuildPdf(document),
                document.Version,
                now),
            CvExportFormat.Docx => new CvExportArtifact(
                format,
                $"{safeName}-v{document.Version}.docx",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                BuildDocx(document),
                document.Version,
                now),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    private static CvReadabilityCheck Check(
        string code,
        string label,
        string detail,
        bool passed,
        bool blocking) =>
        new(
            code,
            label,
            detail,
            passed,
            blocking ? CvValidationSeverity.Blocking : CvValidationSeverity.Warning);

    private static bool HasContent(CvBuilderDocument document, CvSectionKind kind) =>
        document.VisibleSections.Any(section =>
            section.Kind == kind &&
            (!string.IsNullOrWhiteSpace(section.Content) || section.Entries is { Count: > 0 }));

    private static int CountWords(string value) =>
        value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    private static bool IsHexColour(string value) =>
        value.Length == 7 &&
        value[0] == '#' &&
        value[1..].All(Uri.IsHexDigit);

    private static string SafeFileName(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        string normalized = new(
            value.Trim()
                .Select(character => invalid.Contains(character) ? '-' : character)
                .ToArray());
        return string.IsNullOrWhiteSpace(normalized) ? "LifeOS-CV" : normalized;
    }

    private static byte[] BuildPdf(CvBuilderDocument document)
    {
        List<string> lines = RenderPlainText(document)
            .Select(line => line.Length > 95 ? line[..95] : line)
            .ToList();
        List<string> contentLines = [];
        int y = 790;
        foreach (string line in lines)
        {
            if (y < 45)
                break;
            contentLines.Add($"BT /F1 10 Tf 48 {y} Td ({EscapePdf(line)}) Tj ET");
            y -= 15;
        }

        string stream = string.Join("\n", contentLines);
        List<string> objects =
        [
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        ];

        StringBuilder pdf = new("%PDF-1.4\n");
        List<int> offsets = [0];
        for (int index = 0; index < objects.Count; index++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append($"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
        }
        int xref = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        for (int index = 1; index < offsets.Count; index++)
            pdf.Append($"{offsets[index]:D10} 00000 n \n");
        pdf.Append($"trailer << /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static byte[] BuildDocx(CvBuilderDocument document)
    {
        using MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, true))
        {
            WriteEntry(
                archive,
                "[Content_Types].xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>" +
                "</Types>");
            WriteEntry(
                archive,
                "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/>" +
                "</Relationships>");

            string paragraphs = string.Join(
                string.Empty,
                RenderPlainText(document).Select(line =>
                    $"<w:p><w:r><w:t xml:space=\"preserve\">{SecurityElement.Escape(line)}</w:t></w:r></w:p>"));
            WriteEntry(
                archive,
                "word/document.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\">" +
                $"<w:body>{paragraphs}<w:sectPr><w:pgSz w:w=\"11906\" w:h=\"16838\"/>" +
                "<w:pgMar w:top=\"1021\" w:right=\"1021\" w:bottom=\"1021\" w:left=\"1021\"/></w:sectPr>" +
                "</w:body></w:document>");
        }
        return output.ToArray();
    }

    private static IEnumerable<string> RenderPlainText(CvBuilderDocument document)
    {
        yield return document.Name;
        yield return document.TargetRole;
        yield return string.Empty;
        foreach (CvBuilderSection section in document.VisibleSections)
        {
            yield return section.Heading.ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(section.Subtitle))
                yield return section.Subtitle;
            if (section.Entries is { Count: > 0 })
            {
                foreach (CvBuilderEntry entry in section.Entries)
                {
                    yield return string.Join(
                        " - ",
                        new[] { entry.Title, entry.Organization, entry.City }
                            .Where(value => !string.IsNullOrWhiteSpace(value)));
                    if (!string.IsNullOrWhiteSpace(entry.Description))
                        yield return entry.Description;
                }
            }
            else if (!string.IsNullOrWhiteSpace(section.Content))
            {
                yield return section.Content.Replace(Environment.NewLine, " ");
            }
            yield return string.Empty;
        }
    }

    private static string EscapePdf(string value) =>
        value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        ZipArchiveEntry entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using StreamWriter writer = new(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }
}
