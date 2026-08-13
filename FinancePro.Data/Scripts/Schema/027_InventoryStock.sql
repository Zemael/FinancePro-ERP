IF OBJECT_ID(N'[Produtos]', N'U') IS NULL
BEGIN
 CREATE TABLE [Produtos]([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,[Codigo] nvarchar(40) NOT NULL,[Nome] nvarchar(180) NOT NULL,[Categoria] nvarchar(100) NOT NULL DEFAULT '',[Unidade] nvarchar(20) NOT NULL DEFAULT 'UN',[StockMinimo] decimal(18,3) NOT NULL DEFAULT 0,[StockAtual] decimal(18,3) NOT NULL DEFAULT 0,[CustoMedio] decimal(18,4) NOT NULL DEFAULT 0,[Localizacao] nvarchar(100) NULL,[EmpresaId] int NOT NULL,[DataCriacao] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),[DataAtualizacao] datetime2 NULL,[Ativo] bit NOT NULL DEFAULT 1,CONSTRAINT [FK_Produtos_Empresas] FOREIGN KEY([EmpresaId]) REFERENCES [Empresas]([Id]));
 CREATE UNIQUE INDEX [IX_Produtos_EmpresaId_Codigo] ON [Produtos]([EmpresaId],[Codigo]);
END;
IF OBJECT_ID(N'[MovimentosStock]', N'U') IS NULL
BEGIN
 CREATE TABLE [MovimentosStock]([Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,[ProdutoId] int NOT NULL,[Data] datetime2 NOT NULL DEFAULT GETDATE(),[Tipo] int NOT NULL,[Quantidade] decimal(18,3) NOT NULL,[CustoUnitario] decimal(18,4) NOT NULL DEFAULT 0,[SaldoApos] decimal(18,3) NOT NULL,[DocumentoReferencia] nvarchar(80) NULL,[Observacao] nvarchar(500) NULL,[EmpresaId] int NOT NULL,[DataCriacao] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),[DataAtualizacao] datetime2 NULL,[Ativo] bit NOT NULL DEFAULT 1,CONSTRAINT [FK_MovimentosStock_Produtos] FOREIGN KEY([ProdutoId]) REFERENCES [Produtos]([Id]),CONSTRAINT [FK_MovimentosStock_Empresas] FOREIGN KEY([EmpresaId]) REFERENCES [Empresas]([Id]));
 CREATE INDEX [IX_MovimentosStock_EmpresaId_Data] ON [MovimentosStock]([EmpresaId],[Data]);
END;
