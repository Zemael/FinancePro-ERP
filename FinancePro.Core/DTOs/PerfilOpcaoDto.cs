namespace FinancePro.Core.DTOs;

public class PerfilOpcaoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public override string ToString() => Nome;
}
