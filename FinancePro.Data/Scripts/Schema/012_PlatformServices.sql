IF OBJECT_ID(N'dbo.SequenciasDocumentos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SequenciasDocumentos
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SequenciasDocumentos PRIMARY KEY,
        EmpresaId int NOT NULL,
        Modulo nvarchar(50) NOT NULL,
        Prefixo nvarchar(20) NOT NULL,
        Ano int NOT NULL,
        UltimoNumero int NOT NULL CONSTRAINT DF_SequenciasDocumentos_UltimoNumero DEFAULT(0),
        Digitos int NOT NULL CONSTRAINT DF_SequenciasDocumentos_Digitos DEFAULT(6),
        ReiniciarAnualmente bit NOT NULL CONSTRAINT DF_SequenciasDocumentos_Reiniciar DEFAULT(1),
        DataCriacao datetime2 NOT NULL CONSTRAINT DF_SequenciasDocumentos_DataCriacao DEFAULT(SYSUTCDATETIME()),
        DataAtualizacao datetime2 NULL,
        Ativo bit NOT NULL CONSTRAINT DF_SequenciasDocumentos_Ativo DEFAULT(1),
        CONSTRAINT UQ_SequenciasDocumentos_EmpresaModuloAno UNIQUE(EmpresaId, Modulo, Ano),
        CONSTRAINT CK_SequenciasDocumentos_Digitos CHECK(Digitos BETWEEN 3 AND 12)
    );
END;
