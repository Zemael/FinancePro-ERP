IF COL_LENGTH(N'dbo.Empresas', N'Logotipo') IS NULL
BEGIN
    ALTER TABLE dbo.Empresas ADD Logotipo varbinary(max) NULL;
END;
