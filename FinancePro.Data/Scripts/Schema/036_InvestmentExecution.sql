IF COL_LENGTH('dbo.Investments','PhysicalProgress') IS NULL ALTER TABLE dbo.Investments ADD PhysicalProgress decimal(5,2) NOT NULL CONSTRAINT DF_Investments_PhysicalProgress DEFAULT 0;
IF COL_LENGTH('dbo.Investments','FundingSource') IS NULL ALTER TABLE dbo.Investments ADD FundingSource nvarchar(150) NULL;
IF COL_LENGTH('dbo.Investments','FinancedAmount') IS NULL ALTER TABLE dbo.Investments ADD FinancedAmount decimal(18,2) NOT NULL CONSTRAINT DF_Investments_FinancedAmount DEFAULT 0;
IF COL_LENGTH('dbo.Investments','NextMilestone') IS NULL ALTER TABLE dbo.Investments ADD NextMilestone nvarchar(250) NULL;
IF COL_LENGTH('dbo.Investments','NextMilestoneDate') IS NULL ALTER TABLE dbo.Investments ADD NextMilestoneDate date NULL;
