namespace FinancePro.Core.DTOs;

public sealed class MoedaDto
{
    public int Id { get; set; }
    public string CodigoIso { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Simbolo { get; set; } = string.Empty;
    public int CasasDecimais { get; set; }
    public bool Ativo { get; set; } = true;
    public override string ToString() => string.IsNullOrWhiteSpace(CodigoIso) ? Nome : $"{CodigoIso} · {Nome}";
}
