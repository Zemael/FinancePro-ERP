IF OBJECT_ID('dbo.Investments','U') IS NULL
BEGIN
 CREATE TABLE dbo.Investments(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, Code nvarchar(30) NOT NULL,
  Name nvarchar(180) NOT NULL, Type nvarchar(80) NOT NULL, Department nvarchar(120) NOT NULL,
  Manager nvarchar(150) NOT NULL, StartDate date NOT NULL, EndDate date NULL,
  Budget decimal(18,2) NOT NULL DEFAULT 0, ExecutedValue decimal(18,2) NOT NULL DEFAULT 0,
  ExpectedReturn decimal(18,2) NOT NULL DEFAULT 0, Status nvarchar(30) NOT NULL DEFAULT N'Planeado',
  Notes nvarchar(1000) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT UQ_Investments_Company_Code UNIQUE(CompanyId,Code)
 );
END;
