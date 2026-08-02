-- FinancePro ERP — Módulo 08 (Gestão Patrimonial) + Auditoria genérica

CREATE TABLE Bens (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    Codigo            NVARCHAR(20)    NOT NULL,
    NumeroPatrimonial NVARCHAR(30)    NOT NULL,
    Descricao         NVARCHAR(200)   NOT NULL,
    Categoria         NVARCHAR(100)   NULL,
    Marca             NVARCHAR(100)   NULL,
    Modelo            NVARCHAR(100)   NULL,
    Serie             NVARCHAR(100)   NULL,
    Localizacao       NVARCHAR(150)   NULL,
    Responsavel       NVARCHAR(150)   NULL,
    DataAquisicao     DATE            NOT NULL,
    ValorAquisicao    DECIMAL(18,2)   NOT NULL,
    VidaUtilAnos      INT             NOT NULL DEFAULT 0,
    MetodoDepreciacao NVARCHAR(20)    NOT NULL DEFAULT 'Linear',
    Estado            NVARCHAR(20)    NOT NULL DEFAULT 'Ativo',
    EmpresaId         INT             NOT NULL,
    Ativo             BIT             NOT NULL DEFAULT 1,
    DataCriacao       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    DataAtualizacao   DATETIME2       NULL,
    CONSTRAINT FK_Bens_Empresas FOREIGN KEY (EmpresaId) REFERENCES Empresas(Id),
    CONSTRAINT UQ_Bens_Empresa_NumeroPatrimonial UNIQUE (EmpresaId, NumeroPatrimonial)
);

CREATE TABLE LogsAuditoria (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    Data           DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    Entidade       NVARCHAR(60)    NOT NULL,
    RegistoId      INT             NOT NULL,
    Acao           NVARCHAR(30)    NOT NULL, -- Criar | Editar | Eliminar | Aprovar | Rejeitar
    Detalhe        NVARCHAR(MAX)   NULL,
    UtilizadorId   INT             NOT NULL,
    UtilizadorNome NVARCHAR(150)   NOT NULL,
    EmpresaId      INT             NOT NULL
);
CREATE INDEX IX_LogsAuditoria_Entidade_Registo ON LogsAuditoria (Entidade, RegistoId);
CREATE INDEX IX_LogsAuditoria_Empresa_Data ON LogsAuditoria (EmpresaId, Data);
