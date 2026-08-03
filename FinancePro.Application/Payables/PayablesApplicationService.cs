using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Payables;

public sealed class PayablesApplicationService
{
    private readonly IPayablesGateway _gateway;

    public PayablesApplicationService(IPayablesGateway gateway) => _gateway = gateway;

    public async Task<Result<PayablesDashboard>> LoadAsync(PayablesQuery query, CancellationToken cancellationToken = default)
    {
        if (query.EmpresaId <= 0)
            return Result<PayablesDashboard>.Fail("A empresa é obrigatória.");

        try
        {
            var items = await _gateway.ListAsync(query.EmpresaId, cancellationToken);
            var filtered = ApplyFilter(items, query).ToList();
            var suppliers = await _gateway.ListSuppliersAsync(query.EmpresaId, cancellationToken);
            var categories = await _gateway.ListCategoriesAsync(query.EmpresaId, cancellationToken);
            var origins = await _gateway.ListOriginsAsync(query.EmpresaId, cancellationToken);

            return Result<PayablesDashboard>.Ok(new PayablesDashboard(
                filtered,
                suppliers,
                categories,
                origins,
                BuildSummary(items)));
        }
        catch (Exception ex)
        {
            return Result<PayablesDashboard>.Fail(ex.Message, "Não foi possível carregar as contas a pagar.");
        }
    }

    public async Task<Result> CreateAsync(NovaContaPagarDto request, CancellationToken cancellationToken = default)
    {
        var errors = Validate(request);
        if (errors.Count > 0) return Result.Fail(errors);

        try
        {
            request.Descricao = request.Descricao.Trim();
            request.FormaPagamento = Normalize(request.FormaPagamento);
            request.CentroCusto = Normalize(request.CentroCusto);
            await _gateway.CreateAsync(request, cancellationToken);
            return Result.Ok("Conta a pagar registada com sucesso.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, "Não foi possível registar a conta a pagar.");
        }
    }

    public async Task<Result> PayAsync(int contaId, OpcaoOrigemDto? origem, DateTime data, CancellationToken cancellationToken = default)
    {
        if (contaId <= 0) return Result.Fail("Selecione uma conta a pagar válida.");
        if (origem is null) return Result.Fail("Selecione a origem do pagamento.");
        if (!origem.Disponivel) return Result.Fail(origem.MotivoIndisponibilidade ?? "A origem selecionada não está disponível.");
        if (data.Date > DateTime.Today.AddDays(1)) return Result.Fail("A data do pagamento não pode estar no futuro.");

        try
        {
            await _gateway.PayAsync(contaId, origem.Tipo, origem.Id, data, cancellationToken);
            return Result.Ok("Pagamento confirmado e lançado na Tesouraria.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, "Não foi possível confirmar o pagamento.");
        }
    }

    public async Task<Result> CancelAsync(int contaId, CancellationToken cancellationToken = default)
    {
        if (contaId <= 0) return Result.Fail("Selecione uma conta a pagar válida.");

        try
        {
            await _gateway.CancelAsync(contaId, cancellationToken);
            return Result.Ok("Conta a pagar cancelada.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, "Não foi possível cancelar a conta a pagar.");
        }
    }

    private static IReadOnlyList<string> Validate(NovaContaPagarDto request)
    {
        var errors = new List<string>();
        if (request.EmpresaId <= 0) errors.Add("A empresa é obrigatória.");
        if (string.IsNullOrWhiteSpace(request.Descricao)) errors.Add("Indique uma descrição.");
        if (request.Valor <= 0) errors.Add("O valor deve ser superior a zero.");
        if (request.DataVencimento.Date < request.DataEmissao.Date) errors.Add("O vencimento não pode ser anterior à emissão.");
        return errors;
    }

    private static IEnumerable<ContaPagarListItemDto> ApplyFilter(IEnumerable<ContaPagarListItemDto> source, PayablesQuery query)
    {
        var result = source;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            result = result.Where(x =>
                x.Codigo.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Descricao.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (x.FornecedorNome?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            result = result.Where(x => x.EstadoExibicao.Equals(query.Status, StringComparison.OrdinalIgnoreCase));

        if (query.DueFrom.HasValue) result = result.Where(x => x.DataVencimento.Date >= query.DueFrom.Value.Date);
        if (query.DueTo.HasValue) result = result.Where(x => x.DataVencimento.Date <= query.DueTo.Value.Date);
        return result;
    }

    private static PayablesSummary BuildSummary(IEnumerable<ContaPagarListItemDto> items)
    {
        var list = items.ToList();
        var open = list.Where(x => x.EstadoExibicao is "Pendente" or "Atrasado").ToList();
        var overdue = list.Where(x => x.EstadoExibicao == "Atrasado").ToList();
        var today = list.Where(x => x.PodePagar && x.DataVencimento.Date == DateTime.Today).ToList();
        var paid = list.Where(x => x.EstadoExibicao == "Paga").ToList();
        return new PayablesSummary(open.Sum(x => x.Valor), overdue.Sum(x => x.Valor), today.Sum(x => x.Valor), paid.Sum(x => x.Valor), open.Count, overdue.Count);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
