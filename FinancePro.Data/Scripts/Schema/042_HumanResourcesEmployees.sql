IF OBJECT_ID('dbo.HumanResourcesEmployees','U') IS NULL
BEGIN
 CREATE TABLE dbo.HumanResourcesEmployees(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, EmployeeNumber nvarchar(30) NOT NULL,
  FullName nvarchar(180) NOT NULL, TaxId nvarchar(40) NULL, SocialSecurityNumber nvarchar(40) NULL,
  Department nvarchar(100) NOT NULL, CostCenter nvarchar(80) NULL, JobTitle nvarchar(120) NOT NULL,
  ContractType nvarchar(50) NOT NULL, HireDate date NOT NULL, EndDate date NULL,
  BaseSalary decimal(18,2) NOT NULL DEFAULT 0, Allowances decimal(18,2) NOT NULL DEFAULT 0,
  Deductions decimal(18,2) NOT NULL DEFAULT 0, BankName nvarchar(120) NULL, Iban nvarchar(80) NULL,
  Status nvarchar(30) NOT NULL DEFAULT N'Ativo', Notes nvarchar(1000) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT UQ_HumanResourcesEmployees_Number UNIQUE(CompanyId,EmployeeNumber),
  CONSTRAINT CK_HumanResourcesEmployees_Values CHECK(BaseSalary>=0 AND Allowances>=0 AND Deductions>=0)
 );
 CREATE INDEX IX_HumanResourcesEmployees_Department ON dbo.HumanResourcesEmployees(CompanyId,Department,Status,Active);
END;
