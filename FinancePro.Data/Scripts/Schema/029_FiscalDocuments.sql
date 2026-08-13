IF OBJECT_ID('dbo.DocumentosFiscais','U') IS NULL
BEGIN
    CREATE TABLE dbo.DocumentosFiscais(
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DocumentosFiscais PRIMARY KEY,
        ContaReceberId int NOT NULL,
        EmpresaId int NOT NULL,
        Tipo nvarchar(20) NOT NULL,
        Numero nvarchar(40) NOT NULL,
        DataEmissao datetime2 NOT NULL,
        BaseTributavel decimal(18,2) NOT NULL CONSTRAINT DF_DocFiscal_Base DEFAULT 0,
        ValorIva decimal(18,2) NOT NULL CONSTRAINT DF_DocFiscal_Iva DEFAULT 0,
        Total decimal(18,2) NOT NULL,
        Estado nvarchar(20) NOT NULL CONSTRAINT DF_DocFiscal_Estado DEFAULT 'Emitido',
        DocumentoOrigem nvarchar(40) NULL,
        Motivo nvarchar(250) NULL,
        DataCriacao datetime2 NOT NULL CONSTRAINT DF_DocFiscal_Criacao DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_DocumentosFiscais_ContasReceber FOREIGN KEY(ContaReceberId) REFERENCES dbo.ContasReceber(Id),
        CONSTRAINT FK_DocumentosFiscais_Empresas FOREIGN KEY(EmpresaId) REFERENCES dbo.Empresas(Id)
    );
    CREATE UNIQUE INDEX UX_DocumentosFiscais_EmpresaNumero ON dbo.DocumentosFiscais(EmpresaId,Numero);
    CREATE INDEX IX_DocumentosFiscais_Conta ON dbo.DocumentosFiscais(ContaReceberId,DataEmissao);
END;
