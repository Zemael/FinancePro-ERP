using System.Data;
using System.Data.Common;
using FinancePro.Data.Context;
using FinancePro.Platform.HumanResources;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.HumanResources;

public sealed class SqlHumanResourcesService : IHumanResourcesService
{
    private readonly FinanceProDbContext _db;
    public SqlHumanResourcesService(FinanceProDbContext db)=>_db=db;

    public async Task<IReadOnlyList<EmployeeFinancialRow>> ListEmployeesAsync(int companyId,CancellationToken ct=default)
    {
        const string sql=@"SELECT Id,EmployeeNumber,FullName,COALESCE(TaxId,''),COALESCE(SocialSecurityNumber,''),Department,COALESCE(CostCenter,''),JobTitle,ContractType,HireDate,EndDate,BaseSalary,Allowances,Deductions,COALESCE(BankName,''),COALESCE(Iban,''),Status,COALESCE(Notes,'') FROM dbo.HumanResourcesEmployees WHERE CompanyId=@companyId AND Active=1 ORDER BY FullName";
        var list=new List<EmployeeFinancialRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.GetString(6),r.GetString(7),r.GetString(8),r.GetDateTime(9),r.IsDBNull(10)?null:r.GetDateTime(10),r.GetDecimal(11),r.GetDecimal(12),r.GetDecimal(13),r.GetString(14),r.GetString(15),r.GetString(16),r.GetString(17)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveEmployeeAsync(SaveEmployeeFinancialRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||string.IsNullOrWhiteSpace(x.EmployeeNumber)||string.IsNullOrWhiteSpace(x.FullName)||string.IsNullOrWhiteSpace(x.Department)||string.IsNullOrWhiteSpace(x.JobTitle)||string.IsNullOrWhiteSpace(x.ContractType))throw new ArgumentException("Número, nome, departamento, função e contrato são obrigatórios.");if(x.BaseSalary<0||x.Allowances<0||x.Deductions<0||x.Deductions>x.BaseSalary+x.Allowances)throw new ArgumentException("Os valores remuneratórios são inválidos.");if(x.EndDate.HasValue&&x.EndDate.Value.Date<x.HireDate.Date)throw new ArgumentException("A data final não pode ser anterior à admissão.");
        const string sql=@"IF @id IS NULL INSERT dbo.HumanResourcesEmployees(CompanyId,EmployeeNumber,FullName,TaxId,SocialSecurityNumber,Department,CostCenter,JobTitle,ContractType,HireDate,EndDate,BaseSalary,Allowances,Deductions,BankName,Iban,Status,Notes) VALUES(@companyId,@number,@name,@tax,@social,@department,@costCenter,@job,@contract,@hire,@end,@salary,@allowances,@deductions,@bank,@iban,@status,@notes) ELSE UPDATE dbo.HumanResourcesEmployees SET EmployeeNumber=@number,FullName=@name,TaxId=@tax,SocialSecurityNumber=@social,Department=@department,CostCenter=@costCenter,JobTitle=@job,ContractType=@contract,HireDate=@hire,EndDate=@end,BaseSalary=@salary,Allowances=@allowances,Deductions=@deductions,BankName=@bank,Iban=@iban,Status=@status,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@number",x.EmployeeNumber.Trim().ToUpperInvariant()),("@name",x.FullName.Trim()),("@tax",x.TaxId.Trim()),("@social",x.SocialSecurityNumber.Trim()),("@department",x.Department.Trim()),("@costCenter",x.CostCenter.Trim()),("@job",x.JobTitle.Trim()),("@contract",x.ContractType.Trim()),("@hire",x.HireDate.Date),("@end",x.EndDate?.Date),("@salary",x.BaseSalary),("@allowances",x.Allowances),("@deductions",x.Deductions),("@bank",x.BankName.Trim()),("@iban",x.Iban.Trim()),("@status",x.Status.Trim()),("@notes",x.Notes.Trim()));
    }

    public Task ArchiveEmployeeAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.HumanResourcesEmployees SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    public async Task<IReadOnlyList<PayrollRunRow>> ListPayrollRunsAsync(int companyId,CancellationToken ct=default)
    {
        const string sql=@"SELECT r.Id,r.PayrollYear,r.PayrollMonth,r.Status,r.GeneratedAt,r.ApprovedAt,COALESCE(r.Notes,''),COUNT(l.Id),COALESCE(SUM(l.GrossAmount),0),COALESCE(SUM(l.Deductions),0),COALESCE(SUM(l.NetAmount),0) FROM dbo.HumanResourcesPayrollRuns r LEFT JOIN dbo.HumanResourcesPayrollLines l ON l.PayrollRunId=r.Id WHERE r.CompanyId=@companyId AND r.Active=1 GROUP BY r.Id,r.PayrollYear,r.PayrollMonth,r.Status,r.GeneratedAt,r.ApprovedAt,r.Notes ORDER BY r.PayrollYear DESC,r.PayrollMonth DESC";
        var list=new List<PayrollRunRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetInt32(2),r.GetString(3),r.GetDateTime(4),r.IsDBNull(5)?null:r.GetDateTime(5),r.GetString(6),r.GetInt32(7),r.GetDecimal(8),r.GetDecimal(9),r.GetDecimal(10)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task<IReadOnlyList<PayrollLineRow>> ListPayrollLinesAsync(int companyId,int payrollRunId,CancellationToken ct=default)
    {
        const string sql=@"SELECT l.Id,l.EmployeeId,e.EmployeeNumber,e.FullName,e.Department,l.BaseSalary,l.Allowances,l.GrossAmount,l.Deductions,l.NetAmount,l.IncomeTax,l.EmployeeSocialSecurity,l.EmployerSocialSecurity,l.PaymentStatus,l.PaymentDate,COALESCE(l.PaymentReference,''),COALESCE(l.Notes,''),l.LoanDeduction FROM dbo.HumanResourcesPayrollLines l JOIN dbo.HumanResourcesEmployees e ON e.Id=l.EmployeeId WHERE l.CompanyId=@companyId AND l.PayrollRunId=@runId ORDER BY e.FullName";
        var list=new List<PayrollLineRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@runId",payrollRunId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9),r.GetDecimal(10),r.GetDecimal(11),r.GetDecimal(12),r.GetString(13),r.IsDBNull(14)?null:r.GetDateTime(14),r.GetString(15),r.GetString(16),r.GetDecimal(17)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task GeneratePayrollAsync(int companyId,int year,int month,string notes,CancellationToken ct=default)
    {
        if(companyId<=0||year is <2000 or >2200||month is <1 or >12)throw new ArgumentException("Empresa ou período salarial inválido.");var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);await using var tx=await cn.BeginTransactionAsync(ct);try{
        {
            await using var count=cn.CreateCommand();count.Transaction=tx;count.CommandText="SELECT COUNT(*) FROM dbo.HumanResourcesEmployees WHERE CompanyId=@companyId AND Active=1 AND Status=N'Ativo'";Add(count,"@companyId",companyId);if(Convert.ToInt32(await count.ExecuteScalarAsync(ct))==0)throw new InvalidOperationException("Não existem colaboradores ativos para gerar a folha.");
            await using var run=cn.CreateCommand();run.Transaction=tx;run.CommandText="INSERT dbo.HumanResourcesPayrollRuns(CompanyId,PayrollYear,PayrollMonth,Notes) OUTPUT INSERTED.Id VALUES(@companyId,@year,@month,@notes)";Add(run,"@companyId",companyId);Add(run,"@year",year);Add(run,"@month",month);Add(run,"@notes",notes.Trim());var runId=Convert.ToInt32(await run.ExecuteScalarAsync(ct));
            await using var lines=cn.CreateCommand();lines.Transaction=tx;lines.CommandText=@"INSERT dbo.HumanResourcesPayrollLines(CompanyId,PayrollRunId,EmployeeId,BaseSalary,Allowances,GrossAmount,Deductions,NetAmount,IncomeTax,EmployeeSocialSecurity,EmployerSocialSecurity,LoanDeduction) SELECT e.CompanyId,@runId,e.Id,e.BaseSalary,g.Allowances,g.GrossAmount,e.Deductions+COALESCE(a.Deductions,0)+COALESCE(f.AbsenceDeductions,0)+COALESCE(r.IncomeTax,0)+COALESCE(r.EmployeeSS,0)+COALESCE(q.LoanDeduction,0),g.GrossAmount-e.Deductions-COALESCE(a.Deductions,0)-COALESCE(f.AbsenceDeductions,0)-COALESCE(r.IncomeTax,0)-COALESCE(r.EmployeeSS,0)-COALESCE(q.LoanDeduction,0),COALESCE(r.IncomeTax,0),COALESCE(r.EmployeeSS,0),COALESCE(r.EmployerSS,0),COALESCE(q.LoanDeduction,0) FROM dbo.HumanResourcesEmployees e OUTER APPLY(SELECT SUM(CASE WHEN x.AdjustmentType=N'Abono' THEN x.Amount ELSE 0 END) Earnings,SUM(CASE WHEN x.AdjustmentType=N'Desconto' THEN x.Amount ELSE 0 END) Deductions FROM dbo.HumanResourcesEmployeeAdjustments x WHERE x.CompanyId=e.CompanyId AND x.EmployeeId=e.Id AND x.Active=1 AND x.StartDate<=EOMONTH(DATEFROMPARTS(@year,@month,1)) AND (x.EndDate IS NULL OR x.EndDate>=DATEFROMPARTS(@year,@month,1)))a OUTER APPLY(SELECT SUM(x.DeductionAmount) AbsenceDeductions FROM dbo.HumanResourcesAbsences x WHERE x.CompanyId=e.CompanyId AND x.EmployeeId=e.Id AND x.Active=1 AND x.Status=N'Aprovada' AND x.Paid=0 AND YEAR(x.StartDate)=@year AND MONTH(x.StartDate)=@month)f OUTER APPLY(SELECT SUM(x.OvertimeAmount) OvertimeEarnings FROM dbo.HumanResourcesAttendance x WHERE x.CompanyId=e.CompanyId AND x.EmployeeId=e.Id AND x.Active=1 AND x.Status=N'Aprovado' AND YEAR(x.WorkDate)=@year AND MONTH(x.WorkDate)=@month)h CROSS APPLY(SELECT e.Allowances+COALESCE(a.Earnings,0)+COALESCE(h.OvertimeEarnings,0) Allowances,e.BaseSalary+e.Allowances+COALESCE(a.Earnings,0)+COALESCE(h.OvertimeEarnings,0) GrossAmount)g OUTER APPLY(SELECT SUM(CASE WHEN x.RuleType=N'Imposto salarial' THEN g.GrossAmount*x.Rate/100+x.FixedAmount ELSE 0 END) IncomeTax,SUM(CASE WHEN x.RuleType=N'Segurança Social - Trabalhador' THEN g.GrossAmount*x.Rate/100+x.FixedAmount ELSE 0 END) EmployeeSS,SUM(CASE WHEN x.RuleType=N'Segurança Social - Entidade' THEN g.GrossAmount*x.Rate/100+x.FixedAmount ELSE 0 END) EmployerSS FROM dbo.HumanResourcesPayrollRules x WHERE x.CompanyId=e.CompanyId AND x.Active=1 AND x.StartDate<=EOMONTH(DATEFROMPARTS(@year,@month,1)) AND (x.EndDate IS NULL OR x.EndDate>=DATEFROMPARTS(@year,@month,1)) AND g.GrossAmount>=x.MinimumBase AND (x.MaximumBase IS NULL OR g.GrossAmount<=x.MaximumBase))r OUTER APPLY(SELECT SUM(CASE WHEN x.OutstandingBalance<x.InstallmentAmount THEN x.OutstandingBalance ELSE x.InstallmentAmount END) LoanDeduction FROM dbo.HumanResourcesEmployeeLoans x WHERE x.CompanyId=e.CompanyId AND x.EmployeeId=e.Id AND x.Active=1 AND x.Status=N'Ativo' AND x.FirstDeductionDate<=EOMONTH(DATEFROMPARTS(@year,@month,1)))q WHERE e.CompanyId=@companyId AND e.Active=1 AND e.Status=N'Ativo'";Add(lines,"@companyId",companyId);Add(lines,"@runId",runId);Add(lines,"@year",year);Add(lines,"@month",month);await lines.ExecuteNonQueryAsync(ct);
            await using var loanPayments=cn.CreateCommand();loanPayments.Transaction=tx;loanPayments.CommandText=@"INSERT dbo.HumanResourcesLoanPayments(CompanyId,LoanId,PayrollRunId,Amount) SELECT l.CompanyId,l.Id,@runId,CASE WHEN l.OutstandingBalance<l.InstallmentAmount THEN l.OutstandingBalance ELSE l.InstallmentAmount END FROM dbo.HumanResourcesEmployeeLoans l JOIN dbo.HumanResourcesEmployees e ON e.Id=l.EmployeeId AND e.CompanyId=l.CompanyId WHERE l.CompanyId=@companyId AND l.Active=1 AND l.Status=N'Ativo' AND l.FirstDeductionDate<=EOMONTH(DATEFROMPARTS(@year,@month,1)) AND e.Active=1 AND e.Status=N'Ativo'";Add(loanPayments,"@companyId",companyId);Add(loanPayments,"@runId",runId);Add(loanPayments,"@year",year);Add(loanPayments,"@month",month);await loanPayments.ExecuteNonQueryAsync(ct);await tx.CommitAsync(ct);
        }}catch{await tx.RollbackAsync(ct);throw;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task ApprovePayrollAsync(int companyId,int payrollRunId,CancellationToken ct=default)
    {
        const string sql=@"IF NOT EXISTS(SELECT 1 FROM dbo.HumanResourcesPayrollRuns WHERE Id=@id AND CompanyId=@companyId AND Active=1 AND Status=N'Rascunho') THROW 50001,N'A folha deve estar em rascunho.',1; UPDATE l SET OutstandingBalance=CASE WHEN l.OutstandingBalance-p.Amount<0 THEN 0 ELSE l.OutstandingBalance-p.Amount END,Status=CASE WHEN l.OutstandingBalance-p.Amount<=0 THEN N'Liquidado' ELSE l.Status END,UpdatedAt=SYSUTCDATETIME() FROM dbo.HumanResourcesEmployeeLoans l JOIN dbo.HumanResourcesLoanPayments p ON p.LoanId=l.Id WHERE p.PayrollRunId=@id AND p.CompanyId=@companyId AND p.Status=N'Pendente'; UPDATE dbo.HumanResourcesLoanPayments SET Status=N'Confirmado',PaymentDate=CAST(SYSUTCDATETIME() AS date),UpdatedAt=SYSUTCDATETIME() WHERE PayrollRunId=@id AND CompanyId=@companyId AND Status=N'Pendente'; UPDATE dbo.HumanResourcesPayrollRuns SET Status=N'Aprovada',ApprovedAt=SYSUTCDATETIME(),UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1 AND Status=N'Rascunho'";
        await Exec(companyId,payrollRunId,sql,ct);
    }

    public async Task RegisterPayrollPaymentAsync(int companyId,int payrollLineId,DateTime paymentDate,string reference,string notes,CancellationToken ct=default)
    {
        if(companyId<=0||payrollLineId<=0||string.IsNullOrWhiteSpace(reference))throw new ArgumentException("Linha salarial, data e referência de pagamento são obrigatórias.");
        const string sql=@"IF NOT EXISTS(SELECT 1 FROM dbo.HumanResourcesPayrollLines l JOIN dbo.HumanResourcesPayrollRuns r ON r.Id=l.PayrollRunId WHERE l.Id=@id AND l.CompanyId=@companyId AND r.Status IN(N'Aprovada',N'Parcialmente paga')) THROW 50001,N'A folha deve estar aprovada antes do pagamento.',1; UPDATE l SET PaymentStatus=N'Pago',PaymentDate=@paymentDate,PaymentReference=@reference,Notes=@notes,UpdatedAt=SYSUTCDATETIME() FROM dbo.HumanResourcesPayrollLines l WHERE l.Id=@id AND l.CompanyId=@companyId; UPDATE r SET Status=CASE WHEN EXISTS(SELECT 1 FROM dbo.HumanResourcesPayrollLines l WHERE l.PayrollRunId=r.Id AND l.PaymentStatus<>N'Pago') THEN N'Parcialmente paga' ELSE N'Paga' END,UpdatedAt=SYSUTCDATETIME() FROM dbo.HumanResourcesPayrollRuns r JOIN dbo.HumanResourcesPayrollLines currentLine ON currentLine.PayrollRunId=r.Id WHERE currentLine.Id=@id AND r.CompanyId=@companyId";
        await Exec(companyId,payrollLineId,sql,ct,("@paymentDate",paymentDate.Date),("@reference",reference.Trim()),("@notes",notes.Trim()));
    }

    public async Task PayEntirePayrollAsync(int companyId,int payrollRunId,DateTime paymentDate,string reference,CancellationToken ct=default)
    {
        if(companyId<=0||payrollRunId<=0||string.IsNullOrWhiteSpace(reference))throw new ArgumentException("Folha, data e referência de pagamento são obrigatórias.");
        const string sql=@"IF NOT EXISTS(SELECT 1 FROM dbo.HumanResourcesPayrollRuns WHERE Id=@id AND CompanyId=@companyId AND Status IN(N'Aprovada',N'Parcialmente paga')) THROW 50001,N'A folha deve estar aprovada antes do pagamento.',1; UPDATE dbo.HumanResourcesPayrollLines SET PaymentStatus=N'Pago',PaymentDate=@paymentDate,PaymentReference=@reference,UpdatedAt=SYSUTCDATETIME() WHERE PayrollRunId=@id AND CompanyId=@companyId; UPDATE dbo.HumanResourcesPayrollRuns SET Status=N'Paga',UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId";
        await Exec(companyId,payrollRunId,sql,ct,("@paymentDate",paymentDate.Date),("@reference",reference.Trim()));
    }

    public async Task<IReadOnlyList<EmployeeAdjustmentRow>> ListEmployeeAdjustmentsAsync(int companyId,int employeeId,CancellationToken ct=default)
    {
        const string sql=@"SELECT Id,EmployeeId,AdjustmentType,Category,Description,Amount,StartDate,EndDate,Recurring,COALESCE(Notes,'') FROM dbo.HumanResourcesEmployeeAdjustments WHERE CompanyId=@companyId AND EmployeeId=@employeeId AND Active=1 ORDER BY StartDate DESC,Id DESC";
        var list=new List<EmployeeAdjustmentRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@employeeId",employeeId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetDecimal(5),r.GetDateTime(6),r.IsDBNull(7)?null:r.GetDateTime(7),r.GetBoolean(8),r.GetString(9)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveEmployeeAdjustmentAsync(SaveEmployeeAdjustmentRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||x.EmployeeId<=0||(x.AdjustmentType!="Abono"&&x.AdjustmentType!="Desconto")||string.IsNullOrWhiteSpace(x.Category)||string.IsNullOrWhiteSpace(x.Description)||x.Amount<=0)throw new ArgumentException("Colaborador, tipo, categoria, descrição e valor são obrigatórios.");if(x.EndDate.HasValue&&x.EndDate.Value.Date<x.StartDate.Date)throw new ArgumentException("A data final não pode ser anterior à data inicial.");
        const string sql=@"IF @id IS NULL INSERT dbo.HumanResourcesEmployeeAdjustments(CompanyId,EmployeeId,AdjustmentType,Category,Description,Amount,StartDate,EndDate,Recurring,Notes) VALUES(@companyId,@employeeId,@type,@category,@description,@amount,@start,@end,@recurring,@notes) ELSE UPDATE dbo.HumanResourcesEmployeeAdjustments SET EmployeeId=@employeeId,AdjustmentType=@type,Category=@category,Description=@description,Amount=@amount,StartDate=@start,EndDate=@end,Recurring=@recurring,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@employeeId",x.EmployeeId),("@type",x.AdjustmentType),("@category",x.Category.Trim()),("@description",x.Description.Trim()),("@amount",x.Amount),("@start",x.StartDate.Date),("@end",x.EndDate?.Date),("@recurring",x.Recurring),("@notes",x.Notes.Trim()));
    }

    public Task ArchiveEmployeeAdjustmentAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.HumanResourcesEmployeeAdjustments SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    public async Task<IReadOnlyList<EmployeeAbsenceRow>> ListEmployeeAbsencesAsync(int companyId,int employeeId,CancellationToken ct=default)
    {
        const string sql=@"SELECT Id,EmployeeId,AbsenceType,StartDate,EndDate,Days,Paid,DeductionAmount,Status,Reason,COALESCE(Notes,'') FROM dbo.HumanResourcesAbsences WHERE CompanyId=@companyId AND EmployeeId=@employeeId AND Active=1 ORDER BY StartDate DESC,Id DESC";
        var list=new List<EmployeeAbsenceRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@employeeId",employeeId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetString(2),r.GetDateTime(3),r.GetDateTime(4),r.GetDecimal(5),r.GetBoolean(6),r.GetDecimal(7),r.GetString(8),r.GetString(9),r.GetString(10)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveEmployeeAbsenceAsync(SaveEmployeeAbsenceRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||x.EmployeeId<=0||string.IsNullOrWhiteSpace(x.AbsenceType)||string.IsNullOrWhiteSpace(x.Reason)||x.Days<=0)throw new ArgumentException("Colaborador, tipo, período, dias e motivo são obrigatórios.");if(x.EndDate.Date<x.StartDate.Date)throw new ArgumentException("A data final não pode ser anterior à data inicial.");if(x.DeductionAmount<0||x.Paid&&x.DeductionAmount>0)throw new ArgumentException("Uma ausência remunerada não pode ter desconto.");
        const string sql=@"IF @id IS NULL INSERT dbo.HumanResourcesAbsences(CompanyId,EmployeeId,AbsenceType,StartDate,EndDate,Days,Paid,DeductionAmount,Reason,Notes) VALUES(@companyId,@employeeId,@type,@start,@end,@days,@paid,@deduction,@reason,@notes) ELSE UPDATE dbo.HumanResourcesAbsences SET EmployeeId=@employeeId,AbsenceType=@type,StartDate=@start,EndDate=@end,Days=@days,Paid=@paid,DeductionAmount=@deduction,Status=N'Pendente',Reason=@reason,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@employeeId",x.EmployeeId),("@type",x.AbsenceType.Trim()),("@start",x.StartDate.Date),("@end",x.EndDate.Date),("@days",x.Days),("@paid",x.Paid),("@deduction",x.Paid?0:x.DeductionAmount),("@reason",x.Reason.Trim()),("@notes",x.Notes.Trim()));
    }

    public Task SetEmployeeAbsenceStatusAsync(int companyId,int id,string status,CancellationToken ct=default)
    {
        if(status!="Aprovada"&&status!="Rejeitada")throw new ArgumentException("Estado de ausência inválido.");
        return Exec(companyId,id,"UPDATE dbo.HumanResourcesAbsences SET Status=@status,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1",ct,("@status",status));
    }

    public Task ArchiveEmployeeAbsenceAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.HumanResourcesAbsences SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    public async Task<IReadOnlyList<EmployeeAttendanceRow>> ListEmployeeAttendanceAsync(int companyId,int employeeId,CancellationToken ct=default)
    {
        const string sql=@"SELECT Id,EmployeeId,WorkDate,EntryTime,ExitTime,RegularHours,OvertimeHours,OvertimeAmount,Status,COALESCE(Notes,'') FROM dbo.HumanResourcesAttendance WHERE CompanyId=@companyId AND EmployeeId=@employeeId AND Active=1 ORDER BY WorkDate DESC,Id DESC";
        var list=new List<EmployeeAttendanceRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@employeeId",employeeId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetDateTime(2),r.GetFieldValue<TimeSpan>(3),r.IsDBNull(4)?null:r.GetFieldValue<TimeSpan>(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetString(8),r.GetString(9)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveEmployeeAttendanceAsync(SaveEmployeeAttendanceRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||x.EmployeeId<=0||!TimeSpan.TryParse(x.EntryTime,out var entry))throw new ArgumentException("Colaborador, data e hora de entrada são obrigatórios.");TimeSpan? exit=null;if(!string.IsNullOrWhiteSpace(x.ExitTime)){if(!TimeSpan.TryParse(x.ExitTime,out var parsedExit))throw new ArgumentException("A hora de saída é inválida.");exit=parsedExit;}if(exit.HasValue&&exit.Value<=entry)throw new ArgumentException("A hora de saída deve ser posterior à entrada.");if(x.RegularHours<0||x.OvertimeHours<0||x.OvertimeAmount<0)throw new ArgumentException("Horas e valor extraordinário não podem ser negativos.");
        const string sql=@"IF @id IS NULL INSERT dbo.HumanResourcesAttendance(CompanyId,EmployeeId,WorkDate,EntryTime,ExitTime,RegularHours,OvertimeHours,OvertimeAmount,Notes) VALUES(@companyId,@employeeId,@date,@entry,@exit,@regular,@overtime,@amount,@notes) ELSE UPDATE dbo.HumanResourcesAttendance SET EmployeeId=@employeeId,WorkDate=@date,EntryTime=@entry,ExitTime=@exit,RegularHours=@regular,OvertimeHours=@overtime,OvertimeAmount=@amount,Status=N'Pendente',Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@employeeId",x.EmployeeId),("@date",x.WorkDate.Date),("@entry",entry),("@exit",exit),("@regular",x.RegularHours),("@overtime",x.OvertimeHours),("@amount",x.OvertimeAmount),("@notes",x.Notes.Trim()));
    }

    public Task SetEmployeeAttendanceStatusAsync(int companyId,int id,string status,CancellationToken ct=default)
    {
        if(status!="Aprovado"&&status!="Rejeitado")throw new ArgumentException("Estado de assiduidade inválido.");
        return Exec(companyId,id,"UPDATE dbo.HumanResourcesAttendance SET Status=@status,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1",ct,("@status",status));
    }

    public Task ArchiveEmployeeAttendanceAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.HumanResourcesAttendance SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    public async Task<IReadOnlyList<EmployeeLeaveBalanceRow>> ListEmployeeLeaveBalancesAsync(int companyId,int employeeId,CancellationToken ct=default)
    {
        const string sql=@"SELECT b.Id,b.EmployeeId,b.LeaveYear,b.EntitledDays,b.CarriedDays,COALESCE(a.TakenDays,0),COALESCE(b.Notes,'') FROM dbo.HumanResourcesLeaveBalances b OUTER APPLY(SELECT SUM(x.Days) TakenDays FROM dbo.HumanResourcesAbsences x WHERE x.CompanyId=b.CompanyId AND x.EmployeeId=b.EmployeeId AND x.Active=1 AND x.Status=N'Aprovada' AND x.AbsenceType=N'Férias' AND YEAR(x.StartDate)=b.LeaveYear)a WHERE b.CompanyId=@companyId AND b.EmployeeId=@employeeId AND b.Active=1 ORDER BY b.LeaveYear DESC";
        var list=new List<EmployeeLeaveBalanceRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@employeeId",employeeId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetInt32(2),r.GetDecimal(3),r.GetDecimal(4),r.GetDecimal(5),r.GetString(6)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveEmployeeLeaveBalanceAsync(SaveEmployeeLeaveBalanceRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||x.EmployeeId<=0||x.LeaveYear is <2000 or >2200||x.EntitledDays<0||x.CarriedDays<0)throw new ArgumentException("Colaborador, ano e dias de férias devem ser válidos.");
        const string sql=@"IF @id IS NULL INSERT dbo.HumanResourcesLeaveBalances(CompanyId,EmployeeId,LeaveYear,EntitledDays,CarriedDays,Notes) VALUES(@companyId,@employeeId,@year,@entitled,@carried,@notes) ELSE UPDATE dbo.HumanResourcesLeaveBalances SET EmployeeId=@employeeId,LeaveYear=@year,EntitledDays=@entitled,CarriedDays=@carried,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@employeeId",x.EmployeeId),("@year",x.LeaveYear),("@entitled",x.EntitledDays),("@carried",x.CarriedDays),("@notes",x.Notes.Trim()));
    }

    public Task ArchiveEmployeeLeaveBalanceAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.HumanResourcesLeaveBalances SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    public async Task<IReadOnlyList<PayrollRuleRow>> ListPayrollRulesAsync(int companyId,CancellationToken ct=default)
    {
        const string sql=@"SELECT Id,RuleType,Name,Rate,FixedAmount,MinimumBase,MaximumBase,StartDate,EndDate,COALESCE(Notes,'') FROM dbo.HumanResourcesPayrollRules WHERE CompanyId=@companyId AND Active=1 ORDER BY RuleType,Name,StartDate DESC";
        var list=new List<PayrollRuleRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetDecimal(3),r.GetDecimal(4),r.GetDecimal(5),r.IsDBNull(6)?null:r.GetDecimal(6),r.GetDateTime(7),r.IsDBNull(8)?null:r.GetDateTime(8),r.GetString(9)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SavePayrollRuleAsync(SavePayrollRuleRequest x,CancellationToken ct=default)
    {
        var validTypes=new[]{"Imposto salarial","Segurança Social - Trabalhador","Segurança Social - Entidade"};if(x.CompanyId<=0||!validTypes.Contains(x.RuleType)||string.IsNullOrWhiteSpace(x.Name)||x.Rate<0||x.Rate>100||x.FixedAmount<0||x.MinimumBase<0)throw new ArgumentException("Tipo, nome, taxa e limites da regra são inválidos.");if(x.MaximumBase.HasValue&&x.MaximumBase<x.MinimumBase)throw new ArgumentException("O limite máximo não pode ser inferior ao mínimo.");if(x.EndDate.HasValue&&x.EndDate.Value.Date<x.StartDate.Date)throw new ArgumentException("A vigência final não pode ser anterior à inicial.");
        const string sql=@"IF @id IS NULL INSERT dbo.HumanResourcesPayrollRules(CompanyId,RuleType,Name,Rate,FixedAmount,MinimumBase,MaximumBase,StartDate,EndDate,Notes) VALUES(@companyId,@type,@name,@rate,@fixed,@minimum,@maximum,@start,@end,@notes) ELSE UPDATE dbo.HumanResourcesPayrollRules SET RuleType=@type,Name=@name,Rate=@rate,FixedAmount=@fixed,MinimumBase=@minimum,MaximumBase=@maximum,StartDate=@start,EndDate=@end,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@type",x.RuleType),("@name",x.Name.Trim()),("@rate",x.Rate),("@fixed",x.FixedAmount),("@minimum",x.MinimumBase),("@maximum",x.MaximumBase),("@start",x.StartDate.Date),("@end",x.EndDate?.Date),("@notes",x.Notes.Trim()));
    }

    public Task ArchivePayrollRuleAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.HumanResourcesPayrollRules SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId",ct);

    public async Task<IReadOnlyList<EmployeeLoanRow>> ListEmployeeLoansAsync(int companyId,int employeeId,CancellationToken ct=default)
    {
        const string sql=@"SELECT Id,EmployeeId,LoanType,COALESCE(Reference,''),GrantedDate,OriginalAmount,OutstandingBalance,InstallmentAmount,FirstDeductionDate,Status,COALESCE(Notes,'') FROM dbo.HumanResourcesEmployeeLoans WHERE CompanyId=@companyId AND EmployeeId=@employeeId AND Active=1 ORDER BY GrantedDate DESC,Id DESC";
        var list=new List<EmployeeLoanRow>();var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@employeeId",employeeId);await using var r=await cmd.ExecuteReaderAsync(ct);while(await r.ReadAsync(ct))list.Add(new(r.GetInt32(0),r.GetInt32(1),r.GetString(2),r.GetString(3),r.GetDateTime(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDateTime(8),r.GetString(9),r.GetString(10)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SaveEmployeeLoanAsync(SaveEmployeeLoanRequest x,CancellationToken ct=default)
    {
        if(x.CompanyId<=0||x.EmployeeId<=0||string.IsNullOrWhiteSpace(x.LoanType)||x.OriginalAmount<=0||x.OutstandingBalance<0||x.OutstandingBalance>x.OriginalAmount||x.InstallmentAmount<=0)throw new ArgumentException("Colaborador, tipo e valores do empréstimo são inválidos.");if(x.FirstDeductionDate.Date<x.GrantedDate.Date)throw new ArgumentException("A primeira dedução não pode ser anterior à concessão.");
        const string sql=@"IF @id IS NULL INSERT dbo.HumanResourcesEmployeeLoans(CompanyId,EmployeeId,LoanType,Reference,GrantedDate,OriginalAmount,OutstandingBalance,InstallmentAmount,FirstDeductionDate,Notes) VALUES(@companyId,@employeeId,@type,@reference,@granted,@original,@balance,@installment,@first,@notes) ELSE UPDATE dbo.HumanResourcesEmployeeLoans SET EmployeeId=@employeeId,LoanType=@type,Reference=@reference,GrantedDate=@granted,OriginalAmount=@original,OutstandingBalance=@balance,InstallmentAmount=@installment,FirstDeductionDate=@first,Notes=@notes,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1";
        await Exec(x.CompanyId,x.Id,sql,ct,("@employeeId",x.EmployeeId),("@type",x.LoanType.Trim()),("@reference",x.Reference.Trim()),("@granted",x.GrantedDate.Date),("@original",x.OriginalAmount),("@balance",x.OutstandingBalance),("@installment",x.InstallmentAmount),("@first",x.FirstDeductionDate.Date),("@notes",x.Notes.Trim()));
    }

    public Task SetEmployeeLoanStatusAsync(int companyId,int id,string status,CancellationToken ct=default)
    {
        if(status!="Ativo"&&status!="Suspenso")throw new ArgumentException("Estado do empréstimo inválido.");
        return Exec(companyId,id,"UPDATE dbo.HumanResourcesEmployeeLoans SET Status=@status,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND Active=1 AND OutstandingBalance>0",ct,("@status",status));
    }

    public Task ArchiveEmployeeLoanAsync(int companyId,int id,CancellationToken ct=default)=>Exec(companyId,id,"UPDATE dbo.HumanResourcesEmployeeLoans SET Active=0,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId AND OutstandingBalance=0",ct);

    private async Task Exec(int companyId,int? id,string sql,CancellationToken ct,params(string,object?)[] values){var cn=_db.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(ct);try{await using var cmd=cn.CreateCommand();cmd.CommandText=sql;Add(cmd,"@companyId",companyId);Add(cmd,"@id",id);foreach(var value in values)Add(cmd,value.Item1,value.Item2);await cmd.ExecuteNonQueryAsync(ct);}finally{if(close)await cn.CloseAsync();}}
    private static void Add(DbCommand cmd,string name,object? value){var p=cmd.CreateParameter();p.ParameterName=name;p.Value=value??DBNull.Value;cmd.Parameters.Add(p);}
}
