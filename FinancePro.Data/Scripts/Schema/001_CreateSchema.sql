-- FinancePro ERP — Etapa 2: criação do schema base
-- SQL Server. Espelha o modelo Code First de FinancePro.Data.

CREATE TABLE Empresas (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nome            NVARCHAR(150)   NOT NULL,
    NIF             NVARCHAR(30)    NULL,
    Morada          NVARCHAR(250)   NULL,
    Telefone        NVARCHAR(30)    NULL,
    Email           NVARCHAR(150)   NULL,
    Moeda           NVARCHAR(10)    NOT NULL DEFAULT 'FCFA',
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL
);

CREATE TABLE Perfis (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nome            NVARCHAR(60)    NOT NULL,
    Descricao       NVARCHAR(250)   NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT UQ_Perfis_Nome UNIQUE (Nome)
);

CREATE TABLE Utilizadores (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    NomeCompleto    NVARCHAR(150)   NOT NULL,
    Email           NVARCHAR(150)   NOT NULL,
    PasswordHash    NVARCHAR(MAX)   NOT NULL,
    UltimoLogin     DATETIME2       NULL,
    PerfilId        INT             NOT NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT UQ_Utilizadores_Email UNIQUE (Email),
    CONSTRAINT FK_Utilizadores_Perfis FOREIGN KEY (PerfilId) REFERENCES Perfis(Id),
    CONSTRAINT FK_Utilizadores_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);

CREATE TABLE Bancos (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nome            NVARCHAR(120)   NOT NULL,
    CodigoSwift     NVARCHAR(20)    NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL
);

CREATE TABLE ContasBancarias (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    NumeroConta     NVARCHAR(40)    NOT NULL,
    IBAN            NVARCHAR(40)    NULL,
    Titular         NVARCHAR(150)   NOT NULL,
    SaldoInicial    DECIMAL(18,2)   NOT NULL DEFAULT 0,
    Moeda           NVARCHAR(10)    NOT NULL DEFAULT 'FCFA',
    BancoId         INT             NOT NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_ContasBancarias_Bancos FOREIGN KEY (BancoId) REFERENCES Bancos(Id),
    CONSTRAINT FK_ContasBancarias_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);

CREATE TABLE Caixas (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nome            NVARCHAR(100)   NOT NULL,
    SaldoInicial    DECIMAL(18,2)   NOT NULL DEFAULT 0,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_Caixas_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);

CREATE TABLE PlanoContas (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Codigo          NVARCHAR(20)    NOT NULL,
    Nome            NVARCHAR(150)   NOT NULL,
    Tipo            NVARCHAR(30)    NOT NULL, -- Ativo | Passivo | PatrimonioLiquido | Receita | Despesa
    ContaPaiId      INT             NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_PlanoContas_ContaPai FOREIGN KEY (ContaPaiId) REFERENCES PlanoContas(Id),
    CONSTRAINT FK_PlanoContas_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id),
    CONSTRAINT UQ_PlanoContas_Empresa_Codigo UNIQUE (EmpresaId, Codigo)
);

CREATE TABLE Categorias (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nome            NVARCHAR(100)   NOT NULL,
    Tipo            NVARCHAR(20)    NOT NULL, -- Receita | Despesa
    PlanoContasId   INT             NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_Categorias_PlanoContas FOREIGN KEY (PlanoContasId) REFERENCES PlanoContas(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Categorias_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);

CREATE TABLE Fornecedores (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nome            NVARCHAR(150)   NOT NULL,
    NIF             NVARCHAR(30)    NULL,
    Telefone        NVARCHAR(30)    NULL,
    Email           NVARCHAR(150)   NULL,
    Morada          NVARCHAR(250)   NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_Fornecedores_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);

CREATE TABLE Clientes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nome            NVARCHAR(150)   NOT NULL,
    NIF             NVARCHAR(30)    NULL,
    Telefone        NVARCHAR(30)    NULL,
    Email           NVARCHAR(150)   NULL,
    Morada          NVARCHAR(250)   NULL,
    EmpresaId       INT             NOT NULL,
    Ativo           BIT             NOT NULL DEFAULT 1,
    DataCriacao     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao DATETIME2       NULL,
    CONSTRAINT FK_Clientes_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id)
);
