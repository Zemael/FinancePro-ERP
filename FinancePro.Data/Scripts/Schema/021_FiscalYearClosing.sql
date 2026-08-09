IF OBJECT_ID('dbo.FiscalYearClosingHistory','U') IS NULL
BEGIN
 CREATE TABLE dbo.FiscalYearClosingHistory(
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FiscalYearClosingHistory PRIMARY KEY,
  CompanyId INT NOT NULL,FiscalYearId INT NOT NULL,FiscalYear INT NOT NULL,Operation NVARCHAR(30) NOT NULL,
  UserId INT NOT NULL,UserName NVARCHAR(150) NOT NULL,Reason NVARCHAR(500) NOT NULL,
  CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_FiscalYearClosingHistory_CreatedAt DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_FiscalYearClosingHistory_Year FOREIGN KEY(FiscalYearId) REFERENCES dbo.ExerciciosFinanceiros(Id));
 CREATE INDEX IX_FiscalYearClosingHistory_Year ON dbo.FiscalYearClosingHistory(CompanyId,FiscalYearId,CreatedAt DESC);
END;
