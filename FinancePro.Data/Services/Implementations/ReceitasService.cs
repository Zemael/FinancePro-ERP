using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class ReceitasService : IReceitasService
{
    private readonly FinanceProDbContext _context;
    private readonly ITesourariaService _tesourariaService;

    public ReceitasService(FinanceProDbContext context, ITesourariaService tesourariaService)
    {
        _context = context;
        _tesourariaService = tesourariaService;
    }

    public async Task<IReadOnlyList<ContaReceberListItemDto>> ListarAsync(int empresaId)
    {
        var hoje = DateTime.Today;

        var contas = await _context.ContasReceber
            .Where(c => c.EmpresaId == empresaId)
            .Include(c => c.Cliente)
            .Include(c => c.Categoria)
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();

        return contas.Select(c => new ContaReceberListItemDto
        {
            Id = c.Id,
            Codigo = c.Codigo,
            Descricao = c.Descricao,
            Valor = c.Valor,
            ValorLiquidado = c.ValorLiquidado,
            ComercialEstado = c.ComercialEstado, NumeroProposta = c.NumeroProposta, NumeroFatura = c.NumeroFatura, DataAprovacao = c.DataAprovacao, DataFaturacao = c.DataFaturacao,
            DataEmissao = c.DataEmissao,
            DataVencimento = c.DataVencimento,
            DataRecebimento = c.DataRecebimento,
            ClienteNome = c.Cliente?.Nome,
            CategoriaNome = c.Categoria?.Nome,
            FormaPagamento = c.FormaPagamento,
            CentroCusto = c.CentroCusto,
            EstadoExibicao = c.Estado == EstadoConta.Pendente && c.ValorLiquidado > 0 ? "Parcial" :
                c.Estado == EstadoConta.Pendente && c.DataVencimento.Date < hoje ? "Atrasado" : c.Estado.ToString(),
            PodeReceber = c.Estado == EstadoConta.Pendente && c.ComercialEstado == "Faturada",
            PodeCancelar = c.Estado == EstadoConta.Pendente
        }).ToList();
    }

    public async Task<IReadOnlyList<ClienteOpcaoDto>> ListarClientesAsync(int empresaId)
    {
        return await _context.Clientes
            .Where(c => c.EmpresaId == empresaId && c.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new ClienteOpcaoDto { Id = c.Id, Nome = c.Nome })
            .ToListAsync();
    }

    public Task<IReadOnlyList<CategoriaOpcaoDto>> ListarCategoriasAsync(int empresaId) =>
        _tesourariaService.ListarCategoriasAsync(empresaId, TipoCategoria.Receita);

    public Task<IReadOnlyList<OpcaoOrigemDto>> ListarOrigensAsync(int empresaId) =>
        _tesourariaService.ListarOrigensAsync(empresaId);

    public async Task CriarAsync(NovaContaReceberDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao))
        {
            throw new InvalidOperationException("A descrição é obrigatória.");
        }

        if (dto.Valor <= 0)
        {
            throw new InvalidOperationException("O valor tem de ser maior do que zero.");
        }

        var proximoNumero = 1 + await _context.ContasReceber.CountAsync(c => c.EmpresaId == dto.EmpresaId);
        var propostaNumero = $"PROP-{DateTime.Today:yyyy}-{proximoNumero:D5}";

        _context.ContasReceber.Add(new ContaReceber
        {
            Codigo = $"REC-{proximoNumero:D5}",
            ComercialEstado = dto.CriarComoProposta ? "Proposta" : "Faturada",
            NumeroProposta = dto.CriarComoProposta ? propostaNumero : null,
            DataFaturacao = dto.CriarComoProposta ? null : DateTime.Today,
            Descricao = dto.Descricao.Trim(),
            Valor = dto.Valor,
            DataEmissao = dto.DataEmissao,
            DataVencimento = dto.DataVencimento,
            FormaPagamento = dto.FormaPagamento,
            CentroCusto = dto.CentroCusto,
            ClienteId = dto.ClienteId,
            CategoriaId = dto.CategoriaId,
            EmpresaId = dto.EmpresaId,
            Estado = EstadoConta.Pendente
        });

        await _context.SaveChangesAsync();
    }

    public async Task RegistarRecebimentoAsync(int contaReceberId, string origemTipo, int origemId, DateTime dataRecebimento)
    {
        var conta = await _context.ContasReceber
            .Include(c => c.Cliente)
            .SingleOrDefaultAsync(c => c.Id == contaReceberId)
            ?? throw new InvalidOperationException("Conta a receber não encontrada.");

        if (conta.ComercialEstado != "Faturada") throw new InvalidOperationException("A proposta deve ser faturada antes de receber valores.");

        if (conta.Estado != EstadoConta.Pendente)
        {
            throw new InvalidOperationException("Esta conta já não está pendente.");
        }

        if (dataRecebimento.Date < conta.DataEmissao.Date)
        {
            throw new InvalidOperationException("A data do recebimento não pode ser anterior à data de emissão.");
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
                throw new InvalidOperationException("Abra uma sessão para a caixa selecionada antes de registar o recebimento.");
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
            throw new InvalidOperationException("Origem de recebimento inválida.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var movimentoId = await _tesourariaService.RegistarMovimentoAsync(new NovoMovimentoDto
        {
            Data = dataRecebimento,
            Descricao = $"Recebimento {conta.Codigo}: {conta.Descricao}",
            Valor = conta.Valor,
            Tipo = TipoCategoria.Receita,
            TipoOperacao = TipoOperacao.Entrada,
            FormaPagamento = conta.FormaPagamento,
            CentroCusto = conta.CentroCusto,
            CategoriaId = conta.CategoriaId,
            CaixaId = origemTipo == "Caixa" ? origemId : null,
            ContaBancariaId = origemTipo == "ContaBancaria" ? origemId : null,
            ClienteId = conta.ClienteId,
            EmpresaId = conta.EmpresaId
        });

        conta.ValorLiquidado = conta.Valor;
        conta.Estado = EstadoConta.Recebido;
        conta.DataRecebimento = dataRecebimento;
        conta.MovimentoId = movimentoId;
        conta.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }


    public async Task RegistarRecebimentoParcialAsync(int contaReceberId, decimal valor, string origemTipo, int origemId, DateTime dataRecebimento)
    {
        var conta = await _context.ContasReceber.SingleOrDefaultAsync(c => c.Id == contaReceberId)
            ?? throw new InvalidOperationException("Conta a receber não encontrada.");
        if (conta.ComercialEstado != "Faturada") throw new InvalidOperationException("A proposta deve ser faturada antes de receber valores.");
        if (conta.Estado != EstadoConta.Pendente) throw new InvalidOperationException("Esta conta já não está pendente.");
        var saldo = conta.Valor - conta.ValorLiquidado;
        if (valor <= 0 || valor > saldo) throw new InvalidOperationException($"O valor deve ser maior que zero e não pode exceder o saldo de {saldo:N2}.");
        var movimentoId = await _tesourariaService.RegistarMovimentoAsync(new NovoMovimentoDto { Data=dataRecebimento, Descricao=$"Recebimento {conta.Codigo}: {conta.Descricao}", Valor=valor, Tipo=TipoCategoria.Receita, TipoOperacao=TipoOperacao.Entrada, FormaPagamento=conta.FormaPagamento, CentroCusto=conta.CentroCusto, CategoriaId=conta.CategoriaId, CaixaId=origemTipo=="Caixa"?origemId:null, ContaBancariaId=origemTipo=="ContaBancaria"?origemId:null, ClienteId=conta.ClienteId, EmpresaId=conta.EmpresaId });
        conta.ValorLiquidado += valor; conta.MovimentoId = movimentoId; conta.DataAtualizacao=DateTime.UtcNow;
        if (conta.ValorLiquidado >= conta.Valor) { conta.Estado=EstadoConta.Recebido; conta.DataRecebimento=dataRecebimento; }
        await _context.SaveChangesAsync();
    }

    public async Task AprovarPropostaAsync(int contaReceberId)
    {
        var conta = await _context.ContasReceber.FindAsync(contaReceberId) ?? throw new InvalidOperationException("Proposta não encontrada.");
        if (conta.ComercialEstado != "Proposta") throw new InvalidOperationException("Só propostas em preparação podem ser aprovadas.");
        conta.ComercialEstado = "Aprovada"; conta.DataAprovacao = DateTime.UtcNow; conta.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task FaturarAsync(int contaReceberId)
    {
        var conta = await _context.ContasReceber.FindAsync(contaReceberId) ?? throw new InvalidOperationException("Proposta não encontrada.");
        if (conta.ComercialEstado != "Aprovada") throw new InvalidOperationException("A proposta deve estar aprovada antes da faturação.");
        var numero = 1 + await _context.ContasReceber.CountAsync(c => c.EmpresaId == conta.EmpresaId && c.NumeroFatura != null);
        conta.NumeroFatura = $"FAT-{DateTime.Today:yyyy}-{numero:D5}"; conta.ComercialEstado = "Faturada"; conta.DataFaturacao = DateTime.UtcNow; conta.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task CancelarAsync(int contaReceberId)
    {
        var conta = await _context.ContasReceber.FindAsync(contaReceberId)
            ?? throw new InvalidOperationException("Conta a receber não encontrada.");

        if (conta.Estado != EstadoConta.Pendente)
        {
            throw new InvalidOperationException("Só é possível cancelar contas pendentes.");
        }

        conta.Estado = EstadoConta.Cancelado;
        conta.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
