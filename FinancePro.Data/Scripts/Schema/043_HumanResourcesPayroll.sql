IF OBJECT_ID('dbo.HumanResourcesPayrollRuns','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesPayrollRuns(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, PayrollYear int NOT NULL, PayrollMonth int NOT NULL,
  Status nvarchar(30) NOT NULL DEFAULT N'Rascunho', GeneratedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  ApprovedAt datetime2 NULL, Notes nvarchar(500) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT UQ_HumanResourcesPayrollRuns_Period UNIQUE(CompanyId,PayrollYear,PayrollMonth),
  CONSTRAINT CK_HumanResourcesPayrollRuns_Period CHECK(PayrollYear BETWEEN 2000 AND 2200 AND PayrollMonth BETWEEN 1 AND 12)
 );
END;

IF OBJECT_ID('dbo.HumanResourcesPayrollLines','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesPayrollLines(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, PayrollRunId int NOT NULL, EmployeeId int NOT NULL,
  BaseSalary decimal(18,2) NOT NULL DEFAULT 0, Allowances decimal(18,2) NOT NULL DEFAULT 0,
  GrossAmount decimal(18,2) NOT NULL DEFAULT 0, Deductions decimal(18,2) NOT NULL DEFAULT 0,
  NetAmount decimal(18,2) NOT NULL DEFAULT 0, PaymentStatus nvarchar(30) NOT NULL DEFAULT N'Pendente',
  PaymentDate date NULL, PaymentReference nvarchar(80) NULL, Notes nvarchar(500) NULL,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_HumanResourcesPayrollLines_Run FOREIGN KEY(PayrollRunId) REFERENCES dbo.HumanResourcesPayrollRuns(Id),
  CONSTRAINT FK_HumanResourcesPayrollLines_Employee FOREIGN KEY(EmployeeId) REFERENCES dbo.HumanResourcesEmployees(Id),
  CONSTRAINT UQ_HumanResourcesPayrollLines_Employee UNIQUE(PayrollRunId,EmployeeId),
  CONSTRAINT CK_HumanResourcesPayrollLines_Values CHECK(BaseSalary>=0 AND Allowances>=0 AND GrossAmount>=0 AND Deductions>=0)
 );
 CREATE INDEX IX_HumanResourcesPayrollLines_Run ON dbo.HumanResourcesPayrollLines(CompanyId,PayrollRunId,PaymentStatus);
END;
