IF OBJECT_ID('dbo.HumanResourcesLeaveBalances','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesLeaveBalances(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, EmployeeId int NOT NULL, LeaveYear int NOT NULL,
  EntitledDays decimal(8,2) NOT NULL DEFAULT 22, CarriedDays decimal(8,2) NOT NULL DEFAULT 0,
  Notes nvarchar(500) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_HumanResourcesLeaveBalances_Employee FOREIGN KEY(EmployeeId) REFERENCES dbo.HumanResourcesEmployees(Id),
  CONSTRAINT CK_HumanResourcesLeaveBalances_Values CHECK(LeaveYear BETWEEN 2000 AND 2200 AND EntitledDays>=0 AND CarriedDays>=0)
 );
 CREATE INDEX IX_HumanResourcesLeaveBalances_Employee ON dbo.HumanResourcesLeaveBalances(CompanyId,EmployeeId,LeaveYear,Active);
 CREATE UNIQUE INDEX UX_HumanResourcesLeaveBalances_ActiveYear ON dbo.HumanResourcesLeaveBalances(CompanyId,EmployeeId,LeaveYear) WHERE Active=1;
END;
