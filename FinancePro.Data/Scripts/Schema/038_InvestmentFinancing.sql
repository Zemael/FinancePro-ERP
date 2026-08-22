IF OBJECT_ID('dbo.InvestmentFinancings','U') IS NULL
BEGIN
 CREATE TABLE dbo.InvestmentFinancings(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, InvestmentId int NOT NULL,
  ContractNumber nvarchar(40) NOT NULL, Lender nvarchar(180) NOT NULL, FinancingType nvarchar(50) NOT NULL,
  ApprovedAmount decimal(18,2) NOT NULL DEFAULT 0, DisbursedAmount decimal(18,2) NOT NULL DEFAULT 0,
  OutstandingBalance decimal(18,2) NOT NULL DEFAULT 0, AnnualRate decimal(8,4) NOT NULL DEFAULT 0,
  TermMonths int NOT NULL DEFAULT 1, InstallmentAmount decimal(18,2) NOT NULL DEFAULT 0,
  StartDate date NOT NULL, FirstDueDate date NULL, Status nvarchar(30) NOT NULL DEFAULT N'Planeado',
  Notes nvarchar(1000) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_InvestmentFinancings_Investment FOREIGN KEY(InvestmentId) REFERENCES dbo.Investments(Id),
  CONSTRAINT UQ_InvestmentFinancings_Company_Contract UNIQUE(CompanyId,ContractNumber)
 );
 CREATE INDEX IX_InvestmentFinancings_Investment ON dbo.InvestmentFinancings(InvestmentId,Active);
END;
