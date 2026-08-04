using FinancePro.Platform.Documents;
using Xunit;

namespace FinancePro.Application.Tests.Documents;

public sealed class DocumentServiceTests
{
    [Fact]
    public void Generate_ReplacesProvidedFields()
    {
        var service = new DocumentService();
        var result = service.Generate(new DocumentRequest("memorando", new Dictionary<string, string>
        {
            ["Numero"] = "15/2026",
            ["Remetente"] = "DSAF",
            ["Destinatario"] = "Direção-Geral",
            ["Assunto"] = "Teste",
            ["Data"] = "04/08/2026",
            ["Corpo"] = "Conteúdo de teste.",
            ["Responsavel"] = "Responsável"
        }));

        Assert.Contains("MEMORANDO N.º 15/2026", result.Content);
        Assert.Contains("Conteúdo de teste.", result.Content);
        Assert.DoesNotContain("{{", result.Content);
    }

    [Fact]
    public void GetFields_ReturnsUniquePlaceholders()
    {
        var service = new DocumentService();
        var fields = service.GetFields("oficio");
        Assert.NotEmpty(fields);
        Assert.Single(fields.Where(x => x.Key == "Assunto"));
    }
}
