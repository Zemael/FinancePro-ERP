namespace FinancePro.Core.Entities;

/// <summary>Cliente da empresa.</summary>
public class Cliente : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string? NIF { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Morada { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
