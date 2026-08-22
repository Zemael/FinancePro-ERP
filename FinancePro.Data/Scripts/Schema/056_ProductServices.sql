IF COL_LENGTH(N'dbo.Produtos', N'PrecoVenda') IS NULL
BEGIN
    ALTER TABLE dbo.Produtos ADD PrecoVenda decimal(18,2) NOT NULL
        CONSTRAINT DF_Produtos_PrecoVenda DEFAULT(0);
END;

IF COL_LENGTH(N'dbo.Produtos', N'ControlaStock') IS NULL
BEGIN
    ALTER TABLE dbo.Produtos ADD ControlaStock bit NOT NULL
        CONSTRAINT DF_Produtos_ControlaStock DEFAULT(1);
END;
