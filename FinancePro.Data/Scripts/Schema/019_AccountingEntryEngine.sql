SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.AccountingEntries','U') IS NULL
BEGIN
    CREATE TABLE dbo.AccountingEntries(
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CompanyId INT NOT NULL,
        EntryDate DATE NOT NULL,
        DocumentNumber NVARCHAR(50) NOT NULL,
        Reference NVARCHAR(100) NOT NULL CONSTRAINT DF_AccountingEntries_Reference DEFAULT(''),
        Description NVARCHAR(250) NOT NULL,
        SourceModule NVARCHAR(50) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_AccountingEntries_Status DEFAULT('Rascunho'),
        CreatedBy INT NOT NULL,
        CreatedByName NVARCHAR(150) NOT NULL,
        PostedBy INT NULL,
        PostedByName NVARCHAR(150) NULL,
        PostedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AccountingEntries_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_AccountingEntries_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_AccountingEntries_Company_Document UNIQUE(CompanyId,DocumentNumber),
        CONSTRAINT CK_AccountingEntries_Status CHECK(Status IN ('Rascunho','Contabilizado','Estornado'))
    );
    CREATE INDEX IX_AccountingEntries_Company_Date ON dbo.AccountingEntries(CompanyId,EntryDate DESC);
END;

IF OBJECT_ID('dbo.AccountingEntryLines','U') IS NULL
BEGIN
    CREATE TABLE dbo.AccountingEntryLines(
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AccountingEntryId INT NOT NULL,
        AccountId INT NOT NULL,
        CostCenterId INT NULL,
        Description NVARCHAR(250) NOT NULL CONSTRAINT DF_AccountingEntryLines_Description DEFAULT(''),
        Debit DECIMAL(18,2) NOT NULL CONSTRAINT DF_AccountingEntryLines_Debit DEFAULT(0),
        Credit DECIMAL(18,2) NOT NULL CONSTRAINT DF_AccountingEntryLines_Credit DEFAULT(0),
        CONSTRAINT FK_AccountingEntryLines_Entry FOREIGN KEY(AccountingEntryId) REFERENCES dbo.AccountingEntries(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AccountingEntryLines_Account FOREIGN KEY(AccountId) REFERENCES dbo.EnterpriseChartAccounts(Id),
        CONSTRAINT FK_AccountingEntryLines_CostCenter FOREIGN KEY(CostCenterId) REFERENCES dbo.CostCenters(Id),
        CONSTRAINT CK_AccountingEntryLines_Values CHECK(Debit>=0 AND Credit>=0 AND ((Debit>0 AND Credit=0) OR (Credit>0 AND Debit=0)))
    );
    CREATE INDEX IX_AccountingEntryLines_Entry ON dbo.AccountingEntryLines(AccountingEntryId);
    CREATE INDEX IX_AccountingEntryLines_Account ON dbo.AccountingEntryLines(AccountId);
END;

COMMIT TRANSACTION;
