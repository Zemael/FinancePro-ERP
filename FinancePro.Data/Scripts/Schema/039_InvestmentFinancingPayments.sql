IF OBJECT_ID('dbo.InvestmentFinancingPayments','U') IS NULL
BEGIN
 CREATE TABLE dbo.InvestmentFinancingPayments(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, FinancingId int NOT NULL,
  InstallmentNumber int NOT NULL, DueDate date NOT NULL,
  PrincipalAmount decimal(18,2) NOT NULL DEFAULT 0, InterestAmount decimal(18,2) NOT NULL DEFAULT 0,
  AmountDue decimal(18,2) NOT NULL DEFAULT 0, AmountPaid decimal(18,2) NOT NULL DEFAULT 0,
  PaymentDate date NULL, Status nvarchar(30) NOT NULL DEFAULT N'Pendente',
  PaymentReference nvarchar(80) NULL, Notes nvarchar(500) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_InvestmentFinancingPayments_Financing FOREIGN KEY(FinancingId) REFERENCES dbo.InvestmentFinancings(Id),
  CONSTRAINT UQ_InvestmentFinancingPayments_Number UNIQUE(CompanyId,FinancingId,InstallmentNumber)
 );
 CREATE INDEX IX_InvestmentFinancingPayments_DueDate ON dbo.InvestmentFinancingPayments(CompanyId,DueDate,Status,Active);
END;
