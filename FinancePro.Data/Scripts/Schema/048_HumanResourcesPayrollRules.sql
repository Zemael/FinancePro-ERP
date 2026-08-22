IF OBJECT_ID('dbo.HumanResourcesPayrollRules','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesPayrollRules(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, RuleType nvarchar(60) NOT NULL,
  Name nvarchar(120) NOT NULL, Rate decimal(9,4) NOT NULL DEFAULT 0, FixedAmount decimal(18,2) NOT NULL DEFAULT 0,
  MinimumBase decimal(18,2) NOT NULL DEFAULT 0, MaximumBase decimal(18,2) NULL,
  StartDate date NOT NULL, EndDate date NULL, Notes nvarchar(500) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT CK_HumanResourcesPayrollRules_Values CHECK(Rate>=0 AND Rate<=100 AND FixedAmount>=0 AND MinimumBase>=0 AND (MaximumBase IS NULL OR MaximumBase>=MinimumBase) AND (EndDate IS NULL OR EndDate>=StartDate))
 );
 CREATE INDEX IX_HumanResourcesPayrollRules_Period ON dbo.HumanResourcesPayrollRules(CompanyId,StartDate,EndDate,RuleType,Active);
END;

IF COL_LENGTH('dbo.HumanResourcesPayrollLines','IncomeTax') IS NULL
 ALTER TABLE dbo.HumanResourcesPayrollLines ADD IncomeTax decimal(18,2) NOT NULL CONSTRAINT DF_HRPayrollLines_IncomeTax DEFAULT 0;
IF COL_LENGTH('dbo.HumanResourcesPayrollLines','EmployeeSocialSecurity') IS NULL
 ALTER TABLE dbo.HumanResourcesPayrollLines ADD EmployeeSocialSecurity decimal(18,2) NOT NULL CONSTRAINT DF_HRPayrollLines_EmployeeSS DEFAULT 0;
IF COL_LENGTH('dbo.HumanResourcesPayrollLines','EmployerSocialSecurity') IS NULL
 ALTER TABLE dbo.HumanResourcesPayrollLines ADD EmployerSocialSecurity decimal(18,2) NOT NULL CONSTRAINT DF_HRPayrollLines_EmployerSS DEFAULT 0;
