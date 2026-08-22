using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class BemService : IBemService
{
    private readonly FinanceProDbContext _context;
    private readonly IAuditoriaService _auditoria;

    public BemService(FinanceProDbContext context, IAuditoriaService auditoria)
    {
        _context = context;
        _auditoria = auditoria;
    }

    public async Task<IReadOnlyList<BemListItemDto>> ListarAsync(int empresaId)
    {
        var bens = await _context.Bens
            .Where(b => b.EmpresaId == empresaId)
            .OrderBy(b => b.NumeroPatrimonial)
            .ToListAsync();

        return bens.Select(b => new BemListItemDto
        {
            Id = b.Id,
            Codigo = b.Codigo,
            NumeroPatrimonial = b.NumeroPatrimonial,
            Descricao = b.Descricao,
            Categoria = b.Categoria,
            Marca = b.Marca,
            Modelo = b.Modelo,
            Serie = b.Serie,
            Localizacao = b.Localizacao,
            Responsavel = b.Responsavel,
            DataAquisicao = b.DataAquisicao,
            ValorAquisicao = b.ValorAquisicao,
            ValorResidual = b.ValorResidual,
            VidaUtilAnos = b.VidaUtilAnos,
            MetodoDepreciacao = b.MetodoDepreciacao.ToString(),
            Estado = b.Estado.ToString(),
            DepreciacaoAcumulada = CalcularDepreciacaoAcumulada(b),
            DepreciacaoMensal = CalcularDepreciacaoMensal(b),
            ValorLiquidoAtual = CalcularValorLiquido(b)
        }).ToList();
    }

    private static decimal BaseDepreciavel(Bem bem) => Math.Max(bem.ValorAquisicao - bem.ValorResidual, 0m);

    private static decimal CalcularDepreciacaoMensal(Bem bem)
    {
        if (bem.MetodoDepreciacao == MetodoDepreciacao.SemDepreciacao || bem.VidaUtilAnos <= 0) return 0m;
        return BaseDepreciavel(bem) / (bem.VidaUtilAnos * 12m);
    }

    private static decimal CalcularDepreciacaoAcumulada(Bem bem)
    {
        if (bem.MetodoDepreciacao == MetodoDepreciacao.SemDepreciacao || bem.VidaUtilAnos <= 0) return 0m;
        var inicio = new DateTime(bem.DataAquisicao.Year, bem.DataAquisicao.Month, 1);
        var fim = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var meses = Math.Max(0, ((fim.Year - inicio.Year) * 12) + fim.Month - inicio.Month + 1);
        var baseDep = BaseDepreciavel(bem);
        if (bem.MetodoDepreciacao == MetodoDepreciacao.Degressivo)
        {
            var taxaAnual = 2m / bem.VidaUtilAnos;
            var anos = meses / 12m;
            var liquido = bem.ValorResidual + baseDep * (decimal)Math.Pow((double)Math.Max(0m, 1m - taxaAnual), (double)anos);
            return Math.Min(baseDep, Math.Max(0m, bem.ValorAquisicao - liquido));
        }
        return Math.Min(baseDep, CalcularDepreciacaoMensal(bem) * meses);
    }

    private static decimal CalcularValorLiquido(Bem bem) =>
        Math.Max(bem.ValorAquisicao - CalcularDepreciacaoAcumulada(bem), bem.ValorResidual);

    public async Task<int> CriarAsync(NovoBemDto dto, int utilizadorId, string utilizadorNome)
    {
        Validar(dto);

        var jaExiste = await _context.Bens.AnyAsync(b => b.EmpresaId == dto.EmpresaId && b.NumeroPatrimonial == dto.NumeroPatrimonial.Trim());
        if (jaExiste)
        {
            throw new InvalidOperationException("Já existe um bem com esse número patrimonial.");
        }

        var proximoNumero = 1 + await _context.Bens.CountAsync(b => b.EmpresaId == dto.EmpresaId);

        var bem = new Bem
        {
            Codigo = $"BEM-{proximoNumero:D5}",
            NumeroPatrimonial = dto.NumeroPatrimonial.Trim(),
            Descricao = dto.Descricao.Trim(),
            Categoria = dto.Categoria,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Serie = dto.Serie,
            Localizacao = dto.Localizacao,
            Responsavel = dto.Responsavel,
            DataAquisicao = dto.DataAquisicao,
            ValorAquisicao = dto.ValorAquisicao,
            ValorResidual = dto.ValorResidual,
            VidaUtilAnos = dto.VidaUtilAnos,
            MetodoDepreciacao = dto.MetodoDepreciacao,
            EmpresaId = dto.EmpresaId,
            Estado = EstadoBem.Ativo
        };

        _context.Bens.Add(bem);
        await _context.SaveChangesAsync();

        await _auditoria.RegistarAsync("Bem", bem.Id, "Criar", $"Criado o bem {bem.Codigo} ({bem.Descricao}).", utilizadorId, utilizadorNome, dto.EmpresaId);

        return bem.Id;
    }

    public async Task AtualizarAsync(int bemId, NovoBemDto dto, int utilizadorId, string utilizadorNome)
    {
        Validar(dto);

        var bem = await _context.Bens.FindAsync(bemId)
            ?? throw new InvalidOperationException("Bem não encontrado.");

        bem.NumeroPatrimonial = dto.NumeroPatrimonial.Trim();
        bem.Descricao = dto.Descricao.Trim();
        bem.Categoria = dto.Categoria;
        bem.Marca = dto.Marca;
        bem.Modelo = dto.Modelo;
        bem.Serie = dto.Serie;
        bem.Localizacao = dto.Localizacao;
        bem.Responsavel = dto.Responsavel;
        bem.DataAquisicao = dto.DataAquisicao;
        bem.ValorAquisicao = dto.ValorAquisicao;
        bem.ValorResidual = dto.ValorResidual;
        bem.VidaUtilAnos = dto.VidaUtilAnos;
        bem.MetodoDepreciacao = dto.MetodoDepreciacao;
        bem.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditoria.RegistarAsync("Bem", bem.Id, "Editar", $"Dados do bem {bem.Codigo} atualizados.", utilizadorId, utilizadorNome, bem.EmpresaId);
    }

    public async Task EliminarAsync(int bemId, int utilizadorId, string utilizadorNome)
    {
        var bem = await _context.Bens.FindAsync(bemId)
            ?? throw new InvalidOperationException("Bem não encontrado.");

        // Nunca se apaga fisicamente — desativa e regista o abate no histórico.
        bem.Ativo = false;
        bem.Estado = EstadoBem.Abatido;
        bem.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditoria.RegistarAsync("Bem", bem.Id, "Eliminar", $"Bem {bem.Codigo} abatido/desativado.", utilizadorId, utilizadorNome, bem.EmpresaId);
    }

    private static void Validar(NovoBemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NumeroPatrimonial))
        {
            throw new InvalidOperationException("O número patrimonial é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(dto.Descricao))
        {
            throw new InvalidOperationException("A descrição é obrigatória.");
        }

        if (dto.ValorAquisicao < 0)
        {
            throw new InvalidOperationException("O valor de aquisição não pode ser negativo.");
        }

        if (dto.ValorResidual < 0 || dto.ValorResidual > dto.ValorAquisicao)
        {
            throw new InvalidOperationException("O valor residual deve estar entre zero e o valor de aquisição.");
        }

        if (dto.VidaUtilAnos < 0)
        {
            throw new InvalidOperationException("A vida útil não pode ser negativa.");
        }

        if (dto.DataAquisicao > DateTime.Today)
        {
            throw new InvalidOperationException("A data de aquisição não pode ser no futuro.");
        }
    }
}
