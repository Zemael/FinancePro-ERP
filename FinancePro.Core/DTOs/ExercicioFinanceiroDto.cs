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

public sealed record FechoAnualCheckDto(string Codigo, string Descricao, bool Bloqueante, int Quantidade, string Mensagem);

public sealed record FechoAnualPreviewDto(int ExercicioId, int EmpresaId, int Ano, bool Encerrado, IReadOnlyList<FechoAnualCheckDto> Verificacoes)
{
    public int PendenciasBloqueantes => Verificacoes.Count(x => x.Bloqueante && x.Quantidade > 0);
    public bool PodeEncerrar => !Encerrado && PendenciasBloqueantes == 0;
}

public sealed record FechoAnualHistoricoDto(int Id, int ExercicioId, string Operacao, int UtilizadorId, string UtilizadorNome, string Motivo, DateTime CriadoEm);

public sealed record ContaFechoAnualDto(int Id, string Codigo, string Nome, string Natureza, string Classificacao)
{
    public string Descricao => $"{Codigo} · {Nome}";
}
public sealed record ConfiguracaoFechoAnualDto(int EmpresaId, int? ContaResultadoId, int? ContaResultadosTransitadosId);
