using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public sealed class PermissaoService : IPermissaoService
{
    private readonly FinanceProDbContext _context;

    private static readonly (string Codigo, string Descricao)[] Modulos =
    {
        ("Dashboard", "Dashboard"),
        ("Empresas", "Empresas"),
        ("Exercicios", "Exercícios Financeiros"),
        ("Moedas", "Moedas"),
        ("Utilizadores", "Utilizadores"),
        ("Perfis", "Perfis de Acesso"),
        ("Permissoes", "Permissões"),
        ("Caixa", "Caixa"),
        ("Bancos", "Bancos"),
        ("Orcamento", "Gestão Orçamental"),
        ("Tesouraria", "Tesouraria"),
        ("Receitas", "Receitas"),
        ("Despesas", "Despesas"),
        ("Compras", "Compras"),
        ("Patrimonio", "Gestão Patrimonial"),
        ("Configuracoes", "Configurações")
    };

    public PermissaoService(FinanceProDbContext context) => _context = context;

    public async Task<IReadOnlyList<PermissaoPerfilDto>> ObterMatrizAsync(int perfilId)
    {
        var perfil = await _context.Perfis.AsNoTracking().FirstOrDefaultAsync(x => x.Id == perfilId)
            ?? throw new InvalidOperationException("Perfil não encontrado.");
        var administrador = perfil.Nome.Equals("Administrador", StringComparison.OrdinalIgnoreCase);
        var atuais = await _context.PermissoesPerfis.AsNoTracking()
            .Where(x => x.PerfilId == perfilId)
            .ToDictionaryAsync(x => x.Modulo);

        return Modulos.Select(m =>
        {
            atuais.TryGetValue(m.Codigo, out var p);
            return new PermissaoPerfilDto
            {
                Id = p?.Id ?? 0,
                PerfilId = perfilId,
                Modulo = m.Codigo,
                DescricaoModulo = m.Descricao,
                Consultar = administrador || (p?.Consultar ?? false),
                Criar = administrador || (p?.Criar ?? false),
                Editar = administrador || (p?.Editar ?? false),
                Desativar = administrador || (p?.Desativar ?? false),
                Aprovar = administrador || (p?.Aprovar ?? false),
                Exportar = administrador || (p?.Exportar ?? false),
                Administrar = administrador || (p?.Administrar ?? false)
            };
        }).ToList();
    }

    public async Task GuardarMatrizAsync(int perfilId, IReadOnlyCollection<PermissaoPerfilDto> permissoes)
    {
        if (!await _context.Perfis.AnyAsync(x => x.Id == perfilId && x.Ativo))
            throw new InvalidOperationException("Selecione um perfil ativo.");

        var atuais = await _context.PermissoesPerfis
            .Where(x => x.PerfilId == perfilId)
            .ToDictionaryAsync(x => x.Modulo);

        foreach (var dto in permissoes.Where(x => Modulos.Any(m => m.Codigo == x.Modulo)))
        {
            if (!atuais.TryGetValue(dto.Modulo, out var entity))
            {
                entity = new PermissaoPerfil { PerfilId = perfilId, Modulo = dto.Modulo };
                _context.PermissoesPerfis.Add(entity);
            }

            entity.Consultar = dto.Consultar;
            entity.Criar = dto.Criar;
            entity.Editar = dto.Editar;
            entity.Desativar = dto.Desativar;
            entity.Aprovar = dto.Aprovar;
            entity.Exportar = dto.Exportar;
            entity.Administrar = dto.Administrar;
            entity.DataAtualizacao = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<string>> ObterChavesAsync(int perfilId)
    {
        var permissoes = await _context.PermissoesPerfis.AsNoTracking()
            .Where(x => x.PerfilId == perfilId)
            .ToListAsync();

        var chaves = new List<string>();
        foreach (var p in permissoes)
        {
            if (p.Consultar) chaves.Add($"{p.Modulo}.Consultar");
            if (p.Criar) chaves.Add($"{p.Modulo}.Criar");
            if (p.Editar) chaves.Add($"{p.Modulo}.Editar");
            if (p.Desativar) chaves.Add($"{p.Modulo}.Desativar");
            if (p.Aprovar) chaves.Add($"{p.Modulo}.Aprovar");
            if (p.Exportar) chaves.Add($"{p.Modulo}.Exportar");
            if (p.Administrar) chaves.Add($"{p.Modulo}.Administrar");
        }
        return chaves;
    }
}
