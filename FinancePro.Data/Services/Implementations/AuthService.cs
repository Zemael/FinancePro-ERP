using FinancePro.Core.DTOs;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly FinanceProDbContext _context;

    public AuthService(FinanceProDbContext context)
    {
        _context = context;
    }

    public async Task<LoginResultDto> AutenticarAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new LoginResultDto { Sucesso = false, Mensagem = "Indique o utilizador e a senha." };
        }

        var utilizador = await _context.Utilizadores
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

        if (utilizador is null || !BCrypt.Net.BCrypt.Verify(password, utilizador.PasswordHash))
        {
            return new LoginResultDto { Sucesso = false, Mensagem = "Utilizador ou senha inválidos." };
        }

        utilizador.UltimoLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new LoginResultDto
        {
            Sucesso = true,
            UtilizadorId = utilizador.Id,
            NomeCompleto = utilizador.NomeCompleto,
            PerfilNome = utilizador.Perfil.Nome,
            FotoPerfil = utilizador.FotoPerfil,
            EmpresaId = utilizador.EmpresaId,
            Permissoes = utilizador.Perfil.Nome == "Administrador"
                ? new[] { "*" }
                : await ObterPermissoesAsync(utilizador.PerfilId)
        };
    }

    private async Task<IReadOnlyCollection<string>> ObterPermissoesAsync(int perfilId)
    {
        var rows = await _context.PermissoesPerfis.AsNoTracking()
            .Where(x => x.PerfilId == perfilId)
            .ToListAsync();
        var result = new List<string>();
        foreach (var p in rows)
        {
            if (p.Consultar) result.Add($"{p.Modulo}.Consultar");
            if (p.Criar) result.Add($"{p.Modulo}.Criar");
            if (p.Editar) result.Add($"{p.Modulo}.Editar");
            if (p.Desativar) result.Add($"{p.Modulo}.Desativar");
            if (p.Aprovar) result.Add($"{p.Modulo}.Aprovar");
            if (p.Exportar) result.Add($"{p.Modulo}.Exportar");
            if (p.Administrar) result.Add($"{p.Modulo}.Administrar");
        }
        return result;
    }
}
