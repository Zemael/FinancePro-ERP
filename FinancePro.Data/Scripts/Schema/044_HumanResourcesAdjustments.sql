IF OBJECT_ID('dbo.HumanResourcesEmployeeAdjustments','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesEmployeeAdjustments(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, EmployeeId int NOT NULL,
  AdjustmentType nvarchar(30) NOT NULL, Category nvarchar(80) NOT NULL, Description nvarchar(200) NOT NULL,
  Amount decimal(18,2) NOT NULL DEFAULT 0, StartDate date NOT NULL, EndDate date NULL,
  Recurring bit NOT NULL DEFAULT 1, Notes nvarchar(500) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_HumanResourcesEmployeeAdjustments_Employee FOREIGN KEY(EmployeeId) REFERENCES dbo.HumanResourcesEmployees(Id),
  CONSTRAINT CK_HumanResourcesEmployeeAdjustments_Values CHECK(AdjustmentType IN(N'Abono',N'Desconto') AND Amount>0)
 );
 CREATE INDEX IX_HumanResourcesEmployeeAdjustments_Period ON dbo.HumanResourcesEmployeeAdjustments(CompanyId,EmployeeId,StartDate,EndDate,Active);
END;
