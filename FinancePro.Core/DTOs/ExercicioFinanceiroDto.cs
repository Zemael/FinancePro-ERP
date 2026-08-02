namespace FinancePro.Core.DTOs;

public sealed class ExercicioFinanceiroDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public int Ano { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public bool Padrao { get; set; }
    public bool Encerrado { get; set; }
    public bool Ativo { get; set; } = true;
}
