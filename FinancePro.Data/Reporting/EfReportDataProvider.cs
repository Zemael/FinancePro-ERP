using System.Data.Common;
using FinancePro.Data.Context;
using FinancePro.Platform.Reporting;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Reporting;

public sealed class EfReportDataProvider : IReportDataProvider
{
    private readonly FinanceProDbContext _db;
    public EfReportDataProvider(FinanceProDbContext db) => _db = db;

    public async Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        var result = await (request.ReportKey.ToLowerInvariant() switch
        {
            "receivables" => ReceivablesAsync(request, cancellationToken),
            "payables" => PayablesAsync(request, cancellationToken),
            "treasury" => TreasuryAsync(request, cancellationToken),
            "assets" => AssetsAsync(request, cancellationToken),
            "fiscal" => FiscalAsync(request, cancellationToken),
            "accounting-journal" => AccountingJournalAsync(request, cancellationToken),
            "accounting-ledger" => AccountingLedgerAsync(request, cancellationToken),
            "accounting-trial-balance" => TrialBalanceAsync(request, cancellationToken),
            "accounting-income-statement" => IncomeStatementAsync(request, cancellationToken),
            "accounting-balance-sheet" => BalanceSheetAsync(request, cancellationToken),
            "accounting-cash-flow" => CashFlowAsync(request, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(request), "Relatório não suportado.")
        });

        var company = await _db.Empresas.AsNoTracking()
            .Where(x => x.Id == request.CompanyId)
            .Select(x => new { x.Nome, x.NIF })
            .FirstOrDefaultAsync(cancellationToken);

        return result with
        {
            CompanyName = company?.Nome ?? string.Empty,
            CompanyTaxNumber = company?.NIF ?? string.Empty,
            PeriodFrom = request.From.Date,
            PeriodTo = request.To.Date
        };
    }

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

    private async Task<ReportResult> AccountingJournalAsync(ReportRequest request, CancellationToken ct)
    {
        var rows = await QueryAsync(@"SELECT e.EntryDate,e.DocumentNumber,e.Reference,e.Description,e.SourceModule,e.Status,
 a.Code,a.Name,COALESCE(NULLIF(l.Description,''),e.Description),l.Debit,l.Credit
FROM dbo.AccountingEntries e
INNER JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=e.Id
INNER JOIN dbo.EnterpriseChartAccounts a ON a.Id=l.AccountId
WHERE e.CompanyId=@companyId AND e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to)
ORDER BY e.EntryDate,e.Id,l.Id", request, ct, rd =>
            R(("Data",rd.GetDateTime(0)),("Documento",rd.GetString(1)),("Referencia",rd.GetString(2)),("Descricao",rd.GetString(3)),("Modulo",rd.GetString(4)),("Estado",rd.GetString(5)),("Conta",rd.GetString(6)),("NomeConta",rd.GetString(7)),("Historico",rd.GetString(8)),("Debito",rd.GetDecimal(9)),("Credito",rd.GetDecimal(10))));
        return Build("Diário Contabilístico", new[] { C("Data","Data"), C("Documento","Documento"), C("Referencia","Referência"), C("Conta","Conta"), C("NomeConta","Descrição da conta"), C("Historico","Histórico"), C("Debito","Débito"), C("Credito","Crédito"), C("Modulo","Origem"), C("Estado","Estado") }, rows);
    }

    private async Task<ReportResult> AccountingLedgerAsync(ReportRequest request, CancellationToken ct)
    {
        var rows = await QueryAsync(@"WITH O AS (
 SELECT a.Id AccountId,COALESCE(SUM(CASE WHEN e.Status='Contabilizado' AND e.EntryDate<@from THEN l.Debit-l.Credit ELSE 0 END),0) OpeningBalance
 FROM dbo.EnterpriseChartAccounts a
 LEFT JOIN dbo.AccountingEntryLines l ON l.AccountId=a.Id
 LEFT JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId AND e.CompanyId=@companyId
 WHERE a.CompanyId=@companyId GROUP BY a.Id),
M AS (
 SELECT a.Id AccountId,a.Code,a.Name,e.EntryDate,e.Id EntryId,l.Id LineId,e.DocumentNumber,e.Reference,
 COALESCE(NULLIF(l.Description,''),e.Description) Description,l.Debit,l.Credit,O.OpeningBalance
 FROM dbo.AccountingEntries e
 INNER JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=e.Id
 INNER JOIN dbo.EnterpriseChartAccounts a ON a.Id=l.AccountId
 INNER JOIN O ON O.AccountId=a.Id
 WHERE e.CompanyId=@companyId AND e.Status='Contabilizado' AND e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to))
SELECT Code,Name,EntryDate,DocumentNumber,Reference,Description,Debit,Credit,
OpeningBalance+SUM(Debit-Credit) OVER(PARTITION BY AccountId ORDER BY EntryDate,EntryId,LineId ROWS UNBOUNDED PRECEDING) RunningBalance
FROM M ORDER BY Code,EntryDate,EntryId,LineId", request, ct, rd =>
            R(("Conta",rd.GetString(0)),("NomeConta",rd.GetString(1)),("Data",rd.GetDateTime(2)),("Documento",rd.GetString(3)),("Referencia",rd.GetString(4)),("Descricao",rd.GetString(5)),("Debito",rd.GetDecimal(6)),("Credito",rd.GetDecimal(7)),("Saldo",rd.GetDecimal(8))));
        return Build("Razão Geral", new[] { C("Conta","Conta"), C("NomeConta","Descrição da conta"), C("Data","Data"), C("Documento","Documento"), C("Referencia","Referência"), C("Descricao","Histórico"), C("Debito","Débito"), C("Credito","Crédito"), C("Saldo","Saldo acumulado") }, rows);
    }

    private async Task<ReportResult> TrialBalanceAsync(ReportRequest request, CancellationToken ct)
    {
        var rows = await QueryAsync(@"WITH M AS (
 SELECT a.Code,a.Name,
 COALESCE(SUM(CASE WHEN e.Status='Contabilizado' AND e.EntryDate<@from THEN l.Debit ELSE 0 END),0) OpeningDebitRaw,
 COALESCE(SUM(CASE WHEN e.Status='Contabilizado' AND e.EntryDate<@from THEN l.Credit ELSE 0 END),0) OpeningCreditRaw,
 COALESCE(SUM(CASE WHEN e.Status='Contabilizado' AND e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to) THEN l.Debit ELSE 0 END),0) PeriodDebit,
 COALESCE(SUM(CASE WHEN e.Status='Contabilizado' AND e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to) THEN l.Credit ELSE 0 END),0) PeriodCredit
 FROM dbo.EnterpriseChartAccounts a
 LEFT JOIN dbo.AccountingEntryLines l ON l.AccountId=a.Id
 LEFT JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId AND e.CompanyId=@companyId AND e.EntryDate<DATEADD(day,1,@to)
 WHERE a.CompanyId=@companyId GROUP BY a.Code,a.Name)
SELECT Code,Name,
CASE WHEN OpeningDebitRaw-OpeningCreditRaw>0 THEN OpeningDebitRaw-OpeningCreditRaw ELSE 0 END OpeningDebit,
CASE WHEN OpeningCreditRaw-OpeningDebitRaw>0 THEN OpeningCreditRaw-OpeningDebitRaw ELSE 0 END OpeningCredit,
PeriodDebit,PeriodCredit,
CASE WHEN (OpeningDebitRaw-OpeningCreditRaw)+(PeriodDebit-PeriodCredit)>0 THEN (OpeningDebitRaw-OpeningCreditRaw)+(PeriodDebit-PeriodCredit) ELSE 0 END ClosingDebit,
CASE WHEN (OpeningCreditRaw-OpeningDebitRaw)+(PeriodCredit-PeriodDebit)>0 THEN (OpeningCreditRaw-OpeningDebitRaw)+(PeriodCredit-PeriodDebit) ELSE 0 END ClosingCredit
FROM M WHERE OpeningDebitRaw<>0 OR OpeningCreditRaw<>0 OR PeriodDebit<>0 OR PeriodCredit<>0 ORDER BY Code", request, ct, rd =>
            R(("Conta",rd.GetString(0)),("Descricao",rd.GetString(1)),("SaldoInicialDebito",rd.GetDecimal(2)),("SaldoInicialCredito",rd.GetDecimal(3)),("Debitos",rd.GetDecimal(4)),("Creditos",rd.GetDecimal(5)),("SaldoFinalDebito",rd.GetDecimal(6)),("SaldoFinalCredito",rd.GetDecimal(7))));
        return Build("Balancete", new[] { C("Conta","Conta"), C("Descricao","Descrição"), C("SaldoInicialDebito","Saldo inicial débito"), C("SaldoInicialCredito","Saldo inicial crédito"), C("Debitos","Débitos"), C("Creditos","Créditos"), C("SaldoFinalDebito","Saldo final débito"), C("SaldoFinalCredito","Saldo final crédito") }, rows);
    }

    private async Task<ReportResult> IncomeStatementAsync(ReportRequest request, CancellationToken ct)
    {
        var days = (request.To.Date - request.From.Date).Days + 1;
        var previousTo = request.From.Date.AddDays(-1);
        var previousFrom = previousTo.AddDays(-(days - 1));
        var rows = new List<ReportRow>();
        var cn = _db.Database.GetDbConnection(); var close = cn.State != System.Data.ConnectionState.Open; if (close) await cn.OpenAsync(ct);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT a.Code,a.Name,
CASE WHEN a.Code LIKE '41%' THEN 'Deduções' WHEN a.Code LIKE '4%' THEN 'Receitas' WHEN a.Code LIKE '5%' THEN 'Custos' WHEN a.Code LIKE '6%' THEN 'Despesas operacionais' WHEN a.Code LIKE '7%' THEN 'Resultado financeiro' WHEN a.Code LIKE '8%' THEN 'Impostos' ELSE '' END Grupo,
SUM(CASE WHEN e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to) THEN CASE WHEN a.Code LIKE '4%' OR a.Code LIKE '7%' THEN l.Credit-l.Debit ELSE l.Debit-l.Credit END ELSE 0 END) Atual,
SUM(CASE WHEN e.EntryDate>=@previousFrom AND e.EntryDate<DATEADD(day,1,@previousTo) THEN CASE WHEN a.Code LIKE '4%' OR a.Code LIKE '7%' THEN l.Credit-l.Debit ELSE l.Debit-l.Credit END ELSE 0 END) Anterior
FROM dbo.EnterpriseChartAccounts a
INNER JOIN dbo.AccountingEntryLines l ON l.AccountId=a.Id
INNER JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId
WHERE a.CompanyId=@companyId AND e.CompanyId=@companyId AND e.Status='Contabilizado'
AND (a.Code LIKE '4%' OR a.Code LIKE '5%' OR a.Code LIKE '6%' OR a.Code LIKE '7%' OR a.Code LIKE '8%')
AND ((e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to)) OR (e.EntryDate>=@previousFrom AND e.EntryDate<DATEADD(day,1,@previousTo)))
GROUP BY a.Code,a.Name ORDER BY a.Code";
            Add(cmd,"@companyId",request.CompanyId); Add(cmd,"@from",request.From.Date); Add(cmd,"@to",request.To.Date); Add(cmd,"@previousFrom",previousFrom); Add(cmd,"@previousTo",previousTo);
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            decimal revenue=0,deductions=0,costs=0,opex=0,financial=0,tax=0,prevRevenue=0,prevDeductions=0,prevCosts=0,prevOpex=0,prevFinancial=0,prevTax=0;
            while (await rd.ReadAsync(ct))
            {
                var group=rd.GetString(2); var current=rd.GetDecimal(3); var previous=rd.GetDecimal(4);
                rows.Add(R(("Codigo",rd.GetString(0)),("Descricao",rd.GetString(1)),("Grupo",group),("Atual",current),("Anterior",previous),("Variacao",current-previous)));
                switch(group){case "Receitas":revenue+=current;prevRevenue+=previous;break;case "Deduções":deductions+=current;prevDeductions+=previous;break;case "Custos":costs+=current;prevCosts+=previous;break;case "Despesas operacionais":opex+=current;prevOpex+=previous;break;case "Resultado financeiro":financial+=current;prevFinancial+=previous;break;case "Impostos":tax+=current;prevTax+=previous;break;}
            }
            var netRevenue=revenue-deductions; var prevNetRevenue=prevRevenue-prevDeductions; var grossProfit=netRevenue-costs; var prevGrossProfit=prevNetRevenue-prevCosts; var operating=grossProfit-opex; var prevOperating=prevGrossProfit-prevOpex; var beforeTax=operating+financial; var prevBeforeTax=prevOperating+prevFinancial; var net=beforeTax-tax; var prevNet=prevBeforeTax-prevTax;
            rows.Add(R(("Codigo",""),("Descricao","RESULTADO LÍQUIDO"),("Grupo","Subtotal"),("Atual",net),("Anterior",prevNet),("Variacao",net-prevNet)));
        }
        finally { if(close) await cn.CloseAsync(); }
        return Build($"DRE · {request.From:dd/MM/yyyy} a {request.To:dd/MM/yyyy}", new[] { C("Codigo","Código"),C("Descricao","Conta / linha"),C("Grupo","Grupo"),C("Atual","Período atual"),C("Anterior","Período anterior"),C("Variacao","Variação") }, rows);
    }

    private async Task<ReportResult> BalanceSheetAsync(ReportRequest request, CancellationToken ct)
    {
        var previousAsOf = request.From.Date.AddDays(-1);
        var rows = new List<ReportRow>();
        var cn = _db.Database.GetDbConnection(); var close = cn.State != System.Data.ConnectionState.Open; if(close) await cn.OpenAsync(ct);
        try
        {
            await using var cmd=cn.CreateCommand();
            cmd.CommandText=@"SELECT a.Code,a.Name,
CASE WHEN a.Code LIKE '10%' OR a.Code LIKE '11%' OR a.Code LIKE '12%' THEN 'Ativo circulante'
     WHEN a.Code LIKE '1%' THEN 'Ativo não circulante'
     WHEN a.Code LIKE '20%' OR a.Code LIKE '21%' OR a.Code LIKE '22%' THEN 'Passivo circulante'
     WHEN a.Code LIKE '2%' THEN 'Passivo não circulante'
     WHEN a.Code LIKE '3%' THEN 'Património líquido' ELSE '' END Seccao,
SUM(CASE WHEN e.EntryDate<DATEADD(day,1,@asOf) THEN CASE WHEN a.Code LIKE '1%' THEN l.Debit-l.Credit ELSE l.Credit-l.Debit END ELSE 0 END) Atual,
SUM(CASE WHEN e.EntryDate<DATEADD(day,1,@previousAsOf) THEN CASE WHEN a.Code LIKE '1%' THEN l.Debit-l.Credit ELSE l.Credit-l.Debit END ELSE 0 END) Anterior
FROM dbo.EnterpriseChartAccounts a
INNER JOIN dbo.AccountingEntryLines l ON l.AccountId=a.Id
INNER JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId
WHERE a.CompanyId=@companyId AND e.CompanyId=@companyId AND e.Status='Contabilizado' AND (a.Code LIKE '1%' OR a.Code LIKE '2%' OR a.Code LIKE '3%')
GROUP BY a.Code,a.Name ORDER BY a.Code";
            Add(cmd,"@companyId",request.CompanyId);Add(cmd,"@asOf",request.To.Date);Add(cmd,"@previousAsOf",previousAsOf);
            await using var rd=await cmd.ExecuteReaderAsync(ct);
            decimal assets=0,liabilities=0,equity=0,prevAssets=0,prevLiabilities=0,prevEquity=0;
            while(await rd.ReadAsync(ct))
            {
                var section=rd.GetString(2);var current=rd.GetDecimal(3);var previous=rd.GetDecimal(4);
                rows.Add(R(("Conta",rd.GetString(0)),("Descricao",rd.GetString(1)),("Seccao",section),("Atual",current),("Anterior",previous),("Variacao",current-previous)));
                if(section.StartsWith("Ativo")){assets+=current;prevAssets+=previous;}else if(section.StartsWith("Passivo")){liabilities+=current;prevLiabilities+=previous;}else{equity+=current;prevEquity+=previous;}
            }
            rows.Add(R(("Conta",""),("Descricao","TOTAL DO ATIVO"),("Seccao","Subtotal"),("Atual",assets),("Anterior",prevAssets),("Variacao",assets-prevAssets)));
            rows.Add(R(("Conta",""),("Descricao","TOTAL PASSIVO + PATRIMÓNIO LÍQUIDO"),("Seccao","Subtotal"),("Atual",liabilities+equity),("Anterior",prevLiabilities+prevEquity),("Variacao",(liabilities+equity)-(prevLiabilities+prevEquity))));
            rows.Add(R(("Conta",""),("Descricao","DIFERENÇA DE EQUILÍBRIO"),("Seccao","Validação"),("Atual",assets-(liabilities+equity)),("Anterior",prevAssets-(prevLiabilities+prevEquity)),("Variacao",0m)));
        }
        finally { if(close) await cn.CloseAsync(); }
        return Build($"Balanço Patrimonial · {request.To:dd/MM/yyyy}", new[] { C("Conta","Conta"),C("Descricao","Descrição"),C("Seccao","Secção"),C("Atual","Atual"),C("Anterior","Comparativo"),C("Variacao","Variação") }, rows);
    }

    private async Task<ReportResult> CashFlowAsync(ReportRequest request, CancellationToken ct)
    {
        var rows = await QueryAsync(@"SELECT e.EntryDate,e.DocumentNumber,e.Description,
CASE WHEN e.SourceModule IN ('Assets','Patrimonio','Património') THEN 'Investimento'
     WHEN e.SourceModule IN ('Financing','Financiamento','Capital','Loans','Emprestimos','Empréstimos') THEN 'Financiamento'
     ELSE 'Operacional' END Atividade,
SUM(CASE WHEN l.Debit-l.Credit>0 THEN l.Debit-l.Credit ELSE 0 END) Entradas,
SUM(CASE WHEN l.Debit-l.Credit<0 THEN l.Credit-l.Debit ELSE 0 END) Saidas
FROM dbo.AccountingEntries e
INNER JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=e.Id
INNER JOIN dbo.EnterpriseChartAccounts a ON a.Id=l.AccountId
WHERE e.CompanyId=@companyId AND e.Status='Contabilizado' AND e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to)
AND (a.Code LIKE '10%' OR a.Code LIKE '11%' OR a.Code LIKE '12%')
GROUP BY e.EntryDate,e.DocumentNumber,e.Description,e.SourceModule,e.Id
HAVING SUM(l.Debit-l.Credit)<>0 ORDER BY e.EntryDate,e.Id", request, ct, rd =>
            R(("Data",rd.GetDateTime(0)),("Documento",rd.GetString(1)),("Descricao",rd.GetString(2)),("Atividade",rd.GetString(3)),("Entradas",rd.GetDecimal(4)),("Saidas",rd.GetDecimal(5)),("Liquido",rd.GetDecimal(4)-rd.GetDecimal(5))));
        return Build("Fluxo de Caixa", new[] { C("Data","Data"),C("Documento","Documento"),C("Descricao","Descrição"),C("Atividade","Atividade"),C("Entradas","Entradas"),C("Saidas","Saídas"),C("Liquido","Líquido") }, rows);
    }

    private async Task<List<ReportRow>> QueryAsync(string sql, ReportRequest request, CancellationToken ct, Func<DbDataReader,ReportRow> map)
    {
        var cn=_db.Database.GetDbConnection();var close=cn.State!=System.Data.ConnectionState.Open;if(close)await cn.OpenAsync(ct);
        try
        {
            await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",request.CompanyId);Add(cmd,"@from",request.From.Date);Add(cmd,"@to",request.To.Date);
            var rows=new List<ReportRow>();await using var rd=await cmd.ExecuteReaderAsync(ct);while(await rd.ReadAsync(ct))rows.Add(map(rd));return rows;
        }
        finally{if(close)await cn.CloseAsync();}
    }

    private static void Add(DbCommand command, string name, object value)
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
