SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.ConsolidationGroups','U') IS NULL
BEGIN
 CREATE TABLE dbo.ConsolidationGroups(
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsolidationGroups PRIMARY KEY,
  Name NVARCHAR(160) NOT NULL,FiscalYear INT NOT NULL,
  CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ConsolidationGroups_CreatedAt DEFAULT SYSUTCDATETIME(),
  UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_ConsolidationGroups_UpdatedAt DEFAULT SYSUTCDATETIME(),
  CONSTRAINT UQ_ConsolidationGroups_NameYear UNIQUE(Name,FiscalYear));
END;
IF OBJECT_ID('dbo.ConsolidationGroupCompanies','U') IS NULL
BEGIN
 CREATE TABLE dbo.ConsolidationGroupCompanies(
  GroupId INT NOT NULL,CompanyId INT NOT NULL,
  CONSTRAINT PK_ConsolidationGroupCompanies PRIMARY KEY(GroupId,CompanyId),
  CONSTRAINT FK_CGC_Group FOREIGN KEY(GroupId) REFERENCES dbo.ConsolidationGroups(Id) ON DELETE CASCADE,
  CONSTRAINT FK_CGC_Company FOREIGN KEY(CompanyId) REFERENCES dbo.Empresas(Id));
END;
IF OBJECT_ID('dbo.ConsolidationRuns','U') IS NULL
BEGIN
 CREATE TABLE dbo.ConsolidationRuns(
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsolidationRuns PRIMARY KEY,
  GroupId INT NOT NULL,FromDate DATE NOT NULL,ToDate DATE NOT NULL,Status NVARCHAR(20) NOT NULL CONSTRAINT DF_ConsolidationRuns_Status DEFAULT 'Rascunho',
  CreatedBy INT NOT NULL,CreatedByName NVARCHAR(200) NOT NULL,CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ConsolidationRuns_CreatedAt DEFAULT SYSUTCDATETIME(),
  ValidatedAt DATETIME2 NULL,ConsolidatedAt DATETIME2 NULL,ClosedAt DATETIME2 NULL,
  CONSTRAINT FK_ConsolidationRuns_Group FOREIGN KEY(GroupId) REFERENCES dbo.ConsolidationGroups(Id),
  CONSTRAINT CK_ConsolidationRuns_Status CHECK(Status IN ('Rascunho','Validado','Consolidado','Fechado')),
  CONSTRAINT CK_ConsolidationRuns_Period CHECK(FromDate<=ToDate));
END;
IF OBJECT_ID('dbo.ConsolidationEliminations','U') IS NULL
BEGIN
 CREATE TABLE dbo.ConsolidationEliminations(
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsolidationEliminations PRIMARY KEY,
  RunId INT NOT NULL,AccountCode NVARCHAR(50) NOT NULL,Description NVARCHAR(500) NOT NULL,Debit DECIMAL(18,2) NOT NULL CONSTRAINT DF_ConsolidationEliminations_Debit DEFAULT 0,
  Credit DECIMAL(18,2) NOT NULL CONSTRAINT DF_ConsolidationEliminations_Credit DEFAULT 0,Reference NVARCHAR(120) NOT NULL,
  CreatedBy INT NOT NULL,CreatedByName NVARCHAR(200) NOT NULL,CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ConsolidationEliminations_CreatedAt DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_ConsolidationEliminations_Run FOREIGN KEY(RunId) REFERENCES dbo.ConsolidationRuns(Id) ON DELETE CASCADE,
  CONSTRAINT CK_ConsolidationEliminations_Values CHECK(Debit>=0 AND Credit>=0 AND ((Debit>0 AND Credit=0) OR (Credit>0 AND Debit=0))));
END;
IF OBJECT_ID('dbo.ConsolidationRunHistory','U') IS NULL
BEGIN
 CREATE TABLE dbo.ConsolidationRunHistory(
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConsolidationRunHistory PRIMARY KEY,
  RunId INT NOT NULL,PreviousStatus NVARCHAR(20) NOT NULL,NewStatus NVARCHAR(20) NOT NULL,UserId INT NOT NULL,UserName NVARCHAR(200) NOT NULL,
  CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ConsolidationRunHistory_CreatedAt DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_ConsolidationRunHistory_Run FOREIGN KEY(RunId) REFERENCES dbo.ConsolidationRuns(Id) ON DELETE CASCADE);
END;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.ConsolidationRuns') AND name='IX_ConsolidationRuns_Group_Period')
 CREATE INDEX IX_ConsolidationRuns_Group_Period ON dbo.ConsolidationRuns(GroupId,FromDate,ToDate);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.ConsolidationEliminations') AND name='IX_ConsolidationEliminations_Run')
 CREATE INDEX IX_ConsolidationEliminations_Run ON dbo.ConsolidationEliminations(RunId,AccountCode);
COMMIT TRANSACTION;
