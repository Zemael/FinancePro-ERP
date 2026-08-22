IF OBJECT_ID('dbo.InvestmentEvaluations','U') IS NULL
BEGIN
 CREATE TABLE dbo.InvestmentEvaluations(
  Id int IDENTITY PRIMARY KEY, CompanyId int NOT NULL, InvestmentId int NOT NULL,
  ReviewDate date NOT NULL, EvaluationType nvarchar(40) NOT NULL DEFAULT N'Intercalar',
  BenefitsRealized decimal(18,2) NOT NULL DEFAULT 0, AdditionalCosts decimal(18,2) NOT NULL DEFAULT 0,
  ResidualValue decimal(18,2) NOT NULL DEFAULT 0, ObjectivesAchievement decimal(6,2) NOT NULL DEFAULT 0,
  FinalRating nvarchar(30) NOT NULL DEFAULT N'Satisfatório', Recommendation nvarchar(500) NULL,
  Status nvarchar(30) NOT NULL DEFAULT N'Em análise', Notes nvarchar(1000) NULL, Active bit NOT NULL DEFAULT 1,
  CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(), UpdatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_InvestmentEvaluations_Investment FOREIGN KEY(InvestmentId) REFERENCES dbo.Investments(Id),
  CONSTRAINT CK_InvestmentEvaluations_Values CHECK(BenefitsRealized>=0 AND AdditionalCosts>=0 AND ResidualValue>=0 AND ObjectivesAchievement BETWEEN 0 AND 100)
 );
 CREATE INDEX IX_InvestmentEvaluations_InvestmentDate ON dbo.InvestmentEvaluations(CompanyId,InvestmentId,ReviewDate,Active);
END;
