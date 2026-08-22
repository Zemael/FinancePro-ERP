namespace FinancePro.Platform.Investments;

public interface IInvestmentService
{
    Task<IReadOnlyList<InvestmentPlanRow>> ListAsync(int companyId, CancellationToken cancellationToken = default);
    Task SaveAsync(SaveInvestmentRequest request, CancellationToken cancellationToken = default);
    Task SetStatusAsync(int companyId, int id, string status, CancellationToken cancellationToken = default);
    Task ArchiveAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestmentFinancingRow>> ListFinancingsAsync(int companyId, int? investmentId = null, CancellationToken cancellationToken = default);
    Task SaveFinancingAsync(SaveInvestmentFinancingRequest request, CancellationToken cancellationToken = default);
    Task ArchiveFinancingAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestmentFinancingPaymentRow>> ListFinancingPaymentsAsync(int companyId, int financingId, CancellationToken cancellationToken = default);
    Task GeneratePaymentScheduleAsync(int companyId, int financingId, CancellationToken cancellationToken = default);
    Task SaveFinancingPaymentAsync(SaveInvestmentFinancingPaymentRequest request, CancellationToken cancellationToken = default);
    Task ArchiveFinancingPaymentAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestmentCashFlowRow>> ListCashFlowsAsync(int companyId, int investmentId, CancellationToken cancellationToken = default);
    Task SaveCashFlowAsync(SaveInvestmentCashFlowRequest request, CancellationToken cancellationToken = default);
    Task ArchiveCashFlowAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestmentEvaluationRow>> ListEvaluationsAsync(int companyId, int investmentId, CancellationToken cancellationToken = default);
    Task SaveEvaluationAsync(SaveInvestmentEvaluationRequest request, CancellationToken cancellationToken = default);
    Task ArchiveEvaluationAsync(int companyId, int id, CancellationToken cancellationToken = default);
}

public sealed record InvestmentPlanRow(int Id, string Code, string Name, string Type, string Department,
    string Manager, DateTime StartDate, DateTime? EndDate, decimal Budget, decimal ExecutedValue,
    decimal ExpectedReturn, string Status, string Notes, decimal PhysicalProgress, string FundingSource,
    decimal FinancedAmount, string NextMilestone, DateTime? NextMilestoneDate, string RiskLevel,
    string RiskDescription, string MitigationPlan, decimal TargetRoi, DateTime? TargetCompletionDate);

public sealed record SaveInvestmentRequest(int CompanyId, int? Id, string Code, string Name, string Type,
    string Department, string Manager, DateTime StartDate, DateTime? EndDate, decimal Budget,
    decimal ExecutedValue, decimal ExpectedReturn, string Status, string Notes, decimal PhysicalProgress,
    string FundingSource, decimal FinancedAmount, string NextMilestone, DateTime? NextMilestoneDate,
    string RiskLevel, string RiskDescription, string MitigationPlan, decimal TargetRoi, DateTime? TargetCompletionDate);

public sealed record InvestmentFinancingRow(int Id, int InvestmentId, string Investment, string ContractNumber,
    string Lender, string FinancingType, decimal ApprovedAmount, decimal DisbursedAmount,
    decimal OutstandingBalance, decimal AnnualRate, int TermMonths, decimal InstallmentAmount,
    DateTime StartDate, DateTime? FirstDueDate, string Status, string Notes);

public sealed record SaveInvestmentFinancingRequest(int CompanyId, int? Id, int InvestmentId,
    string ContractNumber, string Lender, string FinancingType, decimal ApprovedAmount,
    decimal DisbursedAmount, decimal OutstandingBalance, decimal AnnualRate, int TermMonths,
    decimal InstallmentAmount, DateTime StartDate, DateTime? FirstDueDate, string Status, string Notes);

public sealed record InvestmentFinancingPaymentRow(int Id, int FinancingId, int InstallmentNumber,
    DateTime DueDate, decimal PrincipalAmount, decimal InterestAmount, decimal AmountDue,
    decimal AmountPaid, DateTime? PaymentDate, string Status, string PaymentReference, string Notes);

public sealed record SaveInvestmentFinancingPaymentRequest(int CompanyId, int? Id, int FinancingId,
    int InstallmentNumber, DateTime DueDate, decimal PrincipalAmount, decimal InterestAmount,
    decimal AmountDue, decimal AmountPaid, DateTime? PaymentDate, string Status,
    string PaymentReference, string Notes);

public sealed record InvestmentCashFlowRow(int Id, int InvestmentId, DateTime FlowDate,
    string Description, string FlowType, decimal InflowAmount, decimal OutflowAmount,
    string Reference, string Notes)
{
    public decimal NetAmount => InflowAmount - OutflowAmount;
}

public sealed record SaveInvestmentCashFlowRequest(int CompanyId, int? Id, int InvestmentId,
    DateTime FlowDate, string Description, string FlowType, decimal InflowAmount,
    decimal OutflowAmount, string Reference, string Notes);

public sealed record InvestmentEvaluationRow(int Id, int InvestmentId, DateTime ReviewDate,
    string EvaluationType, decimal BenefitsRealized, decimal AdditionalCosts, decimal ResidualValue,
    decimal ObjectivesAchievement, string FinalRating, string Recommendation, string Status, string Notes)
{
    public decimal NetBenefit => BenefitsRealized + ResidualValue - AdditionalCosts;
}

public sealed record SaveInvestmentEvaluationRequest(int CompanyId, int? Id, int InvestmentId,
    DateTime ReviewDate, string EvaluationType, decimal BenefitsRealized, decimal AdditionalCosts,
    decimal ResidualValue, decimal ObjectivesAchievement, string FinalRating,
    string Recommendation, string Status, string Notes);
