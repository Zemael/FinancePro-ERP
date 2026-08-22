IF OBJECT_ID('dbo.Projects','U') IS NULL
BEGIN
 CREATE TABLE dbo.Projects(Id int IDENTITY PRIMARY KEY,CompanyId int NOT NULL,Code nvarchar(30) NOT NULL,Name nvarchar(160) NOT NULL,ClientId int NULL,CostCenterId int NULL,StartDate date NOT NULL,EndDate date NULL,BudgetRevenue decimal(18,2) NOT NULL DEFAULT 0,BudgetCost decimal(18,2) NOT NULL DEFAULT 0,Status nvarchar(20) NOT NULL DEFAULT 'Planeado',Active bit NOT NULL DEFAULT 1,CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),CONSTRAINT UQ_Projects_Company_Code UNIQUE(CompanyId,Code));
END;
IF OBJECT_ID('dbo.WorkOrders','U') IS NULL
BEGIN
 CREATE TABLE dbo.WorkOrders(Id int IDENTITY PRIMARY KEY,CompanyId int NOT NULL,ProjectId int NOT NULL,Number nvarchar(30) NOT NULL,Description nvarchar(300) NOT NULL,OrderDate date NOT NULL,DueDate date NULL,Status nvarchar(20) NOT NULL DEFAULT 'Aberta',EstimatedCost decimal(18,2) NOT NULL DEFAULT 0,ActualCost decimal(18,2) NOT NULL DEFAULT 0,CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),CONSTRAINT UQ_WorkOrders_Company_Number UNIQUE(CompanyId,Number));
END;
