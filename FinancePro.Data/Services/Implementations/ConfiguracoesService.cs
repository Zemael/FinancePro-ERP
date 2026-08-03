using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class ConfiguracoesService : IConfiguracoesService
{
    private readonly FinanceProDbContext _context;

    public ConfiguracoesService(FinanceProDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteAlgumUtilizadorAsync()
    {
        return await _context.Utilizadores.AnyAsync();
    }

    public async Task<int> ConfigurarInicialAsync(ConfiguracaoInicialDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EmpresaNome))
        {
            throw new InvalidOperationException("O nome da empresa é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(dto.AdminNome) || string.IsNullOrWhiteSpace(dto.AdminEmail))
        {
            throw new InvalidOperationException("Indique o nome e o email do administrador.");
        }

        if (string.IsNullOrWhiteSpace(dto.AdminPassword) || dto.AdminPassword.Length < 6)
        {
            throw new InvalidOperationException("A senha tem de ter pelo menos 6 caracteres.");
        }

        var moeda = string.IsNullOrWhiteSpace(dto.Moeda) ? "FCFA" : dto.Moeda.Trim();
        if (moeda.Length > 20)
        {
            throw new InvalidOperationException("A moeda deve ser só a sigla (ex.: FCFA, EUR, USD), até 20 caracteres.");
        }

        try
        {
            // Garante que os perfis padrão existem (o mesmo conteúdo do seed
            // 001_PerfisIniciais.sql) — assim a configuração inicial funciona
            // mesmo que o script SQL nunca tenha sido corrido manualmente.
            if (!await _context.Perfis.AnyAsync())
            {
                _context.Perfis.AddRange(
                    new Perfil { Nome = "Administrador", Descricao = "Acesso total ao sistema" },
                    new Perfil { Nome = "Gestor", Descricao = "Acesso aos módulos financeiros e relatórios" },
                    new Perfil { Nome = "Operador", Descricao = "Acesso limitado a lançamentos do dia a dia" });
                await _context.SaveChangesAsync();
            }

            var perfilAdmin = await _context.Perfis.FirstAsync(p => p.Nome == "Administrador");

            var empresa = new Empresa { Nome = dto.EmpresaNome.Trim(), Moeda = moeda };
            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            _context.Utilizadores.Add(new Utilizador
            {
                NomeCompleto = dto.AdminNome.Trim(),
                Email = dto.AdminEmail.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.AdminPassword),
                PerfilId = perfilAdmin.Id,
                EmpresaId = empresa.Id
            });
            await _context.SaveChangesAsync();

            return empresa.Id;
        }
        catch (DbUpdateException ex)
        {
            var detalhe = ex.InnerException?.Message ?? ex.Message;
            throw new InvalidOperationException($"Não foi possível gravar na base de dados: {detalhe}");
        }
    }

    public async Task<EmpresaDto?> ObterEmpresaAsync(int empresaId)
    {
        return await _context.Empresas
            .Where(e => e.Id == empresaId)
            .Select(e => new EmpresaDto
            {
                Id = e.Id,
                Nome = e.Nome,
                NIF = e.NIF,
                Morada = e.Morada,
                Telefone = e.Telefone,
                Email = e.Email,
                Moeda = e.Moeda
            })
            .FirstOrDefaultAsync();
    }

    public async Task AtualizarEmpresaAsync(EmpresaDto dto)
    {
        var empresa = await _context.Empresas.FindAsync(dto.Id)
            ?? throw new InvalidOperationException("Empresa não encontrada.");

        empresa.Nome = dto.Nome.Trim();
        empresa.NIF = dto.NIF;
        empresa.Morada = dto.Morada;
        empresa.Telefone = dto.Telefone;
        empresa.Email = dto.Email;
        empresa.Moeda = string.IsNullOrWhiteSpace(dto.Moeda) ? "FCFA" : dto.Moeda;
        empresa.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<PerfilOpcaoDto>> ListarPerfisAsync()
    {
        return await _context.Perfis
            .OrderBy(p => p.Nome)
            .Select(p => new PerfilOpcaoDto { Id = p.Id, Nome = p.Nome })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UtilizadorListItemDto>> ListarUtilizadoresAsync(int empresaId)
    {
        return await _context.Utilizadores
            .Where(u => u.EmpresaId == empresaId)
            .Include(u => u.Perfil)
            .OrderBy(u => u.NomeCompleto)
            .Select(u => new UtilizadorListItemDto
            {
                Id = u.Id,
                NomeCompleto = u.NomeCompleto,
                Email = u.Email,
                PerfilNome = u.Perfil.Nome,
                Ativo = u.Ativo
            })
            .ToListAsync();
    }

    public async Task CriarUtilizadorAsync(NovoUtilizadorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NomeCompleto) || string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new InvalidOperationException("Indique o nome e o email do utilizador.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
        {
            throw new InvalidOperationException("A senha tem de ter pelo menos 6 caracteres.");
        }

        var emailEmUso = await _context.Utilizadores.AnyAsync(u => u.Email == dto.Email.Trim());
        if (emailEmUso)
        {
            throw new InvalidOperationException("Já existe um utilizador com esse email.");
        }

        _context.Utilizadores.Add(new Utilizador
        {
            NomeCompleto = dto.NomeCompleto.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PerfilId = dto.PerfilId,
            EmpresaId = dto.EmpresaId
        });

        await _context.SaveChangesAsync();
    }

    public async Task AlternarAtivoUtilizadorAsync(int utilizadorId, bool ativo)
    {
        var utilizador = await _context.Utilizadores.FindAsync(utilizadorId)
            ?? throw new InvalidOperationException("Utilizador não encontrado.");

        utilizador.Ativo = ativo;
        utilizador.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
