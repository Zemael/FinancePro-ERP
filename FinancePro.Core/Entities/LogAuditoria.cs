namespace FinancePro.Core.Entities;

/// <summary>Registo genérico de auditoria: quem fez o quê, a que registo,
/// e quando — reutilizável por qualquer módulo.</summary>
public class LogAuditoria
{
    public int Id { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public string Entidade { get; set; } = string.Empty;
    public int RegistoId { get; set; }
    public string Acao { get; set; } = string.Empty; // Criar | Editar | Eliminar | Aprovar | Rejeitar
    public string? Detalhe { get; set; }

    public int UtilizadorId { get; set; }
    public string UtilizadorNome { get; set; } = string.Empty;

    public int EmpresaId { get; set; }
}
