IF OBJECT_ID('dbo.PermissoesPerfis', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PermissoesPerfis
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PermissoesPerfis PRIMARY KEY,
        PerfilId INT NOT NULL,
        Modulo NVARCHAR(80) NOT NULL,
        Consultar BIT NOT NULL CONSTRAINT DF_Permissoes_Consultar DEFAULT 0,
        Criar BIT NOT NULL CONSTRAINT DF_Permissoes_Criar DEFAULT 0,
        Editar BIT NOT NULL CONSTRAINT DF_Permissoes_Editar DEFAULT 0,
        Desativar BIT NOT NULL CONSTRAINT DF_Permissoes_Desativar DEFAULT 0,
        Aprovar BIT NOT NULL CONSTRAINT DF_Permissoes_Aprovar DEFAULT 0,
        Exportar BIT NOT NULL CONSTRAINT DF_Permissoes_Exportar DEFAULT 0,
        Administrar BIT NOT NULL CONSTRAINT DF_Permissoes_Administrar DEFAULT 0,
        DataCriacao DATETIME2 NOT NULL CONSTRAINT DF_Permissoes_DataCriacao DEFAULT SYSUTCDATETIME(),
        DataAtualizacao DATETIME2 NULL,
        Ativo BIT NOT NULL CONSTRAINT DF_Permissoes_Ativo DEFAULT 1,
        CONSTRAINT FK_PermissoesPerfis_Perfis FOREIGN KEY (PerfilId) REFERENCES dbo.Perfis(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_PermissoesPerfis_PerfilModulo UNIQUE (PerfilId, Modulo)
    );
END;
GO

-- O Administrador é tratado pela aplicação como acesso total, mesmo sem linhas nesta tabela.
