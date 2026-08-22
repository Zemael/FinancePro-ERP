using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class OrcamentoService : IOrcamentoService
{
    private static readonly string[] NomesMeses =
    {
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };

    private readonly FinanceProDbContext _context;

    public OrcamentoService(FinanceProDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<OrcamentoListItemDto>> ListarAsync(int empresaId)
    {
        return await _context.Orcamentos
            .Where(o => o.EmpresaId == empresaId)
            .OrderByDescending(o => o.Ano)
            .Select(o => new OrcamentoListItemDto
            {
                Id = o.Id,
                Ano = o.Ano,
                Nome = o.Nome,
                DataInicio = o.DataInicio,
                DataFim = o.DataFim,
                Moeda = o.Moeda,
                Estado = o.Estado.ToString(),
                DataAprovacao = o.DataAprovacao
            })
            .ToListAsync();
    }

    public async Task<int> CriarAsync(NovoOrcamentoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            throw new InvalidOperationException("O nome do orçamento é obrigatório.");
        }

        if (dto.Ano < 2000 || dto.Ano > 2100)
        {
            throw new InvalidOperationException("Indique um ano válido.");
        }

        if (dto.DataFim < dto.DataInicio)
        {
            throw new InvalidOperationException("A data de fim não pode ser anterior à data de início.");
        }

        var orcamento = new Orcamento
        {
            Ano = dto.Ano,
            Nome = dto.Nome.Trim(),
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            Moeda = string.IsNullOrWhiteSpace(dto.Moeda) ? "FCFA" : dto.Moeda.Trim(),
            Observacoes = dto.Observacoes,
            ElaboradoPor = dto.ElaboradoPor,
            EmpresaId = dto.EmpresaId
        };

        _context.Orcamentos.Add(orcamento);
        await _context.SaveChangesAsync();
        return orcamento.Id;
    }

    public async Task<IReadOnlyList<PlanoContasOpcaoDto>> ListarPlanoContasAsync(int empresaId)
    {
        return await _context.PlanoContas
            .Where(p => p.EmpresaId == empresaId && p.Ativo)
            .OrderBy(p => p.Codigo)
            .Select(p => new PlanoContasOpcaoDto { Id = p.Id, Nome = $"{p.Codigo} · {p.Nome}" })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<OrcamentoDetalheDto>> ListarDetalhesAsync(int orcamentoId, TipoCategoria tipo)
    {
        var orcamento = await _context.Orcamentos.FindAsync(orcamentoId)
            ?? throw new InvalidOperationException("Orçamento não encontrado.");

        var detalhes = await _context.OrcamentoDetalhes
            .Where(d => d.OrcamentoId == orcamentoId && d.Tipo == tipo)
            .Include(d => d.PlanoContas)
            .OrderBy(d => d.Mes).ThenBy(d => d.PlanoContas.Codigo)
            .ToListAsync();

        var resultado = new List<OrcamentoDetalheDto>();
        foreach (var d in detalhes)
        {
            var realizado = await _context.Movimentos
                .Where(m => m.EmpresaId == orcamento.EmpresaId
                    && m.Tipo == tipo
                    && m.Data.Year == orcamento.Ano
                    && m.Data.Month == d.Mes
                    && m.Categoria != null && m.Categoria.PlanoContasId == d.PlanoContasId)
                .SumAsync(m => (decimal?)m.Valor) ?? 0m;

            resultado.Add(new OrcamentoDetalheDto
            {
                Id = d.Id,
                PlanoContasNome = $"{d.PlanoContas.Codigo} · {d.PlanoContas.Nome}",
                CentroCusto = d.CentroCusto,
                Departamento = d.Departamento,
                Mes = d.Mes,
                MesNome = NomesMeses[d.Mes - 1],
                ValorPrevisto = d.ValorPrevisto,
                ValorRealizado = realizado
            });
        }

        return resultado;
    }

    public async Task AdicionarDetalheAsync(NovoOrcamentoDetalheDto dto)
    {
        if (dto.PlanoContasId <= 0)
        {
            throw new InvalidOperationException("Selecione a conta do plano de contas.");
        }

        if (dto.Mes is < 1 or > 12)
        {
            throw new InvalidOperationException("Mês inválido.");
        }

        if (dto.ValorPrevisto < 0)
        {
            throw new InvalidOperationException("O valor previsto não pode ser negativo.");
        }

        _context.OrcamentoDetalhes.Add(new OrcamentoDetalhe
        {
            OrcamentoId = dto.OrcamentoId,
            PlanoContasId = dto.PlanoContasId,
            CentroCusto = dto.CentroCusto,
            Departamento = dto.Departamento,
            Tipo = dto.Tipo,
            Mes = dto.Mes,
            ValorPrevisto = dto.ValorPrevisto
        });

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ExecucaoMensalDto>> ObterExecucaoMensalAsync(int orcamentoId)
    {
        var orcamento = await _context.Orcamentos.FindAsync(orcamentoId)
            ?? throw new InvalidOperationException("Orçamento não encontrado.");

        var detalhes = await _context.OrcamentoDetalhes
            .Where(d => d.OrcamentoId == orcamentoId)
            .Include(d => d.PlanoContas)
            .ToListAsync();

        var movimentos = await _context.Movimentos
            .Where(m => m.EmpresaId == orcamento.EmpresaId && m.Data.Year == orcamento.Ano)
            .Include(m => m.Categoria)
            .ToListAsync();

        // Compromissos: compras já aprovadas/cotadas/com ordem emitida/recebidas,
        // mas ainda não faturadas. Mantêm-se separadas do realizado para evitar
        // dupla contagem quando a fatura/pagamento entra na execução real.
        var comprasComprometidas = await _context.Compras
            .Where(c => c.EmpresaId == orcamento.EmpresaId
                && c.Data.Year == orcamento.Ano
                && (c.Estado == EstadoCompra.Aprovado
                    || c.Estado == EstadoCompra.Cotado
                    || c.Estado == EstadoCompra.OrdemEmitida
                    || c.Estado == EstadoCompra.Recebido))
            .Select(c => new { c.Data, c.ValorTotal })
            .ToListAsync();

        var resultado = new List<ExecucaoMensalDto>();
        for (var mes = 1; mes <= 12; mes++)
        {
            var previstoReceitas = detalhes.Where(d => d.Mes == mes && d.Tipo == TipoCategoria.Receita).Sum(d => d.ValorPrevisto);
            var previstoDespesas = detalhes.Where(d => d.Mes == mes && d.Tipo == TipoCategoria.Despesa).Sum(d => d.ValorPrevisto);

            var idsContasReceita = detalhes.Where(d => d.Mes == mes && d.Tipo == TipoCategoria.Receita).Select(d => d.PlanoContasId).ToHashSet();
            var idsContasDespesa = detalhes.Where(d => d.Mes == mes && d.Tipo == TipoCategoria.Despesa).Select(d => d.PlanoContasId).ToHashSet();

            var realizadoReceitas = movimentos.Where(m => m.Data.Month == mes && m.Tipo == TipoCategoria.Receita
                && m.Categoria != null && m.Categoria.PlanoContasId.HasValue && idsContasReceita.Contains(m.Categoria.PlanoContasId.Value))
                .Sum(m => m.Valor);

            var realizadoDespesas = movimentos.Where(m => m.Data.Month == mes && m.Tipo == TipoCategoria.Despesa
                && m.Categoria != null && m.Categoria.PlanoContasId.HasValue && idsContasDespesa.Contains(m.Categoria.PlanoContasId.Value))
                .Sum(m => m.Valor);

            var comprometidoDespesas = comprasComprometidas
                .Where(c => c.Data.Month == mes)
                .Sum(c => c.ValorTotal);

            resultado.Add(new ExecucaoMensalDto
            {
                Mes = mes,
                MesNome = NomesMeses[mes - 1],
                PrevistoReceitas = previstoReceitas,
                RealizadoReceitas = realizadoReceitas,
                PrevistoDespesas = previstoDespesas,
                RealizadoDespesas = realizadoDespesas,
                ComprometidoDespesas = comprometidoDespesas
            });
        }

        return resultado;
    }

    public async Task<IReadOnlyList<RevisaoOrcamentalDto>> ListarRevisoesAsync(int orcamentoId)
    {
        return await _context.RevisoesOrcamentais
            .Where(r => r.OrcamentoId == orcamentoId)
            .OrderByDescending(r => r.Versao)
            .Select(r => new RevisaoOrcamentalDto
            {
                Id = r.Id,
                Versao = r.Versao,
                Data = r.Data,
                Motivo = r.Motivo,
                Responsavel = r.Responsavel,
                Estado = r.Estado.ToString()
            })
            .ToListAsync();
    }

    public async Task AdicionarRevisaoAsync(NovaRevisaoOrcamentalDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Motivo) || string.IsNullOrWhiteSpace(dto.Responsavel))
        {
            throw new InvalidOperationException("Indique o motivo e o responsável pela revisão.");
        }

        var ultimaVersao = await _context.RevisoesOrcamentais
            .Where(r => r.OrcamentoId == dto.OrcamentoId)
            .Select(r => (int?)r.Versao)
            .MaxAsync() ?? 0;

        _context.RevisoesOrcamentais.Add(new RevisaoOrcamental
        {
            OrcamentoId = dto.OrcamentoId,
            Versao = ultimaVersao + 1,
            Data = DateTime.Today,
            Motivo = dto.Motivo.Trim(),
            Responsavel = dto.Responsavel.Trim(),
            Estado = EstadoRevisao.Pendente
        });

        await _context.SaveChangesAsync();
    }

    public async Task<OrcamentoRelatorioDto> ObterRelatorioAsync(int orcamentoId)
    {
        var execucao = await ObterExecucaoMensalAsync(orcamentoId);

        return new OrcamentoRelatorioDto
        {
            TotalPrevistoReceitas = execucao.Sum(e => e.PrevistoReceitas),
            TotalRealizadoReceitas = execucao.Sum(e => e.RealizadoReceitas),
            TotalPrevistoDespesas = execucao.Sum(e => e.PrevistoDespesas),
            TotalRealizadoDespesas = execucao.Sum(e => e.RealizadoDespesas),
            TotalComprometidoDespesas = execucao.Sum(e => e.ComprometidoDespesas)
        };
    }
}
