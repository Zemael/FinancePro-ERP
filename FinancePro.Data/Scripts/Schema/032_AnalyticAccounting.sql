IF OBJECT_ID('dbo.Departments','U') IS NULL
BEGIN
 CREATE TABLE dbo.Departments(Id INT IDENTITY(1,1) PRIMARY KEY,CompanyId INT NOT NULL,Code NVARCHAR(30) NOT NULL,Name NVARCHAR(150) NOT NULL,Active BIT NOT NULL DEFAULT(1),CreatedAt DATETIME2 NOT NULL,UpdatedAt DATETIME2 NOT NULL,CONSTRAINT UQ_Departments_Company_Code UNIQUE(CompanyId,Code));
END;
IF COL_LENGTH('dbo.CostCenters','DepartmentId') IS NULL EXEC('ALTER TABLE dbo.CostCenters ADD DepartmentId INT NULL;');
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_CostCenters_Departments') ALTER TABLE dbo.CostCenters ADD CONSTRAINT FK_CostCenters_Departments FOREIGN KEY(DepartmentId) REFERENCES dbo.Departments(Id);
