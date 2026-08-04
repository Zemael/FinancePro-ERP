using FinancePro.Data.Context;
using FinancePro.Platform.Reporting;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Reporting;

public sealed class EfReportDataProvider : IReportDataProvider
{
    private readonly FinanceProDbContext _db;
    public EfReportDataProvider(FinanceProDbContext db) => _db = db;

    public Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken = default) =>
        request.ReportKey.ToLowerInvariant() switch
        {
            "receivables" => ReceivablesAsync(request, cancellationToken),
            "payables" => PayablesAsync(request, cancellationToken),
            "treasury" => TreasuryAsync(request, cancellationToken),
            "assets" => AssetsAsync(request, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(request), "Relatório não suportado.")
        };

    private async Task<ReportResult> ReceivablesAsync(ReportRequest request, CancellationToken ct)
    {
        var data = await _db.ContasReceber.AsNoTracking()
            .Include(x => x.Cliente)
            .Where(x => x.EmpresaId == request.CompanyId && x.DataEmissao >= request.From.Date && x.DataEmissao < request.To.Date.AddDays(1))
            .OrderBy(x => x.DataVencimento)
            .Select(x => new { x.Codigo, Cliente = x.Cliente != null ? x.Cliente.Nome : string.Empty, x.Descricao, x.DataEmissao, x.DataVencimento, x.Valor, Estado = x.Estado.ToString() })
            .ToListAsync(ct);
        return Build("Contas a Receber",
            new[] { C("Codigo","Código"), C("Cliente","Cliente"), C("Descricao","Descrição"), C("DataEmissao","Emissão"), C("DataVencimento","Vencimento"), C("Valor","Valor"), C("Estado","Estado") },
            data.Select(x => R(("Codigo",x.Codigo),("Cliente",x.Cliente),("Descricao",x.Descricao),("DataEmissao",x.DataEmissao),("DataVencimento",x.DataVencimento),("Valor",x.Valor),("Estado",x.Estado))));
    }

    private async Task<ReportResult> PayablesAsync(ReportRequest request, CancellationToken ct)
    {
        var data = await _db.ContasPagar.AsNoTracking()
            .Include(x => x.Fornecedor)
            .Where(x => x.EmpresaId == request.CompanyId && x.DataEmissao >= request.From.Date && x.DataEmissao < request.To.Date.AddDays(1))
            .OrderBy(x => x.DataVencimento)
            .Select(x => new { x.Codigo, Fornecedor = x.Fornecedor != null ? x.Fornecedor.Nome : string.Empty, x.Descricao, x.DataEmissao, x.DataVencimento, x.Valor, Estado = x.Estado.ToString() })
            .ToListAsync(ct);
        return Build("Contas a Pagar",
            new[] { C("Codigo","Código"), C("Fornecedor","Fornecedor"), C("Descricao","Descrição"), C("DataEmissao","Emissão"), C("DataVencimento","Vencimento"), C("Valor","Valor"), C("Estado","Estado") },
            data.Select(x => R(("Codigo",x.Codigo),("Fornecedor",x.Fornecedor),("Descricao",x.Descricao),("DataEmissao",x.DataEmissao),("DataVencimento",x.DataVencimento),("Valor",x.Valor),("Estado",x.Estado))));
    }

    private async Task<ReportResult> TreasuryAsync(ReportRequest request, CancellationToken ct)
    {
        var data = await _db.Movimentos.AsNoTracking()
            .Include(x => x.Caixa).Include(x => x.ContaBancaria)
            .Where(x => x.EmpresaId == request.CompanyId && x.Data >= request.From.Date && x.Data < request.To.Date.AddDays(1))
            .OrderByDescending(x => x.Data)
            .Select(x => new { x.Data, x.Descricao, Tipo = x.Tipo.ToString(), Operacao = x.TipoOperacao.ToString(), Origem = x.Caixa != null ? x.Caixa.Nome : x.ContaBancaria != null ? (x.ContaBancaria.Titular + " - " + x.ContaBancaria.NumeroConta) : string.Empty, x.Valor, Estado = x.Estado.ToString(), x.Conciliado })
            .ToListAsync(ct);
        return Build("Movimentos de Tesouraria",
            new[] { C("Data","Data"), C("Descricao","Descrição"), C("Tipo","Tipo"), C("Operacao","Operação"), C("Origem","Origem"), C("Valor","Valor"), C("Estado","Estado"), C("Conciliado","Conciliado") },
            data.Select(x => R(("Data",x.Data),("Descricao",x.Descricao),("Tipo",x.Tipo),("Operacao",x.Operacao),("Origem",x.Origem),("Valor",x.Valor),("Estado",x.Estado),("Conciliado",x.Conciliado ? "Sim" : "Não"))));
    }

    private async Task<ReportResult> AssetsAsync(ReportRequest request, CancellationToken ct)
    {
        var data = await _db.Bens.AsNoTracking()
            .Where(x => x.EmpresaId == request.CompanyId && x.DataAquisicao <= request.To.Date.AddDays(1))
            .OrderBy(x => x.NumeroPatrimonial)
            .Select(x => new { x.NumeroPatrimonial, x.Descricao, x.Categoria, x.Localizacao, x.Responsavel, x.DataAquisicao, x.ValorAquisicao, Estado = x.Estado.ToString() })
            .ToListAsync(ct);
        return Build("Inventário Patrimonial",
            new[] { C("Numero","N.º patrimonial"), C("Descricao","Descrição"), C("Categoria","Categoria"), C("Localizacao","Localização"), C("Responsavel","Responsável"), C("Data","Aquisição"), C("Valor","Valor"), C("Estado","Estado") },
            data.Select(x => R(("Numero",x.NumeroPatrimonial),("Descricao",x.Descricao),("Categoria",x.Categoria),("Localizacao",x.Localizacao),("Responsavel",x.Responsavel),("Data",x.DataAquisicao),("Valor",x.ValorAquisicao),("Estado",x.Estado))));
    }

    private static ReportColumn C(string key, string header) => new(key, header);
    private static ReportRow R(params (string Key, object? Value)[] values) => new(values.ToDictionary(x => x.Key, x => x.Value));
    private static ReportResult Build(string title, IReadOnlyList<ReportColumn> columns, IEnumerable<ReportRow> rows) => new(title, columns, rows.ToList());
}
