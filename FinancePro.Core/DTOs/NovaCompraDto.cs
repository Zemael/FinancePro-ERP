using FinancePro.Core.Enums;

namespace FinancePro.Core.DTOs;

public class NovaCompraDto
{
    public DateTime Data { get; set; } = DateTime.Today;
    public string? Departamento { get; set; }
    public string? CentroCusto { get; set; }
    public string? Projeto { get; set; }
    public string? Comprador { get; set; }
    public PrioridadeCompra Prioridade { get; set; } = PrioridadeCompra.Normal;
    public decimal ValorTotal { get; set; }
    public int? FornecedorId { get; set; }
    public int EmpresaId { get; set; }
}
