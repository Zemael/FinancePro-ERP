using System.Data;
using System.Data.Common;
using FinancePro.Data.Context;
using FinancePro.Platform.Investments;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Investments;

public sealed class SqlInvestmentService : IInvestmentService
{
    private readonly FinanceProDbContext _db;
    public SqlInvestmentService(FinanceProDbContext db) => _db = db;

    public async Task<IReadOnlyList<InvestmentPlanRow>> ListAsync(int companyId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT Id,Code,Name,Type,Department,Manager,StartDate,EndDate,Budget,ExecutedValue,ExpectedReturn,Status,COALESCE(Notes,''),PhysicalProgress,COALESCE(FundingSource,''),FinancedAmount,COALESCE(NextMilestone,''),NextMilestoneDate,RiskLevel,COALESCE(RiskDescription,''),COALESCE(MitigationPlan,''),TargetRoi,TargetCompletionDate FROM dbo.Investments WHERE CompanyId=@companyId AND Active=1 ORDER BY StartDate DESC,Code";
        var list = new List<InvestmentPlanRow>();
        var connection = _db.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = connection.CreateCommand(); cmd.CommandText = sql; Add(cmd, "@companyId", companyId);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                list.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetDateTime(6), reader.IsDBNull(7) ? null : reader.GetDateTime(7), reader.GetDecimal(8), reader.GetDecimal(9), reader.GetDecimal(10), reader.GetString(11), reader.GetString(12), reader.GetDecimal(13), reader.GetString(14), reader.GetDecimal(15), reader.GetString(16), reader.IsDBNull(17) ? null : reader.GetDateTime(17), reader.GetString(18), reader.GetString(19), reader.GetString(20), reader.GetDecimal(21), reader.IsDBNull(22) ? null : reader.GetDateTime(22)));
            return list;
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    public async Task SaveAsync(SaveInvestmentRequest x, CancellationToken cancellationToken = default)
    {
        if (x.CompanyId <= 0 || string.IsNullOrWhiteSpace(x.Code) || string.IsNullOrWhiteSpace(x.Name) || string.IsNullOrWhiteSpace(x.Type) || string.IsNullOrWhiteSpace(x.Department) || string.IsNullOrWhiteSpace(x.Manager))
            throw new ArgumentException("Código, nome, tipo, departamento e gestor são obrigatórios.");
        if (x.Budget < 0 || x.ExecutedValue < 0 || x.ExpectedReturn < 0) throw new ArgumentException("Os valores financeiros não podem ser negativos.");
        if (x.PhysicalProgress is < 0 or > 100) throw new ArgumentException("A execução física deve estar entre 0% e 100%.");
        if (x.FinancedAmount < 0 || x.FinancedAmount > x.Budget) throw new ArgumentException("O valor financiado deve estar entre zero e o orçamento.");
        if (x.TargetRoi is < -100 or > 10000) throw new ArgumentException("O ROI-alvo deve estar entre -100% e 10.000%.");
        if (x.TargetCompletionDate.HasValue && x.TargetCompletionDate.Value.Date < x.StartDate.Date) throw new ArgumentException("A data-meta não pode ser anterior ao início.");
        if (x.EndDate.HasValue && x.EndDate.Value.Date < x.StartDate.Date) throw new ArgumentException("A data final não pode ser anterior à data inicial.");
        const string sql = @"IF @id IS NULL INSERT dbo.Investments(CompanyId,Code,Name,Type,Department,Manager,StartDate,EndDate,Budget,ExecutedValue,ExpectedReturn,Status,Notes,PhysicalProgress,FundingSource,FinancedAmount,NextMilestone,NextMilestoneDate,RiskLevel,RiskDescription,MitigationPlan,TargetRoi,TargetCompletionDate) VALUES(@companyId,@code,@name,@type,@department,@manager,@start,@end,@budget,@executed,@expected,@status,@notes,@progress,@funding,@financed,@milestone,@milestoneDate,@risk,@riskDescription,@mitigation,@targetRoi,@targetDate) ELSE UPDATE dbo.Investments SET Code=@code,Name=@name,Type=@type,Department=@department,Manager=@manager,StartDate=@start,EndDate=@end,Budget=@budget,ExecutedValue=@executed,ExpectedReturn=@expected,Status=@status,Notes=@notes,PhysicalProgress=@progress,FundingSource=@funding,FinancedAmount=@financed,NextMilestone=@milestone,NextMilestoneDate=@milestoneDate,RiskLevel=@risk,RiskDescription=@riskDescription,MitigationPlan=@mitigation,TargetRoi=@targetRoi,TargetCompletionDate=@targetDate,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId, x.Id, sql, cancellationToken, ("@code", x.Code.Trim().ToUpperInvariant()), ("@name", x.Name.Trim()), ("@type", x.Type.Trim()), ("@department", x.Department.Trim()), ("@manager", x.Manager.Trim()), ("@start", x.StartDate.Date), ("@end", x.EndDate?.Date), ("@budget", x.Budget), ("@executed", x.ExecutedValue), ("@expected", x.ExpectedReturn), ("@status", x.Status), ("@notes", x.Notes.Trim()), ("@progress", x.PhysicalProgress), ("@funding", x.FundingSource.Trim()), ("@financed", x.FinancedAmount), ("@milestone", x.NextMilestone.Trim()), ("@milestoneDate", x.NextMilestoneDate?.Date), ("@risk", x.RiskLevel), ("@riskDescription", x.RiskDescription.Trim()), ("@mitigation", x.MitigationPlan.Trim()), ("@targetRoi", x.TargetRoi), ("@targetDate", x.TargetCompletionDate?.Date));
    }

    public Task SetStatusAsync(int companyId, int id, string status, CancellationToken cancellationToken = default) =>
        Exec(companyId, id, "UPDATE dbo.Investments SET Status=@status,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1", cancellationToken, ("@status", status));

    public Task ArchiveAsync(int companyId, int id, CancellationToken cancellationToken = default) =>
        Exec(companyId, id, "UPDATE dbo.Investments SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId", cancellationToken);

    public async Task<IReadOnlyList<InvestmentFinancingRow>> ListFinancingsAsync(int companyId, int? investmentId = null, CancellationToken cancellationToken = default)
    {
        const string sql=@"SELECT f.Id,f.InvestmentId,i.Code+' - '+i.Name,f.ContractNumber,f.Lender,f.FinancingType,f.ApprovedAmount,f.DisbursedAmount,f.OutstandingBalance,f.AnnualRate,f.TermMonths,f.InstallmentAmount,f.StartDate,f.FirstDueDate,f.Status,COALESCE(f.Notes,'') FROM dbo.InvestmentFinancings f JOIN dbo.Investments i ON i.Id=f.InvestmentId WHERE f.CompanyId=@companyId AND f.Active=1 AND (@investmentId IS NULL OR f.InvestmentId=@investmentId) ORDER BY f.StartDate DESC,f.ContractNumber";
        var list=new List<InvestmentFinancingRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@investmentId",investmentId);await using var r=await cmd.ExecuteReaderAsync(cancellationToken);while(await r.ReadAsync(cancellationToken))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9),r.GetInt32(10),r.GetDecimal(11),r.GetDateTime(12),r.IsDBNull(13)?null:r.GetDateTime(13),r.GetString(14),r.GetString(15)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveFinancingAsync(SaveInvestmentFinancingRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||x.InvestmentId<=0||string.IsNullOrWhiteSpace(x.ContractNumber)||string.IsNullOrWhiteSpace(x.Lender)||string.IsNullOrWhiteSpace(x.FinancingType))throw new ArgumentException("Investimento, contrato, financiador e modalidade são obrigatórios.");
        if(x.ApprovedAmount<=0||x.DisbursedAmount<0||x.DisbursedAmount>x.ApprovedAmount||x.OutstandingBalance<0||x.OutstandingBalance>x.ApprovedAmount)throw new ArgumentException("Valores aprovados, desembolsados ou saldo devedor inválidos.");
        if(x.AnnualRate<0||x.AnnualRate>100||x.TermMonths is <1 or >600||x.InstallmentAmount<0)throw new ArgumentException("Taxa, prazo ou prestação inválidos.");
        const string sql=@"IF @id IS NULL INSERT dbo.InvestmentFinancings(CompanyId,InvestmentId,ContractNumber,Lender,FinancingType,ApprovedAmount,DisbursedAmount,OutstandingBalance,AnnualRate,TermMonths,InstallmentAmount,StartDate,FirstDueDate,Status,Notes) VALUES(@companyId,@investment,@contract,@lender,@type,@approved,@disbursed,@outstanding,@rate,@term,@installment,@start,@due,@status,@notes) ELSE UPDATE dbo.InvestmentFinancings SET InvestmentId=@investment,ContractNumber=@contract,Lender=@lender,FinancingType=@type,ApprovedAmount=@approved,DisbursedAmount=@disbursed,OutstandingBalance=@outstanding,AnnualRate=@rate,TermMonths=@term,InstallmentAmount=@installment,StartDate=@start,FirstDueDate=@due,Status=@status,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@investment",x.InvestmentId),("@contract",x.ContractNumber.Trim().ToUpperInvariant()),("@lender",x.Lender.Trim()),("@type",x.FinancingType),("@approved",x.ApprovedAmount),("@disbursed",x.DisbursedAmount),("@outstanding",x.OutstandingBalance),("@rate",x.AnnualRate),("@term",x.TermMonths),("@installment",x.InstallmentAmount),("@start",x.StartDate.Date),("@due",x.FirstDueDate?.Date),("@status",x.Status),("@notes",x.Notes.Trim()));
    }

    public Task ArchiveFinancingAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.InvestmentFinancings SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    public async Task<IReadOnlyList<InvestmentFinancingPaymentRow>> ListFinancingPaymentsAsync(int companyId,int financingId,CancellationToken ct=default)
    {
        const string sql=@"SELECT Id,FinancingId,InstallmentNumber,DueDate,PrincipalAmount,InterestAmount,AmountDue,AmountPaid,PaymentDate,CASE WHEN Status<>N'Pago' AND DueDate<CAST(GETDATE() AS date) THEN N'Atrasado' ELSE Status END,COALESCE(PaymentReference,''),COALESCE(Notes,'') FROM dbo.InvestmentFinancingPayments WHERE CompanyId=@companyId AND FinancingId=@financingId AND Active=1 ORDER BY InstallmentNumber";
        var list=new List<InvestmentFinancingPaymentRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@financingId",financingId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetInt32(2),r.GetDateTime(3),r.GetDecimal(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.IsDBNull(8)?null:r.GetDateTime(8),r.GetString(9),r.GetString(10),r.GetString(11)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task GeneratePaymentScheduleAsync(int companyId,int financingId,CancellationToken ct=default)
    {
        var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);await using var tx=await cn.BeginTransactionAsync(ct);
        try
        {
            await using var check=cn.CreateCommand();check.Transaction=tx;check.CommandText="SELECT COUNT(*) FROM dbo.InvestmentFinancingPayments WHERE CompanyId=@companyId AND FinancingId=@financingId AND Active=1";Add(check,"@companyId",companyId);Add(check,"@financingId",financingId);if(Convert.ToInt32(await check.ExecuteScalarAsync(ct))>0)throw new InvalidOperationException("O financiamento já possui um plano de prestações.");
            await using var source=cn.CreateCommand();source.Transaction=tx;source.CommandText="SELECT DisbursedAmount,AnnualRate,TermMonths,COALESCE(FirstDueDate,DATEADD(month,1,StartDate)) FROM dbo.InvestmentFinancings WHERE Id=@financingId AND CompanyId=@companyId AND Active=1";Add(source,"@companyId",companyId);Add(source,"@financingId",financingId);
            decimal principal,rate;int months;DateTime firstDue;await using(var r=await source.ExecuteReaderAsync(ct)){if(!await r.ReadAsync(ct))throw new InvalidOperationException("Financiamento não encontrado.");principal=r.GetDecimal(0);rate=r.GetDecimal(1)/1200m;months=r.GetInt32(2);firstDue=r.GetDateTime(3);}if(principal<=0)throw new InvalidOperationException("Informe o valor desembolsado antes de gerar o plano.");
            var installment=rate==0?principal/months:principal*rate/(1-(decimal)Math.Pow(1+(double)rate,-months));var balance=principal;
            for(var number=1;number<=months;number++){var interest=Math.Round(balance*rate,2);var capital=number==months?balance:Math.Round(installment-interest,2);var due=Math.Round(capital+interest,2);await using var insert=cn.CreateCommand();insert.Transaction=tx;insert.CommandText="INSERT dbo.InvestmentFinancingPayments(CompanyId,FinancingId,InstallmentNumber,DueDate,PrincipalAmount,InterestAmount,AmountDue) VALUES(@companyId,@financingId,@number,@dueDate,@principal,@interest,@amountDue)";Add(insert,"@companyId",companyId);Add(insert,"@financingId",financingId);Add(insert,"@number",number);Add(insert,"@dueDate",firstDue.AddMonths(number-1).Date);Add(insert,"@principal",capital);Add(insert,"@interest",interest);Add(insert,"@amountDue",due);await insert.ExecuteNonQueryAsync(ct);balance=Math.Max(0,balance-capital);}
            await tx.CommitAsync(ct);
        }
        catch{await tx.RollbackAsync(ct);throw;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveFinancingPaymentAsync(SaveInvestmentFinancingPaymentRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||x.FinancingId<=0||x.InstallmentNumber<=0)throw new ArgumentException("Financiamento e número da prestação são obrigatórios.");if(x.PrincipalAmount<0||x.InterestAmount<0||x.AmountDue<=0||x.AmountPaid<0||x.AmountPaid>x.AmountDue)throw new ArgumentException("Os valores da prestação são inválidos.");if(x.Status=="Pago"&&(!x.PaymentDate.HasValue||x.AmountPaid<=0))throw new ArgumentException("Informe a data e o valor do pagamento.");
        const string sql=@"IF @id IS NULL INSERT dbo.InvestmentFinancingPayments(CompanyId,FinancingId,InstallmentNumber,DueDate,PrincipalAmount,InterestAmount,AmountDue,AmountPaid,PaymentDate,Status,PaymentReference,Notes) VALUES(@companyId,@financingId,@number,@due,@principal,@interest,@amountDue,@paid,@paymentDate,@status,@reference,@notes) ELSE UPDATE dbo.InvestmentFinancingPayments SET InstallmentNumber=@number,DueDate=@due,PrincipalAmount=@principal,InterestAmount=@interest,AmountDue=@amountDue,AmountPaid=@paid,PaymentDate=@paymentDate,Status=@status,PaymentReference=@reference,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND FinancingId=@financingId AND Active=1; UPDATE f SET OutstandingBalance=CASE WHEN f.DisbursedAmount-p.PaidPrincipal<0 THEN 0 ELSE f.DisbursedAmount-p.PaidPrincipal END,UpdatedAt=SYSUTCDATETIME() FROM dbo.InvestmentFinancings f CROSS APPLY(SELECT COALESCE(SUM(CASE WHEN AmountDue>0 THEN PrincipalAmount*AmountPaid/AmountDue ELSE 0 END),0) PaidPrincipal FROM dbo.InvestmentFinancingPayments WHERE CompanyId=@companyId AND FinancingId=@financingId AND Active=1)p WHERE f.Id=@financingId AND f.CompanyId=@companyId";
        await Exec(x.CompanyId,x.Id,sql,ct,("@financingId",x.FinancingId),("@number",x.InstallmentNumber),("@due",x.DueDate.Date),("@principal",x.PrincipalAmount),("@interest",x.InterestAmount),("@amountDue",x.AmountDue),("@paid",x.AmountPaid),("@paymentDate",x.PaymentDate?.Date),("@status",x.Status),("@reference",x.PaymentReference.Trim()),("@notes",x.Notes.Trim()));
    }

    public Task ArchiveFinancingPaymentAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.InvestmentFinancingPayments SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    public async Task<IReadOnlyList<InvestmentCashFlowRow>> ListCashFlowsAsync(int companyId,int investmentId,CancellationToken ct=default)
    {
        const string sql=@"SELECT Id,InvestmentId,FlowDate,Description,FlowType,InflowAmount,OutflowAmount,COALESCE(Reference,''),COALESCE(Notes,'') FROM dbo.InvestmentCashFlows WHERE CompanyId=@companyId AND InvestmentId=@investmentId AND Active=1 ORDER BY FlowDate,Id";
        var list=new List<InvestmentCashFlowRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@investmentId",investmentId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetDateTime(2),r.GetString(3),r.GetString(4),r.GetDecimal(5),r.GetDecimal(6),r.GetString(7),r.GetString(8)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveCashFlowAsync(SaveInvestmentCashFlowRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||x.InvestmentId<=0||string.IsNullOrWhiteSpace(x.Description)||string.IsNullOrWhiteSpace(x.FlowType))throw new ArgumentException("Investimento, descrição e tipo do fluxo são obrigatórios.");if(x.InflowAmount<0||x.OutflowAmount<0||(x.InflowAmount==0&&x.OutflowAmount==0))throw new ArgumentException("Informe uma entrada ou saída válida.");
        const string sql=@"IF @id IS NULL INSERT dbo.InvestmentCashFlows(CompanyId,InvestmentId,FlowDate,Description,FlowType,InflowAmount,OutflowAmount,Reference,Notes) VALUES(@companyId,@investmentId,@date,@description,@type,@inflow,@outflow,@reference,@notes) ELSE UPDATE dbo.InvestmentCashFlows SET InvestmentId=@investmentId,FlowDate=@date,Description=@description,FlowType=@type,InflowAmount=@inflow,OutflowAmount=@outflow,Reference=@reference,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@investmentId",x.InvestmentId),("@date",x.FlowDate.Date),("@description",x.Description.Trim()),("@type",x.FlowType.Trim()),("@inflow",x.InflowAmount),("@outflow",x.OutflowAmount),("@reference",x.Reference.Trim()),("@notes",x.Notes.Trim()));
    }

    public Task ArchiveCashFlowAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.InvestmentCashFlows SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    public async Task<IReadOnlyList<InvestmentEvaluationRow>> ListEvaluationsAsync(int companyId,int investmentId,CancellationToken ct=default)
    {
        const string sql=@"SELECT Id,InvestmentId,ReviewDate,EvaluationType,BenefitsRealized,AdditionalCosts,ResidualValue,ObjectivesAchievement,FinalRating,COALESCE(Recommendation,''),Status,COALESCE(Notes,'') FROM dbo.InvestmentEvaluations WHERE CompanyId=@companyId AND InvestmentId=@investmentId AND Active=1 ORDER BY ReviewDate DESC,Id DESC";
        var list=new List<InvestmentEvaluationRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@investmentId",investmentId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetDateTime(2),r.GetString(3),r.GetDecimal(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetString(8),r.GetString(9),r.GetString(10),r.GetString(11)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveEvaluationAsync(SaveInvestmentEvaluationRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||x.InvestmentId<=0||string.IsNullOrWhiteSpace(x.EvaluationType)||string.IsNullOrWhiteSpace(x.FinalRating)||string.IsNullOrWhiteSpace(x.Status))throw new ArgumentException("Investimento, tipo, classificação e estado são obrigatórios.");if(x.BenefitsRealized<0||x.AdditionalCosts<0||x.ResidualValue<0||x.ObjectivesAchievement is <0 or >100)throw new ArgumentException("Valores ou realização dos objetivos inválidos.");
        const string sql=@"IF @id IS NULL INSERT dbo.InvestmentEvaluations(CompanyId,InvestmentId,ReviewDate,EvaluationType,BenefitsRealized,AdditionalCosts,ResidualValue,ObjectivesAchievement,FinalRating,Recommendation,Status,Notes) VALUES(@companyId,@investmentId,@date,@type,@benefits,@costs,@residual,@achievement,@rating,@recommendation,@status,@notes) ELSE UPDATE dbo.InvestmentEvaluations SET InvestmentId=@investmentId,ReviewDate=@date,EvaluationType=@type,BenefitsRealized=@benefits,AdditionalCosts=@costs,ResidualValue=@residual,ObjectivesAchievement=@achievement,FinalRating=@rating,Recommendation=@recommendation,Status=@status,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@investmentId",x.InvestmentId),("@date",x.ReviewDate.Date),("@type",x.EvaluationType.Trim()),("@benefits",x.BenefitsRealized),("@costs",x.AdditionalCosts),("@residual",x.ResidualValue),("@achievement",x.ObjectivesAchievement),("@rating",x.FinalRating.Trim()),("@recommendation",x.Recommendation.Trim()),("@status",x.Status.Trim()),("@notes",x.Notes.Trim()));
    }

    public Task ArchiveEvaluationAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.InvestmentEvaluations SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    private async Task Exec(int companyId, int? id, string sql, CancellationToken ct, params (string, object?)[] values)
    { var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@id",id);foreach(var value in values)Add(cmd,value.Item1,value.Item2);await cmd.ExecuteNonQueryAsync(ct);}finally{if(close)await cn.CloseAsync();} }
    private static void Add(DbCommand cmd, string name, object? value) { var p=cmd.CreateParameter();p.ParameterName=name;p.Value=value??DBNull.Value;cmd.Parameters.Add(p); }
}
