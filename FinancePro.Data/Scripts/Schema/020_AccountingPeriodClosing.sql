IF OBJECT_ID('dbo.AccountingPeriodClosingHistory','U') IS NULL
BEGIN
 CREATE TABLE dbo.AccountingPeriodClosingHistory(
  Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountingPeriodClosingHistory PRIMARY KEY,
  CompanyId INT NOT NULL, AccountingPeriodId INT NOT NULL, Operation NVARCHAR(30) NOT NULL,
  PreviousStatus NVARCHAR(20) NOT NULL, NewStatus NVARCHAR(20) NOT NULL,
  UserId INT NOT NULL, UserName NVARCHAR(200) NOT NULL, Reason NVARCHAR(1000) NOT NULL,
  CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AccountingPeriodClosingHistory_CreatedAt DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_AccountingPeriodClosingHistory_Period FOREIGN KEY(AccountingPeriodId) REFERENCES dbo.AccountingPeriods(Id));
 CREATE INDEX IX_AccountingPeriodClosingHistory_Period ON dbo.AccountingPeriodClosingHistory(CompanyId,AccountingPeriodId,CreatedAt DESC);
END;
