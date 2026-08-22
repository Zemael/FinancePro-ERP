IF COL_LENGTH(N'dbo.Utilizadores', N'FotoPerfil') IS NULL
BEGIN
    ALTER TABLE dbo.Utilizadores ADD FotoPerfil varbinary(max) NULL;
END;
