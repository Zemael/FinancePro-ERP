namespace FinancePro.Core.Entities;

/// <summary>
/// Campos comuns a todas as entidades: chave primária e auditoria básica.
/// </summary>
public abstract class EntityBase
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; } = true;
}
