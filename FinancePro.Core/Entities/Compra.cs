using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

/// <summary>Pedido de compra a um fornecedor (Módulo 07 — Compras).</summary>
public class Compra : EntityBase
{
    public string NumeroPedido { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public string? Departamento { get; set; }
    public string? CentroCusto { get; set; }
    public string? Projeto { get; set; }
    public string? Comprador { get; set; }
    public PrioridadeCompra Prioridade { get; set; } = PrioridadeCompra.Normal;
    public EstadoCompra Estado { get; set; } = EstadoCompra.Pendente;
    public decimal ValorTotal { get; set; }

    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
