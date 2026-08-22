namespace FinancePro.Core.DTOs;

public class CompraListItemDto
{
    public int Id { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public string? FornecedorNome { get; set; }
    public DateTime Data { get; set; }
    public string? Departamento { get; set; }
    public string? CentroCusto { get; set; }
    public string? Comprador { get; set; }
    public string Prioridade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public string? NumeroCotacao { get; set; }
    public string? NumeroOrdemCompra { get; set; }

    public bool PodeCotar { get; set; }
    public bool PodeAprovar { get; set; }
    public bool PodeRejeitar { get; set; }
    public bool PodeCancelar { get; set; }
    public bool PodeEmitirOrdem { get; set; }
    public bool PodeReceber { get; set; }
    public bool PodeFaturar { get; set; }
    public override string ToString() => string.IsNullOrWhiteSpace(FornecedorNome) ? NumeroPedido : $"{NumeroPedido} · {FornecedorNome}";
}
