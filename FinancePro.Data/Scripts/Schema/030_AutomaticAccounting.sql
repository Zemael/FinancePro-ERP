SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.AutomaticAccountingRules','U') IS NULL
BEGIN
 CREATE TABLE dbo.AutomaticAccountingRules(
  Id int IDENTITY(1,1) PRIMARY KEY, CompanyId int NOT NULL, EventCode nvarchar(40) NOT NULL,
  DebitAccountId int NOT NULL, CreditAccountId int NOT NULL, Active bit NOT NULL CONSTRAINT DF_AAR_Active DEFAULT(1),
  Description nvarchar(200) NOT NULL CONSTRAINT DF_AAR_Description DEFAULT(''), CreatedAt datetime2 NOT NULL CONSTRAINT DF_AAR_Created DEFAULT SYSUTCDATETIME(),
  CONSTRAINT UQ_AAR_Company_Event UNIQUE(CompanyId,EventCode),
  CONSTRAINT FK_AAR_Debit FOREIGN KEY(DebitAccountId) REFERENCES dbo.EnterpriseChartAccounts(Id),
  CONSTRAINT FK_AAR_Credit FOREIGN KEY(CreditAccountId) REFERENCES dbo.EnterpriseChartAccounts(Id));
END;
IF OBJECT_ID('dbo.AutomaticAccountingLinks','U') IS NULL
BEGIN
 CREATE TABLE dbo.AutomaticAccountingLinks(
  Id int IDENTITY(1,1) PRIMARY KEY, CompanyId int NOT NULL, EventCode nvarchar(40) NOT NULL, SourceId int NOT NULL,
  AccountingEntryId int NOT NULL, CreatedAt datetime2 NOT NULL CONSTRAINT DF_AAL_Created DEFAULT SYSUTCDATETIME(),
  CONSTRAINT UQ_AAL_Source UNIQUE(CompanyId,EventCode,SourceId),
  CONSTRAINT FK_AAL_Entry FOREIGN KEY(AccountingEntryId) REFERENCES dbo.AccountingEntries(Id));
END;
COMMIT TRANSACTION;
