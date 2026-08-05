IF OBJECT_ID('dbo.AccountingPeriods','U') IS NULL
BEGIN
CREATE TABLE dbo.AccountingPeriods(
 Id INT IDENTITY(1,1) PRIMARY KEY, CompanyId INT NOT NULL, FiscalYear INT NOT NULL, Month INT NOT NULL,
 StartDate DATE NOT NULL, EndDate DATE NOT NULL, Status NVARCHAR(20) NOT NULL CONSTRAINT DF_AccountingPeriods_Status DEFAULT 'Aberto',
 Active BIT NOT NULL CONSTRAINT DF_AccountingPeriods_Active DEFAULT 1, CreatedAt DATETIME2 NOT NULL, UpdatedAt DATETIME2 NOT NULL,
 CONSTRAINT CK_AccountingPeriods_Month CHECK(Month BETWEEN 1 AND 12),
 CONSTRAINT CK_AccountingPeriods_Status CHECK(Status IN ('Aberto','Fechado','Bloqueado')),
 CONSTRAINT UQ_AccountingPeriods UNIQUE(CompanyId,FiscalYear,Month));
CREATE INDEX IX_AccountingPeriods_CompanyYear ON dbo.AccountingPeriods(CompanyId,FiscalYear,Month);
END;
