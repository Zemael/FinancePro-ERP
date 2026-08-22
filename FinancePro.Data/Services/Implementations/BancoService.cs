using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class BancoService : IBancoService
{
    private readonly FinanceProDbContext _context;

    public BancoService(FinanceProDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BancoListItemDto>> ListarBancosAsync()
    {
        return await _context.Bancos
            .Where(b => b.Ativo)
            .OrderBy(b => b.Nome)
            .Select(b => new BancoListItemDto
            {
                Id = b.Id,
                Nome = b.Nome,
                Sigla = b.Sigla,
                CodigoSwift = b.CodigoSwift,
                Endereco = b.Endereco,
                Contacto = b.Contacto
            })
            .ToListAsync();
    }

    public async Task CriarBancoAsync(NovoBancoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
        {
            throw new InvalidOperationException("O nome do banco é obrigatório.");
        }

        var jaExiste = await _context.Bancos.AnyAsync(b => b.Nome == dto.Nome.Trim());
        if (jaExiste)
        {
            throw new InvalidOperationException("Já existe um banco com esse nome.");
        }

        _context.Bancos.Add(new Banco
        {
            Nome = dto.Nome.Trim(),
            Sigla = string.IsNullOrWhiteSpace(dto.Sigla) ? null : dto.Sigla.Trim().ToUpperInvariant(),
            CodigoSwift = dto.CodigoSwift?.Trim(),
            Endereco = string.IsNullOrWhiteSpace(dto.Endereco) ? null : dto.Endereco.Trim(),
            Contacto = string.IsNullOrWhiteSpace(dto.Contacto) ? null : dto.Contacto.Trim()
        });
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarBancoAsync(int bancoId, NovoBancoDto dto)
    {
        var banco = await _context.Bancos.FindAsync(bancoId)
            ?? throw new InvalidOperationException("Banco não encontrado.");

        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new InvalidOperationException("O nome do banco é obrigatório.");

        var nome = dto.Nome.Trim();
        if (await _context.Bancos.AnyAsync(b => b.Id != bancoId && b.Nome == nome))
            throw new InvalidOperationException("Já existe outro banco com esse nome.");

        banco.Nome = nome;
        banco.Sigla = string.IsNullOrWhiteSpace(dto.Sigla) ? null : dto.Sigla.Trim().ToUpperInvariant();
        banco.CodigoSwift = string.IsNullOrWhiteSpace(dto.CodigoSwift) ? null : dto.CodigoSwift.Trim().ToUpperInvariant();
        banco.Endereco = string.IsNullOrWhiteSpace(dto.Endereco) ? null : dto.Endereco.Trim();
        banco.Contacto = string.IsNullOrWhiteSpace(dto.Contacto) ? null : dto.Contacto.Trim();
        banco.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ContaBancariaListItemDto>> ListarContasAsync(int empresaId)
    {
        return await _context.ContasBancarias
            .Where(c => c.EmpresaId == empresaId)
            .Include(c => c.Banco)
            .OrderBy(c => c.Banco.Nome)
            .Select(c => new ContaBancariaListItemDto
            {
                Id = c.Id,
                NumeroConta = c.NumeroConta,
                IBAN = c.IBAN,
                Titular = c.Titular,
                SaldoInicial = c.SaldoInicial,
                Moeda = c.Moeda,
                BancoNome = c.Banco.Nome,
                Ativo = c.Ativo
            })
            .ToListAsync();
    }

    public async Task CriarContaAsync(NovaContaBancariaDto dto)
    {
        if (dto.BancoId <= 0)
        {
            throw new InvalidOperationException("Selecione o banco.");
        }

        if (string.IsNullOrWhiteSpace(dto.NumeroConta))
        {
            throw new InvalidOperationException("O número de conta é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(dto.Titular))
        {
            throw new InvalidOperationException("O titular da conta é obrigatório.");
        }

        _context.ContasBancarias.Add(new ContaBancaria
        {
            NumeroConta = dto.NumeroConta.Trim(),
            IBAN = dto.IBAN?.Trim(),
            Titular = dto.Titular.Trim(),
            SaldoInicial = dto.SaldoInicial,
            Moeda = string.IsNullOrWhiteSpace(dto.Moeda) ? "FCFA" : dto.Moeda,
            BancoId = dto.BancoId,
            EmpresaId = dto.EmpresaId
        });

        await _context.SaveChangesAsync();
    }

    public async Task AtualizarContaAsync(int contaId, NovaContaBancariaDto dto)
    {
        var conta = await _context.ContasBancarias.FindAsync(contaId)
            ?? throw new InvalidOperationException("Conta bancária não encontrada.");
        if (dto.BancoId <= 0) throw new InvalidOperationException("Selecione o banco.");
        if (string.IsNullOrWhiteSpace(dto.NumeroConta)) throw new InvalidOperationException("O número de conta é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Titular)) throw new InvalidOperationException("O titular da conta é obrigatório.");
        conta.BancoId = dto.BancoId;
        conta.NumeroConta = dto.NumeroConta.Trim();
        conta.IBAN = string.IsNullOrWhiteSpace(dto.IBAN) ? null : dto.IBAN.Trim();
        conta.Titular = dto.Titular.Trim();
        conta.SaldoInicial = dto.SaldoInicial;
        conta.Moeda = string.IsNullOrWhiteSpace(dto.Moeda) ? "FCFA" : dto.Moeda.Trim().ToUpperInvariant();
        conta.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task AlternarAtivoContaAsync(int contaId, bool ativo)
    {
        var conta = await _context.ContasBancarias.FindAsync(contaId);
        if (conta is null)
        {
            throw new InvalidOperationException("Conta bancária não encontrada.");
        }

        conta.Ativo = ativo;
        conta.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
