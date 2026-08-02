using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class DespesasService : IDespesasService
{
    private readonly FinanceProDbContext _context;
    private readonly ITesourariaService _tesourariaService;

    public DespesasService(FinanceProDbContext context, ITesourariaService tesourariaService)
    {
        _context = context;
        _tesourariaService = tesourariaService;
    }

    public async Task<IReadOnlyList<ContaPagarListItemDto>> ListarAsync(int empresaId)
    {
        var hoje = DateTime.Today;

        var contas = await _context.ContasPagar
            .Where(c => c.EmpresaId == empresaId)
            .Include(c => c.Fornecedor)
            .Include(c => c.Categoria)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();

        return contas.Select(c => new ContaPagarListItemDto
        {
            Id = c.Id,
            Codigo = c.Codigo,
            Descricao = c.Descricao,
            Valor = c.Valor,
            DataEmissao = c.DataEmissao,
            DataVencimento = c.DataVencimento,
            DataPagamento = c.DataPagamento,
            FornecedorNome = c.Fornecedor?.Nome,
            CategoriaNome = c.Categoria?.Nome,
            FormaPagamento = c.FormaPagamento,
            CentroCusto = c.CentroCusto,
            EstadoExibicao = c.Estado == EstadoConta.Pendente && c.DataVencimento.Date < hoje
                ? "Atrasado"
                : (c.Estado == EstadoConta.Recebido ? "Paga" : c.Estado.ToString()),
            PodePagar = c.Estado == EstadoConta.Pendente,
            PodeCancelar = c.Estado == EstadoConta.Pendente
        }).ToList();
    }

    public async Task<IReadOnlyList<FornecedorOpcaoDto>> ListarFornecedoresAsync(int empresaId)
    {
        return await _context.Fornecedores
            .Where(f => f.EmpresaId == empresaId && f.Ativo)
            .OrderBy(f => f.Nome)
            .Select(f => new FornecedorOpcaoDto { Id = f.Id, Nome = f.Nome })
            .ToListAsync();
    }

    public Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId) =>
        _tesourariaService.ListarCategoriasAsync(empresaId, TipoCategoria.Despesa);

    public Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId) =>
        _tesourariaService.ListarOrigensAsync(empresaId);

    public async Task CriarAsync(NovaContaPagarDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao))
        {
            throw new InvalidOperationException("A descrição é obrigatória.");
        }

        if (dto.Valor <= 0)
        {
            throw new InvalidOperationException("O valor tem de ser maior do que zero.");
        }

        var proximoNumero = 1 + await _context.ContasPagar.CountAsync(c => c.EmpresaId == dto.EmpresaId);

        _context.ContasPagar.Add(new ContaPagar
        {
            Codigo = $"DESP-{proximoNumero:D5}",
            Descricao = dto.Descricao.Trim(),
            Valor = dto.Valor,
            DataEmissao = dto.DataEmissao,
            DataVencimento = dto.DataVencimento,
            FormaPagamento = dto.FormaPagamento,
            CentroCusto = dto.CentroCusto,
            FornecedorId = dto.FornecedorId,
            CategoriaId = dto.CategoriaId,
            EmpresaId = dto.EmpresaId,
            Estado = EstadoConta.Pendente
        });

        await _context.SaveChangesAsync();
    }

    public async Task RegistarPagamentoAsync(int contaPagarId, string origemTipo, int origemId, DateTime dataPagamento)
    {
        var conta = await _context.ContasPagar
            .Include(c => c.Fornecedor)
            .SingleOrDefaultAsync(c => c.Id == contaPagarId)
            ?? throw new InvalidOperationException("Conta a pagar não encontrada.");

        if (conta.Estado != EstadoConta.Pendente)
        {
            throw new InvalidOperationException("Esta conta já não está pendente.");
        }

        if (dataPagamento.Date < conta.DataEmissao.Date)
        {
            throw new InvalidOperationException("A data do pagamento não pode ser anterior à data de emissão.");
        }

        if (origemTipo == "Caixa")
        {
            var caixaValida = await _context.Caixas.AnyAsync(c =>
                c.Id == origemId && c.EmpresaId == conta.EmpresaId && c.Ativo);
            if (!caixaValida)
            {
                throw new InvalidOperationException("A caixa selecionada não existe ou está inativa.");
            }

            var sessaoAberta = await _context.SessoesCaixa.AnyAsync(s =>
                s.CaixaId == origemId && s.EmpresaId == conta.EmpresaId &&
                s.Estado == EstadoSessaoCaixa.Aberta);
            if (!sessaoAberta)
            {
                throw new InvalidOperationException("Abra uma sessão para a caixa selecionada antes de registar o pagamento.");
            }
        }
        else if (origemTipo == "ContaBancaria")
        {
            var contaValida = await _context.ContasBancarias.AnyAsync(c =>
                c.Id == origemId && c.EmpresaId == conta.EmpresaId && c.Ativo);
            if (!contaValida)
            {
                throw new InvalidOperationException("A conta bancária selecionada não existe ou está inativa.");
            }
        }
        else
        {
            throw new InvalidOperationException("Origem de pagamento inválida.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var movimentoId = await _tesourariaService.RegistarMovimentoAsync(new NovoMovimentoDto
        {
            Data = dataPagamento,
            Descricao = $"Pagamento {conta.Codigo}: {conta.Descricao}",
            Valor = conta.Valor,
            Tipo = TipoCategoria.Despesa,
            TipoOperacao = TipoOperacao.Saida,
            FormaPagamento = conta.FormaPagamento,
            CentroCusto = conta.CentroCusto,
            CategoriaId = conta.CategoriaId,
            CaixaId = origemTipo == "Caixa" ? origemId : null,
            ContaBancariaId = origemTipo == "ContaBancaria" ? origemId : null,
            EmpresaId = conta.EmpresaId
        });

        conta.Estado = EstadoConta.Recebido; // Neste contexto, Recebido representa Paga.
        conta.DataPagamento = dataPagamento;
        conta.MovimentoId = movimentoId;
        conta.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task CancelarAsync(int contaPagarId)
    {
        var conta = await _context.ContasPagar.FindAsync(contaPagarId)
            ?? throw new InvalidOperationException("Conta a pagar não encontrada.");

        if (conta.Estado != EstadoConta.Pendente)
        {
            throw new InvalidOperationException("Só é possível cancelar contas pendentes.");
        }

        conta.Estado = EstadoConta.Cancelado;
        conta.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
