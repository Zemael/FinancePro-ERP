IF OBJECT_ID('dbo.InvestmentCashFlows','U') IS NULL
BEGIN
 CREATE TABLE dbo.InvestmentCashFlows(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, InvestmentId int NOT NULL,
  FlowDate date NOT NULL, Description nvarchar(250) NOT NULL,
  FlowType nvarchar(30) NOT NULL DEFAULT N'Previsto',
  InflowAmount decimal(18,2) NOT NULL DEFAULT 0, OutflowAmount decimal(18,2) NOT NULL DEFAULT 0,
  Reference nvarchar(80) NULL, Notes nvarchar(500) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_InvestmentCashFlows_Investment FOREIGN KEY(InvestmentId) REFERENCES dbo.Investments(Id),
  CONSTRAINT CK_InvestmentCashFlows_Amounts CHECK(InflowAmount>=0 AND OutflowAmount>=0 AND (InflowAmount>0 OR OutflowAmount>0))
 );
 CREATE INDEX IX_InvestmentCashFlows_InvestmentDate ON dbo.InvestmentCashFlows(CompanyId,InvestmentId,FlowDate,Active);
END;
