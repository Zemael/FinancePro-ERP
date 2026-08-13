using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class TesourariaService : ITesourariaService
{
    private readonly FinanceProDbContext _context;

    public TesourariaService(FinanceProDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MovimentoListItemDto>> ListarMovimentosAsync(int empresaId, int maxRegistos = 100)
    {
        return await _context.Movimentos
            .Where(m => m.EmpresaId == empresaId)
            .Include(m => m.Categoria)
            .Include(m => m.Caixa)
            .Include(m => m.ContaBancaria)
            .OrderByDescending(m => m.Data)
            .ThenByDescending(m => m.Id)
            .Take(maxRegistos)
            .Select(m => new MovimentoListItemDto
            {
                Id = m.Id,
                Data = m.Data,
                Descricao = m.Descricao,
                Tipo = m.Tipo.ToString(),
                TipoOperacao = m.TipoOperacao.ToString(),
                Estado = m.Estado.ToString(),
                Conciliado = m.Conciliado,
                Valor = m.Valor,
                CategoriaNome = m.Categoria != null ? m.Categoria.Nome : null,
                FormaPagamento = m.FormaPagamento,
                CentroCusto = m.CentroCusto,
                Origem = m.Caixa != null ? $"Caixa Â· {m.Caixa.Nome}" : $"Banco Â· {m.ContaBancaria!.NumeroConta}"
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId)
    {
        var caixas = await _context.Caixas
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .Select(c => new OpcaoOrigemDto
            {
                Tipo = "Caixa",
                Id = c.Id,
                Nome = $"Caixa Â· {c.Nome}",
                Disponivel = _context.SessoesCaixa.Any(s =>
                    s.CaixaId == c.Id && s.Estado == EstadoSessaoCaixa.Aberta),
                MotivoIndisponibilidade = _context.SessoesCaixa.Any(s =>
                    s.CaixaId == c.Id && s.Estado == EstadoSessaoCaixa.Aberta)
                    ? null
                    : "Abra uma sessÃ£o de caixa antes de receber neste caixa."
            })
            .ToListAsync();

        var contas = await _context.ContasBancarias
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .Include(c => c.Banco)
            .Select(c => new OpcaoOrigemDto { Tipo = "ContaBancaria", Id = c.Id, Nome = $"Banco Â· {c.Banco.Nome} ({c.NumeroConta})" })
            .ToListAsync();

        return caixas.Concat(contas).ToList();
    }

    public async Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId, TipoCategoria tipo)
    {
        return await _context.Categorias
            .Where(c => c.EmpresaId == empresaId && c.Ativo && c.Tipo == tipo)
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaOpcaoDto { Id = c.Id, Nome = c.Nome })
            .ToListAsync();
    }

    public async Task<int> RegistarMovimentoAsync(NovoMovimentoDto dto)
    {
        var temCaixa = dto.CaixaId.HasValue;
        var temConta = dto.ContaBancariaId.HasValue;

        if (temCaixa == temConta)
        {
            throw new InvalidOperationException("Escolha exatamente uma origem: caixa ou conta bancÃ¡ria.");
        }

        if (string.IsNullOrWhiteSpace(dto.Descricao))
        {
            throw new InvalidOperationException("A descriÃ§Ã£o Ã© obrigatÃ³ria.");
        }

        if (dto.TipoOperacao is TipoOperacao.Transferencia or TipoOperacao.Sangria or TipoOperacao.Reforco)
        {
            throw new InvalidOperationException("TransferÃªncia/Sangria/ReforÃ§o tÃªm de ter origem e destino â€” use RegistarTransferenciaAsync.");
        }

        var valorAbsoluto = Math.Abs(dto.Valor);
        if (valorAbsoluto <= 0)
        {
            throw new InvalidOperationException("O valor tem de ser maior do que zero.");
        }

        // Ajuste pode ser positivo (soma) ou negativo (subtrai), consoante o
        // sinal com que o valor foi introduzido; os outros tipos usam o sinal
        // implÃ­cito na prÃ³pria operaÃ§Ã£o.
        var tipoSinal = dto.TipoOperacao switch
        {
            TipoOperacao.Entrada => TipoCategoria.Receita,
            TipoOperacao.Saida => TipoCategoria.Despesa,
            TipoOperacao.Bloqueio => TipoCategoria.Despesa,
            TipoOperacao.Ajuste => dto.Valor < 0 ? TipoCategoria.Despesa : TipoCategoria.Receita,
            _ => dto.Tipo
        };

        if (tipoSinal == TipoCategoria.Despesa && dto.TipoOperacao != TipoOperacao.Bloqueio && dto.CaixaId.HasValue)
        {
            await GarantirSaldoSuficienteAsync(dto.CaixaId.Value, valorAbsoluto);
        }

        var movimento = new Movimento
        {
            Data = dto.Data,
            Descricao = dto.Descricao.Trim(),
            Valor = valorAbsoluto,
            Tipo = tipoSinal,
            TipoOperacao = dto.TipoOperacao,
            FormaPagamento = dto.FormaPagamento,
            CentroCusto = dto.CentroCusto,
            Conciliado = dto.Conciliado,
            CategoriaId = dto.CategoriaId,
            CaixaId = dto.CaixaId,
            ContaBancariaId = dto.ContaBancariaId,
            ClienteId = dto.ClienteId,
            EmpresaId = dto.EmpresaId
        };

        _context.Movimentos.Add(movimento);
        await _context.SaveChangesAsync();
        return movimento.Id;
    }

    public async Task RegistarTransferenciaAsync(NovaTransferenciaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao))
        {
            throw new InvalidOperationException("A descriÃ§Ã£o Ã© obrigatÃ³ria.");
        }

        if (dto.Valor <= 0)
        {
            throw new InvalidOperationException("O valor tem de ser maior do que zero.");
        }

        if (dto.Data == default)
        {
            throw new InvalidOperationException("A data da transferÃªncia Ã© obrigatÃ³ria.");
        }

        var tiposValidos = new[] { "Caixa", "ContaBancaria" };
        if (!tiposValidos.Contains(dto.OrigemTipo) || !tiposValidos.Contains(dto.DestinoTipo))
        {
            throw new InvalidOperationException("A origem e o destino devem ser Caixa ou Conta BancÃ¡ria.");
        }

        if (dto.OrigemTipo == dto.DestinoTipo && dto.OrigemId == dto.DestinoId)
        {
            throw new InvalidOperationException("A origem e o destino nÃ£o podem ser os mesmos.");
        }

        await ValidarOrigemTransferenciaAsync(dto.EmpresaId, dto.OrigemTipo, dto.OrigemId, exigeSessaoAberta: true);
        await ValidarOrigemTransferenciaAsync(dto.EmpresaId, dto.DestinoTipo, dto.DestinoId, exigeSessaoAberta: false);

        if (dto.OrigemTipo == "Caixa")
        {
            await GarantirSaldoSuficienteAsync(dto.OrigemId, dto.Valor);
        }

        await using var transacao = await _context.Database.BeginTransactionAsync();
        try
        {
            var grupo = Guid.NewGuid();
            var descricao = dto.Descricao.Trim();

            var origem = new Movimento
            {
                Data = dto.Data,
                Descricao = $"{descricao} (saÃ­da)",
                Valor = dto.Valor,
                Tipo = TipoCategoria.Despesa,
                TipoOperacao = dto.TipoOperacao,
                FormaPagamento = dto.FormaPagamento?.Trim(),
                CentroCusto = dto.CentroCusto?.Trim(),
                GrupoTransferenciaId = grupo,
                CaixaId = dto.OrigemTipo == "Caixa" ? dto.OrigemId : null,
                ContaBancariaId = dto.OrigemTipo == "ContaBancaria" ? dto.OrigemId : null,
                EmpresaId = dto.EmpresaId
            };

            var destino = new Movimento
            {
                Data = dto.Data,
                Descricao = $"{descricao} (entrada)",
                Valor = dto.Valor,
                Tipo = TipoCategoria.Receita,
                TipoOperacao = dto.TipoOperacao,
                FormaPagamento = dto.FormaPagamento?.Trim(),
                CentroCusto = dto.CentroCusto?.Trim(),
                GrupoTransferenciaId = grupo,
                CaixaId = dto.DestinoTipo == "Caixa" ? dto.DestinoId : null,
                ContaBancariaId = dto.DestinoTipo == "ContaBancaria" ? dto.DestinoId : null,
                EmpresaId = dto.EmpresaId
            };

            _context.Movimentos.AddRange(origem, destino);
            await _context.SaveChangesAsync();
            await transacao.CommitAsync();
        }
        catch
        {
            await transacao.RollbackAsync();
            throw;
        }
    }

    private async Task ValidarOrigemTransferenciaAsync(
        int empresaId,
        string tipo,
        int id,
        bool exigeSessaoAberta)
    {
        if (tipo == "Caixa")
        {
            var caixa = await _context.Caixas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

            if (caixa is null)
            {
                throw new InvalidOperationException("Caixa nÃ£o encontrada para a empresa ativa.");
            }

            if (!caixa.Ativo)
            {
                throw new InvalidOperationException($"A caixa \"{caixa.Nome}\" estÃ¡ inativa.");
            }

            if (exigeSessaoAberta)
            {
                var sessaoAberta = await _context.SessoesCaixa.AnyAsync(s =>
                    s.CaixaId == id &&
                    s.EmpresaId == empresaId &&
                    s.Estado == EstadoSessaoCaixa.Aberta);

                if (!sessaoAberta)
                {
                    throw new InvalidOperationException(
                        $"Abra uma sessÃ£o na caixa \"{caixa.Nome}\" antes de transferir valores a partir dela.");
                }
            }

            return;
        }

        var conta = await _context.ContasBancarias
            .AsNoTracking()
            .Include(c => c.Banco)
            .FirstOrDefaultAsync(c => c.Id == id && c.EmpresaId == empresaId);

        if (conta is null)
        {
            throw new InvalidOperationException("Conta bancÃ¡ria nÃ£o encontrada para a empresa ativa.");
        }

        if (!conta.Ativo)
        {
            throw new InvalidOperationException(
                $"A conta bancÃ¡ria {conta.NumeroConta} estÃ¡ inativa.");
        }
    }

    public async Task MarcarConciliadoAsync(int movimentoId, bool conciliado)
    {
        var movimento = await _context.Movimentos.FindAsync(movimentoId)
            ?? throw new InvalidOperationException("Movimento nÃ£o encontrado.");

        movimento.Conciliado = conciliado;
        movimento.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>Se a caixa nÃ£o permitir saldo negativo, bloqueia a operaÃ§Ã£o
    /// caso o saldo resultante ficasse abaixo de zero. Movimentos de Bloqueio
    /// nÃ£o entram nesta soma (nÃ£o afetam saldo real).</summary>
    private async Task GarantirSaldoSuficienteAsync(int caixaId, decimal valorASubtrair)
    {
        var caixa = await _context.Caixas.FindAsync(caixaId);
        if (caixa is null || caixa.PermiteSaldoNegativo)
        {
            return;
        }

        var saldoAtual = caixa.SaldoInicial + await _context.Movimentos
            .Where(m => m.CaixaId == caixaId && m.TipoOperacao != TipoOperacao.Bloqueio)
            .SumAsync(m => (decimal?)(m.Tipo == TipoCategoria.Receita ? m.Valor : -m.Valor)) ?? caixa.SaldoInicial;

        if (saldoAtual - valorASubtrair < 0)
        {
            throw new InvalidOperationException(
                $"OperaÃ§Ã£o bloqueada: a caixa \"{caixa.Nome}\" nÃ£o permite saldo negativo " +
                $"(saldo atual {saldoAtual:#,##0} FCFA, insuficiente para {valorASubtrair:#,##0} FCFA).");
        }
    }

    public async Task<TreasuryOverviewDto> ObterResumoAsync(int empresaId)
    {
        var hoje = DateTime.Today;
        var limite7 = hoje.AddDays(7);
        var limite15 = hoje.AddDays(15);
        var limite30 = hoje.AddDays(30);
        var limite60 = hoje.AddDays(60);

        var caixas = await _context.Caixas.Where(x => x.EmpresaId == empresaId && x.Ativo).ToListAsync();
        var contas = await _context.ContasBancarias.Where(x => x.EmpresaId == empresaId && x.Ativo).ToListAsync();
        var movimentos = await _context.Movimentos.Where(x => x.EmpresaId == empresaId).ToListAsync();

        decimal SaldoCaixa(int id, decimal inicial) => inicial + movimentos.Where(m => m.CaixaId == id).Sum(m => m.Tipo == TipoCategoria.Receita ? m.Valor : -m.Valor);
        decimal SaldoBanco(int id, decimal inicial) => inicial + movimentos.Where(m => m.ContaBancariaId == id).Sum(m => m.Tipo == TipoCategoria.Receita ? m.Valor : -m.Valor);
        var totalCaixa = caixas.Sum(x => SaldoCaixa(x.Id, x.SaldoInicial));
        var totalBancos = contas.Sum(x => SaldoBanco(x.Id, x.SaldoInicial));

        var receber = _context.ContasReceber.Where(x => x.EmpresaId == empresaId && x.Estado == EstadoConta.Pendente);
        var pagar = _context.ContasPagar.Where(x => x.EmpresaId == empresaId && x.Estado == EstadoConta.Pendente);

        var aReceber = await receber.SumAsync(x => (decimal?)(x.Valor - x.ValorLiquidado)) ?? 0m;
        var aPagar = await pagar.SumAsync(x => (decimal?)(x.Valor - x.ValorLiquidado)) ?? 0m;
        var receberAtrasado = await receber.Where(x => x.DataVencimento < hoje).SumAsync(x => (decimal?)(x.Valor - x.ValorLiquidado)) ?? 0m;
        var pagarAtrasado = await pagar.Where(x => x.DataVencimento < hoje).SumAsync(x => (decimal?)(x.Valor - x.ValorLiquidado)) ?? 0m;
        var receber7 = await receber.Where(x => x.DataVencimento >= hoje && x.DataVencimento <= limite7).SumAsync(x => (decimal?)(x.Valor - x.ValorLiquidado)) ?? 0m;
        var pagar7 = await pagar.Where(x => x.DataVencimento >= hoje && x.DataVencimento <= limite7).SumAsync(x => (decimal?)(x.Valor - x.ValorLiquidado)) ?? 0m;
        async Task<decimal> ReceberAte(DateTime limite) => await receber.Where(x => x.DataVencimento <= limite).SumAsync(x => (decimal?)(x.Valor - x.ValorLiquidado)) ?? 0m;
        async Task<decimal> PagarAte(DateTime limite) => await pagar.Where(x => x.DataVencimento <= limite).SumAsync(x => (decimal?)(x.Valor - x.ValorLiquidado)) ?? 0m;
        var saldo = totalCaixa + totalBancos;
        var previsao7 = saldo + await ReceberAte(limite7) - await PagarAte(limite7);
        var previsao15 = saldo + await ReceberAte(limite15) - await PagarAte(limite15);
        var previsao30 = saldo + await ReceberAte(limite30) - await PagarAte(limite30);
        var previsao60 = saldo + await ReceberAte(limite60) - await PagarAte(limite60);
        var inadimplencia = aReceber <= 0 ? 0 : Math.Round(receberAtrasado / aReceber * 100m, 1);
        var risco = previsao30 < 0 ? "CrÃ­tico" : previsao7 < 0 || pagarAtrasado > saldo ? "Elevado" : previsao15 < saldo * 0.20m ? "Moderado" : "Baixo";

        return new TreasuryOverviewDto
        {
            TotalCaixa = totalCaixa, TotalBancos = totalBancos, SaldoDisponivel = totalCaixa + totalBancos,
            AReceberPendente = aReceber, APagarPendente = aPagar, AReceberAtrasado = receberAtrasado, APagarAtrasado = pagarAtrasado,
            ReceberProximos7Dias = receber7, PagarProximos7Dias = pagar7,
            Previsao7Dias = previsao7, Previsao15Dias = previsao15, Previsao30Dias = previsao30, Previsao60Dias = previsao60,
            InadimplenciaPercentual = inadimplencia, RiscoLiquidez = risco,
            TitulosReceberAtrasados = await receber.CountAsync(x => x.DataVencimento < hoje),
            TitulosPagarAtrasados = await pagar.CountAsync(x => x.DataVencimento < hoje)
        };
    }

    public async Task<IReadOnlyList<TreasuryForecastItemDto>> ListarPrevisaoAsync(int empresaId, int dias = 30)
    {
        var hoje = DateTime.Today;
        var limite = hoje.AddDays(dias);
        var entradas = await _context.ContasReceber
            .Where(x => x.EmpresaId == empresaId && x.Estado == EstadoConta.Pendente && x.DataVencimento <= limite)
            .Include(x => x.Cliente)
            .Select(x => new TreasuryForecastItemDto { Data=x.DataVencimento, Tipo="Receber", Descricao=x.Descricao, Entidade=x.Cliente != null ? x.Cliente.Nome : string.Empty, Entrada=x.Valor - x.ValorLiquidado, Atrasado=x.DataVencimento < hoje })
            .ToListAsync();
        var saidas = await _context.ContasPagar
            .Where(x => x.EmpresaId == empresaId && x.Estado == EstadoConta.Pendente && x.DataVencimento <= limite)
            .Include(x => x.Fornecedor)
            .Select(x => new TreasuryForecastItemDto { Data=x.DataVencimento, Tipo="Pagar", Descricao=x.Descricao, Entidade=x.Fornecedor != null ? x.Fornecedor.Nome : string.Empty, Saida=x.Valor - x.ValorLiquidado, Atrasado=x.DataVencimento < hoje })
            .ToListAsync();
        return entradas.Concat(saidas).OrderBy(x => x.Data).ThenBy(x => x.Tipo).ToList();
    }

    public async Task<IReadOnlyList<TreasuryAgingDto>> ObterAgingAsync(int empresaId)
    {
        var hoje = DateTime.Today;
        var receber = await _context.ContasReceber.Where(x => x.EmpresaId == empresaId && x.Estado == EstadoConta.Pendente)
            .Select(x => new { x.DataVencimento, Saldo = x.Valor - x.ValorLiquidado }).ToListAsync();
        var pagar = await _context.ContasPagar.Where(x => x.EmpresaId == empresaId && x.Estado == EstadoConta.Pendente)
            .Select(x => new { x.DataVencimento, Saldo = x.Valor - x.ValorLiquidado }).ToListAsync();
        var faixas = new (string Nome, int Min, int Max)[] { ("A vencer", int.MinValue, -1), ("Vence hoje / 30 dias", 0, 30), ("31â€“60 dias", 31, 60), ("61â€“90 dias", 61, 90), ("Mais de 90 dias", 91, int.MaxValue) };
        return faixas.Select(f => new TreasuryAgingDto
        {
            Faixa = f.Nome,
            AReceber = receber.Where(x => { var d=(hoje-x.DataVencimento.Date).Days; return d >= f.Min && d <= f.Max; }).Sum(x => x.Saldo),
            APagar = pagar.Where(x => { var d=(hoje-x.DataVencimento.Date).Days; return d >= f.Min && d <= f.Max; }).Sum(x => x.Saldo)
        }).ToList();
    }

}

