-- FinancePro v6.12.0 - liquidacoes parciais
IF COL_LENGTH('dbo.ContasPagar', 'ValorLiquidado') IS NULL
    ALTER TABLE dbo.ContasPagar ADD ValorLiquidado decimal(18,2) NOT NULL CONSTRAINT DF_ContasPagar_ValorLiquidado DEFAULT(0);
IF COL_LENGTH('dbo.ContasReceber', 'ValorLiquidado') IS NULL
    ALTER TABLE dbo.ContasReceber ADD ValorLiquidado decimal(18,2) NOT NULL CONSTRAINT DF_ContasReceber_ValorLiquidado DEFAULT(0);
