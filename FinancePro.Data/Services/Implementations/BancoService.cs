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
            .Select(b => new BancoListItemDto { Id = b.Id, Nome = b.Nome, CodigoSwift = b.CodigoSwift })
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

        _context.Bancos.Add(new Banco { Nome = dto.Nome.Trim(), CodigoSwift = dto.CodigoSwift?.Trim() });
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
