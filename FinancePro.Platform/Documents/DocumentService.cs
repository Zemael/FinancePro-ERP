using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace FinancePro.Platform.Documents;

public sealed class DocumentService : IDocumentService
{
    private static readonly IReadOnlyList<DocumentTemplate> Templates = new[]
    {
        new DocumentTemplate(
            "informacao-despacho",
            "Informação para Despacho",
            "Institucional",
            "Informação formal dirigida à entidade competente para decisão.",
            "INFORMAÇÃO\n\nAssunto: {{Assunto}}\n\nExcelência,\n\n{{Corpo}}\n\nFace ao exposto, submete-se a presente informação à consideração superior para {{Pedido}}.\n\n{{Local}}, {{Data}}\n\nO Responsável\n{{Responsavel}}"),
        new DocumentTemplate(
            "oficio",
            "Ofício",
            "Correspondência",
            "Comunicação oficial externa com referência e destinatário.",
            "OFÍCIO N.º {{Numero}}\n\nAo(À) {{Destinatario}}\n{{Entidade}}\n\nAssunto: {{Assunto}}\n\n{{Corpo}}\n\nCom os melhores cumprimentos,\n\n{{Local}}, {{Data}}\n\n{{Responsavel}}\n{{Cargo}}"),
        new DocumentTemplate(
            "memorando",
            "Memorando",
            "Correspondência",
            "Comunicação interna breve entre serviços.",
            "MEMORANDO N.º {{Numero}}\n\nDe: {{Remetente}}\nPara: {{Destinatario}}\nAssunto: {{Assunto}}\nData: {{Data}}\n\n{{Corpo}}\n\n{{Responsavel}}"),
        new DocumentTemplate(
            "parecer",
            "Parecer Técnico",
            "Institucional",
            "Parecer técnico com análise, conclusão e recomendação.",
            "PARECER TÉCNICO\n\nAssunto: {{Assunto}}\n\nI. ENQUADRAMENTO\n{{Enquadramento}}\n\nII. ANÁLISE\n{{Analise}}\n\nIII. CONCLUSÃO E RECOMENDAÇÃO\n{{Conclusao}}\n\n{{Local}}, {{Data}}\n\n{{Responsavel}}\n{{Cargo}}"),
        new DocumentTemplate(
            "auto-transferencia",
            "Auto de Transferência Patrimonial",
            "Património",
            "Registo formal da transferência de um bem entre responsáveis ou localizações.",
            "AUTO DE TRANSFERÊNCIA PATRIMONIAL\n\nAos {{Data}}, procede-se à transferência do bem abaixo identificado:\n\nCódigo patrimonial: {{CodigoPatrimonial}}\nDesignação: {{Bem}}\nOrigem: {{Origem}}\nDestino: {{Destino}}\nResponsável anterior: {{ResponsavelAnterior}}\nNovo responsável: {{NovoResponsavel}}\nMotivo: {{Motivo}}\n\nO bem foi entregue e recebido no estado descrito no processo.\n\nEntregue por: ____________________\nRecebido por: ____________________\nAutorizado por: ____________________"),
        new DocumentTemplate(
            "guia-remessa",
            "Guia de Remessa",
            "Logística",
            "Documento de acompanhamento de materiais ou equipamentos.",
            "GUIA DE REMESSA N.º {{Numero}}\n\nOrigem: {{Origem}}\nDestino: {{Destino}}\nData: {{Data}}\nResponsável: {{Responsavel}}\n\nITENS\n{{Itens}}\n\nObservações:\n{{Observacoes}}\n\nEntregue por: ____________________\nRecebido por: ____________________")
    };

    public IReadOnlyList<DocumentTemplate> GetTemplates() => Templates;

    public IReadOnlyList<DocumentField> GetFields(string templateKey)
    {
        var template = FindTemplate(templateKey);
        return Regex.Matches(template.Content, "\\{\\{([A-Za-z0-9]+)\\}\\}")
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(key => new DocumentField(key, SplitLabel(key), DefaultValue(key)))
            .ToArray();
    }

    public GeneratedDocument Generate(DocumentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var template = FindTemplate(request.TemplateKey);
        var content = template.Content;
        foreach (var field in GetFields(template.Key))
        {
            request.Values.TryGetValue(field.Key, out var value);
            content = content.Replace($"{{{{{field.Key}}}}}", string.IsNullOrWhiteSpace(value) ? $"[{field.Label}]" : value.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        return new GeneratedDocument(template.Name, template.Category, content, DateTime.Now);
    }

    public Task ExportTextAsync(GeneratedDocument document, string filePath, CancellationToken cancellationToken = default)
    {
        ValidateExport(document, filePath);
        return File.WriteAllTextAsync(filePath, document.Content, new UTF8Encoding(true), cancellationToken);
    }

    public Task ExportHtmlAsync(GeneratedDocument document, string filePath, CancellationToken cancellationToken = default)
    {
        ValidateExport(document, filePath);
        var encoded = WebUtility.HtmlEncode(document.Content).Replace("\r\n", "<br/>").Replace("\n", "<br/>");
        var html = $"<!doctype html><html><head><meta charset=\"utf-8\"><title>{WebUtility.HtmlEncode(document.Title)}</title><style>body{{font-family:Segoe UI,Arial,sans-serif;max-width:900px;margin:48px auto;line-height:1.6;color:#1f2937}}h1{{color:#0f4c81}}</style></head><body><h1>{WebUtility.HtmlEncode(document.Title)}</h1><div>{encoded}</div></body></html>";
        return File.WriteAllTextAsync(filePath, html, new UTF8Encoding(false), cancellationToken);
    }

    private static DocumentTemplate FindTemplate(string key) =>
        Templates.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException("Modelo documental inválido.", nameof(key));

    private static void ValidateExport(GeneratedDocument document, string filePath)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Caminho de exportação obrigatório.", nameof(filePath));
    }

    private static string DefaultValue(string key) => key switch
    {
        "Data" => DateTime.Today.ToString("dd 'de' MMMM 'de' yyyy"),
        "Local" => "Bissau",
        _ => string.Empty
    };

    private static string SplitLabel(string value) => Regex.Replace(value, "(?<!^)([A-Z])", " $1");
}
