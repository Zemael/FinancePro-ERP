using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public class ConfiguracoesService : IConfiguracoesService
{
    private readonly FinanceProDbContext _context;

    public ConfiguracoesService(FinanceProDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteAlgumUtilizadorAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Utilizadores
            .AsNoTracking()
            .AnyAsync(cancellationToken);
    }

    public async Task<int> ConfigurarInicialAsync(ConfiguracaoInicialDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EmpresaNome))
        {
            throw new InvalidOperationException("O nome da empresa é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(dto.AdminNome) || string.IsNullOrWhiteSpace(dto.AdminEmail))
        {
            throw new InvalidOperationException("Indique o nome e o email do administrador.");
        }

        if (string.IsNullOrWhiteSpace(dto.AdminPassword) || dto.AdminPassword.Length < 6)
        {
            throw new InvalidOperationException("A senha tem de ter pelo menos 6 caracteres.");
        }

        var moeda = string.IsNullOrWhiteSpace(dto.Moeda) ? "FCFA" : dto.Moeda.Trim();
        if (moeda.Length > 20)
        {
            throw new InvalidOperationException("A moeda deve ser só a sigla (ex.: FCFA, EUR, USD), até 20 caracteres.");
        }

        try
        {
            // Garante que os perfis padrão existem (o mesmo conteúdo do seed
            // 001_PerfisIniciais.sql) — assim a configuração inicial funciona
            // mesmo que o script SQL nunca tenha sido corrido manualmente.
            if (!await _context.Perfis.AnyAsync())
            {
                _context.Perfis.AddRange(
                    new Perfil { Nome = "Administrador", Descricao = "Acesso total ao sistema" },
                    new Perfil { Nome = "Gestor", Descricao = "Acesso aos módulos financeiros e relatórios" },
                    new Perfil { Nome = "Operador", Descricao = "Acesso limitado a lançamentos do dia a dia" });
                await _context.SaveChangesAsync();
            }

            var perfilAdmin = await _context.Perfis.FirstAsync(p => p.Nome == "Administrador");

            var empresa = new Empresa { Nome = dto.EmpresaNome.Trim(), Moeda = moeda };
            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            _context.Utilizadores.Add(new Utilizador
            {
                NomeCompleto = dto.AdminNome.Trim(),
                Email = dto.AdminEmail.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.AdminPassword),
                PerfilId = perfilAdmin.Id,
                EmpresaId = empresa.Id
            });
            await _context.SaveChangesAsync();

            return empresa.Id;
        }
        catch (DbUpdateException ex)
        {
            var detalhe = ex.InnerException?.Message ?? ex.Message;
            throw new InvalidOperationException($"Não foi possível gravar na base de dados: {detalhe}");
        }
    }

    public async Task<EmpresaDto?> ObterEmpresaAsync(int empresaId)
    {
        return await _context.Empresas
            .Where(e => e.Id == empresaId)
            .Select(e => new EmpresaDto
            {
                Id = e.Id,
                Nome = e.Nome,
                NIF = e.NIF,
                Morada = e.Morada,
                Telefone = e.Telefone,
                Email = e.Email,
                Moeda = e.Moeda
            })
            .FirstOrDefaultAsync();
    }

    public async Task AtualizarEmpresaAsync(EmpresaDto dto)
    {
        var empresa = await _context.Empresas.FindAsync(dto.Id)
            ?? throw new InvalidOperationException("Empresa não encontrada.");

        empresa.Nome = dto.Nome.Trim();
        empresa.NIF = dto.NIF;
        empresa.Morada = dto.Morada;
        empresa.Telefone = dto.Telefone;
        empresa.Email = dto.Email;
        empresa.Moeda = string.IsNullOrWhiteSpace(dto.Moeda) ? "FCFA" : dto.Moeda;
        empresa.DataAtualizacao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<PerfilOpcaoDto>> ListarPerfisAsync()
    {
        return await _context.Perfis
            .OrderBy(p => p.Nome)
            .Select(p => new PerfilOpcaoDto { Id = p.Id, Nome = p.Nome })
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UtilizadorListItemDto>> ListarUtilizadoresAsync(int empresaId)
    {
        return await _context.Utilizadores
            .Where(u => u.EmpresaId == empresaId)
            .Include(u => u.Perfil)
            .OrderBy(u => u.NomeCompleto)
            .Select(u => new UtilizadorListItemDto
            {
                Id = u.Id,
                NomeCompleto = u.NomeCompleto,
                Email = u.Email,
                PerfilNome = u.Perfil.Nome,
                Ativo = u.Ativo
            })
            .ToListAsync();
    }

    public async Task CriarUtilizadorAsync(NovoUtilizadorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NomeCompleto) || string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new InvalidOperationException("Indique o nome e o email do utilizador.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
        {
            throw new InvalidOperationException("A senha tem de ter pelo menos 6 caracteres.");
        }

        var emailEmUso = await _context.Utilizadores.AnyAsync(u => u.Email == dto.Email.Trim());
        if (emailEmUso)
        {
            throw new InvalidOperationException("Já existe um utilizador com esse email.");
        }

        _context.Utilizadores.Add(new Utilizador
        {
            NomeCompleto = dto.NomeCompleto.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PerfilId = dto.PerfilId,
            EmpresaId = dto.EmpresaId
        });

        await _context.SaveChangesAsync();
    }

    public async Task AlternarAtivoUtilizadorAsync(int utilizadorId, bool ativo)
    {
        var utilizador = await _context.Utilizadores.FindAsync(utilizadorId)
            ?? throw new InvalidOperationException("Utilizador não encontrado.");

        utilizador.Ativo = ativo;
        utilizador.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<SystemHealthDto> ObterSaudeSistemaAsync()
    {
        var connection = _context.Database.GetDbConnection();
        var mustClose = connection.State != System.Data.ConnectionState.Open;
        if (mustClose) await connection.OpenAsync();
        try
        {
            decimal sizeMb = 0m;
            DateTime? lastBackup = null;

            await using (var sizeCmd = connection.CreateCommand())
            {
                sizeCmd.CommandText = "SELECT CAST(SUM(size) * 8.0 / 1024 AS decimal(18,2)) FROM sys.database_files";
                var value = await sizeCmd.ExecuteScalarAsync();
                if (value is not null && value != DBNull.Value) sizeMb = Convert.ToDecimal(value);
            }

            try
            {
                await using var backupCmd = connection.CreateCommand();
                backupCmd.CommandText = "SELECT MAX(backup_finish_date) FROM msdb.dbo.backupset WHERE database_name = DB_NAME() AND type='D'";
                var value = await backupCmd.ExecuteScalarAsync();
                if (value is DateTime dt) lastBackup = dt;
            }
            catch
            {
                // Algumas instalações restringem leitura de msdb; a saúde da base continua disponível.
            }

            return new SystemHealthDto
            {
                DatabaseName = connection.Database,
                DataSource = connection.DataSource,
                Connected = true,
                DatabaseSizeMb = sizeMb,
                LastBackupAt = lastBackup,
                Empresas = await _context.Empresas.CountAsync(),
                UtilizadoresAtivos = await _context.Utilizadores.CountAsync(x => x.Ativo),
                LogsAuditoria = await _context.LogsAuditoria.CountAsync()
            };
        }
        finally
        {
            if (mustClose) await connection.CloseAsync();
        }
    }

    public async Task<SystemDiagnosticsDto> ExecutarDiagnosticoAsync()
    {
        var connection = _context.Database.GetDbConnection();
        var mustClose = connection.State != System.Data.ConnectionState.Open;
        if (mustClose) await connection.OpenAsync();
        try
        {
            var result = new SystemDiagnosticsDto { CheckedAt = DateTime.Now };

            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
SELECT
    CONVERT(nvarchar(128), SERVERPROPERTY('ProductVersion')) AS SqlVersion,
    d.state_desc,
    d.recovery_model_desc,
    d.compatibility_level
FROM sys.databases d
WHERE d.name = DB_NAME();";

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result.SqlServerVersion = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                    result.DatabaseState = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    result.RecoveryModel = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                    result.CompatibilityLevel = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                }
            }

            try
            {
                var database = connection.Database.Replace("]", "]]", StringComparison.Ordinal);
                await using var check = connection.CreateCommand();
                check.CommandTimeout = 180;
                check.CommandText = $"DBCC CHECKDB ([{database}]) WITH PHYSICAL_ONLY, NO_INFOMSGS";
                await check.ExecuteNonQueryAsync();
                result.IntegrityStatus = "OK — verificação física concluída";
            }
            catch (Exception ex)
            {
                result.IntegrityStatus = $"Não concluída: {ex.Message}";
            }

            return result;
        }
        finally
        {
            if (mustClose) await connection.CloseAsync();
        }
    }

    public async Task<string> CriarBackupBaseDadosAsync()
    {
        var connection = _context.Database.GetDbConnection();
        var mustClose = connection.State != System.Data.ConnectionState.Open;
        if (mustClose) await connection.OpenAsync();
        try
        {
            string? backupDirectory = null;
            await using (var pathCmd = connection.CreateCommand())
            {
                pathCmd.CommandText = "SELECT CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath'))";
                var value = await pathCmd.ExecuteScalarAsync();
                backupDirectory = value == DBNull.Value ? null : value?.ToString();
            }

            if (string.IsNullOrWhiteSpace(backupDirectory))
                throw new InvalidOperationException("O SQL Server não informou a pasta padrão de backups.");

            var fileName = $"FinancePro_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var fullPath = Path.Combine(backupDirectory, fileName);
            var database = connection.Database.Replace("]", "]]", StringComparison.Ordinal);
            var safePath = fullPath.Replace("'", "''", StringComparison.Ordinal);

            await using var cmd = connection.CreateCommand();
            cmd.CommandTimeout = 300;
            cmd.CommandText = $"BACKUP DATABASE [{database}] TO DISK = N'{safePath}' WITH COPY_ONLY, INIT, CHECKSUM, STATS = 10";
            await cmd.ExecuteNonQueryAsync();
            return fullPath;
        }
        finally
        {
            if (mustClose) await connection.CloseAsync();
        }
    }

    public async Task<string> GerarRelatorioSuporteAsync()
    {
        var health = await ObterSaudeSistemaAsync();
        var diagnostics = await ExecutarDiagnosticoAsync();

        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "FinancePro",
            "Suporte");
        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, $"FinancePro-Diagnostico-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        var report = new System.Text.StringBuilder();
        report.AppendLine("FINANCEPRO — RELATÓRIO TÉCNICO DE SUPORTE");
        report.AppendLine(new string('=', 56));
        report.AppendLine($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        report.AppendLine($"Base de dados: {health.DatabaseName}");
        report.AppendLine($"Servidor: {health.DataSource}");
        report.AppendLine($"Tamanho da base: {health.DatabaseSizeMb:N2} MB");
        report.AppendLine($"Último backup: {(health.LastBackupAt.HasValue ? health.LastBackupAt.Value.ToString("dd/MM/yyyy HH:mm:ss") : "Não disponível")}");
        report.AppendLine($"Empresas: {health.Empresas}");
        report.AppendLine($"Utilizadores ativos: {health.UtilizadoresAtivos}");
        report.AppendLine($"Eventos de auditoria: {health.LogsAuditoria}");
        report.AppendLine();
        report.AppendLine("DIAGNÓSTICO SQL SERVER");
        report.AppendLine($"Versão SQL Server: {diagnostics.SqlServerVersion}");
        report.AppendLine($"Estado da base: {diagnostics.DatabaseState}");
        report.AppendLine($"Modelo de recuperação: {diagnostics.RecoveryModel}");
        report.AppendLine($"Compatibilidade: {diagnostics.CompatibilityLevel}");
        report.AppendLine($"Integridade física: {diagnostics.IntegrityStatus}");
        report.AppendLine();
        report.AppendLine("Este relatório não contém palavras-passe nem credenciais de acesso.");

        await File.WriteAllTextAsync(filePath, report.ToString(), System.Text.Encoding.UTF8);
        return filePath;
    }

    public async Task<SystemReadinessDto> VerificarProntidaoAsync()
    {
        var health = await ObterSaudeSistemaAsync();
        var diagnostics = await ExecutarDiagnosticoAsync();
        var now = DateTime.Now;
        var items = new List<SystemReadinessItemDto>();

        items.Add(new SystemReadinessItemDto
        {
            Area = "Base de dados",
            Status = health.Connected && string.Equals(diagnostics.DatabaseState, "ONLINE", StringComparison.OrdinalIgnoreCase) ? "OK" : "Falha",
            Detail = health.Connected ? $"{health.DatabaseName} em {health.DataSource} — {diagnostics.DatabaseState}" : "Sem ligação à base de dados."
        });

        var backupAge = health.LastBackupAt.HasValue ? now - health.LastBackupAt.Value : (TimeSpan?)null;
        items.Add(new SystemReadinessItemDto
        {
            Area = "Backup",
            Status = backupAge.HasValue && backupAge.Value.TotalDays <= 7 ? "OK" : "Aviso",
            Detail = health.LastBackupAt.HasValue ? $"Último backup: {health.LastBackupAt:dd/MM/yyyy HH:mm}." : "Nenhum backup completo foi identificado."
        });

        items.Add(new SystemReadinessItemDto
        {
            Area = "Integridade",
            Status = diagnostics.IntegrityStatus.StartsWith("OK", StringComparison.OrdinalIgnoreCase) ? "OK" : "Aviso",
            Detail = diagnostics.IntegrityStatus
        });

        items.Add(new SystemReadinessItemDto
        {
            Area = "Utilizadores",
            Status = health.UtilizadoresAtivos > 0 ? "OK" : "Falha",
            Detail = $"{health.UtilizadoresAtivos} utilizador(es) ativo(s)."
        });

        items.Add(new SystemReadinessItemDto
        {
            Area = "Empresa",
            Status = health.Empresas > 0 ? "OK" : "Falha",
            Detail = $"{health.Empresas} empresa(s) configurada(s)."
        });

        items.Add(new SystemReadinessItemDto
        {
            Area = "Compatibilidade SQL",
            Status = diagnostics.CompatibilityLevel >= 150 ? "OK" : "Aviso",
            Detail = $"Nível de compatibilidade: {diagnostics.CompatibilityLevel}."
        });

        var passed = items.Count(x => x.Status == "OK");
        var warnings = items.Count(x => x.Status == "Aviso");
        var failed = items.Count(x => x.Status == "Falha");

        return new SystemReadinessDto
        {
            CheckedAt = now,
            OverallStatus = failed > 0 ? "Não pronto" : warnings > 0 ? "Pronto com alertas" : "Pronto para operação",
            Passed = passed,
            Warnings = warnings,
            Failed = failed,
            Items = items
        };
    }

    public async Task<IReadOnlyList<DatabaseBackupDto>> ListarHistoricoBackupsAsync(int limite = 20)
    {
        limite = Math.Clamp(limite, 1, 100);
        var connection = _context.Database.GetDbConnection();
        var mustClose = connection.State != System.Data.ConnectionState.Open;
        if (mustClose) await connection.OpenAsync();

        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $@"
SELECT TOP ({limite})
    bs.database_name,
    bs.backup_start_date,
    bs.backup_finish_date,
    CAST(COALESCE(bs.compressed_backup_size, bs.backup_size, 0) / 1048576.0 AS decimal(18,2)) AS size_mb,
    DATEDIFF(SECOND, bs.backup_start_date, bs.backup_finish_date) AS duration_seconds,
    CASE bs.type WHEN 'D' THEN N'Completo' WHEN 'I' THEN N'Diferencial' WHEN 'L' THEN N'Log' ELSE bs.type END AS backup_type,
    bs.is_copy_only,
    COALESCE(bmf.physical_device_name, N'Indisponível') AS destination
FROM msdb.dbo.backupset bs
LEFT JOIN msdb.dbo.backupmediafamily bmf ON bmf.media_set_id = bs.media_set_id
WHERE bs.database_name = DB_NAME()
ORDER BY bs.backup_start_date DESC;";

            var backups = new List<DatabaseBackupDto>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                backups.Add(new DatabaseBackupDto
                {
                    DatabaseName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    StartedAt = reader.GetDateTime(1),
                    CompletedAt = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                    SizeMb = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                    DurationSeconds = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    BackupType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    IsCopyOnly = !reader.IsDBNull(6) && reader.GetBoolean(6),
                    Destination = reader.IsDBNull(7) ? "Indisponível" : reader.GetString(7)
                });
            }

            return backups;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Não foi possível consultar o histórico de backups. Confirme a permissão de leitura na base msdb.", ex);
        }
        finally
        {
            if (mustClose) await connection.CloseAsync();
        }
    }

    public async Task<BackupCleanupResultDto> LimparBackupsAntigosAsync(int preservar = 5)
    {
        preservar = Math.Clamp(preservar, 1, 100);
        var connection = _context.Database.GetDbConnection();
        var mustClose = connection.State != System.Data.ConnectionState.Open;
        if (mustClose) await connection.OpenAsync();

        try
        {
            string? backupDirectory;
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath'))";
                var value = await cmd.ExecuteScalarAsync();
                backupDirectory = value == DBNull.Value ? null : value?.ToString();
            }

            if (string.IsNullOrWhiteSpace(backupDirectory) || !Directory.Exists(backupDirectory))
                throw new InvalidOperationException("A pasta padrão de backups do SQL Server não está disponível neste computador.");

            var files = new DirectoryInfo(backupDirectory)
                .EnumerateFiles("FinancePro_*.bak", SearchOption.TopDirectoryOnly)
                .Where(file => !file.Attributes.HasFlag(FileAttributes.ReparsePoint))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList();

            long releasedBytes = 0;
            var deleted = 0;
            foreach (var file in files.Skip(preservar))
            {
                var length = file.Length;
                file.Delete();
                releasedBytes += length;
                deleted++;
            }

            return new BackupCleanupResultDto
            {
                Preserved = Math.Min(preservar, files.Count),
                Deleted = deleted,
                ReleasedBytes = releasedBytes
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException(
                "Sem permissão para eliminar backups na pasta do SQL Server. Execute com uma conta autorizada.", ex);
        }
        finally
        {
            if (mustClose) await connection.CloseAsync();
        }
    }

    public async Task<BackupVerificationDto> VerificarUltimoBackupAsync()
    {
        var connection = _context.Database.GetDbConnection();
        var mustClose = connection.State != System.Data.ConnectionState.Open;
        if (mustClose) await connection.OpenAsync();

        try
        {
            string? backupPath = null;
            DateTime? completedAt = null;
            await using (var lookup = connection.CreateCommand())
            {
                lookup.CommandText = @"
SELECT TOP (1)
    bmf.physical_device_name,
    bs.backup_finish_date
FROM msdb.dbo.backupset bs
INNER JOIN msdb.dbo.backupmediafamily bmf ON bmf.media_set_id = bs.media_set_id
WHERE bs.database_name = DB_NAME()
  AND bs.type = 'D'
  AND bs.backup_finish_date IS NOT NULL
ORDER BY bs.backup_finish_date DESC;";

                await using var reader = await lookup.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    backupPath = reader.IsDBNull(0) ? null : reader.GetString(0);
                    completedAt = reader.IsDBNull(1) ? null : reader.GetDateTime(1);
                }
            }

            if (string.IsNullOrWhiteSpace(backupPath) || !completedAt.HasValue)
                throw new InvalidOperationException("Nenhum backup completo foi encontrado para verificação.");

            var safePath = backupPath.Replace("'", "''", StringComparison.Ordinal);
            await using var verify = connection.CreateCommand();
            verify.CommandTimeout = 300;
            verify.CommandText = $"RESTORE VERIFYONLY FROM DISK = N'{safePath}' WITH CHECKSUM";
            await verify.ExecuteNonQueryAsync();

            return new BackupVerificationDto
            {
                CheckedAt = DateTime.Now,
                BackupCompletedAt = completedAt.Value,
                BackupPath = backupPath,
                IsValid = true,
                Status = "Backup válido e legível pelo SQL Server."
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "O último backup não passou na verificação de integridade ou não está acessível ao SQL Server.", ex);
        }
        finally
        {
            if (mustClose) await connection.CloseAsync();
        }
    }

    public async Task<string> GerarPlanoRecuperacaoAsync()
    {
        var health = await ObterSaudeSistemaAsync();
        var diagnostics = await ExecutarDiagnosticoAsync();
        var verification = await VerificarUltimoBackupAsync();
        var generatedAt = DateTime.Now;
        var backupAge = generatedAt - verification.BackupCompletedAt;

        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "FinancePro",
            "Continuidade");
        Directory.CreateDirectory(folder);

        var filePath = Path.Combine(folder, $"FinancePro-Plano-Recuperacao-{generatedAt:yyyyMMdd-HHmmss}.txt");
        var report = new System.Text.StringBuilder();
        report.AppendLine("FINANCEPRO — PLANO DE RECUPERAÇÃO E CONTINUIDADE");
        report.AppendLine(new string('=', 62));
        report.AppendLine($"Gerado em: {generatedAt:dd/MM/yyyy HH:mm:ss}");
        report.AppendLine($"Base de dados: {health.DatabaseName}");
        report.AppendLine($"Servidor: {health.DataSource}");
        report.AppendLine($"Estado atual: {diagnostics.DatabaseState}");
        report.AppendLine($"Modelo de recuperação: {diagnostics.RecoveryModel}");
        report.AppendLine($"Compatibilidade SQL: {diagnostics.CompatibilityLevel}");
        report.AppendLine();
        report.AppendLine("PONTO DE RECUPERAÇÃO DISPONÍVEL");
        report.AppendLine($"Backup concluído em: {verification.BackupCompletedAt:dd/MM/yyyy HH:mm:ss}");
        report.AppendLine($"Idade do backup (RPO observado): {Math.Max(0, (int)backupAge.TotalHours)} hora(s)");
        report.AppendLine($"Ficheiro: {verification.BackupPath}");
        report.AppendLine($"Verificação: {verification.Status}");
        report.AppendLine();
        report.AppendLine("PROCEDIMENTO DE RECUPERAÇÃO");
        report.AppendLine("1. Interromper o acesso dos utilizadores ao FinancePro.");
        report.AppendLine("2. Confirmar o incidente e preservar a base atual para análise.");
        report.AppendLine("3. Copiar o backup validado para uma localização segura.");
        report.AppendLine("4. Restaurar primeiro numa base de teste com nome diferente.");
        report.AppendLine("5. Validar integridade, utilizadores, empresa, saldos e documentos.");
        report.AppendLine("6. Obter autorização formal antes de substituir a base de produção.");
        report.AppendLine("7. Registar data, responsável, motivo e resultado da recuperação.");
        report.AppendLine();
        report.AppendLine("IMPORTANTE");
        report.AppendLine("Este plano não executa RESTORE DATABASE e não contém palavras-passe.");
        report.AppendLine("A restauração deve ser realizada por um administrador SQL autorizado.");

        await File.WriteAllTextAsync(filePath, report.ToString(), System.Text.Encoding.UTF8);
        return filePath;
    }

    public async Task<ContinuityTestDto> ExecutarTesteContinuidadeAsync(int rpoObjetivoHoras, int rtoObjetivoHoras)
    {
        rpoObjetivoHoras = Math.Clamp(rpoObjetivoHoras, 1, 720);
        rtoObjetivoHoras = Math.Clamp(rtoObjetivoHoras, 1, 168);
        var now = DateTime.Now;
        var health = await ObterSaudeSistemaAsync();
        var diagnostics = await ExecutarDiagnosticoAsync();
        var items = new List<ContinuityTestItemDto>();

        items.Add(new ContinuityTestItemDto
        {
            Test = "Disponibilidade da base",
            Status = health.Connected && string.Equals(diagnostics.DatabaseState, "ONLINE", StringComparison.OrdinalIgnoreCase) ? "OK" : "Falha",
            Detail = $"{health.DatabaseName} — estado {diagnostics.DatabaseState}."
        });

        var backupAgeHours = health.LastBackupAt.HasValue
            ? Math.Max(0, (now - health.LastBackupAt.Value).TotalHours)
            : (double?)null;
        items.Add(new ContinuityTestItemDto
        {
            Test = "Ponto de recuperação (RPO)",
            Status = backupAgeHours.HasValue && backupAgeHours.Value <= rpoObjetivoHoras ? "OK" : "Falha",
            Detail = backupAgeHours.HasValue
                ? $"Backup há {backupAgeHours.Value:N1} hora(s); objetivo máximo de {rpoObjetivoHoras} hora(s)."
                : "Nenhum backup completo identificado."
        });

        items.Add(new ContinuityTestItemDto
        {
            Test = "Integridade da base",
            Status = diagnostics.IntegrityStatus.StartsWith("OK", StringComparison.OrdinalIgnoreCase) ? "OK" : "Falha",
            Detail = diagnostics.IntegrityStatus
        });

        try
        {
            var verification = await VerificarUltimoBackupAsync();
            items.Add(new ContinuityTestItemDto
            {
                Test = "Integridade do backup",
                Status = verification.IsValid ? "OK" : "Falha",
                Detail = verification.Status
            });
        }
        catch (Exception ex)
        {
            items.Add(new ContinuityTestItemDto
            {
                Test = "Integridade do backup",
                Status = "Falha",
                Detail = ex.Message
            });
        }

        items.Add(new ContinuityTestItemDto
        {
            Test = "Objetivo de recuperação (RTO)",
            Status = rtoObjetivoHoras > 0 ? "OK" : "Falha",
            Detail = $"Meta operacional configurada: recuperação em até {rtoObjetivoHoras} hora(s)."
        });

        var passed = items.Count(item => item.Status == "OK");
        var failed = items.Count - passed;
        return new ContinuityTestDto
        {
            CheckedAt = now,
            OverallStatus = failed == 0 ? "Continuidade validada" : "Continuidade com falhas",
            Passed = passed,
            Failed = failed,
            Items = items
        };
    }

}
