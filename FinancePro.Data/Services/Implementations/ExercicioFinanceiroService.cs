using FinancePro.Core.DTOs;
using FinancePro.Core.Entities;
using FinancePro.Data.Context;
using FinancePro.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Services.Implementations;

public sealed class ExercicioFinanceiroService : IExercicioFinanceiroService
{
    private readonly FinanceProDbContext _context;
    public ExercicioFinanceiroService(FinanceProDbContext context) => _context = context;

    public async Task<IReadOnlyList<ExercicioFinanceiroDto>> ListarAsync(string? pesquisa = null)
    {
        var query = _context.ExerciciosFinanceiros.AsNoTracking().Include(x => x.Empresa).AsQueryable();
        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var termo = pesquisa.Trim();
            query = query.Where(x => x.Ano.ToString().Contains(termo) || x.Empresa.Nome.Contains(termo));
        }

        return await query.OrderByDescending(x => x.Ano).ThenBy(x => x.Empresa.Nome)
            .Select(x => new ExercicioFinanceiroDto
            {
                Id = x.Id, EmpresaId = x.EmpresaId, EmpresaNome = x.Empresa.Nome,
                Ano = x.Ano, DataInicio = x.DataInicio, DataFim = x.DataFim,
                Padrao = x.Padrao, Encerrado = x.Encerrado, Ativo = x.Ativo
            }).ToListAsync();
    }

    public async Task<int> GuardarAsync(ExercicioFinanceiroDto dto)
    {
        if (dto.EmpresaId <= 0) throw new InvalidOperationException("Selecione uma empresa.");
        if (dto.Ano < 2000 || dto.Ano > 2200) throw new InvalidOperationException("Informe um ano válido.");
        if (dto.DataFim < dto.DataInicio) throw new InvalidOperationException("A data final não pode ser anterior à inicial.");

        var duplicado = await _context.ExerciciosFinanceiros.AnyAsync(x => x.Id != dto.Id && x.EmpresaId == dto.EmpresaId && x.Ano == dto.Ano);
        if (duplicado) throw new InvalidOperationException("Já existe este exercício para a empresa selecionada.");

        ExercicioFinanceiro entity;
        if (dto.Id == 0)
        {
            entity = new ExercicioFinanceiro();
            _context.ExerciciosFinanceiros.Add(entity);
        }
        else entity = await _context.ExerciciosFinanceiros.FindAsync(dto.Id) ?? throw new InvalidOperationException("Exercício não encontrado.");

        if (dto.Padrao)
        {
            var outros = await _context.ExerciciosFinanceiros.Where(x => x.EmpresaId == dto.EmpresaId && x.Id != dto.Id && x.Padrao).ToListAsync();
            foreach (var item in outros) item.Padrao = false;
        }

        entity.EmpresaId = dto.EmpresaId;
        entity.Ano = dto.Ano;
        entity.DataInicio = dto.DataInicio;
        entity.DataFim = dto.DataFim;
        entity.Padrao = dto.Padrao;
        entity.Encerrado = dto.Encerrado;
        entity.Ativo = dto.Ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return entity.Id;
    }

    public async Task AlternarAtivoAsync(int id, bool ativo)
    {
        var entity = await _context.ExerciciosFinanceiros.FindAsync(id) ?? throw new InvalidOperationException("Exercício não encontrado.");
        entity.Ativo = ativo;
        entity.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
    public async Task<FechoAnualPreviewDto> ObterPreviewFechoAsync(int id)
    {
        var exercicio = await _context.ExerciciosFinanceiros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("Exercício não encontrado.");
        var checks = new List<FechoAnualCheckDto>();
        var cn = _context.Database.GetDbConnection();
        var fechar = cn.State != System.Data.ConnectionState.Open;
        if (fechar) await cn.OpenAsync();
        try
        {
            async Task<int> CountAsync(string sql)
            {
                await using var cmd = cn.CreateCommand(); cmd.CommandText = sql;
                var p1=cmd.CreateParameter();p1.ParameterName="@empresa";p1.Value=exercicio.EmpresaId;cmd.Parameters.Add(p1);
                var p2=cmd.CreateParameter();p2.ParameterName="@ano";p2.Value=exercicio.Ano;cmd.Parameters.Add(p2);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            var periodos = await CountAsync("SELECT COUNT(*) FROM dbo.AccountingPeriods WHERE CompanyId=@empresa AND FiscalYear=@ano");
            checks.Add(new("PERIODOS_12","Os 12 períodos contabilísticos devem existir",true,periodos == 12 ? 0 : Math.Abs(12-periodos),$"Períodos encontrados: {periodos}/12."));
            var abertos = await CountAsync("SELECT COUNT(*) FROM dbo.AccountingPeriods WHERE CompanyId=@empresa AND FiscalYear=@ano AND Status<>'Fechado'");
            checks.Add(new("PERIODOS_FECHADOS","Todos os períodos mensais devem estar fechados",true,abertos,abertos==0?"Todos os períodos estão fechados.":$"{abertos} período(s) ainda não estão fechados."));
            var rascunhos = await CountAsync("SELECT COUNT(*) FROM dbo.AccountingEntries WHERE CompanyId=@empresa AND YEAR(EntryDate)=@ano AND Status='Rascunho'");
            checks.Add(new("RASCUNHOS","Não podem existir lançamentos em rascunho",true,rascunhos,rascunhos==0?"Sem rascunhos pendentes.":$"{rascunhos} lançamento(s) em rascunho."));
            var desequilibrados = await CountAsync(@"SELECT COUNT(*) FROM (SELECT e.Id FROM dbo.AccountingEntries e JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=e.Id WHERE e.CompanyId=@empresa AND YEAR(e.EntryDate)=@ano AND e.Status='Contabilizado' GROUP BY e.Id HAVING SUM(l.Debit)<>SUM(l.Credit)) q");
            checks.Add(new("EQUILIBRIO","Lançamentos contabilizados devem estar equilibrados",true,desequilibrados,desequilibrados==0?"Todos os lançamentos estão equilibrados.":$"{desequilibrados} lançamento(s) desequilibrado(s)."));
            var configuracao = await CountAsync("SELECT COUNT(*) FROM dbo.FiscalYearClosingSettings WHERE CompanyId=@empresa");
            checks.Add(new("CONFIGURACAO_FECHO","Contas de Resultado e Resultados Transitados devem estar configuradas",true,configuracao==1?0:1,configuracao==1?"Configuração de fecho definida.":"Configure as contas de fecho anual."));
            var semClassificacao = await CountAsync(@"SELECT COUNT(DISTINCT l.AccountId) FROM dbo.AccountingEntries e JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=e.Id JOIN dbo.EnterpriseChartAccounts a ON a.Id=l.AccountId LEFT JOIN dbo.AccountClosingClassifications c ON c.CompanyId=e.CompanyId AND c.AccountId=l.AccountId WHERE e.CompanyId=@empresa AND YEAR(e.EntryDate)=@ano AND e.Status='Contabilizado' AND c.AccountId IS NULL");
            checks.Add(new("CLASSIFICACAO_CONTAS","Todas as contas movimentadas devem possuir classe contabilística",true,semClassificacao,semClassificacao==0?"Todas as contas movimentadas estão classificadas.":$"{semClassificacao} conta(s) movimentada(s) sem classificação."));
            var processado = await CountAsync("SELECT COUNT(*) FROM dbo.FiscalYearBalanceCarryForwards WHERE CompanyId=@empresa AND SourceFiscalYear=@ano");
            checks.Add(new("FECHO_DUPLICADO","O transporte de saldos não pode ser executado duas vezes",true,processado,processado==0?"Nenhum transporte anterior encontrado.":"Este exercício já possui apuração/transporte de saldos."));
        }
        finally { if (fechar) await cn.CloseAsync(); }
        return new(exercicio.Id, exercicio.EmpresaId, exercicio.Ano, exercicio.Encerrado, checks);
    }

    public async Task EncerrarAsync(int id, int utilizadorId, string utilizadorNome)
    {
        if (utilizadorId<=0 || string.IsNullOrWhiteSpace(utilizadorNome)) throw new InvalidOperationException("Utilizador responsável obrigatório.");
        var preview=await ObterPreviewFechoAsync(id);
        if (!preview.PodeEncerrar) throw new InvalidOperationException($"O exercício não pode ser encerrado. Pendências bloqueantes: {preview.PendenciasBloqueantes}.");
        var e=await _context.ExerciciosFinanceiros.FindAsync(id) ?? throw new InvalidOperationException("Exercício não encontrado.");
        await using var tx=await _context.Database.BeginTransactionAsync();
        try
        {
            var fechamento = new DateTime(e.Ano,12,31); var abertura = new DateTime(e.Ano+1,1,1);
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
SET NOCOUNT ON;
IF EXISTS(SELECT 1 FROM dbo.FiscalYearBalanceCarryForwards WHERE CompanyId={e.EmpresaId} AND SourceFiscalYear={e.Ano}) THROW 51001,'O fecho anual já foi processado.',1;
DECLARE @result INT,@retained INT,@closeId INT=NULL,@openId INT=NULL,@net DECIMAL(18,2);
SELECT @result=ResultAccountId,@retained=RetainedEarningsAccountId FROM dbo.FiscalYearClosingSettings WHERE CompanyId={e.EmpresaId};
IF @result IS NULL OR @retained IS NULL THROW 51002,'Configure as contas de Resultado e Resultados Transitados.',1;
SELECT @net=COALESCE(SUM(x.Balance),0) FROM (SELECT l.AccountId,SUM(l.Debit-l.Credit) Balance FROM dbo.AccountingEntries ae JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=ae.Id JOIN dbo.AccountClosingClassifications c ON c.CompanyId=ae.CompanyId AND c.AccountId=l.AccountId WHERE ae.CompanyId={e.EmpresaId} AND YEAR(ae.EntryDate)={e.Ano} AND ae.Status='Contabilizado' AND c.StatementClass IN ('Receita','Despesa') GROUP BY l.AccountId) x;
IF EXISTS(SELECT 1 FROM dbo.AccountingEntries ae JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=ae.Id JOIN dbo.AccountClosingClassifications c ON c.CompanyId=ae.CompanyId AND c.AccountId=l.AccountId WHERE ae.CompanyId={e.EmpresaId} AND YEAR(ae.EntryDate)={e.Ano} AND ae.Status='Contabilizado' AND c.StatementClass IN ('Receita','Despesa'))
BEGIN
 INSERT dbo.AccountingEntries(CompanyId,EntryDate,DocumentNumber,Reference,Description,SourceModule,Status,CreatedBy,CreatedByName,PostedBy,PostedByName,PostedAt,CreatedAt,UpdatedAt) VALUES({e.EmpresaId},{fechamento},{"FY-CLOSE-"+e.Ano},{"FECHO-"+e.Ano},{"Apuração e encerramento do exercício "+e.Ano},{"FechoAnual"},{"Contabilizado"},{utilizadorId},{utilizadorNome.Trim()},{utilizadorId},{utilizadorNome.Trim()},SYSUTCDATETIME(),SYSUTCDATETIME(),SYSUTCDATETIME()); SET @closeId=SCOPE_IDENTITY();
 ;WITH B AS (SELECT l.AccountId,SUM(l.Debit-l.Credit) Balance FROM dbo.AccountingEntries ae JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=ae.Id JOIN dbo.AccountClosingClassifications c ON c.CompanyId=ae.CompanyId AND c.AccountId=l.AccountId WHERE ae.CompanyId={e.EmpresaId} AND YEAR(ae.EntryDate)={e.Ano} AND ae.Status='Contabilizado' AND ae.Id<>@closeId AND c.StatementClass IN ('Receita','Despesa') GROUP BY l.AccountId HAVING SUM(l.Debit-l.Credit)<>0)
 INSERT dbo.AccountingEntryLines(AccountingEntryId,AccountId,Description,Debit,Credit) SELECT @closeId,AccountId,'Zeramento anual',CASE WHEN Balance<0 THEN -Balance ELSE 0 END,CASE WHEN Balance>0 THEN Balance ELSE 0 END FROM B;
 IF @net<>0 BEGIN INSERT dbo.AccountingEntryLines(AccountingEntryId,AccountId,Description,Debit,Credit) VALUES(@closeId,@result,'Apuração do resultado',CASE WHEN @net>0 THEN @net ELSE 0 END,CASE WHEN @net<0 THEN -@net ELSE 0 END); INSERT dbo.AccountingEntryLines(AccountingEntryId,AccountId,Description,Debit,Credit) VALUES(@closeId,@result,'Transferência do resultado',CASE WHEN @net<0 THEN -@net ELSE 0 END,CASE WHEN @net>0 THEN @net ELSE 0 END); INSERT dbo.AccountingEntryLines(AccountingEntryId,AccountId,Description,Debit,Credit) VALUES(@closeId,@retained,'Resultados transitados',CASE WHEN @net>0 THEN @net ELSE 0 END,CASE WHEN @net<0 THEN -@net ELSE 0 END); END
END;
IF EXISTS(SELECT 1 FROM dbo.AccountingEntries ae JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=ae.Id JOIN dbo.AccountClosingClassifications c ON c.CompanyId=ae.CompanyId AND c.AccountId=l.AccountId WHERE ae.CompanyId={e.EmpresaId} AND ae.EntryDate<={fechamento} AND ae.Status='Contabilizado' AND c.StatementClass IN ('Ativo','Passivo','PatrimonioLiquido'))
BEGIN
 INSERT dbo.AccountingEntries(CompanyId,EntryDate,DocumentNumber,Reference,Description,SourceModule,Status,CreatedBy,CreatedByName,PostedBy,PostedByName,PostedAt,CreatedAt,UpdatedAt) VALUES({e.EmpresaId},{abertura},{"FY-OPEN-"+(e.Ano+1)},{"ABERTURA-"+(e.Ano+1)},{"Saldos de abertura do exercício "+(e.Ano+1)},{"FechoAnual"},{"Contabilizado"},{utilizadorId},{utilizadorNome.Trim()},{utilizadorId},{utilizadorNome.Trim()},SYSUTCDATETIME(),SYSUTCDATETIME(),SYSUTCDATETIME()); SET @openId=SCOPE_IDENTITY();
 ;WITH B AS (SELECT l.AccountId,SUM(l.Debit-l.Credit) Balance FROM dbo.AccountingEntries ae JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=ae.Id JOIN dbo.AccountClosingClassifications c ON c.CompanyId=ae.CompanyId AND c.AccountId=l.AccountId WHERE ae.CompanyId={e.EmpresaId} AND ae.EntryDate<={fechamento} AND ae.Status='Contabilizado' AND c.StatementClass IN ('Ativo','Passivo','PatrimonioLiquido') GROUP BY l.AccountId HAVING SUM(l.Debit-l.Credit)<>0)
 INSERT dbo.AccountingEntryLines(AccountingEntryId,AccountId,Description,Debit,Credit) SELECT @openId,AccountId,'Saldo de abertura',CASE WHEN Balance>0 THEN Balance ELSE 0 END,CASE WHEN Balance<0 THEN -Balance ELSE 0 END FROM B;
END;
INSERT dbo.FiscalYearBalanceCarryForwards(CompanyId,SourceFiscalYear,TargetFiscalYear,ClosingEntryId,OpeningEntryId,CreatedAt) VALUES({e.EmpresaId},{e.Ano},{e.Ano+1},@closeId,@openId,SYSUTCDATETIME());");
            e.Encerrado=true;e.Padrao=false;e.DataAtualizacao=DateTime.UtcNow;await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO dbo.FiscalYearClosingHistory(CompanyId,FiscalYearId,FiscalYear,Operation,UserId,UserName,Reason,CreatedAt) VALUES({e.EmpresaId},{e.Id},{e.Ano},{"Fecho"},{utilizadorId},{utilizadorNome.Trim()},{"Apuração e transporte de saldos concluídos"},SYSUTCDATETIME())");
            var seguinte=await _context.ExerciciosFinanceiros.FirstOrDefaultAsync(x=>x.EmpresaId==e.EmpresaId && x.Ano==e.Ano+1);
            if(seguinte is null){seguinte=new ExercicioFinanceiro{EmpresaId=e.EmpresaId,Ano=e.Ano+1,DataInicio=abertura,DataFim=new DateTime(e.Ano+1,12,31),Padrao=true,Encerrado=false,Ativo=true};_context.ExerciciosFinanceiros.Add(seguinte);}else{seguinte.Padrao=true;seguinte.Encerrado=false;seguinte.Ativo=true;}
            foreach(var outro in await _context.ExerciciosFinanceiros.Where(x=>x.EmpresaId==e.EmpresaId && x.Id!=seguinte.Id && x.Padrao).ToListAsync()) outro.Padrao=false;
            await _context.SaveChangesAsync();await tx.CommitAsync();
        }catch{await tx.RollbackAsync();throw;}
    }

    public async Task ReabrirAsync(int id,int utilizadorId,string utilizadorNome,string motivo)
    {
        if(utilizadorId<=0||string.IsNullOrWhiteSpace(utilizadorNome))throw new InvalidOperationException("Utilizador responsável obrigatório.");
        if(string.IsNullOrWhiteSpace(motivo)||motivo.Trim().Length<5)throw new InvalidOperationException("Informe um motivo de reabertura com pelo menos 5 caracteres.");
        var e=await _context.ExerciciosFinanceiros.FindAsync(id)??throw new InvalidOperationException("Exercício não encontrado.");
        if(!e.Encerrado)throw new InvalidOperationException("Apenas exercícios encerrados podem ser reabertos.");
        await using var tx=await _context.Database.BeginTransactionAsync();
        try{
        await _context.Database.ExecuteSqlInterpolatedAsync($@"DECLARE @c INT,@o INT; SELECT @c=ClosingEntryId,@o=OpeningEntryId FROM dbo.FiscalYearBalanceCarryForwards WHERE CompanyId={e.EmpresaId} AND SourceFiscalYear={e.Ano}; DELETE dbo.FiscalYearBalanceCarryForwards WHERE CompanyId={e.EmpresaId} AND SourceFiscalYear={e.Ano}; IF @o IS NOT NULL DELETE dbo.AccountingEntries WHERE Id=@o AND CompanyId={e.EmpresaId} AND SourceModule='FechoAnual'; IF @c IS NOT NULL DELETE dbo.AccountingEntries WHERE Id=@c AND CompanyId={e.EmpresaId} AND SourceModule='FechoAnual';");
        e.Encerrado=false;e.DataAtualizacao=DateTime.UtcNow;await _context.SaveChangesAsync();
        await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO dbo.FiscalYearClosingHistory(CompanyId,FiscalYearId,FiscalYear,Operation,UserId,UserName,Reason,CreatedAt) VALUES({e.EmpresaId},{e.Id},{e.Ano},{"Reabertura"},{utilizadorId},{utilizadorNome.Trim()},{motivo.Trim()},SYSUTCDATETIME())");
        await tx.CommitAsync();}catch{await tx.RollbackAsync();throw;}
    }

    public async Task<IReadOnlyList<FechoAnualHistoricoDto>> ObterHistoricoFechoAsync(int id)
    {
        var e=await _context.ExerciciosFinanceiros.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id)??throw new InvalidOperationException("Exercício não encontrado.");
        var list=new List<FechoAnualHistoricoDto>();var cn=_context.Database.GetDbConnection();var fechar=cn.State!=System.Data.ConnectionState.Open;if(fechar)await cn.OpenAsync();
        try{await using var cmd=cn.CreateCommand();cmd.CommandText="SELECT Id,FiscalYearId,Operation,UserId,UserName,Reason,CreatedAt FROM dbo.FiscalYearClosingHistory WHERE CompanyId=@c AND FiscalYearId=@id ORDER BY CreatedAt DESC,Id DESC";var a=cmd.CreateParameter();a.ParameterName="@c";a.Value=e.EmpresaId;cmd.Parameters.Add(a);var b=cmd.CreateParameter();b.ParameterName="@id";b.Value=id;cmd.Parameters.Add(b);await using var rd=await cmd.ExecuteReaderAsync();while(await rd.ReadAsync())list.Add(new(rd.GetInt32(0),rd.GetInt32(1),rd.GetString(2),rd.GetInt32(3),rd.GetString(4),rd.GetString(5),rd.GetDateTime(6)));return list;}finally{if(fechar)await cn.CloseAsync();}
    }

    public async Task<IReadOnlyList<ContaFechoAnualDto>> ListarContasFechoAsync(int empresaId)
    {
        var list=new List<ContaFechoAnualDto>(); var cn=_context.Database.GetDbConnection();var fechar=cn.State!=System.Data.ConnectionState.Open;if(fechar)await cn.OpenAsync();
        try{await using var cmd=cn.CreateCommand();cmd.CommandText="SELECT a.Id,a.Code,a.Name,a.Nature,COALESCE(c.StatementClass,'') FROM dbo.EnterpriseChartAccounts a LEFT JOIN dbo.AccountClosingClassifications c ON c.CompanyId=a.CompanyId AND c.AccountId=a.Id WHERE a.CompanyId=@c AND a.Active=1 AND a.AllowsPosting=1 ORDER BY a.Code";var p=cmd.CreateParameter();p.ParameterName="@c";p.Value=empresaId;cmd.Parameters.Add(p);await using var rd=await cmd.ExecuteReaderAsync();while(await rd.ReadAsync())list.Add(new(rd.GetInt32(0),rd.GetString(1),rd.GetString(2),rd.GetString(3),rd.GetString(4)));return list;}finally{if(fechar)await cn.CloseAsync();}
    }
    public async Task<ConfiguracaoFechoAnualDto> ObterConfiguracaoFechoAsync(int empresaId)
    {
        var cn=_context.Database.GetDbConnection();var fechar=cn.State!=System.Data.ConnectionState.Open;if(fechar)await cn.OpenAsync();try{await using var cmd=cn.CreateCommand();cmd.CommandText="SELECT ResultAccountId,RetainedEarningsAccountId FROM dbo.FiscalYearClosingSettings WHERE CompanyId=@c";var p=cmd.CreateParameter();p.ParameterName="@c";p.Value=empresaId;cmd.Parameters.Add(p);await using var rd=await cmd.ExecuteReaderAsync();return await rd.ReadAsync()?new(empresaId,rd.GetInt32(0),rd.GetInt32(1)):new(empresaId,null,null);}finally{if(fechar)await cn.CloseAsync();}
    }
    public async Task GuardarConfiguracaoFechoAsync(ConfiguracaoFechoAnualDto x)
    {
        if(x.ContaResultadoId is null||x.ContaResultadosTransitadosId is null||x.ContaResultadoId==x.ContaResultadosTransitadosId)throw new InvalidOperationException("Selecione duas contas de fecho distintas.");
        await _context.Database.ExecuteSqlInterpolatedAsync($@"MERGE dbo.FiscalYearClosingSettings AS t USING (SELECT {x.EmpresaId} CompanyId) s ON t.CompanyId=s.CompanyId WHEN MATCHED THEN UPDATE SET ResultAccountId={x.ContaResultadoId.Value},RetainedEarningsAccountId={x.ContaResultadosTransitadosId.Value},UpdatedAt=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(CompanyId,ResultAccountId,RetainedEarningsAccountId,UpdatedAt) VALUES({x.EmpresaId},{x.ContaResultadoId.Value},{x.ContaResultadosTransitadosId.Value},SYSUTCDATETIME()); MERGE dbo.AccountClosingClassifications AS t USING (SELECT {x.EmpresaId} CompanyId,{x.ContaResultadoId.Value} AccountId) s ON t.CompanyId=s.CompanyId AND t.AccountId=s.AccountId WHEN MATCHED THEN UPDATE SET StatementClass='PatrimonioLiquido',UpdatedAt=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(CompanyId,AccountId,StatementClass,UpdatedAt) VALUES({x.EmpresaId},{x.ContaResultadoId.Value},'PatrimonioLiquido',SYSUTCDATETIME()); MERGE dbo.AccountClosingClassifications AS t USING (SELECT {x.EmpresaId} CompanyId,{x.ContaResultadosTransitadosId.Value} AccountId) s ON t.CompanyId=s.CompanyId AND t.AccountId=s.AccountId WHEN MATCHED THEN UPDATE SET StatementClass='PatrimonioLiquido',UpdatedAt=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(CompanyId,AccountId,StatementClass,UpdatedAt) VALUES({x.EmpresaId},{x.ContaResultadosTransitadosId.Value},'PatrimonioLiquido',SYSUTCDATETIME());");
    }
    public async Task GuardarClassificacaoContaAsync(int empresaId,int contaId,string classificacao)
    {
        var validas=new[]{"Ativo","Passivo","PatrimonioLiquido","Receita","Despesa"};if(!validas.Contains(classificacao))throw new InvalidOperationException("Classificação contabilística inválida.");
        await _context.Database.ExecuteSqlInterpolatedAsync($@"MERGE dbo.AccountClosingClassifications AS t USING (SELECT {empresaId} CompanyId,{contaId} AccountId) s ON t.CompanyId=s.CompanyId AND t.AccountId=s.AccountId WHEN MATCHED THEN UPDATE SET StatementClass={classificacao},UpdatedAt=SYSUTCDATETIME() WHEN NOT MATCHED THEN INSERT(CompanyId,AccountId,StatementClass,UpdatedAt) VALUES({empresaId},{contaId},{classificacao},SYSUTCDATETIME());");
    }

}
