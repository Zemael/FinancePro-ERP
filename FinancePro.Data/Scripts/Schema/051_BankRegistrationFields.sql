IF COL_LENGTH(N'dbo.Bancos', N'Sigla') IS NULL
    ALTER TABLE dbo.Bancos ADD Sigla nvarchar(20) NULL;

IF COL_LENGTH(N'dbo.Bancos', N'Endereco') IS NULL
    ALTER TABLE dbo.Bancos ADD Endereco nvarchar(250) NULL;

IF COL_LENGTH(N'dbo.Bancos', N'Contacto') IS NULL
    ALTER TABLE dbo.Bancos ADD Contacto nvarchar(50) NULL;
