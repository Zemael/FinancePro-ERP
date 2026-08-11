SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.FiscalYearClosingSettings','U') IS NULL
BEGIN
 CREATE TABLE dbo.FiscalYearClosingSettings(
  CompanyId INT NOT NULL CONSTRAINT PK_FiscalYearClosingSettings PRIMARY KEY,
  ResultAccountId INT NOT NULL, RetainedEarningsAccountId INT NOT NULL,
  UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_FYCS_UpdatedAt DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_FYCS_Result FOREIGN KEY(ResultAccountId) REFERENCES dbo.EnterpriseChartAccounts(Id),
  CONSTRAINT FK_FYCS_Retained FOREIGN KEY(RetainedEarningsAccountId) REFERENCES dbo.EnterpriseChartAccounts(Id));
END;
IF OBJECT_ID('dbo.AccountClosingClassifications','U') IS NULL
BEGIN
 CREATE TABLE dbo.AccountClosingClassifications(
  CompanyId INT NOT NULL, AccountId INT NOT NULL, StatementClass NVARCHAR(20) NOT NULL,
  UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_ACC_UpdatedAt DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_AccountClosingClassifications PRIMARY KEY(CompanyId,AccountId),
  CONSTRAINT FK_ACC_Account FOREIGN KEY(AccountId) REFERENCES dbo.EnterpriseChartAccounts(Id),
  CONSTRAINT CK_ACC_Class CHECK(StatementClass IN ('Ativo','Passivo','PatrimonioLiquido','Receita','Despesa')));
END;
IF OBJECT_ID('dbo.FiscalYearBalanceCarryForwards','U') IS NULL
BEGIN
 CREATE TABLE dbo.FiscalYearBalanceCarryForwards(
  Id INT IDENTITY(1,1) PRIMARY KEY, CompanyId INT NOT NULL, SourceFiscalYear INT NOT NULL, TargetFiscalYear INT NOT NULL,
  ClosingEntryId INT NULL, OpeningEntryId INT NULL, CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_FYBCF_CreatedAt DEFAULT SYSUTCDATETIME(),
  CONSTRAINT UQ_FYBCF UNIQUE(CompanyId,SourceFiscalYear),
  CONSTRAINT FK_FYBCF_Close FOREIGN KEY(ClosingEntryId) REFERENCES dbo.AccountingEntries(Id),
  CONSTRAINT FK_FYBCF_Open FOREIGN KEY(OpeningEntryId) REFERENCES dbo.AccountingEntries(Id));
END;
COMMIT TRANSACTION;
