namespace FinancePro.Platform.HumanResources;

public interface IHumanResourcesService
{
    Task<IReadOnlyList<EmployeeFinancialRow>> ListEmployeesAsync(int companyId, CancellationToken cancellationToken = default);
    Task SaveEmployeeAsync(SaveEmployeeFinancialRequest request, CancellationToken cancellationToken = default);
    Task ArchiveEmployeeAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayrollRunRow>> ListPayrollRunsAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayrollLineRow>> ListPayrollLinesAsync(int companyId, int payrollRunId, CancellationToken cancellationToken = default);
    Task GeneratePayrollAsync(int companyId, int year, int month, string notes, CancellationToken cancellationToken = default);
    Task ApprovePayrollAsync(int companyId, int payrollRunId, CancellationToken cancellationToken = default);
    Task RegisterPayrollPaymentAsync(int companyId, int payrollLineId, DateTime paymentDate, string reference, string notes, CancellationToken cancellationToken = default);
    Task PayEntirePayrollAsync(int companyId, int payrollRunId, DateTime paymentDate, string reference, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeAdjustmentRow>> ListEmployeeAdjustmentsAsync(int companyId, int employeeId, CancellationToken cancellationToken = default);
    Task SaveEmployeeAdjustmentAsync(SaveEmployeeAdjustmentRequest request, CancellationToken cancellationToken = default);
    Task ArchiveEmployeeAdjustmentAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeAbsenceRow>> ListEmployeeAbsencesAsync(int companyId, int employeeId, CancellationToken cancellationToken = default);
    Task SaveEmployeeAbsenceAsync(SaveEmployeeAbsenceRequest request, CancellationToken cancellationToken = default);
    Task SetEmployeeAbsenceStatusAsync(int companyId, int id, string status, CancellationToken cancellationToken = default);
    Task ArchiveEmployeeAbsenceAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeAttendanceRow>> ListEmployeeAttendanceAsync(int companyId, int employeeId, CancellationToken cancellationToken = default);
    Task SaveEmployeeAttendanceAsync(SaveEmployeeAttendanceRequest request, CancellationToken cancellationToken = default);
    Task SetEmployeeAttendanceStatusAsync(int companyId, int id, string status, CancellationToken cancellationToken = default);
    Task ArchiveEmployeeAttendanceAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeLeaveBalanceRow>> ListEmployeeLeaveBalancesAsync(int companyId, int employeeId, CancellationToken cancellationToken = default);
    Task SaveEmployeeLeaveBalanceAsync(SaveEmployeeLeaveBalanceRequest request, CancellationToken cancellationToken = default);
    Task ArchiveEmployeeLeaveBalanceAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayrollRuleRow>> ListPayrollRulesAsync(int companyId, CancellationToken cancellationToken = default);
    Task SavePayrollRuleAsync(SavePayrollRuleRequest request, CancellationToken cancellationToken = default);
    Task ArchivePayrollRuleAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeLoanRow>> ListEmployeeLoansAsync(int companyId, int employeeId, CancellationToken cancellationToken = default);
    Task SaveEmployeeLoanAsync(SaveEmployeeLoanRequest request, CancellationToken cancellationToken = default);
    Task SetEmployeeLoanStatusAsync(int companyId, int id, string status, CancellationToken cancellationToken = default);
    Task ArchiveEmployeeLoanAsync(int companyId, int id, CancellationToken cancellationToken = default);
}

public sealed record EmployeeFinancialRow(int Id, string EmployeeNumber, string FullName, string TaxId,
    string SocialSecurityNumber, string Department, string CostCenter, string JobTitle, string ContractType,
    DateTime HireDate, DateTime? EndDate, decimal BaseSalary, decimal Allowances, decimal Deductions,
    string BankName, string Iban, string Status, string Notes)
{
    public decimal NetMonthly => BaseSalary + Allowances - Deductions;
}

public sealed record SaveEmployeeFinancialRequest(int CompanyId, int? Id, string EmployeeNumber,
    string FullName, string TaxId, string SocialSecurityNumber, string Department, string CostCenter,
    string JobTitle, string ContractType, DateTime HireDate, DateTime? EndDate, decimal BaseSalary,
    decimal Allowances, decimal Deductions, string BankName, string Iban, string Status, string Notes);

public sealed record PayrollRunRow(int Id, int PayrollYear, int PayrollMonth, string Status,
    DateTime GeneratedAt, DateTime? ApprovedAt, string Notes, int EmployeeCount,
    decimal GrossAmount, decimal Deductions, decimal NetAmount)
{
    public string Period => $"{PayrollMonth:00}/{PayrollYear}";
}

public sealed record PayrollLineRow(int Id, int EmployeeId, string EmployeeNumber, string EmployeeName,
    string Department, decimal BaseSalary, decimal Allowances, decimal GrossAmount,
    decimal Deductions, decimal NetAmount, decimal IncomeTax, decimal EmployeeSocialSecurity,
    decimal EmployerSocialSecurity, string PaymentStatus, DateTime? PaymentDate,
    string PaymentReference, string Notes, decimal LoanDeduction);

public sealed record EmployeeAdjustmentRow(int Id, int EmployeeId, string AdjustmentType,
    string Category, string Description, decimal Amount, DateTime StartDate, DateTime? EndDate,
    bool Recurring, string Notes);

public sealed record SaveEmployeeAdjustmentRequest(int CompanyId, int? Id, int EmployeeId,
    string AdjustmentType, string Category, string Description, decimal Amount, DateTime StartDate,
    DateTime? EndDate, bool Recurring, string Notes);

public sealed record EmployeeAbsenceRow(int Id, int EmployeeId, string AbsenceType,
    DateTime StartDate, DateTime EndDate, decimal Days, bool Paid, decimal DeductionAmount,
    string Status, string Reason, string Notes);

public sealed record SaveEmployeeAbsenceRequest(int CompanyId, int? Id, int EmployeeId,
    string AbsenceType, DateTime StartDate, DateTime EndDate, decimal Days, bool Paid,
    decimal DeductionAmount, string Reason, string Notes);

public sealed record EmployeeAttendanceRow(int Id, int EmployeeId, DateTime WorkDate,
    TimeSpan EntryTime, TimeSpan? ExitTime, decimal RegularHours, decimal OvertimeHours,
    decimal OvertimeAmount, string Status, string Notes);

public sealed record SaveEmployeeAttendanceRequest(int CompanyId, int? Id, int EmployeeId,
    DateTime WorkDate, string EntryTime, string ExitTime, decimal RegularHours,
    decimal OvertimeHours, decimal OvertimeAmount, string Notes);

public sealed record EmployeeLeaveBalanceRow(int Id, int EmployeeId, int LeaveYear,
    decimal EntitledDays, decimal CarriedDays, decimal TakenDays, string Notes)
{
    public decimal AvailableDays => EntitledDays + CarriedDays - TakenDays;
}

public sealed record SaveEmployeeLeaveBalanceRequest(int CompanyId, int? Id, int EmployeeId,
    int LeaveYear, decimal EntitledDays, decimal CarriedDays, string Notes);

public sealed record PayrollRuleRow(int Id, string RuleType, string Name, decimal Rate,
    decimal FixedAmount, decimal MinimumBase, decimal? MaximumBase, DateTime StartDate,
    DateTime? EndDate, string Notes);

public sealed record SavePayrollRuleRequest(int CompanyId, int? Id, string RuleType, string Name,
    decimal Rate, decimal FixedAmount, decimal MinimumBase, decimal? MaximumBase,
    DateTime StartDate, DateTime? EndDate, string Notes);

public sealed record EmployeeLoanRow(int Id, int EmployeeId, string LoanType, string Reference,
    DateTime GrantedDate, decimal OriginalAmount, decimal OutstandingBalance,
    decimal InstallmentAmount, DateTime FirstDeductionDate, string Status, string Notes);

public sealed record SaveEmployeeLoanRequest(int CompanyId, int? Id, int EmployeeId,
    string LoanType, string Reference, DateTime GrantedDate, decimal OriginalAmount,
    decimal OutstandingBalance, decimal InstallmentAmount, DateTime FirstDeductionDate, string Notes);
