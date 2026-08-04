using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly FinanceProDbContext _context;

    public DashboardService(FinanceProDbContext context) => _context = context;

    public async Task<DashboardResumoDto> ObterResumoAsync(int empresaId)
    {
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var empresa = await _context.Empresas.FindAsync(empresaId);

        var caixas = await _context.Caixas
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .ToListAsync();

        var contas = await _context.ContasBancarias
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .ToListAsync();

        var inicioFluxo = inicioMes.AddMonths(-5);
        var movimentos = await _context.Movimentos
            .Where(m => m.EmpresaId == empresaId && m.Data >= inicioFluxo)
            .ToListAsync();

        decimal SaldoMovimentos(Func<FinancePro.Core.Entities.Movimento, bool> filtro) =>
            movimentos.Where(m => m.TipoOperacao != TipoOperacao.Bloqueio)
                .Where(filtro)
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
                    Mensagem = $"Caixa {caixa.Nome} abaixo do saldo mínimo"
                });
            }
        }

        var saldoBancarioTotal = 0m;
        foreach (var conta in contas)
        {
            var saldo = conta.SaldoInicial + SaldoMovimentos(m => m.ContaBancariaId == conta.Id);
            saldoBancarioTotal += saldo;
            saldosPorOrigem.Add(new SaldoOrigemDto { Tipo = "Banco", Nome = conta.NumeroConta, Saldo = saldo });

            if (saldo < 0)
            {
                alertas.Add(new AlertaDto
                {
                    Severidade = "Critico",
                    Mensagem = $"Conta bancária {conta.NumeroConta} com saldo negativo"
                });
            }
        }

        var operacoesReais = new[] { TipoOperacao.Entrada, TipoOperacao.Saida, TipoOperacao.Ajuste };
        var totalReceitas = movimentos
            .Where(m => m.Tipo == TipoCategoria.Receita && operacoesReais.Contains(m.TipoOperacao) && m.Data >= inicioMes)
            .Sum(m => m.Valor);
        var totalDespesas = movimentos
            .Where(m => m.Tipo == TipoCategoria.Despesa && operacoesReais.Contains(m.TipoOperacao) && m.Data >= inicioMes)
            .Sum(m => m.Valor);

        var contasReceberPendentes = await _context.ContasReceber
            .Where(c => c.EmpresaId == empresaId && c.Estado == EstadoConta.Pendente)
            .Include(c => c.Cliente)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();

        var contasPagarPendentes = await _context.ContasPagar
            .Where(c => c.EmpresaId == empresaId && c.Estado == EstadoConta.Pendente)
            .ToListAsync();

        var atrasadasReceber = contasReceberPendentes.Count(c => c.DataVencimento.Date < hoje);
        var atrasadasPagar = contasPagarPendentes.Count(c => c.DataVencimento.Date < hoje);
        if (atrasadasReceber > 0)
            alertas.Add(new AlertaDto { Severidade = "Aviso", Mensagem = $"{atrasadasReceber} conta(s) a receber em atraso" });
        if (atrasadasPagar > 0)
            alertas.Add(new AlertaDto { Severidade = "Aviso", Mensagem = $"{atrasadasPagar} conta(s) a pagar em atraso" });

        var comprasPendentes = await _context.Compras.CountAsync(c =>
            c.EmpresaId == empresaId && c.Estado == EstadoCompra.Pendente);
        if (comprasPendentes > 0)
            alertas.Add(new AlertaDto { Severidade = "Info", Mensagem = $"{comprasPendentes} compra(s) aguardam aprovação" });

        var valorPatrimonio = await _context.Bens
            .Where(b => b.EmpresaId == empresaId && b.Estado != EstadoBem.Abatido && b.Estado != EstadoBem.Vendido)
            .SumAsync(b => (decimal?)b.ValorAquisicao) ?? 0m;

        var totalOrcamento = await _context.OrcamentoDetalhes
            .Where(d => d.Orcamento.EmpresaId == empresaId && d.Orcamento.Ano == hoje.Year)
            .SumAsync(d => (decimal?)d.ValorPrevisto) ?? 0m;
        var execucaoOrcamental = totalOrcamento > 0
            ? Math.Round(Math.Min(totalDespesas / totalOrcamento * 100m, 999m), 1)
            : 0m;

        var movimentosRecentes = movimentos
            .OrderByDescending(m => m.Data)
            .ThenByDescending(m => m.Id)
            .Take(6)
            .Select(m => new MovimentoRecenteDto
            {
                Data = m.Data,
                Descricao = m.Descricao,
                Tipo = m.Tipo.ToString(),
                Valor = m.Valor
            })
            .ToList();

        var pendencias = contasReceberPendentes.Take(6).Select(c => new ContaReceberListItemDto
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

        var atividades = await _context.LogsAuditoria
            .Where(l => l.EmpresaId == empresaId)
            .OrderByDescending(l => l.Data)
            .Take(8)
            .Select(l => new AtividadeRecenteDto
            {
                Data = l.Data,
                Utilizador = l.UtilizadorNome,
                Modulo = l.Entidade,
                Acao = l.Acao,
                Descricao = l.Detalhe ?? $"{l.Acao} em {l.Entidade}"
            })
            .ToListAsync();

        var fluxoMensal = Enumerable.Range(0, 6)
            .Select(i => inicioMes.AddMonths(i - 5))
            .Select(mes =>
            {
                var fim = mes.AddMonths(1);
                var entradas = movimentos.Where(m => m.Data >= mes && m.Data < fim && m.Tipo == TipoCategoria.Receita && operacoesReais.Contains(m.TipoOperacao)).Sum(m => m.Valor);
                var saidas = movimentos.Where(m => m.Data >= mes && m.Data < fim && m.Tipo == TipoCategoria.Despesa && operacoesReais.Contains(m.TipoOperacao)).Sum(m => m.Valor);
                return new FluxoCaixaMensalDto { Periodo = mes.ToString("MMM/yy"), Entradas = entradas, Saidas = saidas };
            })
            .ToList();

        if (!alertas.Any(a => a.Mensagem.Contains("Backup", StringComparison.OrdinalIgnoreCase)))
            alertas.Add(new AlertaDto { Severidade = "Info", Mensagem = "Backup automático ainda não configurado" });

        return new DashboardResumoDto
        {
            EmpresaNome = empresa?.Nome ?? string.Empty,
            Exercicio = hoje.Year,
            SaldoCaixa = saldoCaixaTotal,
            SaldoBancario = saldoBancarioTotal,
            TotalReceitas = totalReceitas,
            TotalDespesas = totalDespesas,
            TotalAReceber = contasReceberPendentes.Sum(c => c.Valor),
            TotalAPagar = contasPagarPendentes.Sum(c => c.Valor),
            ComprasPendentes = comprasPendentes,
            ValorPatrimonio = valorPatrimonio,
            TotalOrcamento = totalOrcamento,
            ExecucaoOrcamental = execucaoOrcamental,
            TotalAlertas = alertas.Count,
            MovimentosRecentes = movimentosRecentes,
            SaldosPorOrigem = saldosPorOrigem.OrderByDescending(s => s.Saldo).ToList(),
            Pendencias = pendencias,
            Alertas = alertas.Take(8).ToList(),
            AtividadesRecentes = atividades,
            FluxoMensal = fluxoMensal
        };
    }

    public async Task<IReadOnlyList<PesquisaResultadoDto>> PesquisarAsync(int empresaId, string termo)
    {
        if (string.IsNullOrWhiteSpace(termo) || termo.Trim().Length < 2)
            return Array.Empty<PesquisaResultadoDto>();

        var t = termo.Trim();
        var resultados = new List<PesquisaResultadoDto>();

        resultados.AddRange(await _context.Clientes
            .Where(c => c.EmpresaId == empresaId && EF.Functions.Like(c.Nome, $"%{t}%"))
            .Take(5)
            .Select(c => new PesquisaResultadoDto { Tipo = "Cliente", Descricao = c.Nome, Info = c.Telefone })
            .ToListAsync());

        resultados.AddRange(await _context.Fornecedores
            .Where(f => f.EmpresaId == empresaId && EF.Functions.Like(f.Nome, $"%{t}%"))
            .Take(5)
            .Select(f => new PesquisaResultadoDto { Tipo = "Fornecedor", Descricao = f.Nome, Info = f.Telefone })
            .ToListAsync());

        resultados.AddRange(await _context.ContasReceber
            .Where(c => c.EmpresaId == empresaId && (EF.Functions.Like(c.Descricao, $"%{t}%") || EF.Functions.Like(c.Codigo, $"%{t}%")))
            .Take(5)
            .Select(c => new PesquisaResultadoDto { Tipo = "Conta a Receber", Descricao = c.Descricao, Info = c.Codigo })
            .ToListAsync());

        resultados.AddRange(await _context.ContasPagar
            .Where(c => c.EmpresaId == empresaId && (EF.Functions.Like(c.Descricao, $"%{t}%") || EF.Functions.Like(c.Codigo, $"%{t}%")))
            .Take(5)
            .Select(c => new PesquisaResultadoDto { Tipo = "Conta a Pagar", Descricao = c.Descricao, Info = c.Codigo })
            .ToListAsync());

        resultados.AddRange(await _context.Movimentos
            .Where(m => m.EmpresaId == empresaId && EF.Functions.Like(m.Descricao, $"%{t}%"))
            .OrderByDescending(m => m.Data)
            .Take(5)
            .Select(m => new PesquisaResultadoDto { Tipo = "Movimento", Descricao = m.Descricao, Info = m.Valor + " FCFA" })
            .ToListAsync());

        resultados.AddRange(await _context.Bens
            .Where(b => b.EmpresaId == empresaId && (EF.Functions.Like(b.Descricao, $"%{t}%") || EF.Functions.Like(b.NumeroPatrimonial, $"%{t}%")))
            .Take(5)
            .Select(b => new PesquisaResultadoDto { Tipo = "Bem", Descricao = b.Descricao, Info = b.NumeroPatrimonial })
            .ToListAsync());

        return resultados;
    }
}
