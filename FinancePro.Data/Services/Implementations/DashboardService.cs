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
            Alertas = alertas
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
