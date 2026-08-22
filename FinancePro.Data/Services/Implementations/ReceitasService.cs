using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Core.Enums;
using FinancePro.Data.Context;
using FinancePro.Data.Accounting;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinancePro.Services.Implementations;

public class ReceitasService : IReceitasService
{
    private readonly FinanceProDbContext _context;
    private readonly ITesourariaService _tesourariaService;
    private bool _estruturaFaturacaoConfirmada;

    public ReceitasService(FinanceProDbContext context, ITesourariaService tesourariaService)
    {
        _context = context;
        _tesourariaService = tesourariaService;
    }

    public async Task<IReadOnlyList<ContaReceberListItemDto>> ListarAsync(int empresaId)
    {
        await GarantirEstruturaFaturacaoAsync();
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
            ValorLiquidado = c.ValorLiquidado, DescontoGeral = c.DescontoGeral, Frete = c.Frete, OutrasDespesas = c.OutrasDespesas, Observacoes = c.Observacoes,
            ComercialEstado = c.ComercialEstado, NumeroProposta = c.NumeroProposta, NumeroFatura = c.NumeroFatura, DataAprovacao = c.DataAprovacao, DataFaturacao = c.DataFaturacao,
            DataEmissao = c.DataEmissao,
            DataVencimento = c.DataVencimento,
            DataRecebimento = c.DataRecebimento,
            ClienteNome = c.Cliente?.Nome, ClienteId = c.ClienteId,
            ClienteNIF = c.Cliente?.NIF, ClienteMorada = c.Cliente?.Morada,
            ClienteTelefone = c.Cliente?.Telefone, ClienteEmail = c.Cliente?.Email,
            CategoriaNome = c.Categoria?.Nome, CategoriaId = c.CategoriaId,
            FormaPagamento = c.FormaPagamento,
            CentroCusto = c.CentroCusto,
            EstadoExibicao = c.Estado == EstadoConta.Pendente && c.ValorLiquidado > 0 ? "Parcial" :
                c.Estado == EstadoConta.Pendente && c.DataVencimento.Date < hoje ? "Atrasado" : c.Estado.ToString(),
            PodeReceber = c.Estado == EstadoConta.Pendente && c.ComercialEstado == "Faturada",
            PodeCancelar = c.Estado == EstadoConta.Pendente
        }).ToList();
    }

    public async Task<EmpresaDto?> ObterEmpresaAsync(int empresaId)
    {
        var empresa = await _context.Empresas
            .AsNoTracking().Where(x => x.Id == empresaId)
            .Select(x => new EmpresaDto
            {
                Id = x.Id, Nome = x.Nome, NIF = x.NIF, Morada = x.Morada,
                Telefone = x.Telefone, Email = x.Email, Moeda = x.Moeda, Logotipo = x.Logotipo
            }).FirstOrDefaultAsync();

        if (empresa is null) return null;

        var contaPrincipal = await _context.ContasBancarias.AsNoTracking()
            .Where(c => c.EmpresaId == empresaId)
            .OrderBy(c => c.Id)
            .Select(c => new { c.NumeroConta, c.IBAN, c.Titular, BancoNome = c.Banco.Nome })
            .FirstOrDefaultAsync();

        if (contaPrincipal is not null)
        {
            empresa.BancoNome = contaPrincipal.BancoNome;
            empresa.BancoConta = contaPrincipal.NumeroConta;
            empresa.BancoIban = contaPrincipal.IBAN;
            empresa.BancoTitular = contaPrincipal.Titular;
        }

        return empresa;
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
        await GarantirEstruturaFaturacaoAsync();
        if (string.IsNullOrWhiteSpace(dto.Descricao))
        {
            throw new InvalidOperationException("A descrição é obrigatória.");
        }

        if (!dto.CriarComoProposta && dto.Valor <= 0)
        {
            throw new InvalidOperationException("O valor tem de ser maior do que zero.");
        }

        var proximoNumero = 1 + await _context.ContasReceber.CountAsync(c => c.EmpresaId == dto.EmpresaId);
        var propostaNumero = $"PRO-{DateTime.Today:yyyy}-{proximoNumero:D5}";

        _context.ContasReceber.Add(new ContaReceber
        {
            Codigo = $"REC-{proximoNumero:D5}",
            ComercialEstado = dto.CriarComoProposta ? "Proposta" : "Faturada",
            NumeroProposta = dto.CriarComoProposta ? propostaNumero : null,
            DataFaturacao = dto.CriarComoProposta ? null : DateTime.Today,
            Descricao = dto.Descricao.Trim(),
            Valor = dto.Valor,
            DescontoGeral = dto.DescontoGeral,
            Frete = dto.Frete,
            OutrasDespesas = dto.OutrasDespesas,
            Observacoes = dto.Observacoes,
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

    public async Task AtualizarRascunhoAsync(AtualizarFaturaRascunhoDto dto)
    {
        await GarantirEstruturaFaturacaoAsync();
        var conta = await _context.ContasReceber.SingleOrDefaultAsync(x => x.Id == dto.Id && x.EmpresaId == dto.EmpresaId)
            ?? throw new InvalidOperationException("Rascunho não encontrado.");
        if (conta.ComercialEstado != "Proposta" || conta.Estado == EstadoConta.Cancelado)
            throw new InvalidOperationException("Apenas faturas proforma em preparação podem ser editadas.");
        conta.Descricao = dto.Descricao.Trim();
        conta.DataEmissao = dto.DataEmissao; conta.DataVencimento = dto.DataVencimento;
        conta.ClienteId = dto.ClienteId; conta.CategoriaId = dto.CategoriaId;
        conta.FormaPagamento = dto.FormaPagamento; conta.CentroCusto = dto.CentroCusto;
        conta.DescontoGeral = dto.DescontoGeral; conta.Frete = dto.Frete; conta.OutrasDespesas = dto.OutrasDespesas;
        conta.Observacoes = dto.Observacoes; conta.DataAtualizacao = DateTime.UtcNow;
        var itens = await _context.VendaItens.Where(x => x.ContaReceberId == conta.Id).ToListAsync();
        conta.Valor = Math.Max(0, itens.Sum(x => x.Total) - conta.DescontoGeral + conta.Frete + conta.OutrasDespesas);
        await _context.SaveChangesAsync();
    }

    public async Task<int> DuplicarAsync(int contaReceberId, int empresaId)
    {
        await GarantirEstruturaFaturacaoAsync();
        var origem = await _context.ContasReceber.AsNoTracking().Include(x => x.Itens)
            .SingleOrDefaultAsync(x => x.Id == contaReceberId && x.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("Documento de origem não encontrado.");
        if (origem.Estado == EstadoConta.Cancelado) throw new InvalidOperationException("Documentos cancelados não podem ser duplicados.");
        var proximoNumero = 1 + await _context.ContasReceber.CountAsync(x => x.EmpresaId == empresaId);
        var prazoDias = Math.Max(0, (origem.DataVencimento.Date - origem.DataEmissao.Date).Days);
        return await ExecutarTransacaoResilienteAsync(async () =>
        {
            var novo = new ContaReceber
            {
                Codigo = $"REC-{proximoNumero:D5}", NumeroProposta = $"PRO-{DateTime.Today:yyyy}-{proximoNumero:D5}",
                ComercialEstado = "Proposta", Descricao = origem.Descricao, Valor = origem.Valor,
                DescontoGeral = origem.DescontoGeral, Frete = origem.Frete, OutrasDespesas = origem.OutrasDespesas,
                Observacoes = origem.Observacoes, DataEmissao = DateTime.Today, DataVencimento = DateTime.Today.AddDays(prazoDias),
                FormaPagamento = origem.FormaPagamento, CentroCusto = origem.CentroCusto, ClienteId = origem.ClienteId,
                CategoriaId = origem.CategoriaId, EmpresaId = empresaId, Estado = EstadoConta.Pendente
            };
            _context.ContasReceber.Add(novo); await _context.SaveChangesAsync();
            foreach (var item in origem.Itens) _context.VendaItens.Add(new VendaItem
            {
                ContaReceberId = novo.Id, ProdutoId = item.ProdutoId, Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario, DescontoPercentual = item.DescontoPercentual, IvaPercentual = item.IvaPercentual
            });
            await _context.SaveChangesAsync();
            return novo.Id;
        });
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

        await ExecutarTransacaoResilienteAsync(async () =>
        {
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
        var reciboSeq = 1 + await ContarDocumentosAsync(conta.EmpresaId, "Recibo");
        var reciboNumero = $"REC-{dataRecebimento:yyyy}-{reciboSeq:D5}";
        await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO dbo.DocumentosFiscais (ContaReceberId,EmpresaId,Tipo,Numero,DataEmissao,BaseTributavel,ValorIva,Total,Estado,DocumentoOrigem) VALUES ({conta.Id},{conta.EmpresaId},{"Recibo"},{reciboNumero},{dataRecebimento},{conta.Valor},{0m},{conta.Valor},{"Emitido"},{conta.NumeroFatura})");
        await AutomaticAccountingPoster.TryPostAsync(_context, conta.EmpresaId, "VENDA_RECEBIMENTO", movimentoId, dataRecebimento, reciboNumero, conta.NumeroFatura ?? conta.Codigo, $"Recebimento {conta.Codigo}", conta.Valor);
        });
    }


    public async Task RegistarRecebimentoParcialAsync(int contaReceberId, decimal valor, string origemTipo, int origemId, DateTime dataRecebimento)
    {
        var conta = await _context.ContasReceber.SingleOrDefaultAsync(c => c.Id == contaReceberId)
            ?? throw new InvalidOperationException("Conta a receber não encontrada.");
        if (conta.ComercialEstado != "Faturada") throw new InvalidOperationException("A proposta deve ser faturada antes de receber valores.");
        if (conta.Estado != EstadoConta.Pendente) throw new InvalidOperationException("Esta conta já não está pendente.");
        var saldo = conta.Valor - conta.ValorLiquidado;
        if (valor <= 0 || valor > saldo) throw new InvalidOperationException($"O valor deve ser maior que zero e não pode exceder o saldo de {saldo:N2}.");
        await ExecutarTransacaoResilienteAsync(async () =>
        {
            var movimentoId = await _tesourariaService.RegistarMovimentoAsync(new NovoMovimentoDto { Data=dataRecebimento, Descricao=$"Recebimento {conta.Codigo}: {conta.Descricao}", Valor=valor, Tipo=TipoCategoria.Receita, TipoOperacao=TipoOperacao.Entrada, FormaPagamento=conta.FormaPagamento, CentroCusto=conta.CentroCusto, CategoriaId=conta.CategoriaId, CaixaId=origemTipo=="Caixa"?origemId:null, ContaBancariaId=origemTipo=="ContaBancaria"?origemId:null, ClienteId=conta.ClienteId, EmpresaId=conta.EmpresaId });
            conta.ValorLiquidado += valor; conta.MovimentoId = movimentoId; conta.DataAtualizacao=DateTime.UtcNow;
            if (conta.ValorLiquidado >= conta.Valor) { conta.Estado=EstadoConta.Recebido; conta.DataRecebimento=dataRecebimento; }
            await _context.SaveChangesAsync();
            var reciboSeq = 1 + await ContarDocumentosAsync(conta.EmpresaId, "Recibo");
            var reciboNumero = $"REC-{dataRecebimento:yyyy}-{reciboSeq:D5}";
            await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO dbo.DocumentosFiscais (ContaReceberId,EmpresaId,Tipo,Numero,DataEmissao,BaseTributavel,ValorIva,Total,Estado,DocumentoOrigem) VALUES ({conta.Id},{conta.EmpresaId},{"Recibo"},{reciboNumero},{dataRecebimento},{valor},{0m},{valor},{"Emitido"},{conta.NumeroFatura})");
            await AutomaticAccountingPoster.TryPostAsync(_context, conta.EmpresaId, "VENDA_RECEBIMENTO", movimentoId, dataRecebimento, reciboNumero, conta.NumeroFatura ?? conta.Codigo, $"Recebimento parcial {conta.Codigo}", valor);
        });
    }


    public async Task<IReadOnlyList<ProdutoStockDto>> ListarProdutosAsync(int empresaId) => await _context.Produtos.AsNoTracking().Where(x=>x.EmpresaId==empresaId&&x.Ativo).OrderBy(x=>x.Nome).Select(x=>new ProdutoStockDto{Id=x.Id,Codigo=x.Codigo,Nome=x.Nome,Unidade=x.Unidade,StockAtual=x.StockAtual,CustoMedio=x.CustoMedio,PrecoVenda=x.PrecoVenda,ControlaStock=x.ControlaStock}).ToListAsync();
    public async Task<IReadOnlyList<DocumentoItemDto>> ListarItensAsync(int contaReceberId) => await _context.VendaItens.AsNoTracking().Include(x=>x.Produto).Where(x=>x.ContaReceberId==contaReceberId).OrderBy(x=>x.Id).Select(x=>new DocumentoItemDto{Id=x.Id,ProdutoId=x.ProdutoId,ProdutoCodigo=x.Produto.Codigo,ProdutoNome=x.Produto.Nome,Unidade=x.Produto.Unidade,Quantidade=x.Quantidade,PrecoUnitario=x.PrecoUnitario,DescontoPercentual=x.DescontoPercentual,IvaPercentual=x.IvaPercentual,Subtotal=x.Quantidade*x.PrecoUnitario*(1-x.DescontoPercentual/100m),ValorIva=x.Quantidade*x.PrecoUnitario*(1-x.DescontoPercentual/100m)*x.IvaPercentual/100m,Total=x.Quantidade*x.PrecoUnitario*(1-x.DescontoPercentual/100m)*(1+x.IvaPercentual/100m)}).ToListAsync();
    public async Task AdicionarItemAsync(NovoDocumentoItemDto dto)
    {
        ValidarItem(dto.Quantidade, dto.PrecoUnitario, dto.DescontoPercentual, dto.IvaPercentual);
        await ExecutarTransacaoResilienteAsync(async () =>
        {
            var conta = await _context.ContasReceber.FindAsync(dto.DocumentoId)
                ?? throw new InvalidOperationException("Fatura proforma não encontrada.");
            ValidarEdicaoItens(conta);
            if (!await _context.Produtos.AnyAsync(x => x.Id == dto.ProdutoId && x.EmpresaId == conta.EmpresaId && x.Ativo))
                throw new InvalidOperationException("O produto ou serviço selecionado não está disponível para esta empresa.");
            _context.VendaItens.Add(new VendaItem
            {
                ContaReceberId = conta.Id, ProdutoId = dto.ProdutoId, Quantidade = dto.Quantidade,
                PrecoUnitario = dto.PrecoUnitario, DescontoPercentual = dto.DescontoPercentual, IvaPercentual = dto.IvaPercentual
            });
            await _context.SaveChangesAsync();
            await RecalcularVendaAsync(conta.Id);
        });
    }

    public async Task AtualizarItemAsync(AtualizarDocumentoItemDto dto)
    {
        ValidarItem(dto.Quantidade, dto.PrecoUnitario, dto.DescontoPercentual, dto.IvaPercentual);
        await ExecutarTransacaoResilienteAsync(async () =>
        {
            var item = await _context.VendaItens.Include(x => x.ContaReceber)
                .FirstOrDefaultAsync(x => x.Id == dto.Id && x.ContaReceberId == dto.DocumentoId)
                ?? throw new InvalidOperationException("Item não encontrado.");
            ValidarEdicaoItens(item.ContaReceber);
            if (!await _context.Produtos.AnyAsync(x => x.Id == dto.ProdutoId && x.EmpresaId == item.ContaReceber.EmpresaId && x.Ativo))
                throw new InvalidOperationException("O produto ou serviço selecionado não está disponível para esta empresa.");
            item.ProdutoId = dto.ProdutoId; item.Quantidade = dto.Quantidade; item.PrecoUnitario = dto.PrecoUnitario;
            item.DescontoPercentual = dto.DescontoPercentual; item.IvaPercentual = dto.IvaPercentual;
            item.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await RecalcularVendaAsync(dto.DocumentoId);
        });
    }

    public async Task RemoverItemAsync(int itemId)
    {
        await ExecutarTransacaoResilienteAsync(async () =>
        {
            var item = await _context.VendaItens.Include(x => x.ContaReceber).FirstOrDefaultAsync(x => x.Id == itemId)
                ?? throw new InvalidOperationException("Item não encontrado.");
            ValidarEdicaoItens(item.ContaReceber);
            var documentoId = item.ContaReceberId;
            _context.VendaItens.Remove(item);
            await _context.SaveChangesAsync();
            await RecalcularVendaAsync(documentoId);
        });
    }

    private static void ValidarItem(decimal quantidade, decimal preco, decimal desconto, decimal iva)
    {
        if (quantidade <= 0) throw new InvalidOperationException("A quantidade deve ser maior do que zero.");
        if (preco < 0) throw new InvalidOperationException("O preço unitário não pode ser negativo.");
        if (desconto is < 0 or > 100) throw new InvalidOperationException("O desconto deve estar entre 0% e 100%.");
        if (iva is < 0 or > 100) throw new InvalidOperationException("A taxa de IVA deve estar entre 0% e 100%.");
    }

    private static void ValidarEdicaoItens(ContaReceber conta)
    {
        if (conta.ComercialEstado != "Proposta" || conta.Estado == EstadoConta.Cancelado)
            throw new InvalidOperationException("Apenas itens de faturas proforma em preparação podem ser alterados.");
    }
    private async Task RecalcularVendaAsync(int id){ var c=await _context.ContasReceber.FindAsync(id)??throw new InvalidOperationException("Proforma não encontrada."); var itens=await _context.VendaItens.Where(x=>x.ContaReceberId==id).ToListAsync(); var bruto=itens.Sum(x=>x.Total); c.Valor=Math.Max(0,bruto-c.DescontoGeral+c.Frete+c.OutrasDespesas); c.DataAtualizacao=DateTime.UtcNow; await _context.SaveChangesAsync(); }

    public async Task AprovarPropostaAsync(int contaReceberId)
    {
        var conta = await _context.ContasReceber.FindAsync(contaReceberId) ?? throw new InvalidOperationException("Proposta não encontrada.");
        if (conta.ComercialEstado != "Proposta") throw new InvalidOperationException("Só faturas proforma em preparação podem ser validadas.");
        if (!await _context.VendaItens.AnyAsync(x => x.ContaReceberId == contaReceberId) || conta.Valor <= 0)
            throw new InvalidOperationException("Adicione pelo menos um item com valor antes de validar a fatura proforma.");
        conta.ComercialEstado = "Aprovada"; conta.DataAprovacao = DateTime.UtcNow; conta.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task FaturarAsync(int contaReceberId)
    {
        var conta = await _context.ContasReceber.FindAsync(contaReceberId) ?? throw new InvalidOperationException("Proposta não encontrada.");
        if (conta.ComercialEstado != "Aprovada") throw new InvalidOperationException("A fatura proforma deve estar validada antes da emissão definitiva.");
        var itens = await _context.VendaItens.Include(x=>x.Produto).Where(x=>x.ContaReceberId==conta.Id).ToListAsync();
        if(itens.Count==0) throw new InvalidOperationException("Adicione pelo menos um produto antes da faturação.");
        foreach(var i in itens.Where(x=>x.Produto.ControlaStock)) if(i.Produto.StockAtual < i.Quantidade) throw new InvalidOperationException($"Stock insuficiente para {i.Produto.Nome}.");
        var numero = 1 + await _context.ContasReceber.CountAsync(c => c.EmpresaId == conta.EmpresaId && c.NumeroFatura != null);
        conta.NumeroFatura = $"FAT-{DateTime.Today:yyyy}-{numero:D5}";
        await ExecutarTransacaoResilienteAsync(async () =>
        {
            foreach(var i in itens.Where(x=>x.Produto.ControlaStock)){ i.Produto.StockAtual-=i.Quantidade; i.Produto.DataAtualizacao=DateTime.UtcNow; _context.MovimentosStock.Add(new MovimentoStock{EmpresaId=conta.EmpresaId,ProdutoId=i.ProdutoId,Tipo=TipoMovimentoStock.Saida,Quantidade=i.Quantidade,CustoUnitario=i.Produto.CustoMedio,SaldoApos=i.Produto.StockAtual,DocumentoReferencia=conta.NumeroFatura,Observacao="Saída automática por faturação"}); }
            conta.ComercialEstado = "Faturada"; conta.DataFaturacao = DateTime.UtcNow; conta.DataAtualizacao = DateTime.UtcNow;
            var baseTributavel = itens.Sum(x => x.Subtotal);
            var valorIva = itens.Sum(x => x.ValorIva);
            await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO dbo.DocumentosFiscais (ContaReceberId,EmpresaId,Tipo,Numero,DataEmissao,BaseTributavel,ValorIva,Total,Estado) VALUES ({conta.Id},{conta.EmpresaId},{"Fatura"},{conta.NumeroFatura},{DateTime.UtcNow},{baseTributavel},{valorIva},{conta.Valor},{"Emitido"})");
            await _context.SaveChangesAsync();
            await AutomaticAccountingPoster.TryPostAsync(_context, conta.EmpresaId, "VENDA_FATURA", conta.Id, conta.DataFaturacao.Value, conta.NumeroFatura, conta.Codigo, $"Fatura de venda {conta.NumeroFatura}", conta.Valor);
        });
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

    public async Task<IReadOnlyList<DocumentoFiscalDto>> ListarDocumentosAsync(int contaReceberId)
    {
        var result = new List<DocumentoFiscalDto>();
        var conn = _context.Database.GetDbConnection();
        var mustClose = conn.State != System.Data.ConnectionState.Open;
        if (mustClose) await conn.OpenAsync();
        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id,ContaReceberId,Tipo,Numero,DataEmissao,BaseTributavel,ValorIva,Total,Estado,DocumentoOrigem,Motivo FROM dbo.DocumentosFiscais WHERE ContaReceberId=@id ORDER BY DataEmissao DESC,Id DESC";
            if (_context.Database.CurrentTransaction is not null) cmd.Transaction = _context.Database.CurrentTransaction.GetDbTransaction();
            var par = cmd.CreateParameter(); par.ParameterName="@id"; par.Value=contaReceberId; cmd.Parameters.Add(par);
            await using var r = await cmd.ExecuteReaderAsync();
            while(await r.ReadAsync()) result.Add(new DocumentoFiscalDto { Id=r.GetInt32(0), ContaReceberId=r.GetInt32(1), Tipo=r.GetString(2), Numero=r.GetString(3), DataEmissao=r.GetDateTime(4), BaseTributavel=r.GetDecimal(5), ValorIva=r.GetDecimal(6), Total=r.GetDecimal(7), Estado=r.GetString(8), DocumentoOrigem=r.IsDBNull(9)?null:r.GetString(9), Motivo=r.IsDBNull(10)?null:r.GetString(10) });
        }
        finally { if(mustClose) await conn.CloseAsync(); }
        return result;
    }

    public async Task EmitirNotaAsync(NovaNotaFiscalDto dto)
    {
        var conta = await _context.ContasReceber.SingleOrDefaultAsync(x=>x.Id==dto.ContaReceberId) ?? throw new InvalidOperationException("Fatura não encontrada.");
        if(conta.ComercialEstado!="Faturada" || string.IsNullOrWhiteSpace(conta.NumeroFatura)) throw new InvalidOperationException("Só é possível emitir notas para faturas emitidas.");
        if(dto.Tipo == "Credito" && dto.Valor > conta.Valor) throw new InvalidOperationException("A nota de crédito não pode exceder o valor atual da fatura.");
        var tipo = dto.Tipo == "Credito" ? "NotaCredito" : "NotaDebito";
        var seq = 1 + await ContarDocumentosAsync(conta.EmpresaId, tipo);
        var prefix = dto.Tipo == "Credito" ? "NC" : "ND";
        var numero = $"{prefix}-{DateTime.Today:yyyy}-{seq:D5}";
        var itens = await _context.VendaItens.Where(x=>x.ContaReceberId==conta.Id).ToListAsync();
        var ivaOriginal = itens.Sum(x=>x.ValorIva); var totalOriginal = itens.Sum(x=>x.Total);
        var ivaNota = totalOriginal > 0 ? Math.Round(dto.Valor * ivaOriginal / totalOriginal, 2) : 0m;
        var baseNota = dto.Valor - ivaNota;
        await ExecutarTransacaoResilienteAsync(async () =>
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO dbo.DocumentosFiscais (ContaReceberId,EmpresaId,Tipo,Numero,DataEmissao,BaseTributavel,ValorIva,Total,Estado,DocumentoOrigem,Motivo) VALUES ({conta.Id},{conta.EmpresaId},{tipo},{numero},{DateTime.UtcNow},{baseNota},{ivaNota},{dto.Valor},{"Emitido"},{conta.NumeroFatura},{dto.Motivo.Trim()})");
            conta.Valor += dto.Tipo == "Credito" ? -dto.Valor : dto.Valor;
            if(conta.ValorLiquidado > conta.Valor) conta.ValorLiquidado = conta.Valor;
            conta.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        });
    }

    private async Task<int> ContarDocumentosAsync(int empresaId, string tipo)
    {
        var conn = _context.Database.GetDbConnection(); var mustClose=conn.State!=System.Data.ConnectionState.Open; if(mustClose) await conn.OpenAsync();
        try { await using var cmd=conn.CreateCommand(); if (_context.Database.CurrentTransaction is not null) cmd.Transaction = _context.Database.CurrentTransaction.GetDbTransaction(); cmd.CommandText="SELECT COUNT(*) FROM dbo.DocumentosFiscais WHERE EmpresaId=@e AND Tipo=@t"; var e=cmd.CreateParameter();e.ParameterName="@e";e.Value=empresaId;cmd.Parameters.Add(e);var t=cmd.CreateParameter();t.ParameterName="@t";t.Value=tipo;cmd.Parameters.Add(t);return Convert.ToInt32(await cmd.ExecuteScalarAsync()); }
        finally { if(mustClose) await conn.CloseAsync(); }
    }

    private async Task GarantirEstruturaFaturacaoAsync()
    {
        if (_estruturaFaturacaoConfirmada) return;
        const string sql = @"
IF COL_LENGTH('dbo.ContasReceber','DescontoGeral') IS NULL
    ALTER TABLE dbo.ContasReceber ADD DescontoGeral decimal(18,2) NOT NULL CONSTRAINT DF_ContasReceber_DescontoGeral DEFAULT(0);
IF COL_LENGTH('dbo.ContasReceber','Frete') IS NULL
    ALTER TABLE dbo.ContasReceber ADD Frete decimal(18,2) NOT NULL CONSTRAINT DF_ContasReceber_Frete DEFAULT(0);
IF COL_LENGTH('dbo.ContasReceber','OutrasDespesas') IS NULL
    ALTER TABLE dbo.ContasReceber ADD OutrasDespesas decimal(18,2) NOT NULL CONSTRAINT DF_ContasReceber_OutrasDespesas DEFAULT(0);
IF COL_LENGTH('dbo.ContasReceber','Observacoes') IS NULL
    ALTER TABLE dbo.ContasReceber ADD Observacoes nvarchar(1000) NULL;";
        await _context.Database.ExecuteSqlRawAsync(sql);
        _estruturaFaturacaoConfirmada = true;
    }

    private async Task ExecutarTransacaoResilienteAsync(Func<Task> operacao)
    {
        var estrategia = _context.Database.CreateExecutionStrategy();
        await estrategia.ExecuteAsync(async () =>
        {
            await using var transacao = await _context.Database.BeginTransactionAsync();
            await operacao();
            await transacao.CommitAsync();
        });
    }

    private async Task<T> ExecutarTransacaoResilienteAsync<T>(Func<Task<T>> operacao)
    {
        var estrategia = _context.Database.CreateExecutionStrategy();
        return await estrategia.ExecuteAsync(async () =>
        {
            await using var transacao = await _context.Database.BeginTransactionAsync();
            var resultado = await operacao();
            await transacao.CommitAsync();
            return resultado;
        });
    }

}
