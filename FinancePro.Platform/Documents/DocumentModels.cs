namespace FinancePro.Platform.Documents;

public sealed record DocumentTemplate(
    string Key,
    string Name,
    string Category,
    string Description,
    string Content);

public sealed record DocumentField(string Key, string Label, string Value = "");

public sealed record DocumentRequest(
    string TemplateKey,
    IReadOnlyDictionary<string, string> Values);

public sealed record GeneratedDocument(
    string Title,
    string Category,
    string Content,
    DateTime GeneratedAt);
