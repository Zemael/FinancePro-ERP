IF OBJECT_ID('dbo.HumanResourcesEmployeeLoans','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesEmployeeLoans(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, EmployeeId int NOT NULL,
  LoanType nvarchar(40) NOT NULL, Reference nvarchar(80) NULL, GrantedDate date NOT NULL,
  OriginalAmount decimal(18,2) NOT NULL, OutstandingBalance decimal(18,2) NOT NULL,
  InstallmentAmount decimal(18,2) NOT NULL, FirstDeductionDate date NOT NULL,
  Status nvarchar(30) NOT NULL DEFAULT N'Ativo', Notes nvarchar(500) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_HumanResourcesEmployeeLoans_Employee FOREIGN KEY(EmployeeId) REFERENCES dbo.HumanResourcesEmployees(Id),
  CONSTRAINT CK_HumanResourcesEmployeeLoans_Values CHECK(OriginalAmount>0 AND OutstandingBalance>=0 AND OutstandingBalance<=OriginalAmount AND InstallmentAmount>0),
  CONSTRAINT CK_HumanResourcesEmployeeLoans_Status CHECK(Status IN(N'Ativo',N'Suspenso',N'Liquidado'))
 );
 CREATE INDEX IX_HumanResourcesEmployeeLoans_Active ON dbo.HumanResourcesEmployeeLoans(CompanyId,EmployeeId,Status,FirstDeductionDate,Active);
END;

IF OBJECT_ID('dbo.HumanResourcesLoanPayments','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesLoanPayments(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, LoanId int NOT NULL, PayrollRunId int NOT NULL,
  Amount decimal(18,2) NOT NULL, Status nvarchar(20) NOT NULL DEFAULT N'Pendente', PaymentDate date NULL,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_HumanResourcesLoanPayments_Loan FOREIGN KEY(LoanId) REFERENCES dbo.HumanResourcesEmployeeLoans(Id),
  CONSTRAINT FK_HumanResourcesLoanPayments_Run FOREIGN KEY(PayrollRunId) REFERENCES dbo.HumanResourcesPayrollRuns(Id),
  CONSTRAINT UQ_HumanResourcesLoanPayments_Run UNIQUE(LoanId,PayrollRunId),
  CONSTRAINT CK_HumanResourcesLoanPayments_Values CHECK(Amount>0 AND Status IN(N'Pendente',N'Confirmado'))
 );
END;

IF COL_LENGTH('dbo.HumanResourcesPayrollLines','LoanDeduction') IS NULL
 ALTER TABLE dbo.HumanResourcesPayrollLines ADD LoanDeduction decimal(18,2) NOT NULL CONSTRAINT DF_HRPayrollLines_LoanDeduction DEFAULT 0;
