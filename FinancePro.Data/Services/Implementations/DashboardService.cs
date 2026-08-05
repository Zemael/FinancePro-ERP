using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly FinanceProDbContext _context;

    public DashboardService(FinanceProDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResumoDto> ObterResumoAsync(int empresaId)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);

        var caixas = await _context.Caixas
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .ToListAsync();

        var contas = await _context.ContasBancarias
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .ToListAsync();

        var movimentos = await _context.Movimentos
            .Where(m => m.EmpresaId == empresaId)
            .ToListAsync();

        decimal SaldoMovimentos(Func<FinancePro.Core.Entities.Movimento, bool> filtro) =>
            movimentos.Where(m => m.TipoOperacao != TipoOperacao.Bloqueio).Where(filtro)
                .Sum(m => m.Tipo == TipoCategoria.Receita ? m.Valor : -m.Valor);

        var saldosPorOrigem = new List<SaldoOrigemDto>();
        var alertas = new List<AlertaDto>();

        var saldoCaixaTotal = 0m;
        foreach (var caixa in caixas)
        {
            var saldo = caixa.SaldoInicial + SaldoMovimentos(m => m.CaixaId == caixa.Id);
            saldoCaixaTotal += saldo;
            saldosPorOrigem.Add(new SaldoOrigemDto { Tipo = "Caixa", Nome = caixa.Nome, Saldo = saldo });

            if (caixa.SaldoMinimo.HasValue && saldo < caixa.SaldoMinimo.Value)
            {
                alertas.Add(new AlertaDto
                {
                    Severidade = "Critico",
                    Mensagem = $"Caixa \"{caixa.Nome}\" abaixo do saldo mínimo ({saldo:#,##0} / mínimo {caixa.SaldoMinimo:#,##0} FCFA)"
                });
            }
        }

        var saldoBancarioTotal = 0m;
        foreach (var conta in contas)
        {
            var saldo = conta.SaldoInicial + SaldoMovimentos(m => m.ContaBancariaId == conta.Id);
            saldoBancarioTotal += saldo;
            saldosPorOrigem.Add(new SaldoOrigemDto { Tipo = "Banco", Nome = $"{conta.NumeroConta}", Saldo = saldo });

            if (saldo < 0)
            {
                alertas.Add(new AlertaDto
                {
                    Severidade = "Critico",
                    Mensagem = $"Conta bancária \"{conta.NumeroConta}\" está negativa ({saldo:#,##0} FCFA)"
                });
            }
        }

        var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var operacoesReais = new[] { TipoOperacao.Entrada, TipoOperacao.Saida, TipoOperacao.Ajuste };
        var totalReceitas = movimentos.Where(m => m.Tipo == TipoCategoria.Receita && operacoesReais.Contains(m.TipoOperacao) && m.Data >= inicioMes).Sum(m => m.Valor);
        var totalDespesas = movimentos.Where(m => m.Tipo == TipoCategoria.Despesa && operacoesReais.Contains(m.TipoOperacao) && m.Data >= inicioMes).Sum(m => m.Valor);

        var movimentosRecentes = movimentos
            .OrderByDescending(m => m.Data)
            .ThenByDescending(m => m.Id)
            .Take(5)
            .Select(m => new MovimentoRecenteDto { Data = m.Data, Descricao = m.Descricao, Tipo = m.Tipo.ToString(), Valor = m.Valor })
            .ToList();

        var hoje = DateTime.Today;
        var pendentes = await _context.ContasReceber
            .Where(c => c.EmpresaId == empresaId && c.Estado == EstadoConta.Pendente)
            .Include(c => c.Cliente)
            .OrderBy(c => c.DataVencimento)
            .Take(5)
            .ToListAsync();

        var pendencias = pendentes.Select(c => new ContaReceberListItemDto
        {
            Id = c.Id,
            Descricao = c.Descricao,
            Valor = c.Valor,
            DataEmissao = c.DataEmissao,
            DataVencimento = c.DataVencimento,
            ClienteNome = c.Cliente?.Nome,
            EstadoExibicao = c.DataVencimento.Date < hoje ? "Atrasado" : "Pendente",
            PodeReceber = true,
            PodeCancelar = true
        }).ToList();

        var totalAtrasadas = await _context.ContasReceber
            .CountAsync(c => c.EmpresaId == empresaId && c.Estado == EstadoConta.Pendente && c.DataVencimento < hoje);

        if (totalAtrasadas > 0)
        {
            alertas.Add(new AlertaDto
            {
                Severidade = "Aviso",
                Mensagem = $"{totalAtrasadas} conta(s) a receber em atraso"
            });
        }


        var fiscalAtrasadas = 0;
        var fiscalProximas = 0;
        var fiscalCumpridas = 0;
        var taxaConformidadeFiscal = 100m;

        // Integração com o Calendário Fiscal. O bloco é tolerante a instalações
        // que ainda não executaram o script 017_FiscalCalendar.sql.
        try
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose) await connection.OpenAsync();
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = @"SELECT
    SUM(CASE WHEN Active=1 AND Status<>'Cancelada' AND (Status='Atrasada' OR (Status='Pendente' AND DueDate < CAST(GETDATE() AS date))) THEN 1 ELSE 0 END),
    SUM(CASE WHEN Active=1 AND Status='Pendente' AND DueDate >= CAST(GETDATE() AS date) AND DueDate <= DATEADD(day,7,CAST(GETDATE() AS date)) THEN 1 ELSE 0 END),
    SUM(CASE WHEN Active=1 AND Status='Cumprida' THEN 1 ELSE 0 END),
    SUM(CASE WHEN Active=1 AND Status<>'Cancelada' THEN 1 ELSE 0 END)
FROM dbo.FiscalObligations WHERE CompanyId=@companyId";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@companyId";
                parameter.Value = empresaId;
                command.Parameters.Add(parameter);
                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    fiscalAtrasadas = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                    fiscalProximas = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));
                    fiscalCumpridas = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2));
                    var fiscalTotal = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3));
                    taxaConformidadeFiscal = fiscalTotal == 0 ? 100m : Math.Round(fiscalCumpridas * 100m / fiscalTotal, 1);
                }
            }
            finally
            {
                if (shouldClose) await connection.CloseAsync();
            }

            if (fiscalAtrasadas > 0)
                alertas.Add(new AlertaDto { Severidade = "Critico", Mensagem = $"{fiscalAtrasadas} obrigação(ões) fiscal(is) em atraso" });
            if (fiscalProximas > 0)
                alertas.Add(new AlertaDto { Severidade = "Aviso", Mensagem = $"{fiscalProximas} obrigação(ões) fiscal(is) vence(m) nos próximos 7 dias" });
        }
        catch (System.Data.Common.DbException)
        {
            // Calendário fiscal ainda não instalado: o restante Dashboard continua funcional.
        }

        // Lembrete estático — ainda não existe um subsistema de backup real na app.
        alertas.Add(new AlertaDto { Severidade = "Info", Mensagem = "Backup da base de dados não configurado" });

        return new DashboardResumoDto
        {
            EmpresaNome = empresa?.Nome ?? string.Empty,
            Exercicio = DateTime.Today.Year,
            SaldoCaixa = saldoCaixaTotal,
            SaldoBancario = saldoBancarioTotal,
            TotalReceitas = totalReceitas,
            TotalDespesas = totalDespesas,
            MovimentosRecentes = movimentosRecentes,
            SaldosPorOrigem = saldosPorOrigem,
            Pendencias = pendencias,
            Alertas = alertas,
            ObrigacoesFiscaisAtrasadas = fiscalAtrasadas,
            ObrigacoesFiscaisProximas = fiscalProximas,
            ObrigacoesFiscaisCumpridas = fiscalCumpridas,
            TaxaConformidadeFiscal = taxaConformidadeFiscal
        };
    }

    public async Task<IReadOnlyList<PesquisaResultadoDto>> PesquisarAsync(int empresaId, string termo)
    {
        if (string.IsNullOrWhiteSpace(termo) || termo.Trim().Length < 2)
        {
            return Array.Empty<PesquisaResultadoDto>();
        }

        var t = termo.Trim();
        var resultados = new List<PesquisaResultadoDto>();

        var clientes = await _context.Clientes
            .Where(c => c.EmpresaId == empresaId && EF.Functions.Like(c.Nome, $"%{t}%"))
            .Take(5)
            .Select(c => new PesquisaResultadoDto { Tipo = "Cliente", Descricao = c.Nome, Info = c.Telefone })
            .ToListAsync();
        resultados.AddRange(clientes);

        var fornecedores = await _context.Fornecedores
            .Where(f => f.EmpresaId == empresaId && EF.Functions.Like(f.Nome, $"%{t}%"))
            .Take(5)
            .Select(f => new PesquisaResultadoDto { Tipo = "Fornecedor", Descricao = f.Nome, Info = f.Telefone })
            .ToListAsync();
        resultados.AddRange(fornecedores);

        var contasReceber = await _context.ContasReceber
            .Where(c => c.EmpresaId == empresaId && EF.Functions.Like(c.Descricao, $"%{t}%"))
            .Take(5)
            .Select(c => new PesquisaResultadoDto { Tipo = "Conta a Receber", Descricao = c.Descricao, Info = c.Valor + " FCFA" })
            .ToListAsync();
        resultados.AddRange(contasReceber);

        var movimentos = await _context.Movimentos
            .Where(m => m.EmpresaId == empresaId && EF.Functions.Like(m.Descricao, $"%{t}%"))
            .OrderByDescending(m => m.Data)
            .Take(5)
            .Select(m => new PesquisaResultadoDto { Tipo = "Movimento", Descricao = m.Descricao, Info = m.Valor + " FCFA" })
            .ToListAsync();
        resultados.AddRange(movimentos);

        return resultados;
    }
}
