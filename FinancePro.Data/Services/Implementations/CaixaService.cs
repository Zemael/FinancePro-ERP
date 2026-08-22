using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class CaixaService : ICaixaService
{
    private readonly FinanceProDbContext _context;

    public CaixaService(FinanceProDbContext context) => _context = context;

    public async Task<IReadOnlyList<CaixaListItemDto>> ListarAsync(int empresaId) =>
        await _context.Caixas
            .Where(c => c.EmpresaId == empresaId)
            .OrderBy(c => c.Nome)
            .Select(c => new CaixaListItemDto
            {
                Id = c.Id,
                Nome = c.Nome,
                SaldoInicial = c.SaldoInicial,
                SaldoMinimo = c.SaldoMinimo,
                PermiteSaldoNegativo = c.PermiteSaldoNegativo,
                Ativo = c.Ativo
            }).ToListAsync();

    public async Task<int> CriarAsync(NovoCaixaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome)) throw new InvalidOperationException("O nome da caixa é obrigatório.");
        if (dto.SaldoInicial < 0) throw new InvalidOperationException("O saldo inicial não pode ser negativo.");
        if (await _context.Caixas.AnyAsync(c => c.EmpresaId == dto.EmpresaId && c.Nome == dto.Nome.Trim()))
            throw new InvalidOperationException("Já existe uma caixa com esse nome.");

        var caixa = new Caixa
        {
            Nome = dto.Nome.Trim(),
            SaldoInicial = dto.SaldoInicial,
            SaldoMinimo = dto.SaldoMinimo,
            PermiteSaldoNegativo = dto.PermiteSaldoNegativo,
            EmpresaId = dto.EmpresaId
        };
        _context.Caixas.Add(caixa);
        await _context.SaveChangesAsync();
        return caixa.Id;
    }

    public async Task AtualizarAsync(int caixaId, NovoCaixaDto dto)
    {
        var caixa = await _context.Caixas.FindAsync(caixaId) ?? throw new InvalidOperationException("Caixa não encontrada.");
        if (string.IsNullOrWhiteSpace(dto.Nome)) throw new InvalidOperationException("O nome da caixa é obrigatório.");
        if (dto.SaldoInicial < 0) throw new InvalidOperationException("O saldo inicial não pode ser negativo.");
        caixa.Nome = dto.Nome.Trim();
        caixa.SaldoInicial = dto.SaldoInicial;
        caixa.SaldoMinimo = dto.SaldoMinimo;
        caixa.PermiteSaldoNegativo = dto.PermiteSaldoNegativo;
        caixa.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task AlternarAtivoAsync(int caixaId, bool ativo)
    {
        var caixa = await _context.Caixas.FindAsync(caixaId) ?? throw new InvalidOperationException("Caixa não encontrada.");
        if (!ativo && await _context.SessoesCaixa.AnyAsync(x => x.CaixaId == caixaId && x.Estado == EstadoSessaoCaixa.Aberta))
            throw new InvalidOperationException("Feche a sessão atual antes de desativar a caixa.");
        caixa.Ativo = ativo;
        caixa.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<SessaoCaixaDto?> ObterSessaoAbertaAsync(int empresaId, int? caixaId = null)
    {
        var query = _context.SessoesCaixa
            .AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.Estado == EstadoSessaoCaixa.Aberta);
        if (caixaId.HasValue) query = query.Where(x => x.CaixaId == caixaId.Value);
        return await query.OrderByDescending(x => x.DataAbertura)
            .Select(x => new SessaoCaixaDto
            {
                Id = x.Id,
                CaixaId = x.CaixaId,
                CaixaNome = x.Caixa.Nome,
                OperadorNome = x.Utilizador.NomeCompleto,
                DataAbertura = x.DataAbertura,
                SaldoInicial = x.SaldoInicial,
                SaldoAtual = _context.Movimentos
                    .Where(m => m.CaixaId == x.CaixaId && m.Data >= x.DataAbertura && m.Estado == EstadoMovimento.Confirmado)
                    .Sum(m => (decimal?)(m.Tipo == TipoCategoria.Receita ? m.Valor : -m.Valor)) ?? 0m,
                Aberta = true
            }).FirstOrDefaultAsync();
    }

    public async Task<SessaoCaixaDto> AbrirAsync(AbrirCaixaDto dto)
    {
        if (dto.SaldoInicial < 0) throw new InvalidOperationException("O saldo inicial não pode ser negativo.");
        if (dto.UtilizadorId <= 0 || !await _context.Utilizadores.AnyAsync(x => x.Id == dto.UtilizadorId && x.Ativo))
            throw new InvalidOperationException("A sessão do utilizador não é válida. Termine a sessão e entre novamente.");
        var caixa = await _context.Caixas.SingleOrDefaultAsync(x => x.Id == dto.CaixaId && x.EmpresaId == dto.EmpresaId)
            ?? throw new InvalidOperationException("Caixa não encontrada.");
        if (!caixa.Ativo) throw new InvalidOperationException("Não é possível abrir uma caixa inativa.");
        if (await _context.SessoesCaixa.AnyAsync(x => x.CaixaId == dto.CaixaId && x.Estado == EstadoSessaoCaixa.Aberta))
            throw new InvalidOperationException("Este caixa já possui uma sessão aberta.");

        var exercicio = await _context.ExerciciosFinanceiros
            .Where(x => x.EmpresaId == dto.EmpresaId && !x.Encerrado)
            .OrderByDescending(x => x.Padrao).ThenByDescending(x => x.Ano)
            .FirstOrDefaultAsync() ?? throw new InvalidOperationException("Não existe exercício financeiro aberto para esta empresa.");

        var sessao = new SessaoCaixa
        {
            CaixaId = dto.CaixaId,
            EmpresaId = dto.EmpresaId,
            ExercicioFinanceiroId = exercicio.Id,
            UtilizadorId = dto.UtilizadorId,
            DataAbertura = DateTime.UtcNow,
            SaldoInicial = dto.SaldoInicial,
            ObservacaoAbertura = dto.Observacao?.Trim(),
            Estado = EstadoSessaoCaixa.Aberta
        };
        _context.SessoesCaixa.Add(sessao);

        if (dto.SaldoInicial > 0)
        {
            _context.Movimentos.Add(new Movimento
            {
                Data = sessao.DataAbertura,
                Descricao = $"Abertura de caixa — {caixa.Nome}",
                Valor = dto.SaldoInicial,
                Tipo = TipoCategoria.Receita,
                TipoOperacao = TipoOperacao.Entrada,
                Estado = EstadoMovimento.Confirmado,
                CaixaId = caixa.Id,
                EmpresaId = dto.EmpresaId
            });
        }

        // SaveChanges envolve a sessão e o movimento de abertura numa única
        // transação automática, compatível com EnableRetryOnFailure.
        await _context.SaveChangesAsync();
        return await ObterSessaoAbertaAsync(dto.EmpresaId, dto.CaixaId)
            ?? throw new InvalidOperationException("A caixa foi aberta, mas não foi possível recarregar a sessão.");
    }

    public async Task<SessaoCaixaDto> FecharAsync(FecharCaixaDto dto)
    {
        var sessao = await _context.SessoesCaixa
            .Include(x => x.Caixa)
            .SingleOrDefaultAsync(x => x.Id == dto.SessaoCaixaId && x.Estado == EstadoSessaoCaixa.Aberta)
            ?? throw new InvalidOperationException("Sessão de caixa aberta não encontrada.");
        if (dto.SaldoContado < 0) throw new InvalidOperationException("O saldo contado não pode ser negativo.");

        var movimentos = await _context.Movimentos
            .Where(m => m.CaixaId == sessao.CaixaId && m.Data >= sessao.DataAbertura && m.Estado == EstadoMovimento.Confirmado)
            .ToListAsync();
        // O movimento de abertura já representa o saldo inicial. Somar novamente
        // sessao.SaldoInicial duplicaria o valor no saldo calculado.
        var saldoCalculado = movimentos.Sum(m => m.Tipo == TipoCategoria.Receita ? m.Valor : -m.Valor);

        sessao.DataFecho = DateTime.UtcNow;
        sessao.SaldoCalculado = saldoCalculado;
        sessao.SaldoContado = dto.SaldoContado;
        sessao.Diferenca = dto.SaldoContado - saldoCalculado;
        sessao.ObservacaoFecho = dto.Observacao?.Trim();
        sessao.Estado = EstadoSessaoCaixa.Fechada;
        sessao.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new SessaoCaixaDto
        {
            Id = sessao.Id,
            CaixaId = sessao.CaixaId,
            CaixaNome = sessao.Caixa.Nome,
            OperadorNome = string.Empty,
            DataAbertura = sessao.DataAbertura,
            SaldoInicial = sessao.SaldoInicial,
            SaldoAtual = saldoCalculado,
            Aberta = false
        };
    }
}
