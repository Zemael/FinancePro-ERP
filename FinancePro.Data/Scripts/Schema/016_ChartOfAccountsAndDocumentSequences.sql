SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.EnterpriseChartAccounts','U') IS NULL
BEGIN
 CREATE TABLE dbo.EnterpriseChartAccounts(Id int IDENTITY(1,1) PRIMARY KEY,CompanyId int NOT NULL,Code nvarchar(40) NOT NULL,Name nvarchar(200) NOT NULL,ParentId int NULL,AccountType nvarchar(20) NOT NULL,Nature nvarchar(20) NOT NULL,AllowsPosting bit NOT NULL CONSTRAINT DF_ECA_AllowsPosting DEFAULT(1),RequiresCostCenter bit NOT NULL CONSTRAINT DF_ECA_RequiresCC DEFAULT(0),Active bit NOT NULL CONSTRAINT DF_ECA_Active DEFAULT(1),CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,CONSTRAINT UQ_ECA_Company_Code UNIQUE(CompanyId,Code),CONSTRAINT FK_ECA_Parent FOREIGN KEY(ParentId) REFERENCES dbo.EnterpriseChartAccounts(Id));
END;
IF OBJECT_ID('dbo.DocumentSequences','U') IS NULL
BEGIN
 CREATE TABLE dbo.DocumentSequences(Id int IDENTITY(1,1) PRIMARY KEY,CompanyId int NOT NULL,FiscalYear int NOT NULL,Module nvarchar(60) NOT NULL,Prefix nvarchar(30) NOT NULL,CurrentNumber bigint NOT NULL CONSTRAINT DF_DS_Current DEFAULT(0),Digits int NOT NULL CONSTRAINT DF_DS_Digits DEFAULT(6),RestartAnnually bit NOT NULL CONSTRAINT DF_DS_Restart DEFAULT(1),Active bit NOT NULL CONSTRAINT DF_DS_Active DEFAULT(1),CreatedAt datetime2 NOT NULL,UpdatedAt datetime2 NOT NULL,CONSTRAINT UQ_DS_Company_Year_Module UNIQUE(CompanyId,FiscalYear,Module),CONSTRAINT CK_DS_Digits CHECK(Digits BETWEEN 3 AND 12));
END;
COMMIT TRANSACTION;
