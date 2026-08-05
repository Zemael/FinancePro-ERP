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
            "fiscal" => FiscalAsync(request, cancellationToken),
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


    private async Task<ReportResult> FiscalAsync(ReportRequest request, CancellationToken ct)
    {
        var cn = _db.Database.GetDbConnection();
        var shouldClose = cn.State != System.Data.ConnectionState.Open;
        if (shouldClose) await cn.OpenAsync(ct);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT Code,Name,Category,DueDate,Frequency,Status,Notes,Active
FROM dbo.FiscalObligations
WHERE CompanyId=@companyId AND DueDate>=@from AND DueDate<@to
ORDER BY DueDate,Code";
            Add(cmd, "@companyId", request.CompanyId);
            Add(cmd, "@from", request.From.Date);
            Add(cmd, "@to", request.To.Date.AddDays(1));
            var rows = new List<ReportRow>();
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                var dueDate = rd.GetDateTime(3);
                var status = rd.GetString(5);
                if (status == "Pendente" && dueDate.Date < DateTime.Today) status = "Atrasada";
                rows.Add(R(("Codigo",rd.GetString(0)),("Obrigacao",rd.GetString(1)),("Categoria",rd.GetString(2)),("Vencimento",dueDate),("Periodicidade",rd.GetString(4)),("Estado",status),("Observacoes",rd.IsDBNull(6)?string.Empty:rd.GetString(6)),("Ativo",rd.GetBoolean(7)?"Sim":"Não")));
            }
            return Build("Conformidade Fiscal", new[] { C("Codigo","Código"), C("Obrigacao","Obrigação"), C("Categoria","Categoria"), C("Vencimento","Vencimento"), C("Periodicidade","Periodicidade"), C("Estado","Estado"), C("Observacoes","Observações"), C("Ativo","Ativo") }, rows);
        }
        finally { if (shouldClose) await cn.CloseAsync(); }
    }

    private static void Add(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static ReportColumn C(string key, string header) => new(key, header);
    private static ReportRow R(params (string Key, object? Value)[] values) => new(values.ToDictionary(x => x.Key, x => x.Value));
    private static ReportResult Build(string title, IReadOnlyList<ReportColumn> columns, IEnumerable<ReportRow> rows) => new(title, columns, rows.ToList());
}
