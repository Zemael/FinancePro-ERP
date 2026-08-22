IF COL_LENGTH('dbo.ContasReceber','DescontoGeral') IS NULL
    ALTER TABLE dbo.ContasReceber ADD DescontoGeral decimal(18,2) NOT NULL CONSTRAINT DF_ContasReceber_DescontoGeral DEFAULT(0);
IF COL_LENGTH('dbo.ContasReceber','Frete') IS NULL
    ALTER TABLE dbo.ContasReceber ADD Frete decimal(18,2) NOT NULL CONSTRAINT DF_ContasReceber_Frete DEFAULT(0);
IF COL_LENGTH('dbo.ContasReceber','OutrasDespesas') IS NULL
    ALTER TABLE dbo.ContasReceber ADD OutrasDespesas decimal(18,2) NOT NULL CONSTRAINT DF_ContasReceber_OutrasDespesas DEFAULT(0);
IF COL_LENGTH('dbo.ContasReceber','Observacoes') IS NULL
    ALTER TABLE dbo.ContasReceber ADD Observacoes nvarchar(1000) NULL;
