namespace FinancePro.Platform.Documents;

public interface IDocumentService
{
    IReadOnlyList<DocumentTemplate> GetTemplates();
    IReadOnlyList<DocumentField> GetFields(string templateKey);
    GeneratedDocument Generate(DocumentRequest request);
    Task ExportTextAsync(GeneratedDocument document, string filePath, CancellationToken cancellationToken = default);
    Task ExportHtmlAsync(GeneratedDocument document, string filePath, CancellationToken cancellationToken = default);
}
