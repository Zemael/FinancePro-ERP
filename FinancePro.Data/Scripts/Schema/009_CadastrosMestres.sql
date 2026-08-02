/* FinancePro ERP — Cadastros Mestres v0.6
   Execute apenas se não utilizar migrations do Entity Framework. */

IF OBJECT_ID('dbo.Moedas', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Moedas
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Moedas PRIMARY KEY,
        CodigoIso NVARCHAR(3) NOT NULL,
        Nome NVARCHAR(100) NOT NULL,
        Simbolo NVARCHAR(10) NOT NULL,
        CasasDecimais INT NOT NULL CONSTRAINT DF_Moedas_CasasDecimais DEFAULT(0),
        DataCriacao DATETIME2 NOT NULL CONSTRAINT DF_Moedas_DataCriacao DEFAULT(SYSUTCDATETIME()),
        DataAtualizacao DATETIME2 NULL,
        Ativo BIT NOT NULL CONSTRAINT DF_Moedas_Ativo DEFAULT(1),
        CONSTRAINT UQ_Moedas_CodigoIso UNIQUE (CodigoIso)
    );
END;

IF OBJECT_ID('dbo.ExerciciosFinanceiros', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExerciciosFinanceiros
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExerciciosFinanceiros PRIMARY KEY,
        EmpresaId INT NOT NULL,
        Ano INT NOT NULL,
        DataInicio DATE NOT NULL,
        DataFim DATE NOT NULL,
        Padrao BIT NOT NULL CONSTRAINT DF_Exercicios_Padrao DEFAULT(0),
        Encerrado BIT NOT NULL CONSTRAINT DF_Exercicios_Encerrado DEFAULT(0),
        DataCriacao DATETIME2 NOT NULL CONSTRAINT DF_Exercicios_DataCriacao DEFAULT(SYSUTCDATETIME()),
        DataAtualizacao DATETIME2 NULL,
        Ativo BIT NOT NULL CONSTRAINT DF_Exercicios_Ativo DEFAULT(1),
        CONSTRAINT FK_Exercicios_Empresas FOREIGN KEY (EmpresaId) REFERENCES dbo.Empresas(Id),
        CONSTRAINT UQ_Exercicios_Empresa_Ano UNIQUE (EmpresaId, Ano),
        CONSTRAINT CK_Exercicios_Datas CHECK (DataFim >= DataInicio)
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Moedas WHERE CodigoIso = 'XOF')
    INSERT INTO dbo.Moedas (CodigoIso, Nome, Simbolo, CasasDecimais, Ativo)
    VALUES ('XOF', 'Franco CFA da África Ocidental', 'FCFA', 0, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Moedas WHERE CodigoIso = 'EUR')
    INSERT INTO dbo.Moedas (CodigoIso, Nome, Simbolo, CasasDecimais, Ativo)
    VALUES ('EUR', 'Euro', '€', 2, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Moedas WHERE CodigoIso = 'USD')
    INSERT INTO dbo.Moedas (CodigoIso, Nome, Simbolo, CasasDecimais, Ativo)
    VALUES ('USD', 'Dólar dos Estados Unidos', '$', 2, 1);
