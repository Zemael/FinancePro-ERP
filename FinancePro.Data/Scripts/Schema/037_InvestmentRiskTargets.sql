IF COL_LENGTH('dbo.Investments','RiskLevel') IS NULL ALTER TABLE dbo.Investments ADD RiskLevel nvarchar(20) NOT NULL CONSTRAINT DF_Investments_RiskLevel DEFAULT N'Médio';
IF COL_LENGTH('dbo.Investments','RiskDescription') IS NULL ALTER TABLE dbo.Investments ADD RiskDescription nvarchar(500) NULL;
IF COL_LENGTH('dbo.Investments','MitigationPlan') IS NULL ALTER TABLE dbo.Investments ADD MitigationPlan nvarchar(1000) NULL;
IF COL_LENGTH('dbo.Investments','TargetRoi') IS NULL ALTER TABLE dbo.Investments ADD TargetRoi decimal(8,2) NOT NULL CONSTRAINT DF_Investments_TargetRoi DEFAULT 0;
IF COL_LENGTH('dbo.Investments','TargetCompletionDate') IS NULL ALTER TABLE dbo.Investments ADD TargetCompletionDate date NULL;
