-- FinancePro v6.14.0 - Compras e fornecedores avançados
IF COL_LENGTH('dbo.Compras','NumeroCotacao') IS NULL ALTER TABLE dbo.Compras ADD NumeroCotacao nvarchar(30) NULL;
IF COL_LENGTH('dbo.Compras','DataCotacao') IS NULL ALTER TABLE dbo.Compras ADD DataCotacao date NULL;
IF COL_LENGTH('dbo.Compras','NumeroOrdemCompra') IS NULL ALTER TABLE dbo.Compras ADD NumeroOrdemCompra nvarchar(30) NULL;
IF COL_LENGTH('dbo.Compras','DataOrdemCompra') IS NULL ALTER TABLE dbo.Compras ADD DataOrdemCompra date NULL;
IF COL_LENGTH('dbo.Compras','DataRececao') IS NULL ALTER TABLE dbo.Compras ADD DataRececao date NULL;
IF COL_LENGTH('dbo.Compras','DataFatura') IS NULL ALTER TABLE dbo.Compras ADD DataFatura date NULL;
IF COL_LENGTH('dbo.Compras','PrazoPagamentoDias') IS NULL ALTER TABLE dbo.Compras ADD PrazoPagamentoDias int NOT NULL CONSTRAINT DF_Compras_PrazoPagamentoDias DEFAULT(30);
IF COL_LENGTH('dbo.Fornecedores','PrazoPagamentoDias') IS NULL ALTER TABLE dbo.Fornecedores ADD PrazoPagamentoDias int NOT NULL CONSTRAINT DF_Fornecedores_PrazoPagamentoDias DEFAULT(30);
IF COL_LENGTH('dbo.Fornecedores','Avaliacao') IS NULL ALTER TABLE dbo.Fornecedores ADD Avaliacao decimal(3,2) NOT NULL CONSTRAINT DF_Fornecedores_Avaliacao DEFAULT(5);
