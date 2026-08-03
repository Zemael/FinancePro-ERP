using FinancePro.Application.Common.Results;
using FinancePro.Core.DTOs;

namespace FinancePro.Application.Receivables;

public sealed class ReceivablesApplicationService
{
    private readonly IReceivablesGateway _gateway;

    public ReceivablesApplicationService(IReceivablesGateway gateway) => _gateway = gateway;

    public async Task<Result<ReceivablesDashboard>> LoadAsync(ReceivablesQuery query, CancellationToken cancellationToken = default)
    {
        if (query.EmpresaId <= 0)
            return Result<ReceivablesDashboard>.Fail("A empresa é obrigatória.");

        try
        {
            var items = await _gateway.ListAsync(query.EmpresaId, cancellationToken);
            var filtered = ApplyFilter(items, query).ToList();
            var customers = await _gateway.ListCustomersAsync(query.EmpresaId, cancellationToken);
            var categories = await _gateway.ListCategoriesAsync(query.EmpresaId, cancellationToken);
            var origins = await _gateway.ListOriginsAsync(query.EmpresaId, cancellationToken);

            return Result<ReceivablesDashboard>.Ok(new ReceivablesDashboard(
                filtered,
                customers,
                categories,
                origins,
                BuildSummary(items)));
        }
        catch (Exception ex)
        {
            return Result<ReceivablesDashboard>.Fail(ex.Message, "Não foi possível carregar as contas a receber.");
        }
    }

    public async Task<Result> CreateAsync(NovaContaReceberDto request, CancellationToken cancellationToken = default)
    {
        var errors = Validate(request);
        if (errors.Count > 0) return Result.Fail(errors);

        try
        {
            request.Descricao = request.Descricao.Trim();
            request.FormaPagamento = Normalize(request.FormaPagamento);
            request.CentroCusto = Normalize(request.CentroCusto);
            await _gateway.CreateAsync(request, cancellationToken);
            return Result.Ok("Conta a receber registada com sucesso.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, "Não foi possível registar a conta a receber.");
        }
    }

    public async Task<Result> ReceiveAsync(int contaId, OpcaoOrigemDto? origem, DateTime data, CancellationToken cancellationToken = default)
    {
        if (contaId <= 0) return Result.Fail("Selecione uma conta a receber válida.");
        if (origem is null) return Result.Fail("Selecione a origem do recebimento.");
        if (!origem.Disponivel) return Result.Fail(origem.MotivoIndisponibilidade ?? "A origem selecionada não está disponível.");
        if (data.Date > DateTime.Today.AddDays(1)) return Result.Fail("A data do recebimento não pode estar no futuro.");

        try
        {
            await _gateway.ReceiveAsync(contaId, origem.Tipo, origem.Id, data, cancellationToken);
            return Result.Ok("Recebimento confirmado e lançado na Tesouraria.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, "Não foi possível confirmar o recebimento.");
        }
    }

    public async Task<Result> CancelAsync(int contaId, CancellationToken cancellationToken = default)
    {
        if (contaId <= 0) return Result.Fail("Selecione uma conta a receber válida.");
        try
        {
            await _gateway.CancelAsync(contaId, cancellationToken);
            return Result.Ok("Conta a receber cancelada.");
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, "Não foi possível cancelar a conta a receber.");
        }
    }

    private static IReadOnlyList<string> Validate(NovaContaReceberDto request)
    {
        var errors = new List<string>();
        if (request.EmpresaId <= 0) errors.Add("A empresa é obrigatória.");
        if (string.IsNullOrWhiteSpace(request.Descricao)) errors.Add("Indique uma descrição.");
        if (request.Valor <= 0) errors.Add("O valor deve ser superior a zero.");
        if (request.DataVencimento.Date < request.DataEmissao.Date) errors.Add("O vencimento não pode ser anterior à emissão.");
        return errors;
    }

    private static IEnumerable<ContaReceberListItemDto> ApplyFilter(IEnumerable<ContaReceberListItemDto> source, ReceivablesQuery query)
    {
        var result = source;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            result = result.Where(x =>
                x.Codigo.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Descricao.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (x.ClienteNome?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            result = result.Where(x => x.EstadoExibicao.Equals(query.Status, StringComparison.OrdinalIgnoreCase));
        if (query.DueFrom.HasValue) result = result.Where(x => x.DataVencimento.Date >= query.DueFrom.Value.Date);
        if (query.DueTo.HasValue) result = result.Where(x => x.DataVencimento.Date <= query.DueTo.Value.Date);
        return result;
    }

    private static ReceivablesSummary BuildSummary(IEnumerable<ContaReceberListItemDto> items)
    {
        var list = items.ToList();
        var open = list.Where(x => x.EstadoExibicao is "Pendente" or "Atrasado").ToList();
        var overdue = list.Where(x => x.EstadoExibicao == "Atrasado").ToList();
        var today = list.Where(x => x.PodeReceber && x.DataVencimento.Date == DateTime.Today).ToList();
        var received = list.Where(x => x.EstadoExibicao == "Recebido").ToList();
        return new ReceivablesSummary(open.Sum(x => x.Valor), overdue.Sum(x => x.Valor), today.Sum(x => x.Valor), received.Sum(x => x.Valor), open.Count, overdue.Count);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
